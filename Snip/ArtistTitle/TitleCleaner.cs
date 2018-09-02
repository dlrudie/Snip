

namespace Winter.ArtistTitle
{
    internal class TitleCleaner : ICleaner
    {
        #region Public Properties

        /// <inheritdoc />
        public int Priority => 1;

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public string Clean(string input)
        {
            return input
                   .RegexReplace(@"\s*\*+\s?\S+\s?\*+$", "")
                   .RegexReplace(@"\s*video\s*clip", "")
                   .RegexReplace(@"\s+\(?live\)?$", "")
                   .RegexReplace(@"\(\s*\)", "")
                   .RegexReplace(@"\[\s*\]", "")
                   .RegexReplace(@"【\s*】", "")
                   .RegexReplace(@"^(|.*\s)""(.*)""(\s.*|)$", "$2")
                   .RegexReplace(@"^(|.*\s)'(.*)'(\s.*|)$", "$2")
                   .RegexReplace(@"^[/\s,:;~\-–_\s""]+", "")
                   .RegexReplace(@"[/\s,:;~\-–_\s""]+$", "");
        }

        #endregion
    }
}