using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TagExplorer.UriMgr;
using TagExplorer.Utils.Net;

namespace TagExplorer.Utils
{
    interface IRunable
    {
        void Run();
        //public virtual void Run()
        //{
        //    if(ui!=null)
        //    {
        //        ui.Dispatcher.Invoke(ui.GitFinished);  
        //    }
        //}
        //MainWindow ui;

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
        public override string ToString()
        {
            return "DownloadTask:" + Url + " " + Tag + " " + Title;
        }
        public void Run()
        {
            WebHelper.Download(Url, Tag, Title);
        }
    }
    class GitPullTaskInf:IRunable
    {
        MainWindow ui = null;
        public GitPullTaskInf(MainWindow ui)
        {
            this.ui = ui;
        }
        public  void Run()
        {
            GitHelper h = new GitHelper();
            h.Pull();
            if (ui != null)
            {
                ui.Dispatcher.Invoke(ui.GitFinished);
            }
        }
        public override string ToString()
        {
            return "GitPullTask";
        }
    }
    class GitPushTaskInf : IRunable
    {
        public void Run()
        {
            GitHelper h = new GitHelper();
            h.Push();
        }
        public override string ToString()
        {
            return "GitPushTask";
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
        public override string ToString()
        {
            return "UpdateTitle:" + Uri + " " + Tag;
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
        public override string ToString()
        {
            return "DelTag" + Tag;
        }
        public void Run()
        {
            string src = CfgPath.GetDirByTag(Tag);
            
            if(Directory.Exists(src))
            {
                string dst = CfgPath.GetRecycleName(Tag);
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
                TipsCenter.Ins.BackTaskInf = ToString();
            }
        }
        public override string ToString()
        {
            string tip = "";
            foreach(IRunable i in tasks)
            {
                if (i == currentTask) tip += ">>";
                tip += (i.ToString()+"\r\n");
            }
            return tip;
        }
        private IRunable currentTask = null;
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
                    currentTask = t;
                    t.Run();
                    currentTask = null;
                    TipsCenter.Ins.BackTaskInf = ToString();
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
