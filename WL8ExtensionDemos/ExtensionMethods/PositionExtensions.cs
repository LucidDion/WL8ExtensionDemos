using WealthLab.Backtest;
using WealthLab.Core;

namespace WL8ExtensionDemos
{
    //Extension methods for Positions
    public static class PositionExtensions
    {
        //return a TimeSeries of bar-by-bar percentage returns
        public static TimeSeries BarByBarReturns(this Position pos)
        {
            TimeSeries ts = new TimeSeries();
            if (pos.Bars == null)
                return ts;

            //first value is close - entry price for bar 1
            double gain = pos.Bars.Close[pos.EntryBar] - pos.EntryPrice;
            if (pos.PositionType == PositionType.Short)
                gain *= -1.0;
            gain = gain / pos.EntryPrice * 100.0;
            ts.Add(gain, pos.Bars.DateTimes[pos.EntryBar]);

            //determine end index for bar-by-bar span
            int endIdx = pos.IsOpen ? pos.Bars.Count - 1 : pos.ExitBar - 1;
            for(int idx = pos.EntryBar + 1; idx < endIdx; idx++)
            {
                gain = pos.Bars.Close[idx] - pos.Bars.Close[idx - 1];
                gain = gain / pos.Bars.Close[idx - 1] * 100.0;
                ts.Add(gain, pos.Bars.DateTimes[idx]);
            }

            //if position closed, last gain is exit price
            if (!pos.IsOpen)
            {
                gain = pos.ExitPrice - pos.Bars.Close[pos.ExitBar - 1];
                gain = gain / pos.Bars.Close[pos.ExitBar - 1] * 100.0;
                ts.Add(gain, pos.Bars.DateTimes[pos.ExitBar]);
            }

            return ts;
        }
    }
}