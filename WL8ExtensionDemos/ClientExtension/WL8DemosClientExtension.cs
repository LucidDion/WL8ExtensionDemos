using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab.WPF;

namespace WL8ExtensionDemos
{
    //A WL8ClientExtensionBase can install menus in the WL8 main menu, which can open Child Windows
    public class WL8DemosClientExtension : WL8ClientExtensionBase
    {
        //extension name
        public override string Name => "WL8DemosClientExtension";
    }
}