using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Community
{
    //by Springroll
    //https://www.wealth-lab.com/Discussion/New-indicators-for-the-WL8-community-9060
    public class RsiWilliamsVixFix : IndicatorBase
    {
        public RsiWilliamsVixFix() : base()
        { }

        public RsiWilliamsVixFix(BarHistory source, int emaPeriod, int rsiPeriod) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = emaPeriod;
            Parameters[2].Value = rsiPeriod;
            Populate();
        }

        public static RsiWilliamsVixFix Series(BarHistory source, int emaPeriod, int rsiPeriod)
        {
            return new RsiWilliamsVixFix(source, emaPeriod, rsiPeriod);
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, PriceComponent.Close);
            AddParameter("EmaPeriod", ParameterType.Int32, 3);
            AddParameter("RsiPeriod", ParameterType.Int32, 14);
        }

        public override string Name { get { return "RSI Williams Vix Fix"; } }

        public override string Abbreviation { get { return "RWVF"; } }

        public override string HelpDescription { get { return "The VixRSI was presented in the January 2014 issue of the TASC magazine. Many years ago, Larry Williams developed something he calls the VIX fix.What he developed was a simple calculation that closely emulates the performance of the original VIX using only price data, and which can therefore be applied to any tradable. In a nutshell, we take the highest close over the last 22 trading days, subtract today’s low price, and then divide the result by the highest close over the last 22 trading days. Combining the VixFix with the RSI makes it even more powerful to detect Instrument bottoms and oversold areas. Detecting overbought VIX \"greed areas\" can also be useful if you try to catch a fast and profitable long trade on the VIX itself."; } }

        public override string PaneTag { get { return "RWVF"; } }

        public override PlotStyle DefaultPlotStyle { get { return PlotStyle.Histogram; } }

        public override WLColor DefaultColor { get { return WLColor.Green; } }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 emaPeriod = Parameters[1].AsInt;
            Int32 rsiPeriod = Parameters[2].AsInt;
            DateTimes = bars.DateTimes;

            if (bars.Count == 0 || bars.Count < rsiPeriod)
                return;

            TimeSeries wvf = ((((Highest.Series(bars.Close, 22) >> 1) - bars.Low) / (Highest.Series(bars.Close, 22) >> 1) * 100) + 50);
            var ema = EMA.Series(wvf, emaPeriod);
            var rsi = RSI.Series(EMA.Series(bars.Close, emaPeriod), rsiPeriod);
            var result = ema / rsi;
            Values = result.Values;
        }
    }
}
