using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace MusicBot
{
	public class Audio : ModuleBase<SocketCommandContext>
    {        

		private AudioService _audioService { get; set; }

		private MessageService _messageService { get; set; }

		private AudioBridge _audioBridge { get; set; }

		public Audio()
		{

			_audioService = new AudioService();
			_messageService = new MessageService();
			_audioBridge = new AudioBridge();

		}

		[Command("join", RunMode = RunMode.Async)]
		public async Task Join()
		{

			var _audioclient = _audioService.CheckForActiveChannel(Context.Guild);

			if (_audioclient == null)
				_audioclient = await _audioService.Join(Context.Guild, ((IVoiceState)Context.User).VoiceChannel, Context.Channel);

		}

		[Command("play", RunMode = RunMode.Async)]
		public async Task Play([Remainder]string link)
		{

			await _audioBridge.Play(link, ((IVoiceState)Context.User).VoiceChannel, Context.Guild, Context.Channel, Context.User.Username);

		}

	    [Command("stop", RunMode = RunMode.Async)]
	    private async Task Stop()
	    {

		    await LeaveChannel();

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

		[Command("clearq", RunMode = RunMode.Async)]
		public async Task Clearq()
		{

			await _audioService.ClearQueueAsync(Context.Guild);

		}

		[Command("removeat", RunMode = RunMode.Async)]
		public async Task Remove([Remainder]string item)
		{

			if (!int.TryParse(item, out int number)) return;

			await _audioService.RemoveAt(Context.Guild, number);

		}

	    [Command("pause", RunMode = RunMode.Async)]
		public async Task Pause()
	    {

		    await _audioService.Pause(Context.Guild, Context.Channel);

	    }
	}           
}