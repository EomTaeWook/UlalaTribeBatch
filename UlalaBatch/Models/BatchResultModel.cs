using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlalaBatch.Models
{
    public class BatchResultModel
    {
        public CharacterInfoModel Tanker { get; set; }
        public CharacterInfoModel Dealer1 { get; set; }
        public CharacterInfoModel Dealer2 { get; set; }
        public CharacterInfoModel Healer { get; set; }

    }
}
