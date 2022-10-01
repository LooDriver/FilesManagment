using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Zapyck_igr
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SelectedStr { get; set; }

        Database db = new Database();
        DirectoryInfo extensionFile;

        public int switcherFilter = 1;
        public readonly List<string> extentionsFilesAll = new List<string>
        {
            ".docx", ".xlsx", ".pptx", ".exe", ".pdf", ".txt"
        };

        public MainWindow()
        {
            InitializeComponent();
            db.win = this;
            AllFilesBut.Header = "--> Все файлы";
            if (db.Connect("filesindb.db"))
            {
                db.DBCommandSelect();
            }
        }
        private class Database
        {
            public MainWindow win;
            private string _connectString;

            public string connectString { get { return _connectString; } private set { _connectString = value; } }
            public bool Connect(string fileName)
            {
                try
                {
                    using (SQLiteConnection con = new SQLiteConnection($"Data Source={fileName}"))
                    {
                        con.Open();
                        con.Dispose();
                        _connectString = fileName;
                        return true;
                    }
                }
                catch (SQLiteException ex)
                {
                    win.WritingLogs(ex);
                    return false;
                }
            }
            public async Task DeleteDB(string selectedStr)
            {
                using (SQLiteConnection connect = new SQLiteConnection($"Data Source={_connectString}"))
                {
                    await connect.OpenAsync();
                    using (SQLiteCommand command = new SQLiteCommand(connect))
                    {
                        switch (win.switcherFilter)
                        {
                            case 1:
                                {
                                    command.CommandText = $"DELETE FROM ОбщиеФайлы WHERE НазваниеФайла = '{selectedStr}' or ПутьДоФайла = '{selectedStr}'";
                                    await command.ExecuteNonQueryAsync();
                                    command.Dispose();
                                    break;
                                }
                            case 2:
                                {
                                    command.CommandText = $"DELETE FROM ТолькоДокументы WHERE НазваниеДокумента = '{selectedStr}' or ПутьДоДокумента = '{selectedStr}'";
                                    await command.ExecuteNonQueryAsync();
                                    command.Dispose();
                                    break;
                                }
                        }
                    }
                    connect.Dispose();
                    await DBCommandSelect();
                }
            }
            public async Task DBCommandSelect()
            {
                if (win.switcherFilter == 1) { await ReloadDB("SELECT НазваниеФайла, ПутьДоФайла from ОбщиеФайлы"); }
                else if (win.switcherFilter == 2) { await ReloadDB("SELECT НазваниеДокумента, ПутьДоДокумента FROM ТолькоДокументы"); }
            }
            public async Task UploadDB(string fileName, string usrLoadFile, string fileExtension)
            {
                using (SQLiteConnection connect = new SQLiteConnection($@"Data Source={_connectString}"))
                {
                    await connect.OpenAsync();
                    using (SQLiteCommand command = new SQLiteCommand(connect))
                    {
                        int indexExeStr = fileName.LastIndexOf('.');
                        string newFileName = fileName.Remove(indexExeStr, fileName.Length - indexExeStr);
                        switch (win.switcherFilter)
                        {
                            case 1:
                                {
                                    command.CommandText = $"INSERT INTO ОбщиеФайлы ('НазваниеФайла','ПутьДоФайла','Расширение') " +
                                        $"VALUES ('{newFileName}','{usrLoadFile}', '{fileExtension}')";
                                    await command.ExecuteNonQueryAsync();
                                    command.Dispose();
                                    break;
                                }
                            case 2:
                                {
                                    foreach (string docExntetions in win.extentionsFilesAll)
                                    {
                                        if (fileExtension == docExntetions)
                                        {
                                            command.CommandText = $"insert into ТолькоДокументы ('НазваниеДокумента', 'ПутьДоДокумента', 'Расширение') " +
                                                $"values ('{newFileName}', '{usrLoadFile}', '{fileExtension}')";
                                            await command.ExecuteNonQueryAsync();
                                            command.Dispose();
                                            break;
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                    connect.Dispose();
                    await DBCommandSelect();
                }
            }
            public async Task ReloadDB(string SQLCommand)
            {
                using (SQLiteConnection connect = new SQLiteConnection($@"Data Source={_connectString}"))
                {
                    await connect.OpenAsync();
                    using (SQLiteCommand command = new SQLiteCommand(SQLCommand, connect))
                    {
                        await command.ExecuteReaderAsync();
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command.CommandText, connect);
                        DataTable data = new DataTable();
                        adapter.Fill(data);
                        win.AllApp.ItemsSource = data.DefaultView;
                        command.Dispose();
                    }
                    connect.Dispose();
                }
            }
        }
        private async void SelectApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (switcherFilter)
                {
                    case 1:
                        {
                            OpenFileDialog pfg = new OpenFileDialog();
                            pfg.FilterIndex = 2;
                            pfg.Filter = "Все файлы (*.*) | *.*";
                            pfg.ShowDialog();
                            extensionFile = new DirectoryInfo(pfg.FileName);
                            await db.UploadDB(extensionFile.Name, extensionFile.FullName, extensionFile.Extension);
                            break;
                        }
                    case 2:
                        {
                            OpenFileDialog pfg = new OpenFileDialog();
                            pfg.FilterIndex = 1;
                            pfg.Filter = "Документы (*.docx, *.xlsx, *.pptx, *.txt) | *.docx;*.xlsx;*.pptx;*.txt";
                            pfg.ShowDialog();
                            extensionFile = new DirectoryInfo(pfg.FileName);
                            await db.UploadDB(extensionFile.Name, extensionFile.FullName, extensionFile.Extension);
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                await WritingLogs(ex);
            }
        }
        private async void AllApp_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (SelectedStr != "")
                {
                    using (SQLiteConnection con = new SQLiteConnection($"Data Source={db.connectString}"))
                    {
                        await con.OpenAsync();
                        using (SQLiteCommand com = new SQLiteCommand(con))
                        {
                            switch (switcherFilter)
                            {
                                case 1:
                                    {
                                        com.CommandText = "select ПутьДоФайла from ОбщиеФайлы";
                                        await com.ExecuteNonQueryAsync();
                                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(com.CommandText, con);
                                        DataTable data = new DataTable();
                                        dataAdapter.Fill(data);
                                        if (data.Rows.Count != 0)
                                        {
                                            string[] rowsFrmDB = new string[data.Rows.Count];
                                            for (int i = 0; i < rowsFrmDB.Length; i++)
                                            {
                                                data.Rows[i].ItemArray.CopyTo(rowsFrmDB, 0);
                                                if (rowsFrmDB[0].Contains(SelectedStr))
                                                {
                                                    if (!File.Exists(rowsFrmDB[0]))
                                                    {
                                                        int index = rowsFrmDB[0].LastIndexOf('\\');
                                                        string filenameFromPath = rowsFrmDB[0].Remove(0, index + 1);
                                                        foreach (string file in EnumerateFiles($"C:\\Users\\{Environment.MachineName}\\Desktop", filenameFromPath, SearchOption.AllDirectories))
                                                        {
                                                            com.CommandText = $"update ОбщиеФайлы set ПутьДоФайла = '{file}' where ПутьДоФайла = '{SelectedStr}' or НазваниеФайла = '{SelectedStr}'";
                                                            await com.ExecuteNonQueryAsync();
                                                            com.Dispose();
                                                            await db.DBCommandSelect();
                                                            Process.Start(file);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Process.Start(rowsFrmDB[0]);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        com.CommandText = "select ПутьДоДокумента from ТолькоДокументы";
                                        await com.ExecuteNonQueryAsync();
                                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(com.CommandText, con);
                                        DataTable data = new DataTable();
                                        dataAdapter.Fill(data);
                                        if (data.Rows.Count != 0)
                                        {
                                            string[] rowsFrmDB = new string[data.Rows.Count];
                                            for (int i = 0; i < rowsFrmDB.Length; i++)
                                            {
                                                data.Rows[i].ItemArray.CopyTo(rowsFrmDB, 0);
                                                if (rowsFrmDB[0].Contains(SelectedStr))
                                                {
                                                    if (!File.Exists(rowsFrmDB[0]))
                                                    {
                                                        int index = rowsFrmDB[0].LastIndexOf('\\');
                                                        string filenameFromPath = rowsFrmDB[0].Remove(0, index + 1);
                                                        foreach (string file in EnumerateFiles($"C:\\Users\\{Environment.MachineName}\\Desktop", filenameFromPath, SearchOption.AllDirectories))
                                                        {
                                                            com.CommandText = $"update ТолькоДокументы set ПутьДоДокумента = '{file}' where ПутьДоДокумента = '{SelectedStr}' or НазваниеДокумента = '{SelectedStr}'";
                                                            await com.ExecuteNonQueryAsync();
                                                            com.Dispose();
                                                            await db.DBCommandSelect();
                                                            Process.Start(file);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Process.Start(rowsFrmDB[0]);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {

                await WritingLogs(ex);
            }
        }
        public async Task WritingLogs(Exception exception)
        {
            if (Directory.Exists("logs"))
            {
                using (StreamWriter logs = new StreamWriter("logs\\error.txt", true))
                {
                    await logs.WriteLineAsync($"Исключение: {exception.Message}\n\nСтек вызова: {exception.StackTrace}\n\n");
                }
            }
            else
            {
                Directory.CreateDirectory("logs");
                using (StreamWriter logs = new StreamWriter("logs\\error.txt", true))
                {
                    await logs.WriteLineAsync($"Исключение: {exception.Message}\nСтек вызова: {exception.StackTrace}");
                }
            }
        }
        private async void StartingApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedStr != "")
                {
                    using (SQLiteConnection con = new SQLiteConnection($"Data Source={db.connectString}"))
                    {
                        await con.OpenAsync();
                        using (SQLiteCommand com = new SQLiteCommand(con))
                        {
                            switch (switcherFilter)
                            {
                                case 1:
                                    {
                                        com.CommandText = "select ПутьДоФайла from ОбщиеФайлы";
                                        await com.ExecuteNonQueryAsync();
                                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(com.CommandText, con);
                                        DataTable data = new DataTable();
                                        dataAdapter.Fill(data);
                                        if (data.Rows.Count != 0)
                                        {
                                            string[] rowsFrmDB = new string[data.Rows.Count];
                                            for (int i = 0; i < rowsFrmDB.Length; i++)
                                            {
                                                data.Rows[i].ItemArray.CopyTo(rowsFrmDB, 0);
                                                if (rowsFrmDB[0].Contains(SelectedStr))
                                                {
                                                    if (!File.Exists(rowsFrmDB[0]))
                                                    {
                                                        int index = rowsFrmDB[0].LastIndexOf('\\');
                                                        string filenameFromPath = rowsFrmDB[0].Remove(0, index + 1);
                                                        foreach (string file in EnumerateFiles($"C:\\Users\\{Environment.MachineName}\\Desktop", filenameFromPath, SearchOption.AllDirectories))
                                                        {
                                                            com.CommandText = $"update ОбщиеФайлы set ПутьДоФайла = '{file}' where ПутьДоФайла = '{SelectedStr}' or НазваниеФайла = '{SelectedStr}'";
                                                            await com.ExecuteNonQueryAsync();
                                                            com.Dispose();
                                                            await db.DBCommandSelect();
                                                            Process.Start(file);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Process.Start(rowsFrmDB[0]);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        com.CommandText = "select ПутьДоДокумента from ТолькоДокументы";
                                        await com.ExecuteNonQueryAsync();
                                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(com.CommandText, con);
                                        DataTable data = new DataTable();
                                        dataAdapter.Fill(data);
                                        if (data.Rows.Count != 0)
                                        {
                                            string[] rowsFrmDB = new string[data.Rows.Count];
                                            for (int i = 0; i < rowsFrmDB.Length; i++)
                                            {
                                                data.Rows[i].ItemArray.CopyTo(rowsFrmDB, 0);
                                                if (rowsFrmDB[0].Contains(SelectedStr))
                                                {
                                                    if (!File.Exists(rowsFrmDB[0]))
                                                    {
                                                        int index = rowsFrmDB[0].LastIndexOf('\\');
                                                        string filenameFromPath = rowsFrmDB[0].Remove(0, index + 1);
                                                        foreach (string file in EnumerateFiles($"C:\\Users\\{Environment.MachineName}\\Desktop", filenameFromPath, SearchOption.AllDirectories))
                                                        {
                                                            com.CommandText = $"update ТолькоДокументы set ПутьДоДокумента = '{file}' where ПутьДоДокумента = '{SelectedStr}' or НазваниеДокумента = '{SelectedStr}'";
                                                            await com.ExecuteNonQueryAsync();
                                                            com.Dispose();
                                                            await db.DBCommandSelect();
                                                            Process.Start(file);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Process.Start(rowsFrmDB[0]);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {

                await WritingLogs(ex);
            }
        }
        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutDevelMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Разработчик этого чуда: GrayCat\nКонтактная информация: \nТелеграмм - @loodriver\nVK: vk.com\\zloybotinok", "Контактная информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AboutProgMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Программа для удобного хранения игр, документов и т.д.\n" +
                "Версия: 1.0\n" +
                "Список изменений: Исправлен баг с зависанием программы при выборе и удаление текста из таблицы, " +
                "\nТакже был изменен внешний вид программы\n" +
                "Версия 2.0\n" +
                "Список изменений: была улучшена производительность программы и также были исправлены некоторые ошибки\nТакже был добавлен фильтр для файлов", "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Dizayner_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Дизайнер, а также создатель идеи: God_of_Death\nVK: vk.com/id267933505", "Контактная информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Settings settings = new Settings();
            settings.Show();
        }

        private async void AllApp_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int indexColumn = AllApp.CurrentCell.Column.DisplayIndex;
                DataRowView rowView = (DataRowView)AllApp.CurrentCell.Item;
                string value = rowView[indexColumn].ToString();
                SelectedStr = value;
            }
            catch (Exception ex)
            {
                await WritingLogs(ex);
            }
        }

        private async void AllFilesBut_Click(object sender, RoutedEventArgs e)
        {
            AllFilesBut.Header = "--> Все файлы";
            OnlyDocumentsFilesBut.Header = "Только документы";
            switcherFilter = 1;
            await db.DBCommandSelect();
        }

        private async void OnlyDocumentsFilesBut_Click(object sender, RoutedEventArgs e)
        {
            AllFilesBut.Header = "Все файлы";
            OnlyDocumentsFilesBut.Header = "--> Только документы";
            switcherFilter = 2;
            await db.DBCommandSelect();
        }
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path)
                                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }

        private async void DeleteApp_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStr != "")
            {
                await db.DeleteDB(SelectedStr);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            App.Current.Shutdown();
        }
    }
}