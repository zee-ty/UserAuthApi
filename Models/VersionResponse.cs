using System.Text.Json.Serialization;

namespace UserAuthApi.Models;

public class VersionResponse
{
    public string Version { get; set; } = "";
    public EnvironmentKind Environment { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnvironmentKind
{
    dev,
    uat,
    prod
}
