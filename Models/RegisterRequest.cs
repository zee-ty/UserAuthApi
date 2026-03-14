using System.ComponentModel.DataAnnotations;

namespace UserAuthApi.Models;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Must be a valid email address")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "First name is required")]
    [MinLength(1, ErrorMessage = "First name must not be empty")]
    [StringLength(200)]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last name is required")]
    [MinLength(1, ErrorMessage = "Last name must not be empty")]
    [StringLength(200)]
    public string LastName { get; set; } = "";
}
