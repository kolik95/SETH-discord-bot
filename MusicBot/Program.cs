using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MusicBot
{
    public class Program
    {      
        private static async Task Main(string[] args)
        {                       
            
            if (string.IsNullOrEmpty(Config.Bot.Token)) return;

            DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            _client.Log += Log;

	        try
	        {

		        await _client.LoginAsync(TokenType.Bot, Config.Bot.Token);

			}
	        catch (Exception e)
	        {

		        Console.WriteLine("Invalid token");

		        Console.ReadKey();

				Environment.Exit(0);

	        }

            await _client.StartAsync();

            CommandHandler _handler = new CommandHandler(_client);

            await Task.Delay(-1);
            
        }

        private static async Task Log(LogMessage msg)
        {
            await Console.Out.WriteLineAsync(msg.Message);
        }
    }
}