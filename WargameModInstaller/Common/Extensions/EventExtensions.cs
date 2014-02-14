using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Extensions
{
    public static class EventExtensions
    {
        public static void RiseSafe<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            var threadSafeHandler = handler;
            if (null != threadSafeHandler) 
            {
                threadSafeHandler(sender, e);
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="multicastDelegate"></param>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //public static object RiseSafe(this MulticastDelegate multicastDelegate, object sender, EventArgs e)
        //{
        //    object retVal = null;

        //    MulticastDelegate threadSafeMulticastDelegate = multicastDelegate;
        //    if (threadSafeMulticastDelegate != null)
        //    {
        //        foreach (Delegate d in multicastDelegate.GetInvocationList())
        //        {
        //            retVal = d.DynamicInvoke(new[] { sender, e });
        //        }
        //    }

        //    return retVal;
        //}

        /// <summary>Raises the event (on the UI thread if available).</summary>
        /// <param name="multicastDelegate">The event to raise.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        /// <returns>The return value of the event invocation or null if none.</returns>
        public static object RaiseOnUIThread(this MulticastDelegate multicastDelegate, object sender, EventArgs e)
        {
            object retVal = null;

            MulticastDelegate threadSafeMulticastDelegate = multicastDelegate;
            if (threadSafeMulticastDelegate != null)
            {
                foreach (Delegate d in threadSafeMulticastDelegate.GetInvocationList())
                {
                    var synchronizeInvoke = d.Target as ISynchronizeInvoke;
                    if ((synchronizeInvoke != null) && synchronizeInvoke.InvokeRequired)
                    {
                        retVal = synchronizeInvoke.EndInvoke(synchronizeInvoke.BeginInvoke(d, new[] { sender, e }));
                    }
                    else
                    {
                        retVal = d.DynamicInvoke(new[] { sender, e });
                    }
                }
            }

            return retVal;
        }

    }

}
