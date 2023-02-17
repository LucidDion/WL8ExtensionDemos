using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Indicators
{
    public class MinerviniTrendRatio : IndicatorBase
    {
        public MinerviniTrendRatio() : base() 
        {
        }

        public MinerviniTrendRatio(BarHistory source, String rsSymbol, double sma200Up5MonthsMultiplier,
            double priceUp70Pctg52WeekLowMultiplier, double priceWithin25Pctg52WeekHighMultiplier) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = rsSymbol;
            Parameters[2].Value = sma200Up5MonthsMultiplier;
            Parameters[3].Value = priceUp70Pctg52WeekLowMultiplier;
            Parameters[4].Value = priceWithin25Pctg52WeekHighMultiplier;
            Populate();
        }

        public static MinerviniTrendRatio Series(BarHistory source, String rsSymbol, double sma200Up5MonthsMultiplier,
            double priceUp70Pctg52WeekLowMultiplier, double priceWithin25Pctg52WeekHighMultiplier)
        {
            string key = CacheKey("MinerviniTrendRatio", rsSymbol, sma200Up5MonthsMultiplier, priceUp70Pctg52WeekLowMultiplier, priceWithin25Pctg52WeekHighMultiplier);
            if (source.Cache.ContainsKey(key))
                return (MinerviniTrendRatio)source.Cache[key];
            MinerviniTrendRatio mttr = new MinerviniTrendRatio(source, rsSymbol, sma200Up5MonthsMultiplier, priceUp70Pctg52WeekLowMultiplier, priceWithin25Pctg52WeekHighMultiplier);
            source.Cache[key] = mttr;
            return mttr;
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, PriceComponent.Close);
            AddParameter("Relative Strength Index", ParameterType.String, "QQQ");
            AddParameter("Multiplier: SMA 200 trending up at least 5 months", ParameterType.Double, 1.1);
            AddParameter("Multiplier: Price at least 70% above 52 Week Low", ParameterType.Double, 1.2);
            AddParameter("Multiplier: Price within 25% of 52 Week High", ParameterType.Double, 1.2);
        }

        public override string Name { get { return "Minervini Trend Ratio"; } }

        public override string Abbreviation { get { return "MTR"; } }

        public override string HelpDescription { get { return "The Minervini Trend Ratio combines several moving averages as well as closing prices in relation to the 52-week high / 52-week low and the Mansfield relative strength to determine a trend direction to show the current potential. A total of 10 criteria are used, which are compared with each other resulting in a ratio. The condition is considered to be met if the ratio is 90 or greater. Strong trends should be greater than 100."; } }

        public override string PaneTag { get { return "Minervini Trend Ratio"; } }

        public override PlotStyle DefaultPlotStyle { get { return PlotStyle.Line; } }

        public override WLColor DefaultColor { get { return WLColor.CornflowerBlue; } }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            String indexSymbol = Parameters[1].AsString;
            double sma200Up5MonthsMultiplier = Parameters[2].AsDouble;
            double priceUp70Pctg52WeekLowMultiplier = Parameters[3].AsDouble;
            double priceWithin25Pctg52WeekHighMultiplier = Parameters[4].AsDouble;
            DateTimes = bars.DateTimes;

            if (bars.Count == 0 || bars.Count < 200)
                return;

            BarHistory indexDailyBars = WLHost.Instance.GetHistory(indexSymbol, bars.Scale, DateTime.MinValue, DateTime.MaxValue, bars.Count, null);
            TimeSeries indexClose = TimeSeriesSynchronizer.Synchronize(indexDailyBars.Close, bars.Close);
            TimeSeries minerviniRatio = new TimeSeries(bars.DateTimes);
            TimeSeries mansfieldRs = new TimeSeries(bars.DateTimes);

            mansfieldRs = FastSMA.Series(((((bars.Close / indexClose) * 100) / FastSMA.Series(((bars.Close / indexClose) * 100), 200)) - 1) * 100, 5);

            Lowest minervini52WeeklsLow = new Lowest(bars.Low, 252);
            Highest minervini52WeeklsHigh = new Highest(bars.High, 252);
            FastSMA minerviniSma20 = new FastSMA(bars.Close, 20);
            FastSMA minerviniSma50 = new FastSMA(bars.Close, 50);
            FastSMA minerviniSma150 = new FastSMA(bars.Close, 150);
            FastSMA minerviniSma200 = new FastSMA(bars.Close, 200);
            ConsecUp minerviniConsecUpSma200 = new ConsecUp(minerviniSma200, 1);

            for (int idx = 0; idx < bars.Count; idx++)
            {
                bool condition1 = bars.Close[idx] > minerviniSma150[idx] && bars.Close[idx] > minerviniSma200[idx];
                bool condition2 = minerviniSma150[idx] > minerviniSma200[idx];
                bool condition3 = minerviniConsecUpSma200[idx] >= 21;
                bool condition4 = minerviniConsecUpSma200[idx] >= (5 * 21);
                bool condition5 = minerviniSma50[idx] > minerviniSma150[idx] && minerviniSma50[idx] > minerviniSma200[idx];
                bool condition6 = bars.Close[idx] > minerviniSma50[idx];
                bool condition7 = idx - 252 >= 0 && (bars.Close[idx] > minervini52WeeklsLow[idx] * (1.0 + (30.00 / 100.0)));
                bool condition8 = idx - 252 >= 0 && (bars.Close[idx] > minervini52WeeklsLow[idx] * (1.0 + (70.00 / 100.0)));
                bool condition9 = bars.Close[idx] >= (0.75 * minervini52WeeklsHigh[idx]);

                double cumulatedRatio = 0;
                int conditionCount = 10;
                cumulatedRatio += condition1 ? (100 / conditionCount) : 0;
                cumulatedRatio += condition2 ? (100 / conditionCount) : 0;
                cumulatedRatio += condition3 ? (100 / conditionCount) : 0;
                cumulatedRatio += condition4 ? (100 / conditionCount) * sma200Up5MonthsMultiplier : 0;
                cumulatedRatio += condition5 ? (100 / conditionCount) : 0;
                cumulatedRatio += condition6 ? (100 / conditionCount) : 0;
                cumulatedRatio += condition7 ? (100 / conditionCount) : 0;
                cumulatedRatio += condition8 ? (100 / conditionCount) * priceUp70Pctg52WeekLowMultiplier : 0;
                cumulatedRatio += condition9 ? (100 / conditionCount) * priceWithin25Pctg52WeekHighMultiplier : 0;
                double mrsRatio = mansfieldRs[idx] > 30 ? (100 / 10) * 1.3 : mansfieldRs[idx] > 20 ? (100 / 10) * 1.2 : mansfieldRs[idx] > 10 ? (100 / 10) * 1.1 : mansfieldRs[idx] > 0 ? (100 / 10) : 0;
                cumulatedRatio += mrsRatio;

                Values[idx] = cumulatedRatio;
            }
        }
    }
}