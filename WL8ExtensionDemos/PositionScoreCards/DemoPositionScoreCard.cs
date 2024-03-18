using WealthLab.Backtest;
using WealthLab.Core;
using WealthLab.Indicators;

namespace WL8ExtensionDemos
{
    //Position Scorecard that returns a Position metric (StdDev of bar-by-bar returns)
    public class DemoPositionScoreCard : PositionScoreCardBase
    {
        //Scorecard name
        public override string Name => "Demo Position ScoreCard";

        //metric names of position metrics provided
        public override List<string> PositionMetricNames => _names;

        //calculate a position metric
        public override double CalculatePositionMetric(Backtester bt, Position pos, string metricName)
        {
            switch(metricName)
            {
                case "StdDevReturns":
                    TimeSeries returns = pos.BarByBarReturns();
                    return StdDev.Value(returns.Count - 1, returns, returns.Count - 1, StdDevCalculation.Population);
                default:
                    return Double.NaN;
            }
        }

        //don't colorize
        public override bool ColorizeMetric(string metric)
        {
            return false;
        }

        //2 decimals
        public override int DecimalsMetric(string metric)
        {
            return 2;
        }

        //private members
        private static List<string> _names = new List<string>() { "StdDevReturns" };
    }
}