using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;

namespace RemoteUpdateProcess
{
    public class FTPHandle
    {
        public static FTPHandle Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new FTPHandle();
                }
                return m_Instance;
            }
        }

        static private FTPHandle m_Instance = null;

        //private string m_Uri;       //FTP Address

        private FTPHandle()
        {
            //m_Uri = System.Configuration.ConfigurationManager.AppSettings["FtpUri"];
        }

        /// <summary>
        /// 獲取在FTP SERVER的所有文件
        /// </summary>
        /// <returns>FTP SERVER的所有文件</returns>
        public List<string> GetFTPList()
        {
            //建立回傳的list結構：存儲FTP SERVER的所有文件
            List<string> strList = new List<string>();
            //對FTP SERVER提出請求：Create( FTP Address )
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(@"FTP://203.74.115.198/RemoteSystemUpload/"));

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.UseBinary = true;
            request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            //FTP帳號密碼
            request.Credentials = new NetworkCredential(@"ftp_G4G5_Field_Log", @"G4G5npqxyy2/");
            //接收發出請求後的回應：用StreamReader來獲取該資料流
            StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());

            string str = sr.ReadLine();

            while (str != null)
            {
                strList.Add(str);
                str = sr.ReadLine();
            }

            sr.Close();
            sr.Dispose();
            request = null;

            return strList;
        }

        /// <summary>
        /// 建立資料夾從路徑從ftp://xxx.xxx.xxx.xxx/開始輸入
        /// </summary>
        /// <param name="bf">資料夾路徑(包含名稱)</param>
        public void BuildFolder(string bf)
        {
            if (Directory.Exists(@"FTP://203.74.115.198/RemoteSystemUpload/" + bf))
            {
                Console.WriteLine("folder exist");
                return;
            }

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"FTP://203.74.115.198/RemoteSystemUpload/" + bf);    //對FTP SERVER提出請求：Create( FTP Address )
            request.Credentials = new NetworkCredential(@"ftp_G4G5_Field_Log", @"G4G5npqxyy2/");                          //FTP帳號密碼
            request.Method = WebRequestMethods.Ftp.MakeDirectory;                                   //方法為建立資料夾
            request.Timeout = 60 * 1000;                                                            //方法執行timeout時間
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();                        //獲取請求的回應
            response.Close();
        }

        /// <summary>
        /// 依照df下載資料至本機 SERVER，df路徑從ftp://xxx.xxx.xxx.xxx/開始輸入
        /// </summary>
        /// <param name="df">FTP SERVER下載路徑(包含名稱)</param>
        /// <param name="savefilename">儲存在本機端的路徑(包含名稱)</param>
        public void DownloadFileByFTP(string df, string savefilename)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"FTP://203.74.115.198/RemoteSystemUpload/" + df);    //對FTP SERVER提出請求：Create( FTP Address )
            request.Method = WebRequestMethods.Ftp.DownloadFile;                                    //方法為下載檔案
            request.Credentials = new NetworkCredential(@"ftp_G4G5_Field_Log", @"G4G5npqxyy2/");                          //FTP帳號密碼

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();                        
            Stream responseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseStream);                                     //接收發出請求後的回應：用StreamReader來獲取該資料流
            byte[] fileContent = Encoding.UTF8.GetBytes(sr.ReadToEnd());                            //讀取資料流
            Console.WriteLine("status{0}", response.StatusDescription);                             //顯示讀取狀態

            sr.Close();
            responseStream.Close();

            File.WriteAllBytes(savefilename, fileContent);                                          //寫入儲存路徑
        }

        /// <summary>
        /// 依照uf上傳filename的資料至FTP SERVER，uf路徑從ftp://xxx.xxx.xxx.xxx/開始輸入
        /// </summary>
        /// <param name="uf">上傳至FTP的路徑(包含檔名)</param>
        /// <param name="filename">本機端的路徑(包含檔名)</param>
        public void UploadFileByFTP(string uf, string filename)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"FTP://203.74.115.198/RemoteSystemUpload/" + uf);    //對FTP SERVER提出請求：Create( FTP Address )
            request.Credentials = new NetworkCredential(@"ftp_G4G5_Field_Log", @"G4G5npqxyy2/");                          //FTP帳號密碼
            request.Method = WebRequestMethods.Ftp.UploadFile;                                      //方法為上傳檔案

            StreamReader sr = new StreamReader(filename);                                           //*********************************//
            byte[] fileContent = Encoding.UTF8.GetBytes(sr.ReadToEnd());                            //**讀取filename的byte array data**//
            sr.Close();                                                                             //*********************************//

            request.ContentLength = fileContent.Length;                                             //設定上傳資料大小

            Stream requestStream = request.GetRequestStream();                                      //*********************************//
            requestStream.Write(fileContent, 0, fileContent.Length);                                //**     寫入FTP的資料夾裡面     **//
            requestStream.Close();                                                                  //*********************************//

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();                        //獲取請求的回應
            response.Close();
        }
    }
}
