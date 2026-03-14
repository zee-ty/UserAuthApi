namespace UserAuthApi.Models;

public class ErrorResponse
{
    public int Status { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}
