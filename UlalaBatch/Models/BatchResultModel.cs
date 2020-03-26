using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UlalaBatch.Infrastructure;

namespace UlalaBatch.Models
{
    public class BatchResultModel
    {
        public string PositionDesc { 
            get 
            {
                return $"{Position}{Index}";
            } 
        }
        public CharacterInfoModel Tanker { get; set; }
        public CharacterInfoModel Dealer1 { get; set; }
        public CharacterInfoModel Dealer2 { get; set; }
        public CharacterInfoModel Healer { get; set; }
        public Position Position { get; set; }
        public int Index { get; set; }
        public int CombatPower { get; set; }
        public bool IsCombination()
        {
            if(Tanker != null && Healer != null && Dealer1 != null && Dealer2 != null)
            {
                if(Dealer1.JobType != Dealer2.JobType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
