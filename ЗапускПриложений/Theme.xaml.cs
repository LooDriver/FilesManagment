using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zapyck_igr
{
    /// <summary>
    /// Логика взаимодействия для Themes.xaml
    /// </summary>
    public partial class Theme : Page
    {
        public Theme()
        {
            InitializeComponent();
        }

        private void Theme1_Click(object sender, RoutedEventArgs e)
        {
            
            //ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            //Application.Current.Resources.Clear();
            //Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        private void Theme13_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.Clear();
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.GradientStops.Add(new GradientStop(Colors.LightGray, 0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 1));
            //this.Resources.Add("Theme13Style", gradientBrush);
            DynamicResourceExtension dynamic = new DynamicResourceExtension();

            Resources.Add("Theme13Style");
            
            Settings set = new Settings();
            set.SetResourceReference(Settings.ForegroundProperty, "Theme13Style");
        }
    }
}
