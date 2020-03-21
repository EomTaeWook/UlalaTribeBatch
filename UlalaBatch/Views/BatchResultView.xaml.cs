using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UlalaBatch.Infrastructure;
using UlalaBatch.Models;

namespace UlalaBatch.Views
{
    /// <summary>
    /// BatchResultView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BatchResultView : Page
    {
        private TaskQueue _taskQueue = new TaskQueue();
        private List<BatchResultModel> _dataModels = new List<BatchResultModel>();
        public BatchResultView()
        {
            InitializeComponent();

            this.Loaded += BatchResultView_Loaded;
        }

        private void BatchResultView_Loaded(object sender, RoutedEventArgs e)
        {
            _taskQueue.Enqueue(SaveLoad);
            _taskQueue.Enqueue(ProcessBatchCombatOrder);
        }
        private Task SaveLoad()
        {
            return Task.CompletedTask;
        }
        private Task ProcessBatchCombatOrder()
        {

            return Task.CompletedTask;
        }
    }
}
