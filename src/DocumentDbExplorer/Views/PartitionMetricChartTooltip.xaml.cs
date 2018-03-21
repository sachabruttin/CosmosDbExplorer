using System.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for PartitionMetricChartTooltip.xaml
    /// </summary>
    public partial class PartitionMetricChartTooltip : IChartTooltip
    {
        private TooltipData _data;

        public PartitionMetricChartTooltip()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
