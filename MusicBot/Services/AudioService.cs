using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicBot
{
	internal class AudioService
	{
		private static readonly ConcurrentDictionary<ulong, ServerProperties> _serverProperties =
			new ConcurrentDictionary<ulong, ServerProperties>();

		private SpotifyService _spotifyService { get; }

		public AudioService()
		{
			_messageService = new MessageService();

			_spotifyService = new SpotifyService();

		}

		private MessageService _messageService { get; }

		#region Public Methods

		/// <summary>
		/// Returns the current voice channel
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Joins a voice channel
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="voice"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task<IAudioClient> Join(IGuild guild, IVoiceChannel voice, IMessageChannel channel)
		{
			var target = voice;

			if (target.Guild.Id != guild.Id) return null;

			if (_serverProperties[guild.Id].ConnectedChannel != null)
				return _serverProperties[guild.Id].ConnectedChannel;

			var audioClient = await target.ConnectAsync();

			_serverProperties[guild.Id].ConnectedChannel = audioClient;

			return audioClient;
		}

		/// <summary>
		/// Adds the input to the queue
		/// </summary>
		/// <param name="link"></param>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="username"></param>
		/// <returns></returns>
		public async Task AddToQueue(string link, IGuild guild, IMessageChannel channel,IVoiceChannel voice , string username)
		{

			if (link.Contains("soundcloud.com"))
			{

				_serverProperties[guild.Id].Queue.Add(new Track
				{

					DirectURL = StartYoutubeDL($"-f bestaudio -g \"{link.Replace(" ", "")}\"").StandardOutput.ReadLine(),

					Name = "Soundcloud track",

					YoutubeURL = link.Replace(" ", ""),

					Thumbnail = "https://pmcvariety.files.wordpress.com/2018/02/soundcloud-logo.jpg?w=814"

				});

			}

			else if (link.Contains("album:"))
			{

				if (!Config.Bot.Spotify)
				{

					await channel.SendMessageAsync(
						"Spotify features are disabled on this bot. Please ask the bot owner to enable them.");

					Thread.CurrentThread.Abort();

				}

				if (!SpotifyClient.GetInstance.Auth)
				{

					await channel.SendMessageAsync(
						"Spotify reauthentification required. Please ask the bot owner to reauthenticate.");

					Thread.CurrentThread.Abort();

				}

				var tracks = await _spotifyService.GetAlbumAsync(link.Replace("album:", ""), channel, guild);

				foreach (var track in tracks)
				{

					await Join(guild, voice, channel);

					var task = Task.Run(() => SearchYoutube(
						$"-f bestaudio -g -x ytsearch:\"{track.Artists[0].Name + track.Name}\"",
						$"--get-thumbnail ytsearch:\"{track.Artists[0].Name + track.Name}\"",
						$"--get-id ytsearch:\"{track.Artists[0].Name + track.Name}\"",
						$"-e --encoding UTF-16 ytsearch:\"{track.Artists[0].Name + track.Name}\"", true,
						guild));

					task.Wait();

					PlayQueue(guild, channel);

				}
			}

			else if (link.Contains("playlist:"))
			{

				if (!Config.Bot.Spotify)
				{

					await channel.SendMessageAsync(
						"Spotify features are disabled on this bot. Please ask the bot owner to enable them.");

					Thread.CurrentThread.Abort();

				}

				if(!SpotifyClient.GetInstance.Auth)
				{

					await channel.SendMessageAsync(
						"Spotify reauthentification required. Please ask the bot owner to reauthenticate.");

					Thread.CurrentThread.Abort();

				}

				var tracks = await _spotifyService.GetPlaylistAsync(link.Replace("playlist:", ""), channel, guild);

				foreach (var track in tracks)
				{

					await Join(guild, voice, channel);

					var task = Task.Run(() => SearchYoutube(
						$"-f bestaudio -g -x ytsearch:\"{track.Track.Artists[0].Name + track.Track.Name}\"",
						$"--get-thumbnail ytsearch:\"{track.Track.Artists[0].Name + track.Track.Name}\"",
						$"--get-id ytsearch:\"{track.Track.Artists[0].Name + track.Track.Name}\"",
						$"-e --encoding UTF-16 ytsearch:\"{track.Track.Artists[0].Name + track.Track.Name}\"", true,
						guild));

					task.Wait();

					PlayQueue(guild, channel);

				}

			}

			/* else if (link.Contains("&link") && link.Contains("youtube.com"))
			{

				Process yt = StartYoutubeDL($"youtube - dl.exe - i - g--yes - playlist {link}");

				string output = yt.StandardOutput.ReadLine();

				Console.WriteLine(output);

			} */

			else if (link.Contains("youtube.com"))
				{
					SearchYoutube(
						$"-f bestaudio -g \"{link.Replace(" ", "")}\"",
						$"--get-thumbnail \"{link.Replace(" ", "")}\"",
						link.Replace(" ", "")
						, $"-e --encoding UTF-16 \"{link.Replace(" ", "")}\"", false,
						guild);
			}

			else
			{
				await _messageService.SearchingMessage(Color.Green, channel, link);

				SearchYoutube(
					$"-f bestaudio -g -x ytsearch:\"{link}\"",
					$"--get-thumbnail ytsearch:\"{link}\"",
					$"--get-id ytsearch:\"{link}\"",
					$"-e --encoding UTF-16 ytsearch:\"{link}\"", true,
					guild);
			}
		}

		/// <summary>
		/// Starts playing the first item in the queue
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task PlayQueue(IGuild guild, IMessageChannel channel)
		{
			if (_serverProperties[guild.Id].ConnectedChannel == null ||
			    _serverProperties[guild.Id].Queue.Count == 0)
			{
				await Leave(guild);

				return;
			}

			if (!_serverProperties[guild.Id].Playing)
			{
				_serverProperties[guild.Id].Playing = !_serverProperties[guild.Id].Playing;

				if (!_serverProperties[guild.Id].Repeat)
					await _messageService.PlayingMessage(
						channel,
						Color.Blue,
						_serverProperties[guild.Id].Queue[0].Name,
						_serverProperties[guild.Id].Queue[0].YoutubeURL,
						_serverProperties[guild.Id].Queue[0].Thumbnail);

				await SendAudioAsync(
					_serverProperties[guild.Id].ConnectedChannel,
					_serverProperties[guild.Id].Queue[0].DirectURL,
					guild,
					channel);
			}
		}

		/// <summary>
		/// Removes everything from the queue
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task ClearQueueAsync(IGuild guild)
		{
			
			_serverProperties[guild.Id].Queue.Clear();

			_serverProperties[guild.Id].Queue.Add(new Track());
		}

		/// <summary>
		/// Leaves the current voice channel
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task Leave(IGuild guild)
		{
			if (_serverProperties[guild.Id].ConnectedChannel == null) return;

			var client = _serverProperties[guild.Id].ConnectedChannel;

			ClearProperties(guild);

			await client.StopAsync();

			_serverProperties[guild.Id].ConnectedChannel = null;
		}

		/// <summary>
		/// Changes the repeat value
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Skips the current playing track
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task Skip(IGuild guild, IMessageChannel channel)
		{
			if (_serverProperties[guild.Id].Playing)
			{
				_serverProperties[guild.Id].Repeat = false;

				_serverProperties[guild.Id].Player.Kill();

				await channel.SendMessageAsync("Skipped");
			}
		}

		/// <summary>
		/// Sends the queue to the user
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task SendQueue(IGuild guild, IMessageChannel channel)
		{
			var fields = new List<EmbedFieldBuilder>();

			for (var i = 1; i < _serverProperties[guild.Id].Queue.Count; i++)
				fields.Add(new EmbedFieldBuilder
				{
					Name = $"{i}.{_serverProperties[guild.Id].Queue[i].Name}",

					Value = _serverProperties[guild.Id].Queue[i].YoutubeURL
				});

			await _messageService.QueueMessage(channel, fields);
		}

		/// <summary>
		/// Checks if the bot is busy playing
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="username"></param>
		/// <returns></returns>
		public async Task CheckActivity(IGuild guild, IMessageChannel channel, string username)
		{
			if (_serverProperties[guild.Id].Queue.Count == 0)
				return;

			if (_serverProperties[guild.Id].Playing)
				await _messageService.AddedMessage(
					channel,
					Color.LightGrey,
					_serverProperties[guild.Id].Queue[_serverProperties[guild.Id].Queue.Count - 1].YoutubeURL,
					_serverProperties[guild.Id].Queue[_serverProperties[guild.Id].Queue.Count - 1].Thumbnail,
					username);
		}


		/// <summary>
		/// Removes a specific item from the queue
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="item"></param>
		/// <returns></returns>
        public async Task RemoveAt(IGuild guild, int item) => RemoveQueueItem(guild, item);

		#endregion

		#region Private Methods

		private async Task SendAudioAsync(IAudioClient client, string path, IGuild guild, IMessageChannel channel)
		{
			var ffmpeg = CreateStream(path);

			_serverProperties[guild.Id].Player = ffmpeg;

			ffmpeg.EnableRaisingEvents = true;

			ffmpeg.Exited += (sender, e) => StreamEnded(sender, e, guild, channel);

			var output = ffmpeg.StandardOutput.BaseStream;

			var discord = client.CreatePCMStream(AudioApplication.Mixed);

			await output.CopyToAsync(discord);

			await discord.FlushAsync();
		}

		private void StreamEnded(object sender, EventArgs e, IGuild guild, IMessageChannel channel)
		{
			_serverProperties[guild.Id].Playing = false;

			if (!_serverProperties[guild.Id].Repeat) RemoveQueueItem(guild, 0);

			if (_serverProperties[guild.Id].ConnectedChannel == null) return;

			new Thread(async () => await PlayAfterSkip(guild, channel)).Start();
		}

		private void SearchYoutube(string args1, string args2, string args3, string args4, bool link, IGuild guild)
		{

			var track = new Track();

			var thread1 = new Thread(() => AddSong(args1, ref track));

			thread1.Start();

			var thread2 = new Thread(() => AddThumbnail(args2, ref track));

			thread2.Start();

			var thread3 = new Thread(() => AddURL(args3, link, ref track));

			thread3.Start();

			var thread4 = new Thread(() => AddName(args4, ref track));

			thread4.Start();

			thread1.Join();
			thread2.Join();
			thread3.Join();
			thread4.Join();

			_serverProperties[guild.Id].Queue.Add(track);
		}

		private async Task PlayAfterSkip(IGuild guild, IMessageChannel channel)
		{
			Thread.Sleep(3000);

            await PlayQueue(guild, channel);
		}

		public async Task Pause(IGuild guild, IMessageChannel channel)
		{

			if (!_serverProperties[guild.Id].IsSuspended)
			{

				await OSConfig.ProcessManager.Suspend(_serverProperties[guild.Id].Player);

				await channel.SendMessageAsync("Paused.");

			}

			else
			{

				await OSConfig.ProcessManager.Resume(_serverProperties[guild.Id].Player);

				await channel.SendMessageAsync("Unpaused.");

			}

			_serverProperties[guild.Id].IsSuspended = !_serverProperties[guild.Id].IsSuspended;

		}

		#region QueueUtilities

		private void AddSong(string args, ref Track track)
		{
			track.DirectURL = StartYoutubeDL(args).StandardOutput.ReadLine();
		}

		private void AddThumbnail(string args, ref Track track)
		{
			track.Thumbnail = StartYoutubeDL(args).StandardOutput.ReadLine();
		}

		private void AddURL(string args, bool build, ref Track track)
		{
			if (build)
			{
				var builder = new StringBuilder();

				builder.Append("https://www.youtube.com/watch?v=")
					.Append(StartYoutubeDL(args).StandardOutput.ReadLine());

				track.YoutubeURL = builder.ToString();
			}
			else
			{
				track.YoutubeURL = args;
			}
		}

		private void AddName(string args, ref Track track)
		{
			track.Name = StartYoutubeDL(args).StandardOutput.ReadLine();
		}

		private void ClearProperties(IGuild guild)
		{
			_serverProperties[guild.Id].Queue.Clear();

			_serverProperties[guild.Id].Repeat = false;

		}

        private void RemoveQueueItem(IGuild guild, int item)
        {
            _serverProperties[guild.Id].Queue.RemoveAt(item);
        }

        #endregion

        #region Processes

        private Process CreateStream(string path)
		{
			var ffmpeg = new ProcessStartInfo
			{
				FileName = OSConfig.ffmpegProcess,
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
				FileName = OSConfig.youtubeDlProcess,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			return Process.Start(yt);
		}
	}
		#endregion

		#endregion
}