using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CinemaApp.Api.Endpoints.Shared;
using CinemaApp.Application.Commands.ReserveSeats;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Api.Endpoints
{
    public sealed class ReserveSeatsEndpoint : EndpointBase<ReserveSeatsRequest, ReserveSeatsResponse>
    {
        private readonly ISender _mediator;
        
        public ReserveSeatsEndpoint(ILoggerFactory loggerFactory, ISender mediator) : base(loggerFactory)
        {
            _mediator = mediator;
        }

        protected override Http HttpVerb => Http.POST;
        protected override HttpStatusCode SuccessStatusCode => HttpStatusCode.Created;
        protected override string GetRoute() => EndpointRoutes.TicketCreateEndpoint;
        protected override string GetSummary() => "Reserves seats";
        protected override Task<ReserveSeatsResponse> ProtectedHandleAsync(ReserveSeatsRequest req, CancellationToken ct)
        {
            return _mediator.Send(req, ct);
        }

    }
}
