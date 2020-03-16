using System;
using System.Threading;

namespace FileSystem
{
    public static class EventArgExtensions
    {
        public static void Raise<TEventArgs>(this TEventArgs e, Object sender, ref EventHandler<TEventArgs> eventDelegate)
        {
            EventHandler<TEventArgs> temp = Volatile.Read(ref eventDelegate);
            if (temp != null) temp(sender, e);
        }
    }
}
