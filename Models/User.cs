using System.Text.Json.Serialization;

namespace UserAuthApi.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    [JsonIgnore]  // dont send to client
    public string PasswordHash { get; set; } = "";
}
