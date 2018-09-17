using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MusicBot
{
	public class ChatDebug : ModuleBase<SocketCommandContext>
	{
		private ChatDebug()
		{
			_messageService = new MessageService();
		}

		private MessageService _messageService { get; }

		[Command("Test")]
		public async Task A()
		{
			await Context.Channel.SendMessageAsync("čekám na signál");
		}

		[Command("help", RunMode = RunMode.Async)]
		public async Task Help()
		{
			var guild = new DiscordSocketClient().GetGuild(248872442209107968);

			var supersecretchannel = (IMessageChannel) guild.GetChannel(361090138538901504);

			Console.WriteLine(supersecretchannel.GetMessagesAsync() + "ahoj");

			await _messageService.HelpMessage(Context.Channel);
		}
	}
}