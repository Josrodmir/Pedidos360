using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pedidos360.Web.Filters;

/// <summary>
/// Converts transient Azure SQL availability failures into a recoverable response.
/// </summary>
public sealed class DatabaseUnavailableFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (!IsDatabaseUnavailable(context.Exception))
        {
            return Task.CompletedTask;
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Result = new ViewResult
        {
            ViewName = "~/Views/Shared/DatabaseUnavailable.cshtml"
        };
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }

    private static bool IsDatabaseUnavailable(Exception exception) =>
        exception is RetryLimitExceededException ||
        exception is SqlException { Number: 40613 or -2 } ||
        exception.InnerException is not null && IsDatabaseUnavailable(exception.InnerException);
}
