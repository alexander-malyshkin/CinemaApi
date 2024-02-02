using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CinemaApp.Api.Endpoints.Shared;
using CinemaApp.Application.Commands.CreateShowtime;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Api.Endpoints
{
    public sealed class CreateShowtimeEndpoint : EndpointBase<CreateShowtimeRequest, CreateShowtimeResponse>
    {
        private readonly ISender _mediator;
        public CreateShowtimeEndpoint(ILoggerFactory loggerFactory, ISender mediator) : base(loggerFactory)
        {
            _mediator = mediator;
        }

        protected override Http HttpVerb => Http.POST;
        protected override HttpStatusCode SuccessStatusCode => HttpStatusCode.Created;
        protected override string GetRoute() => EndpointRoutes.CreateShowtimeEndpoint;
        protected override string GetSummary() => "Creates a showtime";

        protected override Task<CreateShowtimeResponse> ProtectedHandleAsync(CreateShowtimeRequest req, CancellationToken ct)
        {
            return _mediator.Send(req, ct);
        }
    }
}
