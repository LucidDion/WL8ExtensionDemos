using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Community
{
    public class RealRelativeStrength : IndicatorBase
    {
        //parameterless constructor
        public RealRelativeStrength() : base()
        {
        }

        //for code based construction
        public RealRelativeStrength(BarHistory source, String compareSymbol, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = compareSymbol;
            Parameters[2].Value = period;

            Populate();
        }

        //static Series method
        public static RealRelativeStrength Series(BarHistory source, String compareSymbol, Int32 period)
        {
            string key = CacheKey("RealRelativeStrength", compareSymbol, period);
            if (source.Cache.ContainsKey(key))
                return (RealRelativeStrength)source.Cache[key];
            RealRelativeStrength rrs = new RealRelativeStrength(source, compareSymbol, period);
            source.Cache[key] = rrs;
            return rrs;
        }

        //name
        public override string Name => "RealRelativeStrength";

        //abbreviation
        public override string Abbreviation => "RRS";

        //description
        public override string HelpDescription => "Real Relative Strength measured against a compared symbol or sector ETF";

        //price pane
        public override string PaneTag => "RRS";

        //help URL
        public override string HelpURL => "https://www.reddit.com/r/RealDayTrading/comments/rp5rmx/a_new_measure_of_relative_strength/";

        //default color
        public override WLColor DefaultColor => WLColor.FromArgb(255, 50, 205, 50);

        //default plot style
        public override PlotStyle DefaultPlotStyle => PlotStyle.Oscillator;

        //populate
        public override void Populate()
        {
            BarHistory source = Parameters[0].AsBarHistory;
            String compareSymbol = Parameters[1].AsString;
            Int32 period = Parameters[2].AsInt;

            DateTimes = source.DateTimes;

            BarHistory compSource = WLHost.Instance.GetHistory(compareSymbol, source.Scale, DateTime.MinValue, DateTime.MaxValue, source.Count, null);
            ATR compAtr = ATR.Series(compSource, period);

            if (source != null && source.Count > 0)
            {
                ATR symAtr = ATR.Series(source, period);
                //TimeSeries compAtrSynced = TimeSeriesSynchronizer.Synchronize(compAtr, source);

                for (int n = 0; n < compSource.Count; n++)
                {
                    if (n > period)
                    {
                        double compSymRollingMove = compSource.Close[n] - compSource.Close[n - period];
                        double symRollingMove = source.Close[n] - source.Close[n - period];

                        var powerIndex = compSymRollingMove / compAtr.Values[n];
                        var rrs = (symRollingMove - powerIndex * symAtr.Values[n]) / symAtr.Values[n];

                        Values[n] = rrs;
                    }
                }

            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Compare Symbol", ParameterType.String, "SPY");
            AddParameter("Period", ParameterType.Int32, 12);
        }
    }
}