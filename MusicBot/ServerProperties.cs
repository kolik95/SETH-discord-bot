using Discord.Audio;
using System.Collections.Generic;
using System.Diagnostics;

namespace MusicBot
{
	public class ServerProperties
    {

		public List<Track> Queue { get; }

		public bool Repeat { get; set; }

        public bool Playing { get; set; }

	    public bool IsSuspended { get; set; }

		public IAudioClient ConnectedChannel { get; set; }

        public Process Player { get; set; }

        public ServerProperties()
        {
			Queue = new List<Track>();
			Repeat = false;
            Playing = false;
	        IsSuspended = false;
            ConnectedChannel = null;
            Player = null;

        }      
    }
}