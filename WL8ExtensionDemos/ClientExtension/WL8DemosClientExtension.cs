using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WealthLab.WPF;

namespace WL8ExtensionDemos
{
    //A WL8ClientExtensionBase can install menus in the WL8 main menu, which can open Child Windows
    public class WL8DemosClientExtension : WL8ClientExtensionBase
    {
        //extension name
        public override string Name => "WL8DemosClientExtension";

        //return menu item for extension
        public override List<MenuItem> GetMenuItems()
        {
            List<MenuItem> mi = new List<MenuItem>();
            MenuItem mni = CreateExtensionMenuItem("Demo Extension", GetGlyph(), mniClick);
            mi.Add(mni);
            return mi;
        }

        //menu click handler
        private void mniClick(object sender, RoutedEventArgs e)
        {
            DemoChildWindow dcw = new DemoChildWindow();
            MyClientHost.ShowExtensionChildWindow(dcw, "Demo ChildWindow", GetGlyph());
        }

        //Use the WealthLab.WPF GlyphManager to get an ImageSource from our embedded resource
        private ImageSource GetGlyph()
        {
            return GlyphManager.GetImageSource("WL8ExtensionDemos.Glyphs.Demo.png", this);
        }
    }
}