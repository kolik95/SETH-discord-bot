using System.Runtime;
using System.Runtime.InteropServices;

namespace MusicBot
{
    public static class OS
    {

        private static bool isWindows =
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static string ffmpegProcess;

        public static string youtubeDlProcess;

        static OS()
        {

            if (isWindows)
            {

                ffmpegProcess = "ffmpeg.exe";
                youtubeDlProcess = "youtube-dl.exe";

            }

            else
            {

                ffmpegProcess = "ffmpeg";
                youtubeDlProcess = "youtube-dl";
               
            }               
        }    
    }
}