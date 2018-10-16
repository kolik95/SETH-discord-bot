using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Threading.Tasks;

namespace MusicBot
{
	public class SpotifyClient
    {

	    public SpotifyWebAPI Client { get; set; }

		public bool Auth { get; set; }

	    public static SpotifyClient GetInstance => _instance;

		private ImplictGrantAuth _factory { get; set; }

		private static readonly SpotifyClient _instance = new SpotifyClient();

	    public SpotifyClient()
	    {

		    _factory = new ImplictGrantAuth(
			    "f481e5ffbceb47a9aa8032cd37ea9b15",
			    "http://localhost:8000",
			    "http://localhost:8000",
			    Scope.UserReadPrivate);

		    _factory.AuthReceived += Factory_AuthReceived;

		}

	    public void InitializeClientAsync()
		{

			_factory.Start();

			_factory.OpenBrowser();

		}

		private async void Factory_AuthReceived(object sender, Token payload)
		{

			Client = new SpotifyWebAPI
			{
				AccessToken = payload.AccessToken,
				TokenType = payload.TokenType,
				UseAuth = true
			};

			_factory.Stop(0);

			Auth = true;

			await Task.Delay(3600000);

			Auth = false;

			Console.WriteLine("REAUTH REQUIRED!!!");

		}
	}
}
