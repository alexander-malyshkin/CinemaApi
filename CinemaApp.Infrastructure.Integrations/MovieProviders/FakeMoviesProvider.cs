using CinemaApp.Core.DTO;
using CinemaApp.Core.ServiceContracts;

namespace CinemaApp.Infrastructure.Integrations.MovieProviders
{
    public class FakeMoviesProvider : IMoviesProvider
    {
        private readonly Random _random = new ();

        public async Task<MovieDto?> GetMovieById(string movieId, CancellationToken ct)
        {
            // awaiting a random amount of time to simulate a network delay
            await SimulateDelay(ct);

            // in some cases we throw an exception to simulate a faulty provider
            ThrowExceptionSometimes();

            // in some cases we return null to simulate a missing movie
            if (_random.NextDouble() > 0.9)
            {
                return null;
            }

            return new MovieDto
            {
                Id = movieId,
                Title = "some title",
                Year = "2012",
                ImDbRating = "some rating",
                Crew = "some stars"
            };
        }

        public async Task<ICollection<MovieDto>> SearchMovies(string search, CancellationToken ct)
        {
            // awaiting a random amount of time to simulate a network delay
            await SimulateDelay(ct);

            // in some cases we throw an exception to simulate a faulty provider
            ThrowExceptionSometimes();

            // in some cases we return an empty collection to simulate no results
            if (_random.NextDouble() > 0.9)
            {
                return Array.Empty<MovieDto>();
            }

            return new List<MovieDto>
            {
                new MovieDto
                {
                    Id = "Movie 1",
                    Title = "some title 1",
                    Year = "2015",
                    ImDbRating = "some rating 1",
                    Crew = "some stars 1"
                },
                new MovieDto
                {
                    Id = "Movie 2",
                    Title = "some title 2",
                    Year = "2016",
                    ImDbRating = "some rating 2",
                    Crew = "some stars 2"
                }
            };
        }

        private void ThrowExceptionSometimes()
        {
            // throw exceptions often to simulate a faulty provider
            if (_random.NextDouble() > 0.4)
            {
                throw new HttpProtocolException(12345, "Faulty provider", null);
            }
        }
        private async Task SimulateDelay(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(7000)), ct);
        }
    }
}
