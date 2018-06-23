using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MusicBot
{
    public class Program
    {
        private DiscordSocketClient _client;

        private CommandHandler _handler;

        private static void Main(string[] args)
        {
            new Program().StartAsync().GetAwaiter().GetResult();
        }

        private async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.Bot.Token)) return;

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, Config.Bot.Token);

            await _client.StartAsync();

            _handler = new CommandHandler(_client);

            await Task.Delay(-1);
        }

        private async Task Log(LogMessage msg)
        {
            await Console.Out.WriteLineAsync(msg.Message);
        }
    }
}