using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicBot
{
    public abstract class ProcessManager
    {

        public abstract Task Suspend(Process process);

        public abstract Task Resume(Process process);

    }
}