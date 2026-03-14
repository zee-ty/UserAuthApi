using System.Text.Json.Serialization;

namespace UserAuthApi.Models;

public class HealthResponse
{
    public HealthStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthStatus
{
    healthy,
    unhealthy
}
