using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using ManagedWindowsFunctions;
using Winter.ArtistTitle;

namespace Winter.Players
{
    internal sealed class YouTube : MediaPlayer
    {
        private const string AUDIO_PLAYING_STR = " – Audio playing";
        private const string AUDIO_PLAYING_YOUTUBE_STR = "YouTube" + AUDIO_PLAYING_STR;
        private const string PROCESS_NAME = "chrome";

        private readonly Regex _titleProviderRegex = new Regex("(.*?)\\s-\\s([\\S\\w]+)" + AUDIO_PLAYING_STR, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private List<string> TabTitles
        {
            get
            {
                // Basic caching
                if (_lastUpdated == DateTime.MinValue || _lastUpdated.AddSeconds(30) < DateTime.Now)
                {
                    // Update tabs
                    _tabTitles = GetTabsOf(PROCESS_NAME);
                    _lastUpdated = DateTime.Now;
                }
                return _tabTitles;
            }
        }

        private List<string> _tabTitles;
        private DateTime _lastUpdated;

        /// <inheritdoc />
        public override void Update()
        {
            var tab = TabTitles.FirstOrDefault(t => t.EndsWith(AUDIO_PLAYING_YOUTUBE_STR) && _titleProviderRegex.IsMatch(t));

            if (tab == null)
            {
                // No tabs playing audio.
                if (Globals.SaveAlbumArtwork)
                {
                    this.SaveBlankImage();
                }

                TextHandler.UpdateTextAndEmptyFilesMaybe(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        LocalizedMessages.NoTrackPlaying,
                        LocalizedMessages.YouTube));
                return;
            }

            Match songMatch = _titleProviderRegex.Match(tab);

            string songTitle = songMatch.Groups[1].Value;
            if (songTitle == this.LastTitle && !Globals.RewriteUpdatedOutputFormat)
            {
                return;
            }

            Globals.RewriteUpdatedOutputFormat = false;
            this.LastTitle = songTitle;

            var artistTitle = songTitle.GetArtistTitle(null, null);

            if (artistTitle.Artist != null && artistTitle.Title != null)
            {
                UpdateTrackText(artistTitle.Title, artistTitle.Artist);
            }
            else
            {
                UpdateTrackText(songTitle);
            }
        }

        private void UpdateTrackText(string title, string artist = "")
        {
            title = title.ReplaceAmpersand();
            artist = artist.ReplaceAmpersand();

            if (string.IsNullOrEmpty(artist))
            {

                TextHandler.UpdateText(title);
            }
            else
            {
                TextHandler.UpdateText(title, artist);
            }
        }

        private List<string> GetTabsOf(string processName)
        {
            List<string> ret = new List<string>();
            var procs = Process.GetProcessesByName(processName).Where(p => !string.IsNullOrEmpty(p.MainWindowTitle));
            foreach (Process p in procs)
            {
                IntPtr[] processWindows = ApiHelper.GetProcessWindows(p.Id);

                foreach (IntPtr handle in processWindows)
                {
                    try
                    {
                        // This is CPU intensive.
                        AutomationElement root = AutomationElement.FromHandle(handle);
                        Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, "New Tab");
                        AutomationElement elmNewTab = root.FindFirst(TreeScope.Descendants, condNewTab);

                        if (elmNewTab == null)
                        {
                            continue;
                        }

                        TreeWalker treewalker = TreeWalker.ControlViewWalker;
                        AutomationElement elmTabStrip = treewalker.GetParent(elmNewTab);

                        Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
                        ret.AddRange(elmTabStrip.FindAll(TreeScope.Children, condTabItem).Cast<AutomationElement>().Select(x => x.Current.Name));
                        
                        // Instead of continuing throughout the rest of the processes, stop after we have found a tab playing audio
                        // and matching title provider regex.
                        if (ret.FirstOrDefault(tab => tab.EndsWith(AUDIO_PLAYING_YOUTUBE_STR) && _titleProviderRegex.IsMatch(tab)) != null)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Error while finding tab: " + ex);
                    }
                }
            }
            return ret;
        }
    }
}
