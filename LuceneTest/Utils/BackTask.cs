using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TagExplorer.UriMgr;

namespace TagExplorer.Utils
{
    interface IRunable
    {
        void Run();
    }
    class DownloadTaskInf:IRunable
    {
        string Url, Tag, Title;
        public DownloadTaskInf(string url, string tag, string title)
        {
            Url = url;
            Tag = tag;
            Title = title;
        }

        public void Run()
        {
            WebHelper.Download(Url, Tag, Title);
        }
    }
    class UpdateTitleTaskInf:IRunable
    {
        string Uri = null;
        IUriDB UriDB = null;
        string Tag = null;
        public UpdateTitleTaskInf(string uri,IUriDB db,string tag)
        {
            Uri = uri;
            UriDB = db;
            Tag = tag;
        }
        public void Run()
        {
            string title = WebHelper.GetWebTitle(Uri);
            if(title!=null)
            {
                UriDB.UpdateTitle(Uri, title);
                if (StaticCfg.Ins.Opt.AutoDownloadUrl)
                {
                    //WebHelper.Download(uri, currentTag.Title, title);
                    BackTask.Ins.Add(new DownloadTaskInf(Uri, Tag, title));
                }
            }
        }
    }
    class DelTagTaskInf:IRunable
    {
        string Tag;
        public DelTagTaskInf(string tag)
        {
            Tag = tag;
        }
        public void Run()
        {
            string src = CfgPath.GetDirByTag(Tag);
            string dst = Path.Combine(CfgPath.RecycleDir,Tag);
            if(Directory.Exists(src))
            {
                while(Directory.Exists(dst))
                {
                    dst = dst + "$" + Guid.NewGuid().ToString();
                }
                try
                {
                    Directory.Move(src, dst);
                }
                catch(Exception e)
                {
                    Logger.E(e);
                }
            }
        }
    }
    class BackTask:IDisposable
    {
        private List<IRunable> tasks = new List<IRunable>();
        private static BackTask ins = null;
        Task backTask = null;
        public BackTask()
        {
            backTask = new Task(RunTask);
            backTask.Start();
        }
        public static BackTask Ins
        {
            get
            {
                if(ins==null)
                {
                    ins = new BackTask();
                }
                return ins;
            }
        }
        public void Add(IRunable t)
        {
            lock(this)
            {
                ins.tasks.Add(t);
            }
        }
        
        private void RunTask()
        {
            while(true)
            {
                IRunable t = null;
                lock(this)
                {
                    if (tasks.Count > 0)
                    {
                        t = tasks[0];
                        tasks.RemoveAt(0);
                    }

                }
                if(t!=null)
                {
                    t.Run();
                }
                else
                {
                    System.Threading.Thread.Sleep(2000);
                }
            }
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
