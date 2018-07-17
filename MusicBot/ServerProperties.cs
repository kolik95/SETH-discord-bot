using Discord.Audio;
using System.Collections.Generic;
using System.Diagnostics;

namespace MusicBot
{
	public class ServerProperties
    {
        
        public List<string> Queue { get; }

		public List<string> QueueNames { get; }

		public List<string> QueueThumbnails { get; }

		public List<string> QueueURLs { get; }

		public bool Repeat { get; set; }

        public bool Playing { get; set; }
        
        public IAudioClient ConnectedChannel { get; set; }

        public Process Player { get; set; }

        public ServerProperties()
        {
            Queue = new List<string>();
			QueueNames = new List<string>();
			QueueThumbnails = new List<string>();
			QueueURLs = new List<string>();
			Repeat = false;
            Playing = false;
            ConnectedChannel = null;
            Player = null;

        }      
    }
}