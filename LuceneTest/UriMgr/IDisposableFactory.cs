using System;
using System.Collections.Generic;
using TagExplorer.Utils;

namespace TagExplorer.UriMgr
{

    //所有支持IDispose接口的对象统一在这边管理，在异常退出时调用保证资源的时候，
    //以及该存盘的对象已经完成存盘
    //否则，可能会出现异常退出时没有及时存盘，导致数据丢失
    public class IDisposableFactory
    {
        static HashSet<IDisposable>  all = new HashSet<IDisposable>();
        public static T New<T>(T newObj) where T:IDisposable
        {
            all.Add(newObj);
            return newObj;
        }
        public static void Dispose(IDisposable db)
        {
            if (all.Contains(db))
            {
                all.Remove(db);
            }
            db.Dispose();
        }
        public static void DisposeAll()
        {
            if (all != null)
            {
                foreach (IDisposable db in all)
                {
                    try
                    {
                        db.Dispose();
                    }
                    catch(Exception e)
                    {
                        Logger.E(e);
                    }
                }
                all.Clear();
            }
        }

    }
}
