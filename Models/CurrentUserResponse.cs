namespace UserAuthApi.Models;

/// <summary>User details returned for the authenticated user page (first name, last name, email only).</summary>
public class CurrentUserResponse
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
}
