using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MusicBot
{
    class AudioBridge
    {

		private AudioService _audioService { get; set; }

		private MessageService _messageService { get; set; }

		public AudioBridge()
		{

			_audioService = new AudioService();
			_messageService = new MessageService();

		}

		public async Task Play(string link, IVoiceChannel voice, IGuild guild, IMessageChannel channel, string username)
		{

			var _audioclient = _audioService.CheckForActiveChannel(guild);

			if (_audioclient == null)
				_audioclient = await _audioService.Join(guild, voice, channel);

			_audioService.AddToQueue(link, guild, channel).Wait();

			_audioService.CheckActivity(guild, channel, username);

			_audioService.PlayQueue(guild, channel);

		}

	}
}
