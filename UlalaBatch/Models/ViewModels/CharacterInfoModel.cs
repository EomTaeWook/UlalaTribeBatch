using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UlalaBatch.Infrastructure;

namespace UlalaBatch.Models
{
    public class CharacterInfoModel : INotifyPropertyChanged
    {
        private string _nickname;
        private int _combatPower;
        private JobGroupType _jobGroupType;
        private JobType _jobType;
        private bool _isSelect;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Nickname
        { 
            get => _nickname;
            set 
            { 
                this._nickname = value;
                OnPropertyChanged("Nickname");
            }
        }
        public int CombatPower 
        { 
            get => _combatPower;
            set 
            {
                this._combatPower = value;
                OnPropertyChanged("CombatPower");
            }
        }
        public JobGroupType JobGroupType
        {
            get => this._jobGroupType;
            set
            {
                this._jobGroupType = value;
                OnPropertyChanged("JobGroupType");
            }
        }
        public JobType JobType 
        {
            get => this._jobType;
            set
            {
                this._jobType = value;
                OnPropertyChanged("JobType");
            }
        }
        public bool IsSelect
        {
            get => this._isSelect;
            set
            {
                this._isSelect = value;
                OnPropertyChanged("IsSelect");
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}
