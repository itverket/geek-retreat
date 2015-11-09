using System;

namespace Entities.Twitter.RateLimiting
{
    public class RateLimit
    {
        private long _reset;

        public int Limit { get; set; }

        public int Remaining { get; set; }

        public long Reset
        {
            get { return _reset; }
            set
            {
                _reset = value;

                ResetDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                ResetDateTime = ResetDateTime.AddSeconds(_reset).ToLocalTime();
            }
        }

        public DateTime ResetDateTime { get; private set; }

        public int MillisecondsToReset() => (int) (ResetDateTime - DateTime.Now).TotalMilliseconds;
    }
}