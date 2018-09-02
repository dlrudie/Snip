

namespace Winter.ArtistTitle
{
    internal class FluffCleaner : ICleaner
    {
        #region Public Properties

        /// <inheritdoc />
        public int Priority => 0;

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public string Clean(string input)
        {
            return CleanMVPV(input)
                   .RegexReplace(@"\s*\[[^\]]+]$", "")
                   .RegexReplace(@"^\s*\[[^\]]+]\s*", "")
                   .RegexReplace(@"\s*\([^)]*\bver(\.|sion)?\s*\)$", "")
                   .RegexReplace(@"\s*[a-z]*\s*\bver(\.|sion)?$", "")
                   .RegexReplace(@"\s*(of+icial\s*)?(music\s*)?video", "")
                   .RegexReplace(@"\s*(ALBUM TRACK\s*)?(album track\s*)", "")
                   .RegexReplace(@"\s*\(\s*of+icial\s*\)", "")
                   .RegexReplace(@"\s*\(\s*[0-9]{4}\s*\)", "")
                   .RegexReplace(@"\s+\(\s*(HD|HQ|[0-9]{3,4}p|4K)\s*\)$", "")
                   .RegexReplace(@"[\s\-–_]+(HD|HQ|[0-9]{3,4}p|4K)\s*$", "")
                   .RegexReplace(@"^[\w\s!@#$%^&*()~\-+]+\s\W\W\s", ""); // badstuff!@$ABC || Artist - Title

        }

        #endregion

        #region Private Methods

        private string CleanMVPV(string input)
        {
            return input
                   .RegexReplace(@"\s*\[\s*(?:off?icial\s+)?([PM]\/?V)\s*]", "")
                   .RegexReplace(@"\s*\(\s*(?:off?icial\s+)?([PM]\/?V)\s*\)", "")
                   .RegexReplace(@"\s*【\s*(?:off?icial\s+)?([PM]\/?V)\s*】", "")
                   .RegexReplace(@"[\s\-–_]+(?:off?icial\s+)?([PM]\/?V)\s*", "")
                   .RegexReplace(@"(?:off?icial\s+)?([PM]\/?V)[\s\-–_]+", "");

        }

        #endregion
    }
}