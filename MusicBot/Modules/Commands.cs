using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;

namespace MusicBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private static readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private static ConcurrentDictionary<ulong, List<string>> ServerQueues = new ConcurrentDictionary<ulong, List<string>>();
        
        private static ConcurrentDictionary<ulong, bool> ServerIsPlaying = new ConcurrentDictionary<ulong, bool>();
        
        private static ConcurrentDictionary<ulong, bool> ServerIsRepeating = new ConcurrentDictionary<ulong, bool>();

        [Command("Test")]
        public async Task a()
        {
            await Context.Channel.SendMessageAsync("Test");
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task JoinChannel(string link)
        {

            IAudioClient audioClient;
            
            if (ConnectedChannels.ContainsKey(Context.Guild.Id))
                audioClient = ConnectedChannels[Context.Guild.Id];

            else
                audioClient = await Join();

            if (!ServerQueues.ContainsKey(Context.Guild.Id))
                ServerQueues.TryAdd(Context.Guild.Id, new List<string>());

            if (!ServerIsPlaying.ContainsKey(Context.Guild.Id))
                ServerIsPlaying.TryAdd(Context.Guild.Id, false);

            if (!ServerIsRepeating.ContainsKey(Context.Guild.Id))
                ServerIsRepeating.TryAdd(Context.Guild.Id, false);
            
            var url = GetStreamUrl(link);

            string streamUrl = url.StandardOutput.ReadLine();

            ServerQueues[Context.Guild.Id].Add(streamUrl);

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

            if (ServerIsRepeating[Context.Guild.Id])
            {

                ServerIsRepeating[Context.Guild.Id] = false;

                await Context.Channel.SendMessageAsync("Repeating off");

                return;

            }
            
            if (!ServerIsRepeating[Context.Guild.Id])
            {

                ServerIsRepeating[Context.Guild.Id] = true;

                await Context.Channel.SendMessageAsync("Repeating on");

            }


        }

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

        private Process GetStreamUrl(string path)
        {
            var yt = new ProcessStartInfo
            {
                FileName = "youtube-dl.exe",
                Arguments = $"-f bestaudio -g \"{path}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            return Process.Start(yt);
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            
            var ffmpeg = CreateStream(path);

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

            if (ConnectedChannels.TryGetValue(guild.Id, out client)) return null;

            if (target.Guild.Id != guild.Id) return null;

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
                await Context.Channel.SendMessageAsync("Connected to voice.");

            return audioClient;
        }

        private async Task PlayQueue(IGuild guild)
        {

            if (!ConnectedChannels.ContainsKey(guild.Id)) return;

            if (ServerQueues[guild.Id].Count == 0)
            {

                await Leave(guild);
                
                return;
                
            }

            if (ServerIsPlaying[guild.Id] == false)
            {
                
                Console.WriteLine("Hraju");
                
                ServerIsPlaying[guild.Id] = true;
                
                await SendAsync(ConnectedChannels[guild.Id] , ServerQueues[guild.Id][0]);              

            }    
        }

        private void StreamEnded(object sender, EventArgs e)
        {

            ServerIsPlaying[Context.Guild.Id] = false;

            if(!ServerIsRepeating[Context.Guild.Id])
                ServerQueues[Context.Guild.Id].RemoveAt(0);
            
            PlayQueue(Context.Guild);

        }
        
        private async Task Leave(IGuild guild)
        {
    
            IAudioClient client;

            ServerQueues[guild.Id].Clear();

            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await Context.Channel.SendMessageAsync("Leaving voice.");
                
                ServerIsPlaying[Context.Guild.Id] = false;

                await client.StopAsync();
            }
    
        }
        
    }           
}