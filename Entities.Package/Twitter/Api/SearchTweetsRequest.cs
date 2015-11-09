using System;
using System.Text;

namespace Entities.Twitter.Api
{
    public class SearchTweetsRequest
    {
        public string Query { get; set; }
        public Geocode Geocode { get; set; }
        public string Language { get; set; }
        public string Locale { get; set; }
        public ResultType ResultType { get; set; } = ResultType.Mixed;
        public uint Count { get; set; } = 15;
        public DateTime? Until { get; set; }
        public long? SinceId { get; set; }

        public string GetQueryString()
        {
            var queryStringBuilder = new StringBuilder($"q={ Query }");

            //queryStringBuilder.Append($"&result_type={ ResultType.ToString().ToLower() }");
            queryStringBuilder.Append($"&count={ Math.Min(Count, 100) }");

            if (Geocode != null)
                queryStringBuilder.Append($"&geocode={ Geocode }");
            
            if (!string.IsNullOrWhiteSpace(Language))
                queryStringBuilder.Append($"&lang={ Language }");

            if (!string.IsNullOrWhiteSpace(Locale))
                queryStringBuilder.Append($"&locale={ Locale }");

            if (Until.HasValue)
                queryStringBuilder.Append($"&until={ Uri.EscapeDataString(Until.Value.ToString("yyyy-MM-dd")) }");

            if (SinceId.HasValue)
                queryStringBuilder.Append($"&since_id={ SinceId.Value }");
            
            return queryStringBuilder.ToString();
        }
    }
}
