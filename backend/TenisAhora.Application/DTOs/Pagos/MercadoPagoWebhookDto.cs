using System.Text.Json.Serialization;

namespace TenisAhora.Application.DTOs.Pagos;

public class MercadoPagoWebhookDto
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("data")]
    public MercadoPagoWebhookDataDto? Data { get; set; }
}

public class MercadoPagoWebhookDataDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
