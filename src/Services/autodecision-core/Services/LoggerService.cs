using AutodecisionCore.DTOs;
using Microsoft.AspNetCore.Http.Extensions;

namespace AutodecisionCore.Services;

public interface ILoggerService
{
    void LogInfo(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    );
    void LogWarning(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    );
    void LogError(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    );
    void LogDebug(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    );
}

public class LoggerService : ILoggerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LoggerService> _logger;
    private readonly IConfiguration _configuration;

    public LoggerService(
        ILogger<LoggerService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void LogInfo(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    )
    {
        Log(eventType, message, extraData, LogLevel.Information, "Info", request);
    }

    public void LogWarning(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    )
    {
        Log(eventType, message, extraData, LogLevel.Warning, "Warning", request);
    }

    public void LogError(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    )
    {
        Log(eventType, message, extraData, LogLevel.Error, "Error", request);
    }

    public void LogDebug(
        string eventType,
        string message,
        object? extraData = null,
        LogRequest? request = null
    )
    {
        Log(eventType, message, extraData, LogLevel.Debug, "Debug", request);
    }

    private void Log(
        string eventType,
        string message,
        object? extraData = null,
        LogLevel logSeverity = LogLevel.Debug,
        string severity = "Debug",
        LogRequest? logRequest = null
    )
    {
        var projectId = _configuration["GCP:ProjectId"];
        if (string.IsNullOrEmpty(projectId))
            throw new ArgumentNullException("Google Cloud Project ID is null. See docs at: ###");

        var projectVersion = _configuration["Project:Version"];
        if (string.IsNullOrEmpty(projectId))
            throw new ArgumentNullException("Project Version is null. See docs at: ###");

        var projectName = _configuration["Project:Name"];
        if (string.IsNullOrEmpty(projectId))
            throw new ArgumentNullException("Project Name is null. See docs at: ###");

        var payload = new
        {
            source = projectName,
            severity = severity,
            eventType = eventType,
            timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff"),
            requestId = GetRequestId(),
            request = logRequest ?? GetRequest(),
            configs = new
            {
                sourceVersion = projectVersion,
                messageVersion = "v0.1",
                runtime = new { language = "NET", version = "5.0" }
            },
            attributes = new { message = message, extra = extraData }
        };

        var payloadString = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        _logger.Log(logSeverity, message);
        _logger.Log(logSeverity, payloadString);
    }

    private string? GetRequestId()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["x-request-id"].ToString();
        }
        catch
        {
            return null;
        }
    }

    private LogRequest? GetRequest()
    {
        try
        {
            return new AutodecisionCore.DTOs.LogRequest()
            {
                Url = _httpContextAccessor.HttpContext?.Request?.GetDisplayUrl(),
                Protocol = _httpContextAccessor.HttpContext?.Request?.Protocol,
                Method = _httpContextAccessor.HttpContext?.Request?.Method
            };
        }
        catch
        {
            return null;
        }
    }
}
