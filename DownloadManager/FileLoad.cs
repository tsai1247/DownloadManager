using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DownloadManager
{
    class FileLoad
    {
        static DateTime lastUpdateTime = DateTime.MinValue;
        /// <summary>
        /// Get all files under path
        /// </summary>
        /// <param name="path">folder path</param>
        /// <param name="list">save the filenames into the List</param>
        /// <param name="maxLength">the maximum number of files to get</param>
        public static void GetAllFiles(string path, out List<string> list, int maxLength = int.MaxValue)
        {

            if (lastUpdateTime == DateTime.MinValue)
            {
                GetLastUpdateTime();

            }

            list = new List<string>();

            // 執行檔路徑下的 MyDir 資料夾
            string folderName = path;

            // 取得資料夾內所有檔案
            foreach (string fname in System.IO.Directory.GetFiles(folderName))
            {
                FileInfo f = new FileInfo(fname);
                if (f.CreationTime > lastUpdateTime && !Ignore(fname))
                {
                    string curPath = fname.Replace(path, "").Replace("\\", "");
                    list.Add(curPath + "\t");// + f.LastWriteTime.ToString("t"));
                }
            }

        }

        private static bool Ignore(string fname)
        {
            bool ret = false;
            SqliteConnection sql = new SqliteConnection("Data Source=Ignore.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Select Count(*) from IgnoreFiles where name = @para1";
            cur.Parameters.AddWithValue("@para1", fname);
            var reader = cur.ExecuteReader();
            reader.Read();

            ret = (long)reader[0] != 0;

            reader.Close();
            cur.Cancel();
            sql.Close();

            return ret;
        }

        private static void GetLastUpdateTime()
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Select Time from LastStatus";
            var reader = cur.ExecuteReader();
            reader.Read();
            DateTime.TryParse(reader["Time"].ToString(), out lastUpdateTime);

            reader.Close();
            cur.Cancel();
            sql.Close();
        }

        internal static void SetLastUpdateTime(DateTime now)
        {
            SqliteConnection sql = new SqliteConnection("Data Source=Data.db");
            sql.Open();
            var cur = sql.CreateCommand();

            cur.CommandText = "Update LastStatus set Time = @para1";
            cur.Parameters.AddWithValue("@para1", now);
            cur.ExecuteNonQuery();

            cur.Cancel();
            sql.Close();
        }
    }
}