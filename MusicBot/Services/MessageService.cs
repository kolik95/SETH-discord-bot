using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MusicBot
{
	class MessageService
    {

		public async Task SearchingMessage(Color color, IMessageChannel channel, string link)
		{

			await channel.SendMessageAsync("", false, new EmbedBuilder
			{

				Title = $"Searching for: {link}",

				Color = color

			}.Build());

		}

		public async Task AddedMessage(IMessageChannel channel, Color color, string description, string thumbnail, string username)
		{

			await channel.SendMessageAsync("", false, new EmbedBuilder
			{

				Title = "Added to queue",

				Color = color,

				Description = description,

				ThumbnailUrl = thumbnail,

				Footer = new EmbedFooterBuilder { Text = $"Added by {username}" }

				

			}.Build());

		}

		public async Task PlayingMessage(IMessageChannel channel, Color color, string title, string url, string imageurl)
		{

			await channel.SendMessageAsync("", false, new EmbedBuilder
			{

				Title = $"Playing: {title}",

				Color = color,

				Url = url,

				ImageUrl = imageurl

			}.Build());

		}

		public async Task HelpMessage(IMessageChannel channel)
		{

			await channel.SendMessageAsync("Hey, my name is SETH. Here is a list of my commands:", false, new EmbedBuilder
			{

				Color = Color.Red,

				Fields = new List<EmbedFieldBuilder>
				{

					new EmbedFieldBuilder{ Name = "1.help", Value = "Gives you some help."},

					new EmbedFieldBuilder{ Name = "2.join", Value = "Joins your voice channel."},

					new EmbedFieldBuilder{ Name = "3.play + link/name of the song", Value = "Plays a song."},

					new EmbedFieldBuilder{ Name = "4.leave/stop", Value = "Disconnects from a voice channel."},

					new EmbedFieldBuilder{ Name = "5.repeat", Value = "Makes the current song play in a loop."},

					new EmbedFieldBuilder{ Name = "6.skip", Value = "Skips the current song playing."},

					new EmbedFieldBuilder{ Name = "7.queue", Value = "Displays the current queue."},

					new EmbedFieldBuilder{ Name = "8.clearq", Value = "Clears the queue."},

					new EmbedFieldBuilder{ Name = "9.removeat + the songs position in queue", Value = "Removes the selected song from queue."},

					new EmbedFieldBuilder{ Name = "10.pause", Value = "Pauses audio."},

					new EmbedFieldBuilder{ Name = "11.msgdel + number of messages (works kinda flimsy atm)", Value = "Removes the selected amount of messages."},

					new EmbedFieldBuilder{ Name = "Prefix", Value = Config.Bot.Prefix},

				}

			}.Build());

		}

		public async Task QueueMessage(IMessageChannel channel, List<EmbedFieldBuilder> list)
		{

			await channel.SendMessageAsync("", false, new EmbedBuilder
			{

				Title = "Queue",

				Color = Color.DarkRed,

				Fields = list

			}.Build());

		}

	    public async Task DeleteMessages(IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages, ITextChannel channel)
	    {
		    foreach (var thing in messages.ToEnumerable())
		    {

			    foreach (var thing2 in thing)
			    {

				    await thing2.DeleteAsync();

					Thread.Sleep(3000);

			    }
			}
	    }
	}
}