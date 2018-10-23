using Discord;
using Discord.WebSocket;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MusicBot
{
	class CompanionAppHandler
    {

		private SimpleTcpServer _server;

		private DiscordSocketClient _client;

		private AudioBridge _audioBridge;

	    private List<Socket> clients;

		public CompanionAppHandler(DiscordSocketClient Client)
		{

			_client = Client;

			_audioBridge = new AudioBridge();

			_server = new SimpleTcpServer();

			_server.Delimiter = 0x13;

			_server.StringEncoder = Encoding.UTF8;

			_server.DataReceived += DataReceivedAsync;

			_server.ClientConnected += ClientConnected;

			Start();

		}

        private async void DataReceivedAsync(object sender, Message e) => await Play(e);

        private void ClientConnected(object sender, System.Net.Sockets.TcpClient e)
		{

			Console.WriteLine("Client connected");

			clients.Add(e.Client);

		}

		private void Start()
		{

			_server.Start(25565);

			Console.WriteLine("Server Started");

		}

		private async Task Play(Message e)
		{

			var parsedMessage = e.MessageString.Split("|p");

			var guild = _client.GetGuild(ulong.Parse(parsedMessage[1]));

			var channel = guild.GetTextChannel(ulong.Parse(parsedMessage[3]));

			var voice = guild.GetVoiceChannel(ulong.Parse(parsedMessage[4]));

			await _audioBridge.Play(parsedMessage[5], voice, guild, channel, "Companion APP");

		}
    }
}
