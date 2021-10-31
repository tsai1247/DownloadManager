using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Data.Sqlite;

namespace DownloadManager
{
    /// <summary>
    /// Output_Folder.xaml 的互動邏輯
    /// </summary>
    public partial class Output_Folder : UserControl
    {
        private string LastPath = "";
        internal int ID = -1;
        public Output_Folder()
        {
            InitializeComponent();
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            if(LastPath == "")
            {
                LastPath = GetCurrentInputPath();
            }

            dlg.InputPath = LastPath;

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.ResultPath;

                FullPath.Content = path;
                OutputPath.Text = path.Split("\\")[^1];
                if (OutputPath.Text == "")
                    OutputPath.Text = path;

                SaveOutputPath(path);
            }
        }

        private void SaveOutputPath(string path)
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Update OutputFolder Set Path = @para1 where ID = @para2";
            cur.Parameters.AddWithValue("@para1", path);
            cur.Parameters.AddWithValue("@para2", ID);
            cur.ExecuteNonQuery();

            cur.Cancel();
            sql.Close();
        }

        private string GetCurrentInputPath()
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();
            cur.CommandText = "Select InputPath from LastStatus";
            var reader = cur.ExecuteReader();

            string ret = "";
            if(reader.Read())
            {
                ret = (string)reader[0];
            }
            else
            {
                ret = "C:\\";
            }
            reader.Close();
            cur.Cancel();
            sql.Close();
            return ret;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LastPath = (string)FullPath.Content;

        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            List<Process> moving = new List<Process>();
            string inputfolder = MainWindow.curDownloadPath;
            foreach (var inputfile in MainWindow.fileList)
            {
                if ((bool)inputfile.Data.IsChecked)
                {
                    string filePath = inputfolder + inputfile.Data.Content.ToString().Split("\t")[0];
                    string commandText = string.Format("/C move {0}{1}{0} {0}{2}{0}", "\"", filePath, FullPath.Content);
                    moving.Add( Process.Start("cmd", commandText));
                }
            }
            for (int i = 0; i < moving.Count; i++)
                moving[i].WaitForExit();


            FileLoad.SetLastUpdateTime(DateTime.Now);
            ((MainWindow)Application.Current.MainWindow).Refresh_Click(null, null);

        }
    }
}
