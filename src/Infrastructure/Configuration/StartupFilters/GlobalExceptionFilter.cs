using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Configuration.StartupFilters;

public class GlobalExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        base.OnException(context);
        var resultObject = new
        {
            ExeptionType = context.Exception.GetType().FullName,
            Message = context.Exception.Message
        };

        _logger.LogError(context.Exception, context.Exception.Message);

        var jsonResult = new JsonResult(resultObject)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.Result = jsonResult;
    }
}