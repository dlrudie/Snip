using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Winter.ArtistTitle
{
    // Credits: https://github.com/goto-bus-stop/get-artist-title
    public static class StringExtensions
    {
        #region Fields

        private const string OPEN_CHARS = "([{«";
        private const string CLOSE_CHARS = ")]}»";
        private const string TOGGLE_CHARS = "\"'";

        private static readonly string[] Separators =
        {
            " -- ",
            "--",
            " - ",
            " – ",
            " — ",
            " _ ",
            "-",
            "–",
            "—",
            ":",
            "|",
            "///",
            " / ",
            "_",
            "/"
        };

        private static readonly List<ICleaner> InputCleaners = new List<ICleaner>
        {
            new FluffCleaner()
        };

        private static readonly List<ICleaner> TitleCleaners = new List<ICleaner>
        {
            new FluffCleaner(),
            new TitleCleaner()
        };

        private static readonly List<ICleaner> ArtistCleaners = new List<ICleaner>
        {
            new FluffCleaner(),
            new ArtistCleaner()
        };

        #endregion

        #region Public Methods

        /// <summary>
        ///     Get an <see cref="ArtistTitle" /> for a given string.
        /// </summary>
        /// <param name="input">The string that contains the song and artist (a YouTube video title for example).</param>
        /// <param name="fallbackArtist">The artist to fallback to if no artist can be found.</param>
        /// <param name="fallbackTitle">The title to fallback to if no title can be found.</param>
        /// <returns>The <see cref="ArtistTitle" /> for the given input.</returns>
        public static ArtistTitle GetArtistTitle(this string input, string fallbackArtist = "Unknown Artist",
                                                 string fallbackTitle = "Unknown Song")
        {
            // Clean with input cleaners
            InputCleaners.ForEach(cleaner => input = cleaner.Clean(input));


            // Artist - Title
            string[] artistTitle = input.SplitArtistTitle();
            if (artistTitle == null) return new ArtistTitle(fallbackArtist, fallbackTitle);

            if (string.IsNullOrEmpty(artistTitle[0])) artistTitle[0] = fallbackArtist;

            if (string.IsNullOrEmpty(artistTitle[1])) artistTitle[1] = fallbackTitle;


            string artist = artistTitle[0];
            ArtistCleaners.ForEach(cleaner => artist = cleaner.Clean(artist));

            string title = artistTitle[1];
            TitleCleaners.ForEach(cleaner => title = cleaner.Clean(title));

            return new ArtistTitle(artist, title);
        }

        public static string[] SplitArtistTitle(this string input)
        {
            foreach (string sep in Separators)
            {
                int index = input.IndexOf(sep, StringComparison.Ordinal);
                if (index > -1 && !input.InQuotes(index))
                    return new[] { input.Substring(0, index), input.Substring(index + sep.Length) };
            }

            return null;
        }

        public static string RegexReplace(this string input, string regex, string replacement)
        {
            return Regex.Replace(input, regex, replacement, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public static string ReplaceAmpersand(this string input)
        {
            int index;
            while ((index = input.IndexOf('&')) > -1)
            {
                input = input.Remove(index, 1);
                input = input.Insert(index, "and");
            }
            return input;
        }

        public static bool InQuotes(this string input, int index)
        {
            Dictionary<char, int> open = new Dictionary<char, int>
            {
                { ')', 0 },
                { ']', 0 },
                { '}', 0 },
                { '»', 0 },
                { '"', 0 },
                { '\'', 0 }
            };
            for (var i = 0; i < index; i++)
            {
                int openIndex = OPEN_CHARS.IndexOf(input[i]);

                if (openIndex != -1)
                    open[CLOSE_CHARS[openIndex]]++;
                else if (CLOSE_CHARS.IndexOf(input[i]) != -1 && open[input[i]] > 0) open[input[i]]--;

                if (TOGGLE_CHARS.IndexOf(input[i]) != -1) open[input[i]] = 1 - open[input[i]];

            }

            return open.Keys.Aggregate(0, (acc, k) => acc + open[k]) > 0;
        }

        #endregion
    }
}