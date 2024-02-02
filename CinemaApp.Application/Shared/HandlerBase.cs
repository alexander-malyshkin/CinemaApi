using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CinemaApp.Application.Shared
{
    public abstract class HandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
        where TResponse : ResponseBase
    {
        private const string ValidationFailed = "Invalid request";
        private readonly IValidator<TRequest> _validator;
        
        protected HandlerBase(IValidator<TRequest> validator)
        {
            _validator = validator;
        }
        
        public async Task<TResponse> Handle(TRequest request, CancellationToken ct)
        {
            ValidationResult? validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
                return ProduceValidationErrorResponse(validationResult);
            
            return await HandleInternal(request, ct);
        }
        
        private TResponse ProduceValidationErrorResponse(ValidationResult validationResult)
        {
            var details = string.Join(". ", validationResult.Errors.Select(x => x.ErrorMessage));
            var errorResponse = new ResponseBase(false, ValidationFailed, details, validationResult.IsValid);
            return ToResponse(errorResponse);
        }
        protected abstract TResponse ToResponse(ResponseBase errorResponse);
        protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken ct);
    }
}
