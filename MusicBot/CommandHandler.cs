using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace MusicBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;

        private readonly string _prefix;

        private readonly CommandService _service;

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommandAsymc;

			if(Config.Bot.Server)
			 new CompanionAppHandler(_client);

            _prefix = Config.Bot.Prefix;

		}


        private async Task HandleCommandAsymc(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null)
                return;

            var context = new SocketCommandContext(_client, msg);

            var argPos = _prefix.Length - 1;
            if (msg.HasStringPrefix(_prefix, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos);

                if (!result.IsSuccess && result.Error == CommandError.UnknownCommand)
                    await context.Channel.SendMessageAsync("I can't do that.");
            }
        }
    }
}