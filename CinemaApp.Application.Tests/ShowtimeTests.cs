using CinemaApp.Application.Commands.CreateShowtime;
using CinemaApp.Core.DTO;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Core.ServiceContracts;
using FluentValidation;
using Moq;
using Xunit;

namespace CinemaApp.Application.Tests;

public sealed class ShowtimeTests
{
    private readonly Mock<IShowtimesRepository> _showtimesRepositoryMock;
    private readonly Mock<IMoviesProvider> _moviesProviderMock;
    
    public ShowtimeTests()
    {
        _showtimesRepositoryMock = new Mock<IShowtimesRepository>();
        SetupShowtimesRepositoryMock();
        
        _moviesProviderMock = new Mock<IMoviesProvider>();
        SetupMoviesProviderMock();
    }
    private void SetupShowtimesRepositoryMock()
    {
        _showtimesRepositoryMock.Setup(repo => repo.CreateShowtime(It.IsAny<ShowtimeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShowtimeEntity showtime, CancellationToken ct) =>
            {
                showtime.Id = 1;
                return showtime;
            });
    }
    
    private void SetupMoviesProviderMock()
    {
        _moviesProviderMock.Setup(provider => provider.GetMovieById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken ct) =>
            {
                var rnd = new Random();
                
                // 50% chance to return null
                if (rnd.Next(0, 2) > 1)
                    return null;
                
                return new MovieDto
                {
                    Id = id,
                    ImDbRating = "some rating",
                    Title = "Some title",
                    Crew = "Some crew",
                    Year = "2021"
                };
            });        
    }

    [Theory]
    [InlineData(0, "1")]
    [InlineData(1, "")]
    public async Task CreateShowtime_InvalidInputs_CauseValidationError(int auditoriumId, string movieId)
    {
        var req = new CreateShowtimeRequest
        {
            AuditoriumId = auditoriumId,
            SessionDate = new DateTime(2021,1,1),
            MovieId = movieId
        };

        IValidator<CreateShowtimeRequest> validator = new CreateShowtimeValidator();
        var handler = new CreateShowtimeHandler(_showtimesRepositoryMock.Object, _moviesProviderMock.Object, validator);
        CreateShowtimeResponse response = await handler.Handle(req, CancellationToken.None);

        if (auditoriumId <= 0 || string.IsNullOrEmpty(movieId))
        {
            Assert.False(response.Success);
            Assert.False(response.RequestValid);
            Assert.NotNull(response.Title);
            Assert.NotEmpty(response.Title);
            Assert.NotNull(response.Details);
            Assert.NotEmpty(response.Details);
        }
        else
        {
            Assert.True(response.Success);
        }
    }
    
    [Fact]
    public async Task CreateShowtime_ValidInputs_ReturnsSuccess()
    {
        var req = new CreateShowtimeRequest
        {
            AuditoriumId = 1,
            SessionDate = new DateTime(2021,1,1),
            MovieId = "1"
        };

        IValidator<CreateShowtimeRequest> validator = new CreateShowtimeValidator();
        var handler = new CreateShowtimeHandler(_showtimesRepositoryMock.Object, _moviesProviderMock.Object, validator);
        CreateShowtimeResponse response = await handler.Handle(req, CancellationToken.None);

        Assert.True(response.Success);
        Assert.True(response.RequestValid);
        Assert.True(response.ShowtimeId > 0);
    }
}
