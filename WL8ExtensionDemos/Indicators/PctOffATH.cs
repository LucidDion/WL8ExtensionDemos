using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Community
{
    //by Springroll
    //https://www.wealth-lab.com/Discussion/New-indicators-for-the-WL8-community-9060
    public class PercentOffATH : IndicatorBase
    {
        public PercentOffATH() : base()
        {
        }

        public PercentOffATH(BarHistory source) : base()
        {
            Parameters[0].Value = source;
            Populate();
        }

        public static PercentOffATH Series(BarHistory source)
        {
            return new PercentOffATH(source);
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, PriceComponent.Close);
        }

        public override string Name { get { return "Percent Off All-time High"; } }

        public override string Abbreviation { get { return "PoATH"; } }

        public override string HelpDescription { get { return "Percent Off All-time High tracks the percentage difference between the previous day’s closing price and the All-Time high price of the entire price history. This metric tells you how far the price has deviated from/converged on the all-time high price."; } }

        public override string PaneTag { get { return "Percent Off All-time High"; } }

        public override PlotStyle DefaultPlotStyle { get { return PlotStyle.Line; } }

        public override WLColor DefaultColor { get { return WLColor.CornflowerBlue; } }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            DateTimes = bars.DateTimes;

            if (bars.Count == 0 || bars.Count < 10)
                return;

            double highestClose = Double.MinValue;
            TimeSeries offAllTimeHigh = new TimeSeries(bars.DateTimes);

            for (int bar = 1; bar < bars.Count; bar++)
            {
                if (bars.Close[bar] > highestClose)
                    highestClose = bars.Close[bar];
                offAllTimeHigh[bar] = -Math.Abs((highestClose - bars.Close[bar]) * 100.0 / highestClose);
            }
            Values = offAllTimeHigh.Values;
        }
    }
}