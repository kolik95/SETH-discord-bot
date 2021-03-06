﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace MusicBot
{
    public static class Config
    {
        private const string configFile = "Config.json";
        private const string configFolder = "Config";

        public static BotConfig Bot;

        static Config()
        {
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))
            {
                Bot = new BotConfig();

                var json = JsonConvert.SerializeObject(Bot, Formatting.Indented);

                File.WriteAllText(configFolder + "/" + configFile, json);

                Console.WriteLine("Please configure the bot using the config file. (Do not configure the SpotifyAuthToken!)");

                Console.ReadKey();

                Environment.Exit(0);
            }

            else
			{
	            try
	            {

		            var json = File.ReadAllText(configFolder + "/" + configFile);

					Bot = JsonConvert.DeserializeObject<BotConfig>(json);

				}
	            catch (Exception e)
	            {

					Console.WriteLine("Invalid configuration data");

		            Console.ReadKey();

		            Environment.Exit(0);

				}

	            if (Bot.Token != null && Bot.Prefix != null) return;
	            Console.WriteLine("Please configure the bot using the config file. (Do not configure the SpotifyAuthToken!)");

	            Console.ReadKey();

	            Environment.Exit(0);
            }
        }

    }

    public struct BotConfig
    {
        public string Token;

        public string Prefix;

	    public string OwnerID;

		public bool Server;

	    public bool Spotify;

    }
}