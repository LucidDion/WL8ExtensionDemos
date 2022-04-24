using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WealthLab.Backtest;
using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.Community
{
    public static class CosmeticExtensions
    {
        /* Usage: run after Execute has finished
        public override void BacktestComplete()
        {
            this.DrawTradeLines(GetPositions(), false);
        } 
        */

        public static void DrawTradeLines(this UserStrategyBase obj, List<Position> lst, bool showSignal = false)
        {
            for (int iPos = 0; iPos < lst.Count; iPos++)
            {
                Position position = lst[iPos];
                int positionExitBar;
                double positionExitPrice;

                if (position.IsOpen)
                {
                    positionExitBar = position.Bars.Count - 1;
                    positionExitPrice = position.Bars.Close[positionExitBar];
                }
                else
                {
                    positionExitBar = position.ExitBar;
                    positionExitPrice = position.ExitPrice;
                }

                Color col;
                if (position.PositionType == PositionType.Long)
                    col = (positionExitPrice - position.EntryPrice) > 0 ? Color.Green : Color.Red;
                else
                    col = (positionExitPrice - position.EntryPrice) > 0 ? Color.Red : Color.Green;

                obj.DrawLine(position.EntryBar, position.EntryPrice, positionExitBar, positionExitPrice, col);

                if (showSignal)
                {
                    obj.DrawBarAnnotation(position.EntrySignalName, position.EntryBar, position.PositionType == PositionType.Long, Color.Black, 12);
                    obj.DrawBarAnnotation(position.ExitSignalName, position.ExitBar, position.PositionType == PositionType.Short, Color.Black, 12);
                }
            }
        }

        /* Usage: run in Execute:
        public override void Execute(BarHistory bars, int idx)
        {
            Color col = new Color();
            if (idx >= bars.Count-100)
            {
				if ( bars.Close[idx] > bars.Close[idx - 50])
					col = Color.Green;
				else col = Color.Red;

				DrawLinRegChannel(idx, bars.AveragePriceHL, 45, 2, Color.FromArgb(30, col), PlotStyles.Line, 2);
            }
        } 
        */

        public static void DrawLinRegChannel(this UserStrategyBase obj, int bar, TimeSeries series, int period, double width, Color color, PlotStyles style, int line)
        {
            double Slope = (period - 1) * LRSlope.Series(series, period)[bar];
            double Intercept = LR.Series(series, period)[bar];
            width *= StdError.Series(series, period)[bar];
            obj.DrawLine(bar - (period - 1), Intercept - Slope - width, bar, Intercept - width, color, 1);
            obj.DrawLine(bar - (period - 1), Intercept - Slope + width, bar, Intercept + width, color, 1);
        }
    }
}
