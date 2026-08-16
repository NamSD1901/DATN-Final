using System;
using System.Text.Json.Serialization;

namespace MyPetClinic.Application.DTOs
{
    public class SePayWebhookDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("gateway")]
        public string? Gateway { get; set; }

        [JsonPropertyName("transactionDate")]
        public string? TransactionDate { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("subAccount")]
        public string? SubAccount { get; set; }

        [JsonPropertyName("amountIn")]
        public decimal AmountIn { get; set; }

        [JsonPropertyName("amountOut")]
        public decimal AmountOut { get; set; }

        [JsonPropertyName("accumulated")]
        public decimal Accumulated { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("transactionContent")]
        public string? TransactionContent { get; set; }

        [JsonPropertyName("referenceNumber")]
        public string? ReferenceNumber { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }
}
