using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DownloadManager
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        internal static List<InputFile> fileList = new List<InputFile>();
        internal static string curDownloadPath = "";
        public void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if(sender!=null && e!=null)
                FileLoad.SetLastUpdateTime(DateTime.Now);

            LoadInputPath();

            while (fileList.Count>0)
            {
                Files.Children.Remove(fileList[0]);
                fileList.RemoveAt(0);
            }

            List<string> files;
            FileLoad.GetAllFiles(DownloadPath.Text, out files);

            string ret = "";
            foreach (var i in files)
            {
                ret += i + "\n\n";

                InputFile fileViewer = new InputFile();
                fileViewer.Data.IsChecked = true;
                fileViewer.Data.Content = i;

                fileList.Add(fileViewer);
                Files.Children.Add(fileViewer);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = DownloadPath.Text;
            if (dlg.ShowDialog() == true)
            {
                string originPath = DownloadPath.Text;
                SaveInputPath(dlg.ResultPath);
                try
                {
                    Refresh_Click(null, null);
                    curDownloadPath = DownloadPath.Text + "\\";
                }
                catch (Exception)
                {
                    SaveInputPath(originPath);
                    Refresh_Click(null, null);
                    MessageBox.Show("該路徑無法存取", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveInputPath(string path)
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Update LastStatus Set InputPath = @para1";
            cur.Parameters.AddWithValue("@para1", path);
            cur.ExecuteNonQuery();

            cur.Cancel();
            sql.Close();
        }

        private void AddOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            AddOutputFolderData();
        }

        private void AddOutputFolderData(int ID = -1, string path = "")
        {
            if(ID == -1) // insert new value into database
            {
                ID = Gen_ID();
                path = DownloadPath.Text;
                NewOutputPath(ID, path);
            }

            Output_Folder output_Folder = new Output_Folder();
            DockPanel.SetDock(output_Folder, Dock.Top);
            output_Folder.VerticalAlignment = VerticalAlignment.Top;
            output_Folder.FullPath.Content = path;

            output_Folder.OutputPath.Text = path.Split("\\")[^1];
            if (output_Folder.OutputPath.Text == "")
                output_Folder.OutputPath.Text = path;


                if (ID > 0)
                output_Folder.ID = ID;
            else
                output_Folder.ID = Gen_ID();

                OutputRegion.Children.Add(output_Folder);
        }

        private void NewOutputPath(int ID, string path)
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Insert into OutputFolder values(@para1, @para2)";
            cur.Parameters.AddWithValue("@para1", ID);
            cur.Parameters.AddWithValue("@para2", path);
            cur.ExecuteNonQuery();
            cur.Cancel();
            sql.Close();

        }

        private int Gen_ID()
        {
            Random rand = new Random();
            int ret = -1;
            while (true)
            {
                ret = rand.Next();

                SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
                sql.Open();
                var cur = sql.CreateCommand();

                cur.CommandText = "Select Count(*) from OutputFolder where ID = @para1";
                cur.Parameters.AddWithValue("@para1", ret);
                var reader = cur.ExecuteReader();
                reader.Read();

                bool isNewID = (long)reader[0] == 0;
                reader.Close();
                cur.Cancel();
                sql.Close();

                if (isNewID)
                {
                    break;
                }
            }
            return ret;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInputPath();

            LoadOutputPath();

            Refresh_Click(null, null);

        }

        private void LoadOutputPath()
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Select * from OutputFolder";
            var reader = cur.ExecuteReader();

            while (reader.Read())
            {
                int ID = (int)(long)reader["ID"];
                string path = (string)reader["Path"];
                AddOutputFolderData(ID, path);
            }

            reader.Close();
            cur.Cancel();
            sql.Close();
        }

        private void LoadInputPath()
        {   
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            SqliteCommand cur = sql.CreateCommand();
            while (cur.CommandText == "")
            {
                cur.CommandText = "Select InputPath, Time from LastStatus";
            }
            SqliteDataReader reader = cur.ExecuteReader();

            reader.Read();

            if(reader["InputPath"] != DBNull.Value)
                DownloadPath.Text = (string)reader["InputPath"];
            else
                DownloadPath.Text = "C:\\";
            
            if (reader["Time"] != DBNull.Value)
                LastUpdateTimeLabel.Text = DateTime.Parse((string)reader["Time"]).ToString("yyyy-MM-dd HH:mm:ss");
            else
                LastUpdateTimeLabel.Text = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
            
            reader.Close();
            cur.Cancel();
            sql.Close();

            curDownloadPath = DownloadPath.Text + "\\";

        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i=0; i<fileList.Count; i++)
            {
                fileList[i].Data.IsChecked = SelectAll.IsChecked;
            }
        }
    }
}
