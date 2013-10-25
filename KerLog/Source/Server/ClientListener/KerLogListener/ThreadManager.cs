using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClientListener.KerLogListener
{
    class ThreadManager
    {
        private List<Thread> _threads;

        private int _maxSize;

        private Timer _timer;

        private object _threadsLock;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ThreadManager(int maxSize)
        {
            log.Info(string.Format("Created a new thread pool with a size of {0}", maxSize));
            _maxSize = maxSize;
            _threads = new List<Thread>();
            _timer = new Timer(TimerEvent, null, 0, 4000);
            _threadsLock = new object();
        }

        private void TimerEvent (object state)
        {
            lock (_threadsLock)
            {
                for (int i = 0; i < _threads.Count; i++)
                {
                    if (_threads[i].ThreadState.Equals(ThreadState.Stopped))
                    {
                        _threads.Remove(_threads[i]);
                        log.Info(string.Format("Removed a stopped thread from the thread pool, current thread pool size is {0}", _threads.Count));
                    }
                }
            }
        }

        public Thread CreateNewThread(ParameterizedThreadStart threadStart)
        {
            lock (_threadsLock)
            {
                if (_threads.Count <= _maxSize)
                {
                    log.Info("Created a new thread for a new task");
                    Thread thread = new Thread(threadStart);
                    _threads.Add(thread);
                    return thread;
                }
                else
                {
                    log.Info("No thread was created because the current thread pool is too big");
                    return null;
                }
            }
        }



    }
}
