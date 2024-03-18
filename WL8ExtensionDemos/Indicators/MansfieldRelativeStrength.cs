using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Community
{
    public class MansfieldRelativeStrength : IndicatorBase
    {
        public MansfieldRelativeStrength() : base()
        {
        }

        public MansfieldRelativeStrength(BarHistory source, String rsSymbol, int dorseyRsPeriod) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = rsSymbol;
            Parameters[2].Value = dorseyRsPeriod;
            Populate();
        }

        public static MansfieldRelativeStrength Series(BarHistory source, String rsSymbol, int dorseyRsPeriod)
        {
            string key = CacheKey("MansfieldRelativeStrength", rsSymbol, dorseyRsPeriod);
            if (source.Cache.ContainsKey(key))
                return (MansfieldRelativeStrength)source.Cache[key];
            MansfieldRelativeStrength mrs = new MansfieldRelativeStrength(source, rsSymbol, dorseyRsPeriod);
            source.Cache[key] = mrs;
            return mrs;
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, PriceComponent.Close);
            AddParameter("Relative Strength Index", ParameterType.String, "$NDX");
            AddParameter("Dorsey RS Period", ParameterType.Int32, 200);
        }

        public override string Name { get { return "Mansfield Relative Strength"; } }

        public override string Abbreviation { get { return "MRS"; } }

        public override string HelpDescription { get { return "The Mansfield RS Indicator (by Springroll) represents the relative strength correlation between two securities or generally between a security and its sectoral or market index."; } }

        public override string PaneTag { get { return "Mansfield Relative Strength"; } }

        public override PlotStyle DefaultPlotStyle { get { return PlotStyle.Line; } }

        public override WLColor DefaultColor { get { return WLColor.CornflowerBlue; } }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            String indexRefSymbol = Parameters[1].AsString;
            int dorseyRsPeriod = Parameters[2].AsInt;
            DateTimes = bars.DateTimes;

            if (bars.Count == 0 || bars.Count < dorseyRsPeriod)
                return;

            BarHistory indexDailyBars = WLHost.Instance.GetHistory(indexRefSymbol, bars.Scale, DateTime.MinValue, DateTime.MaxValue, bars.Count, null);
            TimeSeries dorseyRs = bars.Close / indexDailyBars?.Close * 100;
            TimeSeries mansfieldRs = (dorseyRs / FastSMA.Series(dorseyRs, dorseyRsPeriod) - 1) * 100;

            Values = mansfieldRs.Values;
        }
    }
}
