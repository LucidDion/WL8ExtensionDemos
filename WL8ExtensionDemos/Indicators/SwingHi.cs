using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Community
{
    public class SwingHi : IndicatorBase
    {
        //parameterless constructor
        public SwingHi() : base()
        {
        }

        public static SwingHi Series(TimeSeries ds, int leftBars, double leftThreshold, int rightBars, double rightThreshold, double equalPriceFloat, bool percentMode, bool returnLeftSwing, bool returnOuterSwings, bool steppedSeries)
        {
            string key = CacheKey("SwingHi", leftBars, leftThreshold, rightBars, rightThreshold, equalPriceFloat, percentMode, returnLeftSwing, returnOuterSwings, steppedSeries);
            if (ds.Cache.ContainsKey(key))
                return (SwingHi)ds.Cache[key];
            SwingHi swingHi = new SwingHi(ds, leftBars, leftThreshold, rightBars, rightThreshold, equalPriceFloat, percentMode, returnLeftSwing, returnOuterSwings, steppedSeries);
            ds.Cache[key] = swingHi;
            return swingHi;
        }

        public SwingHi(TimeSeries ds, int leftBars, int rightBars, double equalPriceFloat, bool returnLeftSwing, bool returnOuterSwings, bool steppedSeries)
            : base()
        {
            this.FirstValidIndex = leftBars + rightBars + 1;
            int lastBar = leftBars + rightBars + 1;
            int padL = 0;
            int stepNum = 0;
            int maxPadL = Math.Max(leftBars * 2, 100);
            double stepSH = double.NaN;
            double stepAccum = double.NaN;
            bool haveSH = false;

            Parameters[0].Value = ds;
            Parameters[1].Value = leftBars;
            Parameters[2].Value = rightBars;
            Parameters[3].Value = equalPriceFloat;
            Parameters[4].Value = returnLeftSwing;
            Parameters[5].Value = returnOuterSwings;
            Parameters[6].Value = steppedSeries;

            for (int bar = 0; bar < ds.Count; bar++)
                Add(ds[bar], ds.DateTimes[bar]);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < leftBars + rightBars + 1)
                {
                    Values[bar] = ds[bar];
                    lastBar = bar;
                }
                else
                {
                    haveSH = false;
                    for (int rbar = bar; rbar > bar - rightBars; rbar--)
                    {
                        if (ds[bar - rightBars] > ds[rbar])
                            haveSH = true;
                        else
                        {
                            haveSH = false;
                            break;
                        }
                    }
                    if (haveSH == true)
                    {
                        padL = 0;
                        int inBtwn = 0;
                        for (int Lbar = bar - rightBars - 1; Lbar >= bar - rightBars - leftBars - padL + inBtwn; Lbar--)
                        {
                            if (Math.Abs(ds[bar - rightBars] - ds[Lbar]) < equalPriceFloat)
                            {
                                if (Lbar - 1 > leftBars && padL + 1 < maxPadL)
                                {
                                    if (Lbar < bar - rightBars - 2 && ds[Lbar + 1] > ds[Lbar]) inBtwn++;
                                    padL = (bar - rightBars) - Lbar;
                                    continue;
                                }
                            }
                            if (ds[bar - rightBars] < ds[Lbar])
                            {
                                haveSH = false;
                                break;
                            }
                        }
                    }
                    if (haveSH == true)
                    {
                        int offset = 0;
                        if (returnOuterSwings == true)
                        {
                            if (ds[lastBar] < ds[bar - rightBars])
                                offset = padL;
                            else if (ds[lastBar] > ds[bar - rightBars])
                                offset = 0;
                        }
                        else
                            offset = returnLeftSwing == true ? padL : 0;
                        Values[bar - rightBars - offset] = ds[bar - rightBars - offset];
                        if (steppedSeries == false)
                        {
                            stepNum = bar - rightBars - offset - lastBar;
                            stepSH = (ds[lastBar] - ds[bar - rightBars - offset]) / stepNum;
                            stepAccum = ds[bar - rightBars - offset];
                            for (int i = bar - rightBars - 1 - offset; i > lastBar; i--)
                            {
                                stepAccum += stepSH;
                                Values[i] = stepAccum;
                            }
                        }
                        else
                        {
                            for (int i = bar - rightBars - 1 - offset; i > lastBar; i--)
                                Values[i] = ds[lastBar];
                        }
                        lastBar = bar - rightBars - offset;
                    }
                    if (bar == ds.Count - 1)
                        for (int i = bar; i > lastBar; i--) Values[i] = Values[lastBar];
                }
            }
        }

        public SwingHi(TimeSeries ds, int leftBars, double leftThreshold, int rightBars, double rightThreshold, double equalPriceFloat, bool percentMode, bool returnLeftSwing, bool returnOuterSwings, bool steppedSeries)
            : base()
        {
            this.FirstValidIndex = leftBars + rightBars + 1;
            int lastBar = leftBars + rightBars + 1;
            int padL = 0;
            int stepNum = 0;
            int targetBar = 0;
            int stepBk = 0;
            int maxPadL = Math.Max(leftBars * 2, 100);
            double stepSH = double.NaN;
            double stepAccum = double.NaN;
            double minChng = double.NaN;
            double stepChng = double.NaN;
            bool haveSH = false;
            bool requireLeftThreshold = leftThreshold <= 0 ? false : true;
            bool requireRightThreshold = rightThreshold <= 0 ? false : true;

            Parameters[0].Value = ds;
            Parameters[1].Value = leftBars;
            Parameters[2].Value = leftThreshold;
            Parameters[3].Value = rightBars;
            Parameters[4].Value = rightThreshold;
            Parameters[5].Value = equalPriceFloat;
            Parameters[6].Value = percentMode;
            Parameters[7].Value = returnLeftSwing;
            Parameters[8].Value = returnOuterSwings;
            Parameters[9].Value = steppedSeries;

            for (int bar = 0; bar < ds.Count; bar++)
                Add(ds[bar], ds.DateTimes[bar]);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < leftBars + rightBars + 1)
                {
                    Values[bar] = ds[bar];
                    lastBar = bar;
                }
                else
                {
                    targetBar = bar - rightBars + stepBk;
                    haveSH = false;
                    minChng = requireRightThreshold == false ? 0 : 999999;
                    if (requireRightThreshold == true)
                    {
                        for (int rbar = targetBar + 1; rbar <= bar; rbar++)
                        {
                            if (ds[rbar] >= ds[targetBar])
                            {
                                haveSH = false;
                                break;
                            }
                            minChng = Math.Min(minChng, ds[rbar]);
                            stepChng = percentMode == true ? (ds[targetBar] - minChng) / ds[targetBar] * 100 : ds[targetBar] - minChng;
                            if (stepChng >= rightThreshold - equalPriceFloat)
                            {
                                haveSH = true;
                                break;
                            }
                        }
                    }
                    if (haveSH == false)
                    {
                        for (int rbar = bar - rightBars + 1; rbar <= bar; rbar++)
                        {
                            if (ds[bar - rightBars] <= ds[rbar])
                            {
                                haveSH = false;
                                break;
                            }
                            if (rbar == bar)
                                haveSH = true;
                        }
                    }
                    if (haveSH == true)
                    {
                        padL = 0;
                        int inBtwn = 0;
                        minChng = requireLeftThreshold == false ? 0 : 999999;
                        if (leftBars != 0)
                        {
                            int Lbar = targetBar;
                            while (inBtwn < leftBars && Lbar >= targetBar - leftBars - padL)
                            {
                                Lbar--;
                                if (ds[targetBar] < ds[Lbar])
                                {
                                    haveSH = false;
                                    break;
                                }
                                if (Math.Abs(ds[targetBar] - ds[Lbar]) < equalPriceFloat)
                                {
                                    if (Lbar - 1 > leftBars && padL + 1 < maxPadL)
                                    {
                                        padL = (targetBar) - Lbar;
                                        continue;
                                    }
                                }
                                else
                                {
                                    inBtwn++;
                                    if (requireLeftThreshold == true)
                                        minChng = Math.Min(minChng, ds[Lbar]);
                                }
                            }
                            if (requireLeftThreshold == true)
                            {
                                stepChng = percentMode == true ? (ds[targetBar] - minChng) / minChng * 100 : (ds[targetBar] - minChng);
                                if (stepChng < leftThreshold - equalPriceFloat)
                                    haveSH = false;
                            }
                        }
                        else
                        {
                            int Lbar = targetBar - 1;
                            while (Lbar > 0)
                            {
                                if (ds[Lbar] > ds[targetBar] + equalPriceFloat)
                                {
                                    haveSH = false;
                                    break;
                                }
                                else if (Math.Abs(ds[targetBar] - ds[Lbar]) < equalPriceFloat)
                                {
                                    if (Lbar < targetBar - 2 && ds[Lbar + 1] > ds[Lbar]) inBtwn++;
                                    padL = (targetBar) - Lbar;
                                }
                                else
                                {
                                    minChng = Math.Min(minChng, ds[Lbar]);
                                    stepChng = percentMode == true ? (ds[targetBar] - minChng) / minChng * 100 : ds[targetBar] - minChng;
                                    if (stepChng >= leftThreshold - equalPriceFloat)
                                        break;
                                }
                                Lbar--;
                            }
                        }
                    }
                    if (haveSH == true)
                    {
                        int offset = 0;
                        if (returnOuterSwings == true)
                        {
                            if (ds[lastBar] < ds[targetBar])
                                offset = padL;
                            else if (ds[lastBar] > ds[targetBar])
                                offset = 0;
                        }
                        else
                            offset = returnLeftSwing == true ? padL : 0;
                        this[targetBar - offset] = ds[targetBar - offset];
                        if (steppedSeries == false)
                        {
                            stepNum = Math.Max(1, targetBar - offset - lastBar);
                            stepSH = (ds[targetBar - offset] - ds[lastBar]) / stepNum;
                            stepAccum = ds[targetBar - offset];
                            for (int i = targetBar - 1 - offset; i > lastBar; i--)
                            {
                                stepAccum -= stepSH;
                                Values[i] = stepAccum;
                            }
                        }
                        else
                        {
                            for (int i = targetBar - 1 - offset; i > lastBar; i--)
                                Values[i] = ds[lastBar];
                        }
                        lastBar = targetBar - offset;
                    }
                    if (bar == ds.Count - 1)
                    {
                        stepBk++;
                        bar--;
                    }
                    if (rightBars - stepBk < 1)
                        break;
                }
            }
            for (int i = ds.Count - 1; i > lastBar; i--) Values[i] = Values[lastBar];
        }

        public override string Name => "SwingHi";

        public override string Abbreviation => "SwingHi";

        public override string HelpDescription
        {
            get
            {
                return @"Control swings with variable left and right side number of bars and "
                + "price or percent as reversal amount." + Environment.NewLine + Environment.NewLine
                + "Each side of a swing can use either/or or both bars and price to define a swing. When both bar quota "
                + "and price are used the early(left) history will require a match from both parameters, whereas the "
                + "recent(right) history will be matched when either parameter is fulfilled." + Environment.NewLine + Environment.NewLine
                + "To drop price-percent filtering set their parameter(s) to zero. Alternatively, to drop the bars quota "
                + "set the left side parameter to zero, but on the right side set its parameter to a minimum of one." + Environment.NewLine + Environment.NewLine
                + "For cases of multi-bar swings the default bar returned is the right side. This can be overridden with "
                + "the SetLeftSwings parameter. Alternatively the parameter SetOutSideSwings will force the outside bar "
                + "of multi-bar swings to be returned (in cases of consecutively lower swings). "
                + "Set the parameter EqualPriceThreshold to the maximum tolerable value to return equal prices." + Environment.NewLine + Environment.NewLine
                + "The SetSteppedSeries parameter will force only the last swings values to be plotted box style. The "
                + "default plot increments the series values between swings." + Environment.NewLine + Environment.NewLine
                + "The parameter types are; bars-Bars, Left&RightBars-int, L&RPrice-double, EqualPriceThreshold-double, "
                + "all others-bool. The main method parameters are ordered;" + Environment.NewLine
                + " SwingLo.Series( dataSeries, LeftBars, LeftReversalAmount, RightBars, RightReversalAmount, EqualPriceThreshold, "
                + " PercentMode, SetLeftSwings, SetOuterSwings, SetSteppedSeries )" + Environment.NewLine + Environment.NewLine
                + "Alternative overload :-" + Environment.NewLine
                + " new SwingLo( dataSeries, LeftBars, RightBars, EqualPriceThreshold, SetLeftSwings, SetOuterSwings, SetSteppedSeries, \"mySwingLow\" )" + Environment.NewLine + Environment.NewLine
                + "While the series 'peaks', it can be avoided if used inconjunction with associated methods IsSwingHi() and IsSwingLo()." + Environment.NewLine
                + "The methods are bool with 3 additional parameters to their series counterparts:- " + Environment.NewLine
                + "- 'farBack' controls how many past bars(from 'bar') to search." + Environment.NewLine
                + "- 'occur' controls which of the 1st, 2nd, 3rd and so on, most recent swings to return" + Environment.NewLine
                + "- 'out swingLowBar' assigns an int variable with the bar number of 'occur'." + Environment.NewLine + Environment.NewLine
                + "IsSwingLo( bar, dataSeries, LeftBars, LeftReversalAmount, RightBars, RightReversalAmount, farback, occur, EqualPriceThreshold, PercentMode, SetLeftSwings, out swingLowBar ) " + Environment.NewLine
                + "IsSwingLo( bar, dataSeries, LeftBars, RightBars, farback, occur, EqualPriceThreshold, SetLeftSwings, out swingLowBar ) " + Environment.NewLine;
            }
        }

        public override string PaneTag => "Price";

        public override WLColor DefaultColor => WLColor.Blue;

        //peek ahead flag
        public override bool PeekAheadFlag => true;

        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 leftBars = Parameters[1].AsInt;
            Double leftReversalAmount = Parameters[2].AsDouble;
            Int32 rightBars = Parameters[3].AsInt;
            Double rightReversalAmount = Parameters[4].AsDouble;
            Double equalPriceThreshold = Parameters[5].AsDouble;
            bool percentMode = Parameters[6].AsBoolean;
            bool setLeftSwings = Parameters[7].AsBoolean;
            bool setOuterSwings = Parameters[8].AsBoolean;
            bool setSteppedSeries = Parameters[9].AsBoolean;

            DateTimes = source.DateTimes;
            int period = Math.Max(leftBars, rightBars);
            if (period > DateTimes.Count)
                period = DateTimes.Count;
            if (DateTimes.Count < period || period <= 0)
                return;

            Values = SwingHi.Series(source, leftBars, leftReversalAmount, rightBars, rightReversalAmount, equalPriceThreshold, percentMode, setLeftSwings, setOuterSwings, setSteppedSeries).Values;
        }

        //return companion indicators
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("SwingLo");
                return c;
            }
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.High);
            AddParameter("Left Bars", ParameterType.Int32, 13);
            AddParameter("Left Reversal Amount", ParameterType.Double, 3.5);
            AddParameter("Right Bars", ParameterType.Int32, 13);
            AddParameter("Right Reversal Amount", ParameterType.Double, 3.5);
            AddParameter("Equal Price Threshold", ParameterType.Double, 0.000001).StepValue = 0.000001;
            AddParameter("Percent Mode", ParameterType.Boolean, false);
            AddParameter("Set Left Swings", ParameterType.Boolean, false);
            AddParameter("Set Outer Swings", ParameterType.Boolean, false);
            AddParameter("Set Stepped Series", ParameterType.Boolean, false);
        }
    }
}