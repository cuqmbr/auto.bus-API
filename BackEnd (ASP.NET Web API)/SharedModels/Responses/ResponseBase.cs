using System.Text.Json.Serialization;

namespace SharedModels.Responses;

public class ResponseBase
{
    [JsonInclude]
    public string? Message;
}