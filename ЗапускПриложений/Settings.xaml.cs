using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Zapyck_igr
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            MainWindow win = new MainWindow();
            win.Show();
        }

        private void Themes_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new Theme();
        }
    }
}
