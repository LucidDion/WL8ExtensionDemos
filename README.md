# WL8ExtensionDemos

An open-source class library for Wealth-Lab 8, containing Indicators, and an example custom extension with child window.  You can download the DLL for this library and save it into your WL8 installation folder from this link:

https://drive.google.com/file/d/1nBnYIxQekc0CxMBe-B7fmR9Z6izEUg4s/view?usp=sharing

To use indicators in your strategies, add a _using WealthLab.Community_ directive as shown below:

```
using WealthLab.Backtest;
using WealthLab.Core;
using WealthLab.Community;

namespace WealthScript2
{
	public class MyStrategy : UserStrategyBase
	{
		//create indicators and other objects here, this is executed prior to the main trading loop
		public override void Initialize(BarHistory bars)
		{
			PlotIndicator(new HiLoLimit(bars, 14, 2.00, 2.00), WLColor.FromArgb(255, 0, 0, 255), PlotStyle.Line);
		}

		//execute the strategy rules here, this is executed once for each bar in the backtest history
		public override void Execute(BarHistory bars, int idx)
		{
		}

		//declare private variables below

	}
}
```
