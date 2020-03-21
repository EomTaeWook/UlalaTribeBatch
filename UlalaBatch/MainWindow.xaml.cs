using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UlalaBatch.Infrastructure;
using UlalaBatch.Models;

namespace UlalaBatch
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ObservableCollection<CharacterInfoModel> characterList = new ObservableCollection<CharacterInfoModel>();
        private HashSet<string> nicknames = new HashSet<string>();
        private CharacterInfoModel selectCharacterInfo = null;
        private CharacterInfoModel dummy = new CharacterInfoModel();
        private TaskQueue _taskQueue = new TaskQueue();
        public MainWindow()
        {
            InitializeComponent();
            InitEvent();
            Init();
        }
        private void InitEvent()
        {
            this.btnSave.Click += BtnSave_Click;
            this.btnGithub.Click += BtnGithub_Click;
            this.listSubscribers.SelectionChanged += ListSubscribers_SelectionChanged;
            this.btnCancel.Click += BtnCancel_Click;
            this.btnEdit.Click += BtnEdit_Click;
            this.btnCategoryBattleBatch.Click += BtnCategoryBattleBatch_Click;
        }

        private void BtnCategoryBattleBatch_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(selectCharacterInfo != null)
            {
                if (Validate() == true)
                {
                    if (selectCharacterInfo.Nickname.Equals(txtNickname.Text) == false)
                    {
                        if(nicknames.Contains(txtNickname.Text) == true)
                        {
                            MessageBox.Show("동일한 닉네임이 존재합니다.");
                            return;
                        }
                        nicknames.Remove(selectCharacterInfo.Nickname);
                        nicknames.Add(txtNickname.Text);
                    }
                    selectCharacterInfo.Nickname = txtNickname.Text;
                    selectCharacterInfo.CombatPower = (int)numCombatPower.Value.Value;
                    selectCharacterInfo.JobType = (JobType)Enum.Parse(typeof(JobType), comboJob.SelectedItem.ToString());
                    selectCharacterInfo.JobGroupType = GetJobGroupType(selectCharacterInfo.JobType);
                    Clear();
                }
                    
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void ListSubscribers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.listSubscribers.SelectedItem is CharacterInfoModel selectItem)
            {
                this.btnSave.Visibility = Visibility.Hidden;
                this.btnEdit.Visibility = Visibility.Visible;
                this.btnCancel.Visibility = Visibility.Visible;

                this.txtNickname.Text = selectItem.Nickname;
                this.numCombatPower.Value = selectItem.CombatPower;
                this.comboJob.SelectedValue = selectItem.JobType;

                this.selectCharacterInfo = selectItem;
            }
        }

        private void Init()
        {
            var jobItems = new List<JobType>();
            for(var e = JobType.Fighter; e<JobType.Max; ++e)
            {
                jobItems.Add(e);
            }
            
            this.comboJob.ItemsSource = jobItems;

            FileLoad();
            this.listSubscribers.ItemsSource = characterList;
            Clear();
        }
        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Consts.HelpUrl);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if(Validate() == true)
            {
                var nickname = txtNickname.Text.Trim();

                if(nicknames.Contains(nickname) == true)
                {
                    MessageBox.Show("동일한 닉네임이 존재합니다.");
                    return;
                }
                var combatPower = 0;
                if (numCombatPower.Value.HasValue)
                {
                    combatPower = (int)numCombatPower.Value.Value;
                }

                var jobType = (JobType)Enum.Parse(typeof(JobType), comboJob.SelectedItem.ToString());
                var item = new CharacterInfoModel()
                {
                    Nickname = nickname,
                    CombatPower = combatPower,
                    JobType = jobType,
                    JobGroupType = GetJobGroupType(jobType)
                };

                characterList.Add(item);
                nicknames.Add(item.Nickname);
                
                Clear();

                _taskQueue.Enqueue(FIleSave);
            }
        }
        private Task FileLoad()
        {
            if(File.Exists($"./{Consts.SaveFileName}"))
            {
                var json = File.ReadAllText($"./{Consts.SaveFileName}");
                characterList = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<CharacterInfoModel>>(json);
            }
            return Task.CompletedTask;
        }
        private Task FIleSave()
        {
            if(File.Exists($"./{Consts.SaveFileName}"))
            {
                File.Delete($"./{Consts.SaveFileName}");
            }
            var save = this.characterList.OrderBy(r => r.Nickname).ToList();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(save);
            File.WriteAllText($"./{Consts.SaveFileName}", json);

            return Task.CompletedTask;
        }
        private bool Validate()
        {
            if (string.IsNullOrEmpty(txtNickname.Text))
            {
                MessageBox.Show("닉네임을 입력하세요.");
                return false;
            }
            else if (numCombatPower.Value.HasValue == false)
            {
                MessageBox.Show("전투력을 입력하세요.");
                return false;
            }
            else if(numCombatPower.Value.Value <=0)
            {
                MessageBox.Show("전투력을 입력하세요.");
                return false;
            }
            return true;
        }
        private void Clear()
        {
            txtNickname.Text = "";
            numCombatPower.Value = null;
            comboJob.SelectedIndex = 0;
            selectCharacterInfo = dummy;
            listSubscribers.SelectedIndex = -1;

            this.btnSave.Visibility = Visibility.Visible;
            this.btnEdit.Visibility = Visibility.Hidden;
            this.btnCancel.Visibility = Visibility.Hidden;
        }
        private JobGroupType GetJobGroupType(JobType jobType)
        {
            if (jobType == JobType.Fighter || jobType == JobType.Warrior)
            {
                return JobGroupType.Tanker;
            }
            else if (jobType == JobType.Thief || jobType == JobType.Hunter || jobType == JobType.Warlock || jobType == JobType.Wizard)
            {
                return JobGroupType.Dealer;
            }
            else if (jobType == JobType.Xiamen || jobType == JobType.Druid)
            {
                return JobGroupType.Healer;
            }
            else
            {
                return JobGroupType.Max;
            }
        }
       
    }
}
