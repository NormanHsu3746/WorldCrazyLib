using System.IO;
using WorldCrazyLib.Utility;

namespace WorldCrazyLib
{
    /// <summary>
    /// 紀錄事件與日誌工具
    /// </summary>
    public class Serilog
    {
        /// <summary>
        /// 日誌存放路徑
        /// </summary>
        public string? LogPath { get; set; }

        /// <summary>
        /// 日誌名稱，預設以 yyyy-mm-dd 進行命名，每天只會產生一個檔案
        /// </summary>
        public string? LogName { get; set; }

        /// <summary>
        /// 使否啟用安靜模式，預設為 false，啟用後本類別將不再寫檔
        /// </summary>
        public bool SilentMode { get; set; } = false;

        /// <summary>
        /// 讀取 log 的批次大小，預設 1mb
        /// </summary>
        public int PageSize { get; set; } = 2048;
        //public int PageSize { get; set; } = 50;

        /// <summary>
        /// 最後一次 Log 檔案寫入位置
        /// </summary>
        public string? LastWritePath { get; set; }

        /// <summary>
        /// 建構子，可指定 log 檔案名稱預設將以今天 yyyyMMdd 形式命名，也可自行指定其他日期，以利讀取
        /// </summary>
        public Serilog()
        {

        }

        /// <summary>
        /// 建構子，可指定 log 檔案名稱，預設將以今天 yyyyMMdd 形式命名，也可自行指定其他日期，以利讀取
        /// </summary>
        /// <param name="Name">日誌檔名，預設今天 yyyyMMdd</param>
        public Serilog(string logName)
        {

            this.LogName = logName;
        }

        /// <summary>
        /// 將訊息寫入實體日誌檔案,優先採用指定路徑，第二採用 Config 設定中的 Serilog，第三採用 Config 設定中的 Storage
        /// </summary>
        /// <param name="msg">訊息</param>
        public async Task Write(string logMessage)
        {
            if (SilentMode)
            {
                return;
            }

            var logLastPath = GetLastPath().Result;
            if(string.IsNullOrWhiteSpace(logLastPath)) return;


            try
            {
                string logTime = DateTime.Now.ToString("HH:mm:ss");
                string logText = string.Format("[{0}]{1}", logTime, logMessage) + Environment.NewLine;

                await WriteLog(logLastPath: logLastPath, logText: logText);
            }
            catch(Exception ex)
            {
                await WriteLog(logLastPath, ex.ToString());
            }
        }

        /// <summary>
        /// 取得最終的log檔案位置，如果無法取得則回傳 Null
        /// </summary>
        /// <returns>log檔案位置</returns>
        private async Task<string?> GetLastPath()
        {
            string? logfile = LogPath ?? Config.Get("Serilog") ?? Config.Get("Storage");
            if (!Directory.Exists(logfile)) return null;

            try
            {
                string logName = LogName ?? (DateTime.Now.ToString("yyyyMMdd") + ".txt");
                return string.Format(@"{0}\{1}", logfile, logName);
            }
            catch (Exception ex)
            {
                await WriteLog(logfile, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLastPath"></param>
        /// <param name="logText"></param>
        /// <returns></returns>
        private async Task WriteLog(string logLastPath, string logText)
        {
            try
            {
                await File.AppendAllTextAsync(logLastPath, logText);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(Config.Get("ForceLog")))
                {
                    logLastPath += "-" + Guid.NewGuid().ToString();
                    logText = ex.ToString() + "\n" + logText;
                    await WriteLog(logLastPath, logText);
                }
            }
        }
    }
}
