using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Audio;

namespace MusicBot
{
    public class ServerProperties
    {
        
        public List<string> Queue { get;}

        public bool Repeat { get; set; }

        public bool Playing { get; set; }
        
        public IAudioClient ConnectedChannel { get; set; }

        public Process Player { get; set; }

        public ServerProperties(bool repeat, bool playing, List<string> queue)
        {
            this.Queue = queue;
            this.Repeat = repeat;
            this.Playing = playing;
            ConnectedChannel = null;
            Player = null;

        }      
    }
}