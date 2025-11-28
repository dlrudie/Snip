using System.Text;
using System;

namespace Winter
{
    public static class ApplicationKeys
    {
        //new config setup
        public static string SpotifyClientId { get; private set; }
        public static string clientsecret { get; private set; }
        public static string Spotify = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", SpotifyClientId, clientsecret)));

        static ApplicationKeys()
        {
            string path = System.IO.Path.Combine(
                System.Windows.Forms.Application.StartupPath,
                "spotify_keys.txt");

            if (!System.IO.File.Exists(path))
            {
                System.Windows.Forms.MessageBox.Show(
                    "Please create spotify_keys.txt with your Spotify client ID and secret.\r\n" +
                    "Line 1: client id\r\nLine 2: client secret",
                    "Snip – Spotify config missing");
                throw new System.Exception("Missing spotify_keys.txt");
            }

            var lines = System.IO.File.ReadAllLines(path);
            if (lines.Length < 2)
                throw new System.Exception("spotify_keys.txt must have at least two lines");

            SpotifyClientId = lines[0].Trim();
            clientsecret = lines[1].Trim();
        }
    }
}
