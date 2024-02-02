using CinemaApp.Application.Commands.ReserveSeats;
using CinemaApp.Application.Commands.ReserveSeats.Validation;
using CinemaApp.Core.Exceptions;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace CinemaApp.Application.Tests
{
    public sealed class ReserveSeatsTests
    {
        private readonly Mock<ITicketsRepository> _ticketsRepoMock;
        private readonly Mock<IShowtimesRepository> _showtimesRepoMock;
        private readonly Mock<IAuditoriumsRepository> _auditoriumsRepoMock;
        private readonly Mock<IConfiguration> _configurationMock;
        
        private const int NonExistentShowtimeId = 12345;

        private const int ShowtimeIdWithBookedSeats = 1;
        private const int BookedSeatRow = 3;
        private const int BookedSeatNumber = 5;
        private const int BookedAuditoriumId = 9;
        
        private const int ShowtimeIdWithExpiredReservation = 2;
        
        public ReserveSeatsTests()
        {
            _ticketsRepoMock = new Mock<ITicketsRepository>();
            SetupTicketsRepoMock();
            _showtimesRepoMock = new Mock<IShowtimesRepository>();
            SetupShowtimesRepoMock();
            _auditoriumsRepoMock = new Mock<IAuditoriumsRepository>();
            SetupAuditoriumsRepoMock();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.SetupGet(config => config[It.IsAny<string>()]).Returns("10");
        }
        private void SetupAuditoriumsRepoMock()
        {
            // mocking GetAsync method:
            _auditoriumsRepoMock.Setup(repo => 
                    repo.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                {
                    return new AuditoriumEntity
                    {
                        Id = id,
                        Seats = GenerateSeats(id, 10, 35)
                    };
                });
        }
        
        private static List<SeatEntity> GenerateSeats(int auditoriumId, short rows, short seatsPerRow)
        {
            var seats = new List<SeatEntity>();
            for (short r = 1; r <= rows; r++)
            for (short s = 1; s <= seatsPerRow; s++)
                seats.Add(new SeatEntity { AuditoriumId = auditoriumId, Row = r, SeatNumber = s });

            return seats;
        }
        
        private void SetupShowtimesRepoMock()
        {
            _showtimesRepoMock.Setup(repo => 
                    repo.GetWithTicketsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                {
                    if (id == NonExistentShowtimeId)
                        return null;

                    var showtimeEntity = new ShowtimeEntity
                    {
                        Id = id,
                        AuditoriumId = BookedAuditoriumId + 1,
                        Movie = new MovieEntity
                        {
                            Id = 1,
                            Title = "test title"
                        },
                        Tickets = new List<TicketEntity>()
                    };

                    switch (id)
                    {
                        case ShowtimeIdWithBookedSeats:
                            showtimeEntity.AuditoriumId = BookedAuditoriumId;
                            showtimeEntity.Tickets = new List<TicketEntity>
                            {
                                CreateTicketWithBookedSeats(BookedSeatRow, BookedSeatNumber, false)
                            };
                            break;
                        
                        case ShowtimeIdWithExpiredReservation:
                            showtimeEntity.AuditoriumId = BookedAuditoriumId;
                            showtimeEntity.Tickets = new List<TicketEntity>
                            {
                                CreateTicketWithBookedSeats(BookedSeatRow, BookedSeatNumber, true)
                            };
                            break;
                    }

                    return showtimeEntity;
                });
        }
        private static TicketEntity CreateTicketWithBookedSeats(short seatRow, short seatNumber, bool ticketExpired)
        {
            return new TicketEntity
            {
                ShowtimeId = ShowtimeIdWithBookedSeats,
                CreatedTime = ticketExpired ? DateTime.Now.AddDays(-1) : DateTime.Now,
                Seats = new List<SeatEntity>
                {
                    new SeatEntity {Row = seatRow, SeatNumber = seatNumber, AuditoriumId = BookedAuditoriumId}
                }
            };
        }
        
        private void SetupTicketsRepoMock()
        {
            _ticketsRepoMock.Setup(repo => 
                    repo.CreateAsync(It.IsAny<ShowtimeEntity>(), It.IsAny<IEnumerable<SeatEntity>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShowtimeEntity showtime, IEnumerable<SeatEntity> seats, CancellationToken ct) =>
                {
                    return new TicketEntity
                    {
                        Id = Guid.NewGuid(),
                        ShowtimeId = showtime.Id,
                        Seats = seats.ToList()
                    };
                });
        }
        [Fact]
        public async Task ShouldNotReserveSeats_WithNoSeats()
        {
            var reserveSeatsRequest = new ReserveSeatsRequest
            {
                ShowtimeId = 1,
                Seats = new List<RowSeat>()
            };
            ReserveSeatsResponse reservationResponse = await TryReserveSeats(reserveSeatsRequest);

            Assert.False(reservationResponse.RequestValid);
        }

        [Fact]
        public async Task ShouldNotReserveSeats_WithInvalidShowtimeId()
        {
            var reserveSeatsRequest = new ReserveSeatsRequest
            {
                ShowtimeId = 0,
                Seats = new List<RowSeat>
                {
                    new RowSeat {Row = 1, SeatNumber = 1}
                }
            };
            ReserveSeatsResponse reservationResponse = await TryReserveSeats(reserveSeatsRequest);

            Assert.False(reservationResponse.RequestValid);
        }
        
        [Fact]
        public async Task ShouldNotReserveSeats_WithNonContiguousSeats()
        {
            var reserveSeatsRequest = new ReserveSeatsRequest
            {
                ShowtimeId = 1,
                Seats = new List<RowSeat>
                {
                    new RowSeat {Row = 1, SeatNumber = 1},
                    new RowSeat {Row = 1, SeatNumber = 3}
                }
            };
            ReserveSeatsResponse reservationResponse = await TryReserveSeats(reserveSeatsRequest);

            Assert.False(reservationResponse.RequestValid);
        }

        [Theory]
        [InlineData(NonExistentShowtimeId)]
        [InlineData(NonExistentShowtimeId + 1)]
        public async Task ReserveSeats_ValidateShowtimeExistence(int showtimeId)
        {
            var reserveSeatsRequest = new ReserveSeatsRequest
            {
                ShowtimeId = showtimeId,
                Seats = new List<RowSeat>
                {
                    new RowSeat {Row = 1, SeatNumber = 1},
                    new RowSeat {Row = 1, SeatNumber = 2}
                }
            };

            if (showtimeId == NonExistentShowtimeId)
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(async () => await TryReserveSeats(reserveSeatsRequest));
            }
            else
            {
                ReserveSeatsResponse reservationResponse = await TryReserveSeats(reserveSeatsRequest);
                Assert.True(reservationResponse.Success);
            }
        }

        [Theory]
        [InlineData(BookedSeatRow, BookedSeatNumber)]
        [InlineData(BookedSeatRow + 1, BookedSeatNumber + 1)]
        public async Task ReserveSeats_CheckConflictingReservations(short row, short seatNumber)
        {
            var reserveSeatsRequest = new ReserveSeatsRequest
            {
                ShowtimeId = ShowtimeIdWithBookedSeats,
                Seats = new List<RowSeat>
                {
                    new RowSeat {Row = row, SeatNumber = seatNumber},
                }
            };

            if (row == BookedSeatRow && seatNumber == BookedSeatNumber)
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await TryReserveSeats(reserveSeatsRequest));
            }
            else
            {
                ReserveSeatsResponse reservationResponse = await TryReserveSeats(reserveSeatsRequest);
                Assert.True(reservationResponse.Success);
            }
        }

        [Fact]
        public async Task ReserveSeats_ShouldWork_ForExpiredReservations()
        {
            var reserveSeatsRequest = new ReserveSeatsRequest
            {
                ShowtimeId = ShowtimeIdWithExpiredReservation,
                Seats = new List<RowSeat>
                {
                    new RowSeat {Row = BookedSeatRow, SeatNumber = BookedSeatNumber},
                }
            };
            
            ReserveSeatsResponse reservationResponse = await TryReserveSeats(reserveSeatsRequest);
            Assert.True(reservationResponse.Success);
        }
        
        // TODO: add a test for checking that auditorium exists
        // TODO: add a test for checking that seats exist in the auditorium
        
        private Task<ReserveSeatsResponse> TryReserveSeats(ReserveSeatsRequest reserveSeatsRequest)
        {
            IValidator<ReserveSeatsRequest> validator = new ReserveSeatsValidator();
            var handler = new ReserveSeatsHandler(validator, _ticketsRepoMock.Object, _showtimesRepoMock.Object,
                _auditoriumsRepoMock.Object, _configurationMock.Object);

            return handler.Handle(reserveSeatsRequest, CancellationToken.None);
        }
    }
}
