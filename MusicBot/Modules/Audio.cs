using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace MusicBot
{
	public class Commands : ModuleBase<SocketCommandContext>
    {        

		private AudioService _audioService;

		private MessageService _messageService;

		private Commands()
		{

			_audioService = new AudioService();
			_messageService = new MessageService();

		}

		[Command("play", RunMode = RunMode.Async)]
		public async Task JoinChannel([Remainder]string link)
		{

			var _audioclient = _audioService.CheckForActiveChannel(Context.Guild);

			if (_audioclient == null)
				_audioclient = await _audioService.Join(Context.Guild, Context.Message, Context.Channel);

			_audioService.AddToQueue(link, Context.Guild, Context.Channel).Wait();

			_audioService.CheckActivity(Context.Guild, Context.Channel, Context.Message.Author.Username);

			await _audioService.PlayQueue(Context.Guild, Context.Channel);

		}
	    
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveChannel()
        {

			await _audioService.Leave(Context.Guild);

        }
        
        [Command("repeat", RunMode = RunMode.Async)]
        public async Task Repeat()
        {

			await _audioService.SetRepeat(Context.Guild, Context.Channel);

        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {

			await _audioService.Skip(Context.Guild, Context.Channel);

        }

		[Command("queue", RunMode = RunMode.Async)]
		public async Task Queue()
		{

			await _audioService.SendQueue(Context.Guild, Context.Channel);

		}
	}           
}