using System.Diagnostics;
using System.Threading.Tasks;

namespace MusicBot.ProcessManagers
{
    public class LinProcessManager : IProcessManager
    {
        public async Task Suspend(Process process)
        {

            var id = process.Id;

            new Process
            {

                StartInfo = new ProcessStartInfo
                {

                    FileName = "kill",
                    Arguments = $"-STOP {id}",
                    UseShellExecute = false

                }
            }.Start();
        }

        public async Task Resume(Process process)
        {
            
            var id = process.Id;

            new Process
            {

                StartInfo = new ProcessStartInfo
                {

                    FileName = "kill",
                    Arguments = $"-CONT {id}",
                    UseShellExecute = false

                }
            }.Start();           
        }
    }
}