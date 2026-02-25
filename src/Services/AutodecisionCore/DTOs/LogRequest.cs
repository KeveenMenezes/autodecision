using Newtonsoft.Json;

namespace AutodecisionCore.DTOs;

public class LogRequest
{
    [JsonProperty(propertyName: "url")]
    public string? Url { get; set; }

    [JsonProperty(propertyName: "protocol")]
    public string? Protocol { get; set; }

    [JsonProperty(propertyName: "method")]
    public string? Method { get; set; }

    [JsonProperty(propertyName: "body")]
    public string? Body { get; set; }

    [JsonProperty(propertyName: "topic")]
    public string? Topic { get; set; }

    [JsonProperty(propertyName: "grpcClient")]
    public string? GrpcClient { get; set; }
}
