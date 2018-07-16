using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Newtonsoft.Json.Schema;

namespace MusicBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {        
        private static readonly ConcurrentDictionary<ulong, ServerProperties> _serverProperties = 
            new ConcurrentDictionary<ulong, ServerProperties>();

		#region Commands

		[Command("play", RunMode = RunMode.Async)]
		public async Task JoinChannel([Remainder]string link)
		{

			IAudioClient audioClient;

			Process url;

			if (_serverProperties.ContainsKey(Context.Guild.Id))
			{

				if (_serverProperties[Context.Guild.Id].ConnectedChannel != null)
					audioClient = _serverProperties[Context.Guild.Id].ConnectedChannel;

				else
					audioClient = await Join();

			}

			else
			{

				_serverProperties.TryAdd(Context.Guild.Id,
					new ServerProperties());

				audioClient = await Join();


			}

			if (link.Contains("soundcloud.com"))
			{

				url = GetStreamUrl($"-f bestaudio -g \"{link.Replace(" ", "")}\"");

				_serverProperties[Context.Guild.Id].Queue.Add(url.StandardOutput.ReadLine());

				_serverProperties[Context.Guild.Id].QueueNames.Add(GetStreamUrl($"-e \"{link.Replace(" ", "")}\"").StandardOutput.ReadLine());

			}

			else if (link.Contains("youtube.com"))
			{

				LaunchThreads($"-f bestaudio -g \"{link.Replace(" ", "")}\"", $"--get-thumbnail \"{link.Replace(" ", "")}\"", link.Replace(" ", ""), $"-e \"{link.Replace(" ", "")}\"", false);

			}

			else
			{

				await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
				{

					Title = $"Searching for: {link}",

					Color = Color.Green

				}.Build());

				LaunchThreads($"-f bestaudio -g -x ytsearch:\"{link}\"", $"--get-thumbnail ytsearch:\"{link}\"", $"--get-id ytsearch:\"{link}\"", $"-e ytsearch:\"{link}\"", true);

			}

			if (_serverProperties[Context.Guild.Id].Playing)
				await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
				{

					Title = "Added to queue",

					Color = Color.LighterGrey,

					Description = _serverProperties[Context.Guild.Id].QueueURLs[_serverProperties[Context.Guild.Id].QueueURLs.Count - 1],

					ThumbnailUrl = _serverProperties[Context.Guild.Id].QueueThumbnails[_serverProperties[Context.Guild.Id].QueueThumbnails.Count - 1],

					Footer = new EmbedFooterBuilder { Text = $"Added by {Context.Message.Author.Username}" }

					}.Build());

			await PlayQueue(Context.Guild);

		}

	    [Command("playlocal", RunMode = RunMode.Async)]
	    public async Task PlayLocal([Remainder]string link)
	    {
		    
		    IAudioClient audioClient;

		    Process url;

		    if (!File.Exists(link)) return;
		    
		    if (_serverProperties.ContainsKey(Context.Guild.Id))
		    {

			    if (_serverProperties[Context.Guild.Id].ConnectedChannel != null)
				    audioClient = _serverProperties[Context.Guild.Id].ConnectedChannel;

			    else
				    audioClient = await Join();

		    }

		    else
		    {

			    _serverProperties.TryAdd(Context.Guild.Id,
				    new ServerProperties());

			    audioClient = await Join();


		    };

		    _serverProperties[Context.Guild.Id].QueueNames.Add(link);
		    
		    _serverProperties[Context.Guild.Id].Queue.Add(link);

		    await Context.Channel.SendMessageAsync("Added to queue.");
		    
		    await PlayQueue(Context.Guild);
		    
	    }
	    
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveChannel()
        {
            IGuild guild = Context.Guild;

            await Leave(guild);

        }
        
        [Command("repeat", RunMode = RunMode.Async)]
        public async Task Repeat()
        {

            if (_serverProperties[Context.Guild.Id].Repeat)
            {

                _serverProperties[Context.Guild.Id].Repeat = false;

                await Context.Channel.SendMessageAsync("Repeating off");

            }
            
            else
            {

                _serverProperties[Context.Guild.Id].Repeat = true;

                await Context.Channel.SendMessageAsync("Repeating on");

            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {

            if (_serverProperties[Context.Guild.Id].Playing)
            {

	            _serverProperties[Context.Guild.Id].Repeat = false;
	            
                _serverProperties[Context.Guild.Id].Player.Kill();

                await Context.Channel.SendMessageAsync("Skipped");

            }

        }

		[Command("queue", RunMode = RunMode.Async)]
		public async Task Queue()
		{

		    string queue = string.Empty;

			var builder = new StringBuilder();

			foreach (var name in _serverProperties[Context.Guild.Id].QueueNames)
			{

				builder.Append(name + "\n");

			}
			
			await Context.Channel.SendMessageAsync(builder.ToString());

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

        private Process GetStreamUrl(string arguments)
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
        
        #region Utils
        
        private async Task SendAsync(IAudioClient client, string path)
        {
            
            var ffmpeg = CreateStream(path);
            
            _serverProperties[Context.Guild.Id].Player = ffmpeg;

            ffmpeg.EnableRaisingEvents = true;

            ffmpeg.Exited += StreamEnded;
            
            var output = ffmpeg.StandardOutput.BaseStream;
            
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            
            await output.CopyToAsync(discord);
            
            await discord.FlushAsync();

        }

        private async Task<IAudioClient> Join()
        {
            var target = ((IVoiceState) Context.Message.Author).VoiceChannel;

            IGuild guild = Context.Guild;
            
            IAudioClient client;
            
            if (target.Guild.Id != guild.Id) return null;

            var audioClient = await target.ConnectAsync();

            _serverProperties[Context.Guild.Id].ConnectedChannel = audioClient;
            
            await Context.Channel.SendMessageAsync("Connected to voice.");

            return audioClient;
        }

        private async Task PlayQueue(IGuild guild)
        {
            
            if (_serverProperties[guild.Id].ConnectedChannel == null) return;
            
            if (_serverProperties[guild.Id].Queue.Count == 0)
            {

                await Leave(guild);
                
                return;
                
            }

            if (_serverProperties[guild.Id].Playing == false)
            {

                _serverProperties[guild.Id].Playing = true;

				await Context.Channel.SendMessageAsync("", false, CreateEmbed(_serverProperties[Context.Guild.Id].QueueNames[0], _serverProperties[Context.Guild.Id].QueueThumbnails[0], _serverProperties[Context.Guild.Id].QueueURLs[0]));

				await SendAsync(_serverProperties[guild.Id].ConnectedChannel , _serverProperties[guild.Id].Queue[0]);     				

			}    
        }

        private void StreamEnded(object sender, EventArgs e)
        {

            _serverProperties[Context.Guild.Id].Playing = false;

            if (!_serverProperties[Context.Guild.Id].Repeat)
            {

                _serverProperties[Context.Guild.Id].Queue.RemoveAt(0);
                
                _serverProperties[Context.Guild.Id].QueueNames.RemoveAt(0);

				_serverProperties[Context.Guild.Id].QueueThumbnails.RemoveAt(0);

				_serverProperties[Context.Guild.Id].QueueURLs.RemoveAt(0);

			}

            PlayQueue(Context.Guild);

		}
        
        private async Task Leave(IGuild guild)
        {
            
            if (_serverProperties[guild.Id].ConnectedChannel == null) return;
            
            IAudioClient client = _serverProperties[guild.Id].ConnectedChannel;

            _serverProperties[guild.Id].Queue.Clear();

            _serverProperties[guild.Id].QueueNames.Clear();

			_serverProperties[guild.Id].QueueThumbnails.Clear();

			_serverProperties[guild.Id].QueueURLs.Clear();

			_serverProperties[guild.Id].ConnectedChannel = null;
            
            await Context.Channel.SendMessageAsync("Leaving voice.");

            _serverProperties[guild.Id].Playing = false;

            await client.StopAsync();
    
        }

		private Embed CreateEmbed(string title, string thumbnail, string url)
		{

			EmbedBuilder builder = new EmbedBuilder();

			builder.WithTitle($"Playing: {title}");

			builder.WithUrl(url);

			builder.WithImageUrl(thumbnail);

			builder.WithColor(Color.Blue);

			return builder.Build();

		}

		private void AddSong(string args)
		{

			_serverProperties[Context.Guild.Id].Queue.Add(GetStreamUrl(args).StandardOutput.ReadLine());

		}

		private void AddThumbnail(string args)
		{

			_serverProperties[Context.Guild.Id].QueueThumbnails.Add(GetStreamUrl(args).StandardOutput.ReadLine());

		}

		private void AddURL(string args, bool build)
		{

			if(build)
			{

				var builder = new StringBuilder();

				builder.Append("https://www.youtube.com/watch?v=").Append(GetStreamUrl(args).StandardOutput.ReadLine());

				_serverProperties[Context.Guild.Id].QueueURLs.Add(builder.ToString());

			}
			else
			{

				_serverProperties[Context.Guild.Id].QueueURLs.Add(args);

			}
		}

		private void AddName(string args)
		{

			_serverProperties[Context.Guild.Id].QueueNames.Add(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(GetStreamUrl(args).StandardOutput.ReadLine())));

		}

		private void LaunchThreads(string args1, string args2, string args3, string args4, bool link)
		{

			var thread1 = new Thread(() => AddSong(args1));

			thread1.Start();

			var thread2 = new Thread(() => AddThumbnail(args2));

			thread2.Start();

			var thread3 = new Thread(() => AddURL(args3, link));

			thread3.Start();

			var thread4 = new Thread(() => AddName(args4));

			thread4.Start();

			thread1.Join();
			thread2.Join();
			thread3.Join();
			thread4.Join();

		}

		#endregion

	}           
}