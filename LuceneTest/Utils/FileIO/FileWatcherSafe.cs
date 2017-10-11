using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TagExplorer.Utils
{
    class FileWatcherSafe
    {
        public delegate bool FileChangedHandler(FileSystemEventArgs e);
        private int TimeoutMillis = 2000;

        System.IO.FileSystemWatcher fsw = null;
        System.Threading.Timer m_timer = null;
        List<FileSystemEventArgs> files = new List<FileSystemEventArgs>();
        List<FileSystemEventArgs> files_retry = new List<FileSystemEventArgs>();
        FileChangedHandler fswHandler = null;
        int retryTimes = 0;
        public FileWatcherSafe(FileChangedHandler watchHandler)
        {

            fsw = new FileSystemWatcher(CfgPath.DocDir);
            fsw.IncludeSubdirectories = true;
            fsw.Path = CfgPath.DocDir;
            fsw.EnableRaisingEvents = true;
            fsw.Renamed += Fsw_Renamed;
            fsw.Created += Fsw_Created;
            fsw.Deleted += Fsw_Deleted;

            m_timer = new System.Threading.Timer(new TimerCallback(OnTimer),
                         null, TimeoutMillis, TimeoutMillis);
            fswHandler = watchHandler;

        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            OnFileChanged(sender, e);
            
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            OnFileChanged(sender, e);
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            FileSystemEventArgs ee = new FileSystemEventArgs(WatcherChangeTypes.Renamed,
                Path.GetDirectoryName(e.FullPath), Path.GetFileName(e.FullPath));
            
            OnFileChanged(sender, ee);
        }



        //public FileWatcherSafe(FileChangedHandler watchHandler, int timerInterval)
        //{
        //    m_timer = new System.Threading.Timer(new TimerCallback(OnTimer),
        //                null, Timeout.Infinite, Timeout.Infinite);
        //    TimeoutMillis = timerInterval;
        //    fswHandler = watchHandler;

        //}

        public void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Logger.I("OnFileChanged {0} - {1}",e.FullPath,e.ChangeType);
            if (!CfgPath.NeedSkipByUri(e.FullPath))
            {
                Push(e);
            }
            //lock (this)
            //{
            //    Push(e);
            //    //if (!files.Contains(e))
            //    //{
            //    //    files.Add(e);
            //    //}
            //    //m_timer.Change(TimeoutMillis, Timeout.Infinite);
            //}
        }
        private bool CanProcessNow(FileSystemEventArgs e)
        {
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Renamed:
                    return  
                        (
                            File.Exists(e.FullPath ) &&
                            CfgPath.CheckAccess(e.FullPath) 
                        ) || 
                        (
                            Directory.Exists(e.FullPath)
                        );
                case WatcherChangeTypes.Deleted:
                    return  !File.Exists(e.FullPath) && 
                            !Directory.Exists(e.FullPath);
                

            }
            
            return false;
        }
        private FileSystemEventArgs Pop()
        {
            FileSystemEventArgs ret = null;
            lock (this)
            {
                if(files.Count>0)
                {
                    ret = files[0];
                    files.RemoveAt(0);
                    Logger.I("Pop {0} - {1}", ret.FullPath, ret.ChangeType);
                }
            }
            return ret;
        }
        private void Push(FileSystemEventArgs e)
        {
            Logger.I("push {0} - {1}",e.FullPath,e.ChangeType);
            lock(this)
            {
                if (!files.Contains(e))
                {
                    Logger.I("push-Add {0} - {1}", e.FullPath, e.ChangeType);
                    files.Add(e);
                    retryTimes = 0;
                }
            }
        }
        private void PushRetry()
        {
            lock (this)
            {
                if (files.Count == 0 && files_retry.Count>0)
                {
                    foreach(FileSystemEventArgs e in files_retry)
                    {
                        if (retryTimes > 10)
                        {
                            if (//文件已经删除，却还一直存在
                                ( (e.ChangeType & WatcherChangeTypes.Deleted)!=0 && 
                                   File.Exists(e.FullPath) 
                                ) 
                                        
                                        || 

                                  //或者文件被创建出来了，但一直不存在
                                 ( (e.ChangeType & WatcherChangeTypes.Created|WatcherChangeTypes.Renamed | WatcherChangeTypes.Changed )!=0 && 
                                    !File.Exists(e.FullPath) 
                                 )
                                )
                            {
                                Logger.E("the File Delete,but Exist（Create Not Exist）,Try 10 Times,Drop It Now! {0} - {1}", e.FullPath,e.ChangeType);
                                continue;
                            }

                        }
                        files.Add(e);
                    }
                    files_retry.Clear();
                    retryTimes++;
                }
            }
        }
        private void OnTimer(object state)
        {
            DateTime start = DateTime.Now;
            FileSystemEventArgs f = null;
            while ((f = Pop())!=null)
            {
                
                bool ret = false;
                if (CanProcessNow(f))
                {
                    ret = fswHandler(f);
                }
                else
                {
                    ret = false;
                    //该文件存在，但是没有访问权限
                }
                if (!ret) //处理失败(或暂时还不能处理)，将其移到最后等待下次处理
                {
                    Logger.E("处理文件失败，将其放到队列尾，等待下次处理 - {0}：{1}", f.FullPath,f.ChangeType);
                    files_retry.Add(f);
                }

                if ((DateTime.Now - start).TotalMilliseconds >500)
                {
                    break;
                }
            }

            PushRetry();
            
        }

    }
}
