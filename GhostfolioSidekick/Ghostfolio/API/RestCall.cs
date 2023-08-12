﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RestSharp;
using System;

namespace GhostfolioSidekick.Ghostfolio.API
{
    public class RestCall
    {
        private static int _maxRetryAttempts = 5;
        private static TimeSpan _pauseBetweenFailures = TimeSpan.FromSeconds(10);

        private readonly IMemoryCache memoryCache;
        private readonly string url;
        private readonly string accessToken;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        private readonly RetryPolicy<RestResponse> retryPolicy;

        public RestCall(
            IMemoryCache memoryCache,
            ILogger<GhostfolioAPI> logger,
            string url,
            string accessToken)
        {
            this.memoryCache = memoryCache;
            this.url = url;
            this.accessToken = accessToken;

            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            retryPolicy = Policy
               .HandleResult<RestResponse>(x => !x.IsSuccessful)
               .WaitAndRetry(_maxRetryAttempts, x => _pauseBetweenFailures, async (iRestResponse, timeSpan, retryCount, context) =>
               {
                   logger.LogWarning($"The request failed. HttpStatusCode={iRestResponse.Result.StatusCode}. Waiting {timeSpan} seconds before retry. Number attempt {retryCount}. Uri={iRestResponse.Result.ResponseUri};");
               });
        }

        public async Task<string?> DoRestGet(string suffixUrl)
        {
            if (memoryCache.TryGetValue<string?>(suffixUrl, out var result))
            {
                return result;
            }

            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = false,
                ThrowOnDeserializationError = false
            };

            var client = new RestClient(options);
            var request = new RestRequest($"{url}/{suffixUrl}")
            {
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("Authorization", $"Bearer {await GetAuthenticationToken()}");
            request.AddHeader("Content-Type", "application/json");

            var r = retryPolicy.Execute(() => client.ExecuteGetAsync(request).Result);

            if (!r.IsSuccessStatusCode)
            {
                throw new NotSupportedException($"Error executing url [{r.StatusCode}]: {url}/{suffixUrl}");
            }

            memoryCache.Set(suffixUrl, r.Content, cacheEntryOptions);

            return r.Content;
        }

        internal async Task<RestResponse?> DoRestPost(string suffixUrl, string body)
        {
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = false,
                ThrowOnDeserializationError = false
            };

            var client = new RestClient(options);
            var request = new RestRequest($"{url}/{suffixUrl}")
            {
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("Authorization", $"Bearer {await GetAuthenticationToken()}");
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(body);
            var r = retryPolicy.Execute(() => client.ExecutePostAsync(request).Result);

            if (!r.IsSuccessStatusCode)
            {
                throw new NotSupportedException($"Error executing url [{r.StatusCode}]: {url}/{suffixUrl}");
            }

            return r;
        }

        private async Task<string> GetAuthenticationToken()
        {
            using (var client = new HttpClient())
            {
                string requestUri = $"{url}/api/v1/auth/anonymous/{accessToken}";
                var content = await client.GetStringAsync(requestUri);

                dynamic stuff = JsonConvert.DeserializeObject(content);
                var token = stuff.authToken.ToString();
                return token;
            }
        }
    }
}