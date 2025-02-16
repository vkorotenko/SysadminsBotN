﻿using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SysadminsBot
{
    public class DeepSeekResult
    {
        private string _apikey = "";
        public DeepSeekResult(string apiKey)
        {
            _apikey = apiKey;
        }

        public async Task<string?> Reply(string body)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepseek.com/chat/completions");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {_apikey}");
            var prep =
                "{\n  \"messages\": [\n    {\n      \"content\": \"MSG\",\n      \"role\": \"system\"\n    },\n    {\n      \"content\": \"Hi\",\n      \"role\": \"user\"\n    }\n  ],\n  \"model\": \"deepseek-chat\",\n  \"frequency_penalty\": 0,\n  \"max_tokens\": 2048,\n  \"presence_penalty\": 0,\n  \"response_format\": {\n    \"type\": \"text\"\n  },\n  \"stop\": null,\n  \"stream\": false,\n  \"stream_options\": null,\n  \"temperature\": 1,\n  \"top_p\": 1,\n  \"tools\": null,\n  \"tool_choice\": \"none\",\n  \"logprobs\": false,\n  \"top_logprobs\": null\n}";
            body = body.Replace("\n", "\\n").Replace("\r", "\\r");
            prep = prep.Replace("MSG", body);
            var content = new StringContent(prep, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return await response.Content.ReadAsStringAsync();
        }
    }
    public class ApiResponse
    {
        public string Result { get; set; }
    }

    public class ReqModel
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "deepseek-r1:7b";
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = "";
        [JsonPropertyName("stream")]
        public bool Stream = false;
    }
    public class LocalDeepSeek
    {
        public async Task<string?> Reply(string body)
        {
            var chat = "http://localhost:11434/api/chat";
            var generate = "http://localhost:11434/api/generate";
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 4, 0);
            var request = new HttpRequestMessage(HttpMethod.Post, generate);
            // request.Headers.Add("Accept", "application/json");
            var model = new ReqModel { Prompt = body ,Stream = false};
            var prep = JsonSerializer.Serialize(model);
            var content = new StringContent(prep, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return await response.Content.ReadAsStringAsync();
        }
    }
}
