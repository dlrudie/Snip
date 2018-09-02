namespace Winter.ArtistTitle
{
    internal interface ICleaner
    {
        #region Public Properties

        int Priority { get; }

        #endregion

        #region Public Methods

        string Clean(string input);

        #endregion
    }
}