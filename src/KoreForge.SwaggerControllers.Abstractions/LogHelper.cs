using System.Collections;
using Microsoft.Extensions.Logging;

namespace KoreForge.SwaggerControllers;

public static class LogHelper
{
    public static void LogExceptionStructured(
        this ILogger logger,
        Exception ex,
        string className,
        string methodName,
        string operationKey,
        string correlationId,
        Dictionary<string, object?>? additionalFields = null)
    {
        var exceptionData = new Dictionary<string, object?>();
        if (ex.Data.Count > 0)
        {
            foreach (DictionaryEntry entry in ex.Data)
            {
                exceptionData[entry.Key?.ToString() ?? "unknown"] = entry.Value;
            }
        }

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["ClassName"] = className,
            ["MethodName"] = methodName,
            ["OperationKey"] = operationKey,
            ["CorrelationId"] = correlationId,
            ["ExceptionType"] = ex.GetType().FullName,
            ["ExceptionData"] = exceptionData.Count > 0 ? exceptionData : null,
            ["StackTrace"] = ex.StackTrace,
            ["InnerExceptionMessage"] = ex.InnerException?.Message
        }))
        {
            if (additionalFields is { Count: > 0 })
            {
                using (logger.BeginScope(additionalFields))
                {
                    logger.LogError(ex,
                        "[{CorrelationId}] {ClassName}.{MethodName} {OperationKey} threw {ExceptionType}: {ExceptionMessage}",
                        correlationId, className, methodName, operationKey,
                        ex.GetType().Name, ex.Message);
                }
            }
            else
            {
                logger.LogError(ex,
                    "[{CorrelationId}] {ClassName}.{MethodName} {OperationKey} threw {ExceptionType}: {ExceptionMessage}",
                    correlationId, className, methodName, operationKey,
                    ex.GetType().Name, ex.Message);
            }
        }
    }

    public static void LogOutcomeStructured<T>(
        this ILogger logger,
        Outcome<T> outcome,
        string className,
        string methodName,
        string operationKey)
    {
        if (outcome.Success)
        {
            logger.LogInformation(
                "[{CorrelationId}] {ClassName}.{MethodName} {OperationKey} completed successfully",
                outcome.CorrelationId, className, methodName, operationKey);
        }
        else
        {
            var error = outcome.Error!;
            logger.LogWarning(
                "[{CorrelationId}] {ClassName}.{MethodName} {OperationKey} failed: [{ErrorKind}] {ErrorCode} - {ErrorMessage}",
                outcome.CorrelationId, className, methodName, operationKey,
                error.Kind, error.Code, error.Message);
        }
    }
}
