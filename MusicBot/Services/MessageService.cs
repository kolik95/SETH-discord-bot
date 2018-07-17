using Discord;
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
    }
}
