using LuceneTest.Core;
using System;
using System.IO;
using Trinet.Core.IO.Ntfs;

namespace LuceneTest.UriMgr
{
    public class NtfsFileID
    {
        public static Guid GetID(string uri)
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    return _GetID(uri);
                }catch
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            System.Diagnostics.Debug.Assert(false);//word产生的临时文件，是没有权限修改的。导致了该断言。
            return Guid.NewGuid();
            
        }
        public static Guid _GetID(string uri)
        {
            const string TID = "lguid";
            if(File.Exists(uri) || Directory.Exists(uri))
            {
                if(FileSystem.AlternateDataStreamExists(uri,TID))
                {
                    Logger.Log("{0} begin reader", uri);
                    AlternateDataStreamInfo adfs = FileSystem.GetAlternateDataStream(uri, TID);

                    FileStream fs = adfs.OpenRead();
                    StreamReader sr = new StreamReader(fs);
                    string guid = sr.ReadToEnd();
                    Logger.Log("{0} begin close", uri);
                    sr.Close();
                    fs.Close();
                    sr.Dispose();
                    fs.Dispose();
                    Logger.Log("{0} close reader", uri);
                    return Guid.Parse(guid);
                    
                }
                else
                {
                    Guid id = Guid.NewGuid();
                    AlternateDataStreamInfo adfs = FileSystem.GetAlternateDataStream(uri, TID);
                    FileStream fs = adfs.OpenWrite();
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(id.ToString());
                    sw.Close();
                    fs.Close();
                    sw.Dispose();
                    fs.Dispose();
                    return id;
                }
            }
            else
            {
                return Guid.NewGuid();
            }
        }
    }
}
