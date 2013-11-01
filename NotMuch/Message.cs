using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
    public class Message : Disposable
    {
        internal Message(IntPtr ptr)
            : base(ptr)
        {
        }

        protected override void DestroyHandle()
        {
            Native.notmuch_message_destroy(m_ptr);
        }

        public string FileName
        {
            get
            {
                IntPtr sp = Native.notmuch_message_get_filename(m_ptr);

                return Marshal.PtrToStringAnsi(sp);
            }
        }
    }
}

