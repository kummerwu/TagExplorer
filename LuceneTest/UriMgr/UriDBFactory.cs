using LuceneTest.Core;
using System;
using System.Collections.Generic;

namespace LuceneTest.UriMgr
{
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
    public class UriDBFactory
    {
        public static IUriDB CreateUriDB()
        {
            return IDisposableFactory.New<IUriDB>(new LuceneUriDB());
        }
    }
}
