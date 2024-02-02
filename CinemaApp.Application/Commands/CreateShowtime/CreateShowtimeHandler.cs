using CinemaApp.Application.Shared;
using CinemaApp.Core.DTO;
using CinemaApp.Core.Exceptions;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Core.ServiceContracts;
using FluentValidation;

namespace CinemaApp.Application.Commands.CreateShowtime
{
    public class CreateShowtimeHandler : HandlerBase<CreateShowtimeRequest, CreateShowtimeResponse>
    {
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly IMoviesProvider _moviesProvider;
        
        public CreateShowtimeHandler(IShowtimesRepository showtimesRepository, IMoviesProvider moviesProvider, IValidator<CreateShowtimeRequest> validator)
            : base(validator)
        {
            _showtimesRepository = showtimesRepository;
            _moviesProvider = moviesProvider;
        }

        protected override CreateShowtimeResponse ToResponse(ResponseBase errorResponse)
        {
            return new CreateShowtimeResponse(0, errorResponse.Success, errorResponse.Title, errorResponse.Details, errorResponse.RequestValid);
        }
        protected async override Task<CreateShowtimeResponse> HandleInternal(CreateShowtimeRequest request, CancellationToken ct)
        {
            MovieDto? foundMovie = await _moviesProvider.GetMovieById(request.MovieId, ct);
            if (foundMovie is null)
                throw new EntityNotFoundException(nameof(request.MovieId), "Movie not found");
            
            var showtime = new ShowtimeEntity
            {
                AuditoriumId = request.AuditoriumId,
                Movie = ConvertMovie(foundMovie),
                SessionDate = request.SessionDate!.Value,
                Tickets = new List<TicketEntity>()
            };
            
            ShowtimeEntity createdShowtime = await _showtimesRepository.CreateShowtime(showtime, ct);
            return new CreateShowtimeResponse(createdShowtime.Id, true);
        }
        private MovieEntity ConvertMovie(MovieDto foundMovie)
        {
            int releaseYear = int.TryParse(foundMovie.Year, out releaseYear) ? releaseYear : 1970;
            return new MovieEntity
            {
                ImdbId = foundMovie.ImDbRating,
                ReleaseDate = new DateTime(releaseYear, 1, 1),
                Stars = foundMovie.Crew,
                Title = foundMovie.Title
            };
        }
    }
}