namespace Winter.ArtistTitle
{
    public class ArtistTitle
    {
        #region Public Properties

        public string Artist { get; }
        public string Title { get; }

        #endregion

        #region Constructors

        public ArtistTitle(string artist, string title)
        {
            Artist = artist;
            Title = title;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Artist} - {Title}";
        }

        #endregion
    }
}