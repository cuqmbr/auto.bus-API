using System.Text.Json.Serialization;

namespace SharedModels.Responses;

public class ResponseBase
{
    public string? Message { get; set; }
}