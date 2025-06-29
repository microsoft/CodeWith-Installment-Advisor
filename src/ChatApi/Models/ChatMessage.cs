﻿using Newtonsoft.Json;

namespace InstallmentAdvisor.ChatApi.Models
{
    public record ChatMessage
    {
        [JsonProperty(PropertyName = "id")]
        public required string Id { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public required string UserId { get; set; }

        [JsonProperty(PropertyName = "threadId")]
        public required string ThreadId { get; set; }

        [JsonProperty(PropertyName = "role")]
        public required string Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public required string Content { get; set; }

        [JsonProperty(PropertyName = "created")]
        public required DateTime Created { get; set; }

    }
}
