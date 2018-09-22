using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicBot.ProcessManagers
{
    interface IProcessManager
    {
        Task Suspend(Process process);
        Task Resume(Process process);
    }
}
