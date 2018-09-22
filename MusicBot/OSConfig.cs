using System.Runtime;
using System.Runtime.InteropServices;
using MusicBot.ProcessManagers;

namespace MusicBot
{
    public static class OSConfig
    {

        private static bool isWindows =
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static string ffmpegProcess;

        public static string youtubeDlProcess;

        internal static IProcessManager ProcessManager { get; set; }

        static OSConfig()
        {

            if (isWindows)
            {

                ffmpegProcess = "ffmpeg.exe";
                youtubeDlProcess = "youtube-dl.exe";
                ProcessManager = new WinProcessManager();

            }

            else
            {

                ffmpegProcess = "ffmpeg";
                youtubeDlProcess = "youtube-dl";
                ProcessManager = new LinProcessManager();
               
            }               
        }    
    }
}