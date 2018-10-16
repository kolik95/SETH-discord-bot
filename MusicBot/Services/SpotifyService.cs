using Discord;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicBot
{
	public class SpotifyService
	{

		private MessageService _messageService { get; }

		public static ConcurrentDictionary<ulong,int> Selections { get; set; } 
			= new ConcurrentDictionary<ulong, int>();

		private SpotifyClient _spotify { get; }

		public SpotifyService()
		{

			_messageService = new MessageService();

			_spotify = SpotifyClient.GetInstance;

		}

		public async Task<List<SimpleTrack>> GetAlbumAsync(string input, IMessageChannel channel, IGuild guild)
		{
					
			var search = await _spotify.Client.SearchItemsAsync(input.Replace(" ", "&20"), SearchType.Album, 10);

			await UserAlbumSelectAsync(search, channel, guild);

			var tracks = await _spotify.Client.GetAlbumTracksAsync(search.Albums.Items[Math.Clamp(Selections[guild.Id], 0, search.Albums.Items.Count) - 1].Id, 100);

			Selections[guild.Id] = 1;

			return tracks.Items;
		}

		public async Task<List<PlaylistTrack>> GetPlaylistAsync(string input, IMessageChannel channel, IGuild guild)
		{

			var search = await _spotify.Client.SearchItemsAsync(input.Replace(" ", "&20"), SearchType.Playlist, 10);

			await UserPlaylistSelectAsync(search, channel, guild);

			var tracks = await _spotify.Client.GetPlaylistTracksAsync(search.Playlists.Items[Math.Clamp(Selections[guild.Id], 0, search.Playlists.Items.Count) - 1].Id, search.Playlists.Items[Math.Clamp(Selections[guild.Id], 0, search.Playlists.Items.Count) - 1].Id);

			Selections[guild.Id] = 1;

			return tracks.Items;
		}

		public async Task UserAlbumSelectAsync(SearchItem search, IMessageChannel channel, IGuild guild)
		{

			var albums = search.Albums.Items;

			var builders = new List<EmbedFieldBuilder>();

			for(var i = 0; i < albums.Count; i++)
			{

				builders.Add(new EmbedFieldBuilder
				{

					Name = $"{i + 1}.{albums[i].Name}",

					Value = albums[i].Id

				});
			}

			await _messageService.SelectMessage(channel, builders);

			Selections.TryAdd(guild.Id, 1);

			await Task.Delay(10000);

		}

		public async Task UserPlaylistSelectAsync(SearchItem search, IMessageChannel channel, IGuild guild)
		{

			var playlists = search.Playlists.Items;

			var builders = new List<EmbedFieldBuilder>();

			for (var i = 0; i < playlists.Count; i++)
			{

				builders.Add(new EmbedFieldBuilder
				{

					Name = $"{i + 1}.{playlists[i].Name} {playlists[i].Owner.DisplayName}",

					Value = playlists[i].Id

				});
			}

			await _messageService.SelectMessage(channel, builders);

			Selections.TryAdd(guild.Id, 1);

			await Task.Delay(10000);

		}
	}
}