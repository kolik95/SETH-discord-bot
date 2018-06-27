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
        
        [Command("Test")]
        public async Task A()
        {
            
            await Context.Channel.SendMessageAsync("Test");
            
        }
        
    }
}