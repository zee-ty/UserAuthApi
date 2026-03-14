using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserAuthApi.Models;
using UserAuthApi.Options;

namespace UserAuthApi.Swagger;

// adds example error responses in swagger using the messages from config
public class ErrorResponseExampleFilter : IOperationFilter
{
    private readonly ErrorMessageOptions _messages;

    public ErrorResponseExampleFilter(IOptions<ErrorMessageOptions> messages)
    {
        _messages = messages.Value;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var response in operation.Responses)
        {
            if (!int.TryParse(response.Key, out var statusCode))
                continue;

            var content = response.Value.Content;
            if (content == null || !content.ContainsKey("application/json"))
                continue;

            var mediaType = content["application/json"];
            if (mediaType.Schema?.Reference?.Id != nameof(ErrorResponse))
                continue;

            var (example, examples) = GetExamplesForStatus(statusCode);
            if (example != null)
                mediaType.Example = example;
            if (examples != null)
                mediaType.Examples = examples;
        }
    }

    private (OpenApiObject? Example, IDictionary<string, OpenApiExample>? Examples) GetExamplesForStatus(int statusCode)
    {
        switch (statusCode)
        {
            case 400:
                return (new OpenApiObject
                {
                    ["status"] = new OpenApiInteger(400),
                    ["errors"] = new OpenApiArray { new OpenApiString(_messages.ValidationFailed) }
                }, null);
            case 401:
                return (null, new Dictionary<string, OpenApiExample>
                {
                    ["login"] = new OpenApiExample { Value = new OpenApiObject { ["status"] = new OpenApiInteger(401), ["errors"] = new OpenApiArray { new OpenApiString(_messages.InvalidEmailOrPassword) } }, Summary = "Invalid email or password (e.g. login)" },
                    ["getUser"] = new OpenApiExample { Value = new OpenApiObject { ["status"] = new OpenApiInteger(401), ["errors"] = new OpenApiArray { new OpenApiString(_messages.InvalidToken) } }, Summary = "Invalid or missing token (e.g. GET /user)" }
                });
            case 404:
                return (new OpenApiObject
                {
                    ["status"] = new OpenApiInteger(404),
                    ["errors"] = new OpenApiArray { new OpenApiString(_messages.UserNotFound) }
                }, null);
            case 409:
                return (new OpenApiObject
                {
                    ["status"] = new OpenApiInteger(409),
                    ["errors"] = new OpenApiArray { new OpenApiString(_messages.UserAlreadyExists) }
                }, null);
            default:
                return (null, null);
        }
    }
}
