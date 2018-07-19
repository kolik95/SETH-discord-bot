using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicBot
{
	class AudioService
    {

		private static readonly ConcurrentDictionary<ulong, ServerProperties> _serverProperties =
			new ConcurrentDictionary<ulong, ServerProperties>();

		private MessageService _messageService;

		public AudioService()
		{

			_messageService = new MessageService();

		}

		public IAudioClient CheckForActiveChannel(IGuild guild)
		{

			IAudioClient audioClient;

			if (_serverProperties.ContainsKey(guild.Id))
			{

				if (_serverProperties[guild.Id].ConnectedChannel != null)
					audioClient = _serverProperties[guild.Id].ConnectedChannel;

				else
					return null;
			}

			else
			{

				_serverProperties.TryAdd(guild.Id,
					new ServerProperties());

				return null;

			}

			return audioClient;

		}

		public async Task<IAudioClient> Join(IGuild guild, IMessage message, IMessageChannel channel)
		{
			var target = ((IVoiceState)message.Author).VoiceChannel;

			if (target.Guild.Id != guild.Id) return null;

			var audioClient = await target.ConnectAsync();

			_serverProperties[guild.Id].ConnectedChannel = audioClient;

			return audioClient;

		}

		public async Task AddToQueue(string link, IGuild guild, IMessageChannel channel)
		{

			if (link.Contains("soundcloud.com"))
			{

				//url = StartYoutubeDL($"-f bestaudio -g \"{link.Replace(" ", "")}\"");

				//_serverProperties[guild.Id].Queue.Add(url.StandardOutput.ReadLine());

				//_serverProperties[guild.Id].QueueNames.Add(StartYoutubeDL($"-e \"{link.Replace(" ", "")}\"").StandardOutput.ReadLine());

			}

			else if (link.Contains("youtube.com"))
			{

				SearchYoutube(
					$"-f bestaudio -g \"{link.Replace(" ", "")}\"",
					$"--get-thumbnail \"{link.Replace(" ", "")}\"",
					link.Replace(" ", "")
					, $"-e \"{link.Replace(" ", "")}\"", false,
					guild);

			}

			else
			{

				await _messageService.SearchingMessage(Color.Green, channel, link);

				SearchYoutube(
					$"-f bestaudio -g -x ytsearch:\"{link}\"",
					$"--get-thumbnail ytsearch:\"{link}\"",
					$"--get-id ytsearch:\"{link}\"",
					$"-e ytsearch:\"{link}\"", true,
					guild);

			}
		}

		public async Task PlayQueue(IGuild guild, IMessageChannel channel)
		{

			Thread.Sleep(3000);

			if (_serverProperties[guild.Id].ConnectedChannel == null) return;

			if (_serverProperties[guild.Id].Queue.Count == 0)
			{

				await Leave(guild);

				return;

			}

			if (_serverProperties[guild.Id].Playing == false)
			{

				_serverProperties[guild.Id].Playing = true;

				if(!_serverProperties[guild.Id].Repeat)
					await _messageService.PlayingMessage(
						channel,
						Color.Blue,
						_serverProperties[guild.Id].QueueNames[0],
						_serverProperties[guild.Id].QueueURLs[0],
						_serverProperties[guild.Id].QueueThumbnails[0]);

				await SendAudioAsync(
					_serverProperties[guild.Id].ConnectedChannel,
					_serverProperties[guild.Id].Queue[0],
					guild,
					channel);

			}
		}

		public async Task Leave(IGuild guild)
		{

			if (_serverProperties[guild.Id].ConnectedChannel == null) return;

			IAudioClient client = _serverProperties[guild.Id].ConnectedChannel;

			ClearProperties(guild);

			_serverProperties[guild.Id].Playing = false;

			await client.StopAsync();

		}

		public async Task SetRepeat(IGuild guild, IMessageChannel channel)
		{

			if (_serverProperties[guild.Id].Repeat)
			{

				_serverProperties[guild.Id].Repeat = false;

				await channel.SendMessageAsync("Repeating off");

			}

			else
			{

				_serverProperties[guild.Id].Repeat = true;

				await channel.SendMessageAsync("Repeating on");

			}

		}

		public async Task Skip(IGuild guild, IMessageChannel channel)
		{

			if (_serverProperties[guild.Id].Playing)
			{

				_serverProperties[guild.Id].Repeat = false;

				KillProcess(_serverProperties[guild.Id].Player.Id);

				await channel.SendMessageAsync("Skipped");

			}

		}

		public async Task SendQueue(IGuild guild, IMessageChannel channel)
		{

			var builder = new StringBuilder();

			foreach (var name in _serverProperties[guild.Id].QueueNames)
			{

				builder.Append(name + "\n");

			}

			await channel.SendMessageAsync(builder.ToString());

		}

		public async Task CheckActivity(IGuild guild, IMessageChannel channel, string username)
		{

			if (_serverProperties[guild.Id].Playing)
			{

				await _messageService.AddedMessage(
					channel,
					Color.LightGrey,
					_serverProperties[guild.Id].QueueURLs[_serverProperties[guild.Id].QueueURLs.Count - 1]
					, _serverProperties[guild.Id].QueueThumbnails[_serverProperties[guild.Id].QueueThumbnails.Count - 1],
					username);

			}

		}

		private async Task SendAudioAsync(IAudioClient client, string path, IGuild guild, IMessageChannel channel)
		{

			Console.WriteLine("Playing");

			var ffmpeg = CreateStream(path);

			_serverProperties[guild.Id].Player = ffmpeg;

			ffmpeg.EnableRaisingEvents = true;

			ffmpeg.Exited += (sender, e) => StreamEnded(sender, e , guild, channel);

			var output = ffmpeg.StandardOutput.BaseStream;

			var discord = client.CreatePCMStream(AudioApplication.Mixed);

			await output.CopyToAsync(discord);

			await discord.FlushAsync();

		}

		private void StreamEnded(object sender, EventArgs e, IGuild guild, IMessageChannel channel)
		{

			_serverProperties[guild.Id].Playing = false;

			if (!_serverProperties[guild.Id].Repeat)
			{

				_serverProperties[guild.Id].Queue.RemoveAt(0);

				_serverProperties[guild.Id].QueueNames.RemoveAt(0);

				_serverProperties[guild.Id].QueueThumbnails.RemoveAt(0);

				_serverProperties[guild.Id].QueueURLs.RemoveAt(0);

			}

			new Thread(() => PlayQueue(guild, channel)).Start();

		}

		private void SearchYoutube(string args1, string args2, string args3, string args4, bool link, IGuild guild)
		{

			var thread1 = new Thread(() => AddSong(args1, guild));

			thread1.Start();

			var thread2 = new Thread(() => AddThumbnail(args2, guild));

			thread2.Start();

			var thread3 = new Thread(() => AddURL(args3, link, guild));

			thread3.Start();

			var thread4 = new Thread(() => AddName(args4, guild));

			thread4.Start();

			thread1.Join();
			thread2.Join();
			thread3.Join();
			thread4.Join();

		}

		private void KillProcess(int PID)
		{
			Process killFfmpeg = new Process();

			ProcessStartInfo taskkillStartInfo = new ProcessStartInfo
			{
				FileName = "taskkill",
				Arguments = "/PID " + Convert.ToString(PID) + " /T /F",
				UseShellExecute = false,
				CreateNoWindow = true
			};

			killFfmpeg.StartInfo = taskkillStartInfo;

			killFfmpeg.Start();

		}

		#region QueueUtilities

		private void AddSong(string args, IGuild guild)
		{

			_serverProperties[guild.Id].Queue.Add(StartYoutubeDL(args).StandardOutput.ReadLine());

		}

		private void AddThumbnail(string args, IGuild guild)
		{

			_serverProperties[guild.Id].QueueThumbnails.Add(StartYoutubeDL(args).StandardOutput.ReadLine());

		}

		private void AddURL(string args, bool build, IGuild guild)
		{

			if (build)
			{

				var builder = new StringBuilder();

				builder.Append("https://www.youtube.com/watch?v=").Append(StartYoutubeDL(args).StandardOutput.ReadLine());

				_serverProperties[guild.Id].QueueURLs.Add(builder.ToString());

			}
			else
			{

				_serverProperties[guild.Id].QueueURLs.Add(args);

			}
		}

		private void AddName(string args, IGuild guild)
		{

			_serverProperties[guild.Id].QueueNames.Add(StartYoutubeDL(args).StandardOutput.ReadLine());

		}

		private void ClearProperties(IGuild guild)
		{

			_serverProperties[guild.Id].Queue.Clear();

			_serverProperties[guild.Id].QueueNames.Clear();

			_serverProperties[guild.Id].QueueThumbnails.Clear();

			_serverProperties[guild.Id].QueueURLs.Clear();

			_serverProperties[guild.Id].Repeat = false;

			_serverProperties[guild.Id].ConnectedChannel = null;

		}

		#endregion

		#region Processes

		private Process CreateStream(string path)
		{
			var ffmpeg = new ProcessStartInfo
			{
				FileName = "ffmpeg.exe",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true
			};

			return Process.Start(ffmpeg);
		}

		private Process StartYoutubeDL(string arguments)
		{
			var yt = new ProcessStartInfo
			{
				FileName = "youtube-dl.exe",
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			return Process.Start(yt);
		}

		#endregion
	}
}