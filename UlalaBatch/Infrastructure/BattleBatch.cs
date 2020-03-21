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
        private List<CharacterInfoModel> _sortCharacterInfoModels;
        private HashSet<string> _includeBattleNickname = new HashSet<string>();
        private volatile int _isProcess = 0;
        public void Init(IEnumerable<CharacterInfoModel> characterInfoModels)
        {
            this._sortCharacterInfoModels = characterInfoModels.OrderByDescending(r => r.CombatPower).ToList();
            _includeBattleNickname.Clear();
        }
        public ObservableCollection<BatchResultModel> Batch()
        {
            var result = new ObservableCollection<BatchResultModel>();
            if (this._sortCharacterInfoModels == null)
            {
                return result;
            }

            if (Interlocked.Increment(ref _isProcess) == 1)
            {
                var index = 0;
                
                for (; ; )
                {
                    var position = Position.Attack;
                    if (_includeBattleNickname.Count == this._sortCharacterInfoModels.Count)
                    {
                        break;
                    }
                    var batchModel = new BatchResultModel
                    {
                        Tanker = FindTopTanker(),
                        Healer = FindTopHealer(),
                        Dealer1 = FindTopDealer(null)
                    };

                    if (batchModel.Dealer1 != null)
                    {
                        batchModel.Dealer2 = FindTopDealer(batchModel.Dealer1);
                    }
                    if(batchModel.Dealer2 == null)
                    {
                        batchModel.Dealer2 = FindTopDealer(null);
                    }
                    if(index == 0)
                    {
                        position = Position.Elite;
                    }
                    else if (index >= Consts.MaxPositionIndex)
                    {
                        position = Position.Defence;
                    }
                    batchModel.Position = position;
                    batchModel.Index = index++;

                    result.Add(batchModel);
                }
                Interlocked.Decrement(ref _isProcess);

                return result;
            }
            return result;
        }
        private CharacterInfoModel FindTopTanker()
        {
            for (int i=0; i< this._sortCharacterInfoModels.Count; ++i)
            {
                if(this._sortCharacterInfoModels[i].JobGroupType == JobGroupType.Tanker)
                {
                    if (_includeBattleNickname.Contains(this._sortCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    _includeBattleNickname.Add(this._sortCharacterInfoModels[i].Nickname);
                    return this._sortCharacterInfoModels[i];
                }
            }
            return null;
        }
        private CharacterInfoModel FindTopDealer(CharacterInfoModel dealer)
        {
            for (int i = 0; i < this._sortCharacterInfoModels.Count; ++i)
            {
                if (this._sortCharacterInfoModels[i].JobGroupType == JobGroupType.Dealer)
                {
                    if (_includeBattleNickname.Contains(this._sortCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    if (dealer != null)
                    {
                        if(dealer.JobType == this._sortCharacterInfoModels[i].JobType)
                        {
                            continue;
                        }
                    }
                    _includeBattleNickname.Add(this._sortCharacterInfoModels[i].Nickname);
                    return this._sortCharacterInfoModels[i];
                }
            }
            return null;
        }
        private CharacterInfoModel FindTopHealer()
        {
            for (int i = 0; i < this._sortCharacterInfoModels.Count; ++i)
            {
                if (this._sortCharacterInfoModels[i].JobGroupType == JobGroupType.Healer)
                {
                    if (_includeBattleNickname.Contains(this._sortCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    _includeBattleNickname.Add(this._sortCharacterInfoModels[i].Nickname);
                    return this._sortCharacterInfoModels[i];
                }
            }
            return null;
        }
    }
}
