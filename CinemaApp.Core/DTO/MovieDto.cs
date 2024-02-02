namespace CinemaApp.Core.DTO
{
    public class MovieDto
    {
        public string Id { get; set; }
        public string Rank { get; set; }
        public string Title { get; set; }
        public string FullTitle { get; set; }
        public string Year { get; set; }
        public string Image { get; set; }
        public string Crew { get; set; }
        public string ImDbRating { get; set; }
        public string ImDbRatingCount { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Year);
    }
}
