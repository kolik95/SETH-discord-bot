using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
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

				_serverProperties[Context.Guild.Id].QueueNames.Add(GetStreamUrl($"-e \"{link.Replace(" ", "")}\"").StandardOutput.ReadLine());

			}

			else if (link.Contains("youtube.com"))
			{

				url = GetStreamUrl($"-f bestaudio -g \"{link.Replace(" ", "")}\"");

				_serverProperties[Context.Guild.Id].QueueNames.Add(GetStreamUrl($"-e \"{link.Replace(" ", "")}\"").StandardOutput.ReadLine());

				_serverProperties[Context.Guild.Id].QueueThumbnails.Add(GetStreamUrl($"--get-thumbnail \"{link.Replace(" ", "")}\"").StandardOutput.ReadLine());

				_serverProperties[Context.Guild.Id].QueueURLs.Add(link.Replace(" ", ""));

			}

			else
			{

				await Context.Channel.SendMessageAsync($"Searching for request by {Context.Message.Author.Username}: {link}");
	
				_serverProperties[Context.Guild.Id].QueueThumbnails.Add(GetStreamUrl($"--get-thumbnail ytsearch:\"{link}\"").StandardOutput.ReadLine());

				url = GetStreamUrl($"-f bestaudio -g -x ytsearch:\"{link}\"");
			    
			    _serverProperties[Context.Guild.Id].QueueNames.Add(GetStreamUrl($"-e ytsearch:\"{link}\"").StandardOutput.ReadLine());

				_serverProperties[Context.Guild.Id].QueueURLs.Add("https://www.youtube.com/watch?v=" + GetStreamUrl($"--get-id ytsearch:\"{link}\"").StandardOutput.ReadLine());

			}   

            string streamUrl = url.StandardOutput.ReadLine();
            
            _serverProperties[Context.Guild.Id].Queue.Add(streamUrl);

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

			foreach (var name in _serverProperties[Context.Guild.Id].QueueNames)
			{

				queue += name + "\n";

			}
			
			await Context.Channel.SendMessageAsync(queue);

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
                
                Console.WriteLine("Playing");

				await Context.Channel.SendMessageAsync("",false, CreateEmbed(_serverProperties[guild.Id].QueueNames[0], _serverProperties[guild.Id].QueueThumbnails[0], _serverProperties[guild.Id].QueueURLs[0]));

                _serverProperties[guild.Id].Playing = true;
                
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
        
        #endregion
        
    }           
}