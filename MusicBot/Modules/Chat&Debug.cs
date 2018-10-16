using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

			await _messageService.HelpMessage(Context.Channel);

		}

		[RequireBotPermission(ChannelPermission.ManageMessages)]
		[RequireUserPermission(ChannelPermission.ManageMessages)]
		[Command("msgdel", RunMode = RunMode.Async)]
		public async Task MessageDelete([Remainder] string input)
		{

			IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages; 

			if (input == "all")
			{

				messages = Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 100000);

				new Thread(() => _messageService.DeleteMessages(messages, (ITextChannel)Context.Channel)).Start();

				return;

			}

			if (!int.TryParse(input, out var k))
				return;

			messages = Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, k);

			new Thread(() => _messageService.DeleteMessages(messages, (ITextChannel)Context.Channel)).Start();

		}

		[Command("Select")]
		public async Task Select([Remainder]string input)
		{

			if (!int.TryParse(input, out int k)) return;

			SpotifyService.Selections[Context.Guild.Id] = k;

		}
	}
}