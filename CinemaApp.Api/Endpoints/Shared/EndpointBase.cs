using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CinemaApp.Application.Shared;
using CinemaApp.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Api.Endpoints.Shared
{
    public abstract class EndpointBase<TRequest, TResponse> : Endpoint<TRequest, TResponse> 
        where TRequest : notnull
        where TResponse : ResponseBase
    {
        private const string HandlingStartedFormat = "Endpoint {endpoint} started handling request: {request}";
        private const string HandlingEndedFormat = "Endpoint {endpoint} handled the request and produced the following response: {response}";
        private const string FailedToHandleRequest = "Endpoint {endpoint} failed to handle the request: {request}";

        private const string InternalServerErrorTitle = nameof(HttpStatusCode.InternalServerError);
        private const string InternalServerErrorDetails = "An unexpected server error occurred.";
        private const string DefaultNotFoundMessage = "Entity not found";
        
        private readonly ILogger _logger;
        private readonly string _handlerName;

        protected EndpointBase(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _handlerName = GetType().Name;
        }
        
        public sealed override void Configure()
        {
            Verbs(HttpVerb);
            Routes(GetRoute());
            DefinePermissions();
            Summary(s =>
            {
                s.Summary = GetSummary();
                s.Description = GetDescription();
            });
        }

        protected abstract Http HttpVerb { get; }

        protected virtual void DefinePermissions()
        {
            AllowAnonymous();
        }
        protected virtual string GetDescription() => GetSummary();
        protected abstract string GetSummary();
        protected abstract string GetRoute();
        protected abstract Task<TResponse> ProtectedHandleAsync(TRequest req, CancellationToken ct);
        protected abstract HttpStatusCode SuccessStatusCode { get; }
        
        public sealed async override Task HandleAsync(TRequest req, CancellationToken ct)
        {
            try
            {
                _logger.LogTrace(HandlingStartedFormat, _handlerName, req);

                TResponse response = await ProtectedHandleAsync(req, ct);

                if (!response.RequestValid)
                {
                    await SendErrorResponse(response.Title, response.Details, HttpStatusCode.BadRequest, ct);
                }
                else if (StatusCodeWritable(SuccessStatusCode))
                {
                    await SendAsync(response, (int)SuccessStatusCode, cancellation: ct);
                }

                _logger.LogTrace(HandlingEndedFormat, _handlerName, response);
            }
            catch (ValidationException e)
            {
                _logger.LogWarning(e, e.Message);
                await SendErrorResponse(nameof(HttpStatusCode.BadRequest), e.Message, HttpStatusCode.BadRequest, ct);
            }
            catch (Exception e)
                when (TryRetrieveEntityNotFoundException(e) is not null)
            {
                _logger.LogWarning(e, e.Message);
                await SendErrorResponse(nameof(HttpStatusCode.NotFound), RetrieveEntityNotFoundExceptionMessage(e),
                    HttpStatusCode.NotFound, ct);
            }
            catch (Exception e)
            {
                _logger.LogError(e, FailedToHandleRequest, _handlerName, req);
                await SendErrorResponse(InternalServerErrorTitle, InternalServerErrorDetails, HttpStatusCode.InternalServerError, ct);
            }
        }
        
        private Task SendErrorResponse(string title, string details, HttpStatusCode errorCode, CancellationToken ct)
        {
            HttpContext.MarkResponseStart();
            HttpContext.Response.StatusCode = (int)errorCode;
            HttpContext.Response.ContentType = "application/json";
            HttpContext.Response.WriteAsJsonAsync(new ResponseBase(false, title, details, false), ct);
            return HttpContext.Response.StartAsync(ct);
        }
        
        private static bool StatusCodeWritable(HttpStatusCode code)
        {
            return code != HttpStatusCode.NoContent;
        }
        
        private string RetrieveEntityNotFoundExceptionMessage(Exception e)
        {
            EntityNotFoundException? entityNotFoundException = TryRetrieveEntityNotFoundException(e);
            if (entityNotFoundException is null)
            {
                _logger.LogError("Could not parse exception as EntityNotFoundException: {exception}", e);
                return DefaultNotFoundMessage;
            }

            return entityNotFoundException.Message;
        }
        
        private static EntityNotFoundException? TryRetrieveEntityNotFoundException(Exception e)
        {
            if (e is EntityNotFoundException notFoundException)
            {
                return notFoundException;
            }

            if (e is AggregateException aggregateException)
            {
                AggregateException flattenedAggrExc = aggregateException.Flatten();

                return (EntityNotFoundException?)flattenedAggrExc.InnerExceptions
                    .FirstOrDefault(ie => ie is EntityNotFoundException);
            }

            return null;
        }
    }
}
