using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SporeCommunity.ModBrowser.GithubApi
{
    class GithubApiException : Exception
    {
        internal GithubApiException(string message, GithubApiException innerException) : base(message, innerException)
        {
            HttpResponseMessage = innerException.HttpResponseMessage;
        }

        /// <summary>
        /// Creates an appropriate GithubApiException for the specified GitHub API HTTP response.
        /// </summary>
        /// <param name="response">The GitHub API HTTP response.</param>
        /// <returns>An exception describing the API error, or null if the response was successful.</returns>
        internal GithubApiException(HttpResponseMessage response)
        {
            HttpResponseMessage = response;

            // Make sure there was actually an API error
            if (response.IsSuccessStatusCode)
            {
                throw new ArgumentException("GitHub API response was successful.", this);
            }

            // Check if rate limit exceeded
            else if (IsRateLimitExceeded)
            {
                throw new GithubApiException($"GitHub rate limit exceeded. The current limit is {RateLimit} and it resets at {RateLimitReset}. Try again in {SecondsUntilRateLimitReset} seconds.", this);
            }

            // Throw exception with message
            else
            {
                throw new GithubApiException($"GitHub API error {StatusCode}: {GetApiError().Result}", this);

            }
        }

        public HttpResponseMessage HttpResponseMessage { get; }

        public HttpStatusCode StatusCode => HttpResponseMessage.StatusCode;

        public bool IsRateLimitExceeded => RateLimitRemaining == 0;

        /// <summary>
        /// The current rate limit for the GitHUb API.
        /// </summary>
        public int RateLimit => int.Parse(HttpResponseMessage.Headers.GetValues("x-ratelimit-limit").First());

        /// <summary>
        /// The current number of requests that can be made before the rate limit is exceeded. 
        /// </summary>
        public int RateLimitRemaining => int.Parse(HttpResponseMessage.Headers.GetValues("x-ratelimit-remaining").First());

        /// <summary>
        /// The time at which the rate limit will reset.
        /// </summary>
        public DateTime RateLimitReset => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(HttpResponseMessage.Headers.GetValues("x-ratelimit-reset").First())).ToLocalTime();

        /// <summary>
        /// The number of seconds remaining until the rate limit resets.
        /// </summary>
        public int SecondsUntilRateLimitReset
        {
            get
            {
                var remainingTime = DateTime.Now.Subtract(RateLimitReset);
                var remainingSeconds = -Math.Ceiling(remainingTime.TotalSeconds);
                return (int)remainingSeconds;
            }
        }

        /// <summary>
        /// Gets the string content of the GitHub API response.
        /// </summary>
        public async Task<string> GetContent()
        {
            return await HttpResponseMessage.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Gets the JSON content of the GitHub API response.
        /// </summary>
        public async Task<JObject> GetJsonContent()
        {
            return JObject.Parse(await GetContent());
        }

        public async Task<string> GetApiError()
        {
            var json = await GetJsonContent();
            return (string)json["message"]!;
        }
    }
}
