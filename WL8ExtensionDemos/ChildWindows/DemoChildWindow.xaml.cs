using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WealthLab.Core;
using WealthLab.Data;
using WealthLab.Indicators;
using WealthLab.WPF;

namespace WL8ExtensionDemos
{
    //This child window will appear when the "Demo" menu item is selected under the WL7 Extensions menu
    public partial class DemoChildWindow : ChildWindow
    {
        //constructor
        public DemoChildWindow()
        {
            InitializeComponent();
        }

        //a "Token" is used for internal tracking of the Child Window, specifically when saving and restoring Workspaces
        public override string Token => "Demo";

        //remember the selected indicator we used when our ChildWindow is saved in a Workspace
        //typically the "value" string is some tokenized string containing multiple items, but in this case we're saving only a single item so we can do a straight assignment
        public override string WorkspaceString
        {
            get
            {
                return _indicatorName;
            }
            set
            {
                _indicatorName = value;
            }
        }

        //private members
        private string _indicatorName = "";

        //Inidicator OK button clicked
        private void btnOkClick(object sender, RoutedEventArgs e)
        {
            if (cmbIndicators.SelectedItem == null)
                return;
            _indicatorName = (string)cmbIndicators.SelectedItem;
            PlotIndicator();
        }

        //Plot the selected indicator onto the chart
        private void PlotIndicator()
        {
            if (_indicatorName == "")
                return;

            //make sure chart has data plotted
            BarHistory bars = chart.Core.Bars;
            if (bars == null || bars.Count == 0)
                return;

            //clear old plotted indicator
            chart.Core.ClearStrategyItems();

            //find the selected indicator
            IndicatorBase ib = IndicatorFactory.Instance.Find(_indicatorName);
            if (ib == null)
                return;

            //create a new instance using the data plotted in the chart
            ib = IndicatorFactory.Instance.CreateIndicator(_indicatorName, ib.Parameters, bars);

            //add it to the chart's list of strategy-plotted indicators
            List<IndicatorBase> lst = new List<IndicatorBase>();
            lst.Add(ib);
            chart.Core.StrategyIndicators = lst;

            //refresh chart
            chart.ReloadChart();
        }

        //after new data is loaded into the chart, regenerate the selected indicator
        private void chartBarsAssigned(object sender, EventArgs e)
        {
            PlotIndicator();
        }

        //after the selected indicator changes, refresh the chart
        private void cmbIndChange(object sender, SelectionChangedEventArgs e)
        {
            PlotIndicator();
        }

        //key down on symbol field
        private void txtSymbolKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string symbol = txtSymbol.Text.Trim();
                if (symbol == "")
                    return;
                BarHistory bars = WLHost.Instance.GetHistory(symbol, HistoryScale.Daily, DateTime.MinValue, DateTime.MaxValue, 0, new DataRequestOptions());
                if (bars == null || bars.Count == 0)
                    return;
                string secName = bars.SecurityName;
                bars.SecurityName = secName + " Daily";
                coreDaily.Bars = bars;
                BarHistory weekly = BarHistoryCompressor.ToWeekly(bars);
                weekly.SecurityName = secName + " Weekly";
                coreWeekly.Bars = weekly;
                BarHistory monthly = BarHistoryCompressor.ToMonthly(bars);
                monthly.SecurityName = secName + " Monthly";
                coreMonthly.Bars = monthly;
                BarHistory quarterly = BarHistoryCompressor.ToQuarterly(bars);
                quarterly.SecurityName = secName + " Quarterly";
                coreQuarterly.Bars = quarterly;
                txtSymbol.Text = "";
            }
        }
    }
}