using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CinemaApp.Api.Endpoints.Shared;
using CinemaApp.Application.Commands.BuySeats;
using CinemaApp.Application.Commands.ConfirmReservation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Api.Endpoints
{
    public class ConfirmReservationEndpoint : EndpointBase<ConfirmReservationRequest, ConfirmReservationResponse>
    {
        private readonly ISender _mediator;
        
        public ConfirmReservationEndpoint(ILoggerFactory loggerFactory, ISender mediator) : base(loggerFactory)
        {
            _mediator = mediator;
        }

        protected override Http HttpVerb => Http.PUT;
        protected override HttpStatusCode SuccessStatusCode => HttpStatusCode.OK;
        protected override string GetRoute() => EndpointRoutes.TicketConfirmEndpoint;
        protected override string GetSummary() => "Confirms reservation";
        protected override Task<ConfirmReservationResponse> ProtectedHandleAsync(ConfirmReservationRequest req, CancellationToken ct)
        {
            return _mediator.Send(req, ct);
        }

    }
}
