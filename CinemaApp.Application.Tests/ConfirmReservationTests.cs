using CinemaApp.Application.Commands.BuySeats;
using CinemaApp.Application.Commands.ConfirmReservation;
using CinemaApp.Core.Exceptions;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace CinemaApp.Application.Tests
{
    public class ConfirmReservationTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        
        private readonly static Guid ValidReservationId = new Guid("00000000-0000-0000-0000-000000000001");
        private readonly static Guid InvalidReservationId = new Guid("00000000-0000-0000-0000-000000000002");
        private readonly static Guid PaidReservationId = new Guid("00000000-0000-0000-0000-000000000003");
        private readonly static Guid ExpiredReservationId = new Guid("00000000-0000-0000-0000-000000000003");
        
        private const int ShowtimeId = 123;
        
        public ConfirmReservationTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x["SeatReservation:ValidPeriodInMinutes"]).Returns("10");
            
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            SetupRepoMock();
        }
        
        private void SetupRepoMock()
        {
            var unpaidTicket = new TicketEntity
            {
                Id = ValidReservationId,
                Paid = false,
                ShowtimeId = ShowtimeId
            };
            var paidTicket = new TicketEntity
            {
                Id = PaidReservationId,
                Paid = true,
                ShowtimeId = ShowtimeId
            };
            var expiredTicket = new TicketEntity
            {
                Id = ExpiredReservationId,
                Paid = false,
                ShowtimeId = ShowtimeId,
                CreatedTime = DateTime.Now.AddDays(-1)
            };
            
            // mocking GetAsync method:
            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(ValidReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unpaidTicket);
            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(PaidReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paidTicket);
            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(ExpiredReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredTicket);
            
            // mocking ConfirmPaymentAsync method:
            _ticketsRepositoryMock
                .Setup(x => x.ConfirmPaymentAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TicketEntity ticket, CancellationToken ct) => ticket);
        }
        
        [Fact]
        public async Task ConfirmReservation_WhenReservationIsValid_ShouldConfirmReservation()
        {
            ConfirmReservationResponse response = await TryConfirmReservation(ValidReservationId);

            Assert.True(response.Success);
            Assert.True(response.RequestValid);
        }
        
        [Fact]
        public Task ConfirmReservation_WhenReservationIsValid_ShouldFail()
        {
            return Assert.ThrowsAsync<EntityNotFoundException>(async () => await TryConfirmReservation(InvalidReservationId));
        }
        
        [Fact]
        public Task ConfirmReservation_WhenReservationIsPaid_ShouldFail()
        {
            return Assert.ThrowsAsync<ValidationException>(async () => await TryConfirmReservation(PaidReservationId));
        }
        
        [Fact]
        public Task ConfirmReservation_WhenReservationIsExpired_ShouldFail()
        {
            return Assert.ThrowsAsync<ValidationException>(async () => await TryConfirmReservation(ExpiredReservationId));
        }
        
        private async Task<ConfirmReservationResponse> TryConfirmReservation(Guid? reservationId)
        {
            var validator = new ConfirmReservationValidator();
            var handler = new ConfirmReservationHandler(validator, _ticketsRepositoryMock.Object, _configurationMock.Object);
            
            return await handler.Handle(new ConfirmReservationRequest
                {
                    ReservationId = reservationId
                }, CancellationToken.None);
        }
    }
}
