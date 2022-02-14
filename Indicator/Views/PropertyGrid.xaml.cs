using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Mvvm.CodeGenerators;

namespace Indicator.Views
{
    /// <summary>
    /// Interaction logic for PropertyGrid.xaml
    /// </summary>
    public partial class PropertyGrid : UserControl
    {
        public PropertyGrid()
        {
            InitializeComponent();
        }
        private void PropertyGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            //propertyGrid1.Expand("ValueArgs");
            //propertyGrid1.Expand("ChildSignals");
        }
        private void PropertyGridControl_CustomExpand(object sender, DevExpress.Xpf.PropertyGrid.CustomExpandEventArgs args)
        {
           // if (args.Row.FullPath.Contains("ValueArgs") || args.Row.FullPath.Contains("ChildSignals"))
                args.IsExpanded = true;
        }
    }
}
