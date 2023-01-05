# WL8ExtensionDemos

An open-source class library for Wealth-Lab 8, containing Indicators, and an example custom extension with child window.  You can download the DLL for this library and save it into your WL8 installation folder from this link:

https://drive.google.com/file/d/1dKsJJLfymF2njHIYHxH-ttkOiwEdLsZ7/view?usp=sharing

Note: you may need to right-click on the downloaded file and select Unblock from the pop up menu before Windows will allow it to be loaded when WL8 starts.

The next time you launch WL8 you should see the Community indicators in this folder:

![image](https://user-images.githubusercontent.com/3159496/210844526-3beb14c8-824b-49ba-aa5e-685042ef4e23.png)

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

Using DrawTradeLines:

```
// Usage: run after Execute has finished
        public override void BacktestComplete()
        {
            this.DrawTradeLines(GetPositions(), false);
        } 
```

Using DrawLinRegChannel:

```
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
```
