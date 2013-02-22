using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace WeTongji.Business
{
    public class WTDispatcher
    {
        #region [Fields]
        private DispatcherTimer dt = null;

        private Object obj = null;
        private Queue<Action> actionQueue = null;

        private static WTDispatcher instance = null;
        #endregion

        #region [Instance]
        public static WTDispatcher Instance
        {
            get
            {
                if (instance == null)
                    instance = new WTDispatcher();
                return instance;
            }
        }
        #endregion

        #region [Constructor]
        private WTDispatcher()
        {
            dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(30) };
            dt.Tick += DoCore;
            obj = new Object();
            actionQueue = new Queue<Action>();
        }
        #endregion

        #region [Functions]
        public void Do(Action act)
        {
            lock (obj)
            {
                actionQueue.Enqueue(act);
            }
            dt.Start();
        }

        private void DoCore(object sender, EventArgs e)
        {
            while (true)
            {
                lock (obj)
                {
                    if (actionQueue.Count == 0)
                    {
                        dt.Stop();
                        return;
                    }
                }

                var a = actionQueue.Dequeue();
                a();
            }
        }
        #endregion
    }
}
