using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UlalaBatch.Infrastructure;
using UlalaBatch.Models;

namespace UlalaBatch
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ObservableCollection<CharacterInfoModel> _characterList = new ObservableCollection<CharacterInfoModel>();
        private HashSet<string> _includeNicknames = new HashSet<string>();
        private CharacterInfoModel _selectCharacterInfo = null;
        private CharacterInfoModel _dummy = new CharacterInfoModel();
        private TaskQueue _taskQueue = new TaskQueue();
        private BattleBatch _battleBatch = new BattleBatch();
        private Dictionary<string, ListSortDirection> latestOrderd = new Dictionary<string, ListSortDirection>();
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
            this.btnDelete.Click += BtnDelete_Click;
            this.btnCategoryBattleBatch.Click += BtnCategoryBattleBatch_Click;
            this.btnCategorySubscribers.Click += BtnCategorySubscribers_Click;
        }

        private void BtnCopyMenu_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach(var selectItem in this.listBattleBatch.SelectedItems)
            {
                var item = selectItem as BatchResultModel;
                var text = $@"[위치 : {item.PositionDesc}] [탱커 : {item.Tanker?.Nickname}] [딜러1 : {item.Dealer1?.Nickname}] [딜러2 : {item.Dealer2?.Nickname}] [힐러 : {item.Healer?.Nickname}]";
                sb.AppendLine(text);
            }
            Clipboard.SetText(sb.ToString());
        }
        private void BtnExcelMenu_Click(object sender, RoutedEventArgs e)
        {
            var excel = new Microsoft.Office.Interop.Excel.Application();
            try
            {
                var workBook = excel.Workbooks.Add(Type.Missing);
                var workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                workSheet.Name = "batch";
                var columns = new string[] { "위치", "탱커", "딜러1", "딜러2", "힐러" };
                for(int i=0; i< columns.Length; ++i)
                {
                    workSheet.Cells[1, i+1] = columns[i];
                }
                var row = 2;
                foreach (var selectItem in this.listBattleBatch.SelectedItems)
                {
                    var item = selectItem as BatchResultModel;

                    workSheet.Cells[row, 1] = item.PositionDesc;
                    workSheet.Cells[row, 2] = item.Tanker? .Nickname;
                    workSheet.Cells[row, 3] = item.Dealer1? .Nickname;
                    workSheet.Cells[row, 4] = item.Dealer2? .Nickname;
                    workSheet.Cells[row, 5] = item.Healer? .Nickname;
                    row++;
                }
                Microsoft.Office.Interop.Excel.Range range = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[row, columns.Length]];
                range.EntireColumn.AutoFit();
                workBook.SaveAs($"{AppDomain.CurrentDomain.BaseDirectory + "UlalaTribeBatch"}",
                    Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                workBook.Close();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                excel.Quit();
            }
        }
        private void BtnCategorySubscribers_Click(object sender, RoutedEventArgs e)
        {
            listBattleBatch.Visibility = Visibility.Hidden;
            listSubscribers.Visibility = Visibility.Visible;
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectCharacterInfo != null)
            {
                _includeNicknames.Remove(_selectCharacterInfo.Nickname);
                this._characterList.Remove(this._selectCharacterInfo);
                _taskQueue.Enqueue(FileSave);
                Clear();
            }
        }

        private void BtnCategoryBattleBatch_Click(object sender, RoutedEventArgs e)
        {
            _battleBatch.Init(this._characterList);
            _taskQueue.Enqueue(()=> 
            {
                var result = _battleBatch.Batch();
                listBattleBatch.ItemsSource = result;

                return Task.CompletedTask;
            });
            listBattleBatch.Visibility = Visibility.Visible;
            listSubscribers.Visibility = Visibility.Hidden;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(_selectCharacterInfo != null)
            {
                if (Validate() == true)
                {
                    var nickname = txtNickname.Text.Trim();
                    if (_selectCharacterInfo.Nickname.Equals(nickname) == false)
                    {
                        if(_includeNicknames.Contains(nickname) == true)
                        {
                            MessageBox.Show("동일한 닉네임이 존재합니다.");
                            return;
                        }
                        _includeNicknames.Remove(_selectCharacterInfo.Nickname);
                        _includeNicknames.Add(nickname);
                    }
                    _selectCharacterInfo.Nickname = nickname;
                    _selectCharacterInfo.CombatPower = (int)numCombatPower.Value.Value;
                    _selectCharacterInfo.JobType = (JobType)Enum.Parse(typeof(JobType), comboJob.SelectedItem.ToString());
                    _selectCharacterInfo.JobGroupType = GetJobGroupType(_selectCharacterInfo.JobType);
                    Clear();
                    _taskQueue.Enqueue(FileSave);
                }
                    
            }
        }
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if(this.latestOrderd[headerClicked.Column.Header.ToString()] == ListSortDirection.Ascending)
            {
                this.latestOrderd[headerClicked.Column.Header.ToString()] = ListSortDirection.Descending;
            }
            else
            {
                this.latestOrderd[headerClicked.Column.Header.ToString()] = ListSortDirection.Ascending;
            }
            Sort(headerClicked.Column.Header as string, this.latestOrderd[headerClicked.Column.Header.ToString()], this.listSubscribers.ItemsSource);
        }
        private void Sort(string propertyName, ListSortDirection direction, object itemsSource)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(itemsSource);

            dataView.SortDescriptions.Clear();

            var sortDescription = new SortDescription(propertyName, direction);
            dataView.SortDescriptions.Add(sortDescription);
            dataView.Refresh();
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
                this.btnDelete.Visibility = Visibility.Visible;
                this.btnCancel.Visibility = Visibility.Visible;

                this.txtNickname.Text = selectItem.Nickname;
                this.numCombatPower.Value = selectItem.CombatPower;
                this.comboJob.SelectedValue = selectItem.JobType;

                this._selectCharacterInfo = selectItem;
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
            this.listSubscribers.ItemsSource = _characterList;

            var gridView = (this.listSubscribers.View as GridView);
            foreach(var column in gridView.Columns)
            {
                this.latestOrderd.Add(column.Header.ToString(), ListSortDirection.Ascending);
            }
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

                if(_includeNicknames.Contains(nickname) == true)
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

                _characterList.Add(item);
                _includeNicknames.Add(item.Nickname);
                
                Clear();

                _taskQueue.Enqueue(FileSave);
            }
        }
        private Task FileLoad()
        {
            if(File.Exists($"./{Consts.SaveFileName}"))
            {
                var json = File.ReadAllText($"./{Consts.SaveFileName}");
                _characterList = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<CharacterInfoModel>>(json);
                this._includeNicknames.Clear();

                this.listSubscribers.ItemsSource = _characterList;
                int total = 0;
                for(int i=0; i<_characterList.Count; ++i)
                {
                    _includeNicknames.Add(_characterList[i].Nickname);
                    total += Convert.ToInt32(_characterList[i].CombatPower);
                }
                this.labelTribeSubscribers.Content = string.Format(Consts.TribeSubscribersString, _characterList.Count);
                this.labelAveragePower.Content = string.Format(Consts.AveragePowerString, total / _characterList.Count);
            }
            return Task.CompletedTask;
        }
        private Task FileSave()
        {
            if(File.Exists($"./{Consts.SaveFileName}"))
            {
                File.Delete($"./{Consts.SaveFileName}");
            }
            var sortList = this._characterList.OrderBy(r => r.Nickname).ToList();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(sortList);
            File.WriteAllText($"./{Consts.SaveFileName}", json);

            return FileLoad();
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
            _selectCharacterInfo = _dummy;
            listSubscribers.SelectedIndex = -1;

            this.btnSave.Visibility = Visibility.Visible;
            this.btnEdit.Visibility = Visibility.Hidden;
            this.btnCancel.Visibility = Visibility.Hidden;
            this.btnDelete.Visibility = Visibility.Hidden;
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
