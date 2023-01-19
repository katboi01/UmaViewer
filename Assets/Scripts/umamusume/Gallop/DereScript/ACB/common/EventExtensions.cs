using System;

namespace DereTore.Common
{
    public static class EventExtensions
    {

        public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
        {
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {

            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public static IAsyncResult RaiseAsync<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
        {
            if (handler != null)
            {
                return handler.BeginInvoke(sender, e, handler.EndInvoke, null);
            }
            return null;
        }

        public static IAsyncResult RaiseAsync(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
            {
                return handler.BeginInvoke(sender, e, handler.EndInvoke, null);
            }
            return null;
        }

    }
}
