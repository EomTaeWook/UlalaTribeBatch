using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UlalaBatch.Models;

namespace UlalaBatch.Infrastructure
{
    public class BattleBatch
    {
        private List<CharacterInfoModel> _sortNonePartyCharacterInfoModels;
        private IEnumerable<CharacterInfoModel> characterInfoModels;
        private List<BatchResultModel> _queueBatchResult = new List<BatchResultModel>();
        private readonly ObservableCollection<BatchResultModel> _result = new ObservableCollection<BatchResultModel>();
        private readonly HashSet<string> _includeBattleNickname = new HashSet<string>();

        private volatile int _isProcess = 0;
        public void Init(IEnumerable<CharacterInfoModel> characterInfoModels)
        {
            this.characterInfoModels = characterInfoModels;

            this._sortNonePartyCharacterInfoModels = characterInfoModels.Where(r=>r.PartyGroup == 0).OrderByDescending(r => r.CombatPower).ToList();

            _includeBattleNickname.Clear();
            _queueBatchResult.Clear();
            _result.Clear();
        }
        public ObservableCollection<BatchResultModel> Batch()
        {
            if (this.characterInfoModels == null)
            {
                return _result;
            }
            if (Interlocked.Increment(ref _isProcess) == 1)
            {
                MakeNonePartyBatchResultModel();
                MakePartyBatchResultModel();

                foreach (var item in SortingPositionIndex(Position.Elite, _queueBatchResult))
                {
                    _result.Add(item);
                }
                foreach (var item in SortingPositionIndex(Position.Attack, _queueBatchResult))
                {
                    _result.Add(item);
                }
                foreach (var item in SortingPositionIndex(Position.Defence, _queueBatchResult))
                {
                    _result.Add(item);
                }
                Interlocked.Decrement(ref _isProcess);
                return _result;
            }
            return _result;
        }
        private List<BatchResultModel> SortingCombatPower(Position position, IEnumerable<BatchResultModel> unorderList)
        {
            return new List<BatchResultModel>(unorderList.Where(r => r.Position == position).OrderByDescending(r => r.CombatPower));
        }
        private List<BatchResultModel> SortingPositionIndex(Position position, IEnumerable<BatchResultModel> unorderList)
        {
            if (position == Position.Elite)
            {
                return new List<BatchResultModel>(unorderList.Where(r => r.Position == Position.Elite));
            }
            else if (position == Position.Attack)
            {
                return new List<BatchResultModel>(unorderList.Where(r => r.Position == position).OrderBy(r => r.Index));
            }
            else if (position == Position.Defence)
            {
                return new List<BatchResultModel>(unorderList.Where(r => r.Position == Position.Defence).OrderByDescending(r => r.Index));
            }
            else
            {
                return new List<BatchResultModel>();
            }
        }
        private void MakePartyBatchResultModel()
        {
            var partyGroups = characterInfoModels.Where(r=>r.PartyGroup > 0).GroupBy(r => r.PartyGroup);
            foreach (var party in partyGroups)
            {
                var item = new BatchResultModel
                {
                    Position = Position.Elite
                };
                var dealer = 0;
                var participation = new List<CharacterInfoModel>();
                foreach (var character in party)
                {
                    if(character.IsEliteExclusion == true)
                    {
                        item.Position = Position.Attack;
                    }
                    if (character.IsOnlyDefence)
                    {
                        item.Position = Position.Defence;
                    }
                    if (character.JobGroupType == JobGroupType.Tanker && item.Tanker == null)
                    {
                        item.Tanker = character;
                        participation.Add(character);
                    }
                    else if (character.JobGroupType == JobGroupType.Dealer)
                    {
                        if (dealer == 0)
                        {
                            item.Dealer1 = character;
                            dealer++;
                            participation.Add(character);

                        }
                        else if(dealer == 1)
                        {
                            item.Dealer2 = character;
                            dealer++;
                            participation.Add(character);
                        }
                        else
                        {
                        }
                        
                    }
                    else if (character.JobGroupType == JobGroupType.Healer && item.Healer == null)
                    {
                        item.Healer = character;
                        participation.Add(character);
                    }
                    _includeBattleNickname.Add(character.Nickname);
                }
                
                if (participation.Count() != party.Count())
                {
                    ReSettingBattleResultModel(participation, party.ToList(), item);
                }
                SetSumCombatPower(item);
                _queueBatchResult.Add(item);
            }

            var eliteGroups = SortingCombatPower(Position.Elite, _queueBatchResult);
            var index = 1;
            for (int i = 1; i<eliteGroups.Count; ++i)
            {
                eliteGroups[i].Index = index;
                eliteGroups[i].Position = Position.Attack;
            }

            var attackGroups = SortingCombatPower(Position.Attack, _queueBatchResult);
            foreach(var party in attackGroups)
            {
                party.Index = index;
                if(index > Consts.MaxPositionIndex)
                {
                    party.Position = Position.Defence;
                }
                index++;
            }
            index = Consts.MaxPositionIndex;
            var defanceGroups = SortingCombatPower(Position.Defence, _queueBatchResult);
            foreach (var party in defanceGroups)
            {
                party.Index = index;
                if (index <= 0)
                {
                    party.Position = Position.Max;
                    index = 0;
                }
                index--;
            }
        }
        private void ReSettingBattleResultModel(IEnumerable<CharacterInfoModel> participation, IEnumerable<CharacterInfoModel> party, BatchResultModel batchResultModel)
        {
            foreach(var character in party)
            {
                if(participation.Any(r=>r == character))
                {
                    continue;
                }
                if(batchResultModel.Tanker == null)
                {
                    batchResultModel.Tanker = character;
                }
                else if(batchResultModel.Dealer1 == null)
                {
                    batchResultModel.Dealer1 = character;
                }
                else if (batchResultModel.Dealer2 == null)
                {
                    batchResultModel.Dealer2 = character;
                }
                else if (batchResultModel.Healer == null)
                {
                    batchResultModel.Healer = character;
                }
            }
        }
        private void MakeNonePartyBatchResultModel()
        {
            var index = 0;
            var increase = 1;
            var position = Position.Max;
            for (; ;)
            {
                if (_includeBattleNickname.Count == _sortNonePartyCharacterInfoModels.Count)
                {
                    break;
                }
                if (position == Position.Defence && index == 0)
                {
                    break;
                }
                if (index == 0)
                {
                    position = Position.Elite;
                }
                else if (index <= Consts.MaxPositionIndex && position == Position.Elite)
                {
                    position = Position.Attack;
                }
                else if (index > Consts.MaxPositionIndex && position == Position.Attack)
                {
                    position = Position.Defence;
                    index--;
                    increase = -1;
                }
                var batchModel = new BatchResultModel
                {
                    Tanker = FindFreeTopCombatPower(position, JobGroupType.Tanker),
                    Healer = FindFreeTopCombatPower(position, JobGroupType.Healer),
                    Dealer1 = FindFreeTopCombatPower(position, JobGroupType.Dealer)
                };

                if (batchModel.Dealer1 != null)
                {
                    batchModel.Dealer2 = FindFreeTopCombatPower(position, JobGroupType.Dealer, batchModel.Dealer1);
                }
                if (batchModel.Dealer2 == null)
                {
                    batchModel.Dealer2 = FindFreeTopCombatPower(position, JobGroupType.Dealer);
                }
                batchModel.Position = position;
                batchModel.Index = index;

                SetSumCombatPower(batchModel);
                index += increase;
                _queueBatchResult.Add(batchModel);
            }
        }
        private void SetSumCombatPower(BatchResultModel batchResultModel)
        {
            int sumCombatPower = 0;
            if (batchResultModel.Tanker != null)
            {
                sumCombatPower += batchResultModel.Tanker.CombatPower;
            }
            if (batchResultModel.Dealer1 != null)
            {
                sumCombatPower += batchResultModel.Dealer1.CombatPower;
            }
            if (batchResultModel.Dealer2 != null)
            {
                sumCombatPower += batchResultModel.Dealer2.CombatPower;
            }
            if (batchResultModel.Healer != null)
            {
                sumCombatPower += batchResultModel.Healer.CombatPower;
            }
            batchResultModel.CombatPower = sumCombatPower;
        }
        private CharacterInfoModel FindFreeTopCombatPower(Position position, JobGroupType jobGroupType, CharacterInfoModel dealer = null)
        {
            for (int i = 0; i < this._sortNonePartyCharacterInfoModels.Count; ++i)
            {
                if (this._sortNonePartyCharacterInfoModels[i].JobGroupType == jobGroupType)
                {
                    if (_includeBattleNickname.Contains(this._sortNonePartyCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    else if(position == Position.Elite && this._sortNonePartyCharacterInfoModels[i].IsEliteExclusion)
                    {
                        continue;
                    }
                    else if ((position == Position.Attack || position == Position.Elite) && this._sortNonePartyCharacterInfoModels[i].IsOnlyDefence)
                    {
                        continue;
                    }
                    if (dealer != null)
                    {
                        if (dealer.JobType == this._sortNonePartyCharacterInfoModels[i].JobType)
                        {
                            continue;
                        }
                    }
                    _includeBattleNickname.Add(this._sortNonePartyCharacterInfoModels[i].Nickname);
                    return this._sortNonePartyCharacterInfoModels[i];
                }
            }
            return null;
        }
    }
}
