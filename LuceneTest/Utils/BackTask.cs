using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer.Utils
{
    class BackTaskInf
    {
        public string Type = null;
        public string[] Args = null;
        virtual public void Run()
        {
            throw new NotImplementedException();
        }
    }
    class DownloadTaskInf:BackTaskInf
    {
        public DownloadTaskInf(string url, string tag, string title)
        {
            Type = nameof(DownloadTaskInf);
            Args = new string[3];
            Args[0] = url;
            Args[1] = tag;
            Args[2] = title;
        }

        public override void Run()
        {
            WebHelper.Download(Args[0], Args[1], Args[2]);
        }
    }
    class BackTask:IDisposable
    {
        private List<BackTaskInf> tasks = new List<BackTaskInf>();
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
        public void Add(BackTaskInf t)
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
                BackTaskInf t = null;
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
