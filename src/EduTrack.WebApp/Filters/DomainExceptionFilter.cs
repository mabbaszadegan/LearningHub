using EduTrack.Application.Common.Exceptions;
using EduTrack.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduTrack.WebApp.Filters;

/// <summary>
/// Global exception filter for handling domain exceptions
/// </summary>
public class DomainExceptionFilter : IExceptionFilter
{
    private readonly ILogger<DomainExceptionFilter> _logger;

    public DomainExceptionFilter(ILogger<DomainExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        switch (exception)
        {
            case EntityNotFoundException entityNotFound:
                _logger.LogWarning("Entity not found: {EntityType} with ID: {EntityId}", 
                    entityNotFound.EntityType, entityNotFound.EntityId);
                context.Result = new NotFoundObjectResult(Result<string>.Failure($"Entity not found: {entityNotFound.Message}"));
                break;

            case ValidationException validation:
                _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", validation.Errors));
                context.Result = new BadRequestObjectResult(Result<string>.Failure(string.Join(", ", validation.Errors)));
                break;

            case BusinessRuleViolationException businessRule:
                _logger.LogWarning("Business rule violated: {Message}", businessRule.Message);
                context.Result = new BadRequestObjectResult(Result<string>.Failure(businessRule.Message));
                break;

            case OperationNotAllowedException operationNotAllowed:
                _logger.LogWarning("Operation not allowed: {Message}", operationNotAllowed.Message);
                context.Result = new ForbidResult();
                break;

            case ConcurrencyException concurrency:
                _logger.LogWarning("Concurrency conflict: {Message}", concurrency.Message);
                context.Result = new ConflictObjectResult(Result<string>.Failure(concurrency.Message));
                break;

            case DomainException domainException:
                _logger.LogError(domainException, "Domain exception occurred: {Message}", domainException.Message);
                context.Result = new BadRequestObjectResult(Result<string>.Failure(domainException.Message));
                break;

            default:
                _logger.LogError(exception, "Unhandled exception occurred");
                context.Result = new ObjectResult(Result<string>.Failure("An unexpected error occurred"))
                {
                    StatusCode = 500
                };
                break;
        }

        context.ExceptionHandled = true;
    }
}
