namespace Winter.ArtistTitle
{
    internal class ArtistCleaner : ICleaner
    {
        #region Public Properties

        /// <inheritdoc />
        public int Priority => 2;

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public string Clean(string input)
        {
            return input
                   .RegexReplace(@"\s*[0-1][0-9][0-1][0-9][0-3][0-9]\s*", "")
                   .RegexReplace(@"^[/\s,:;~\-–_\s""]+", "")
                   .RegexReplace(@"[/\s,:;~\-–_\s""]+$", "");
        }

        #endregion
    }
}