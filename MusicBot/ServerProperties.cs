using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Audio;

namespace MusicBot
{
    public class ServerProperties
    {
        
        public List<string> Queue { get; set; }

        public bool Repeat { get; set; }

        public bool Playing { get; set; }
        
        public IAudioClient ConnectedChannel { get; set; }

        public ServerProperties(bool Repeat, bool Playing, List<string> Queue)
        {
            this.Queue = Queue;
            this.Repeat = Repeat;
            this.Playing = Playing;
            ConnectedChannel = null;

        }      
    }
}