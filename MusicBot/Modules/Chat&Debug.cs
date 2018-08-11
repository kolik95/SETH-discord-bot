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

namespace MusicBot
{
    public class ChatDebug : ModuleBase<SocketCommandContext>
    {
        
        private MessageService _messageService { get; set; }

        private ChatDebug()
        {

            _messageService = new MessageService();

        }
        
        [Command("Test")]
        public async Task A()
        {
            
            await Context.Channel.SendMessageAsync("čekám na signál");
            
        }
        
        [Command("help", RunMode = RunMode.Async)]
        public async Task Help()
        {

            await _messageService.HelpMessage(Context.Channel);

        }
        
    }
}