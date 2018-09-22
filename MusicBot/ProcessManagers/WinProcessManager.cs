using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MusicBot.ProcessManagers
{
    public class WinProcessManager : IProcessManager
    {
        #region DLL Import

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        #endregion

        public async Task Suspend(Process process)
        {

            foreach (ProcessThread thread in process.Threads)
            {

                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);

                if (pOpenThread == IntPtr.Zero)
                {

                    break;

                }

                SuspendThread(pOpenThread);
            }
        }

        public async Task Resume(Process process)
        {

            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);

                if (pOpenThread == IntPtr.Zero)
                {

                    break;

                }

                ResumeThread(pOpenThread);
            }
        }
    }
}