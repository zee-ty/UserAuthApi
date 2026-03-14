namespace UserAuthApi.Options;

// messages for error responses, set in appsettings ErrorMessages section
public class ErrorMessageOptions
{
    public const string SectionName = "ErrorMessages";

    public string RequestBodyRequired { get; set; } = "Request body is required";
    public string UserAlreadyExists { get; set; } = "User already exists";
    public string InvalidEmailOrPassword { get; set; } = "Invalid email or password";
    public string InvalidToken { get; set; } = "Invalid or missing token";
    public string UserNotFound { get; set; } = "User not found";
    public string ValidationFailed { get; set; } = "Validation failed";
}
