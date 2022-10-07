To build Snip you'll need to create your own ApplicationKeys.cs file.

The contents of the file should be as follows.

```
namespace Winter
{
    public static class ApplicationKeys
    {
        public static string SpotifyClientId = "";
        public static string SpotifyClientSecret = "";
        public static string Spotify = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", SpotifyClientId, SpotifyClientSecret)));
    }
}
```