using System;
using System.Windows;
using System.Windows.Controls;

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
            this.Resources = new ResourceDictionary()
            { Source = new Uri("./Themes/Theme1.xaml", UriKind.Relative) };
        }

        private void Theme13_Click(object sender, RoutedEventArgs e)
        {
            this.Resources = new ResourceDictionary()
            { Source = new Uri("./Themes/Theme13.xaml", UriKind.Relative) };
        }
    }
}
