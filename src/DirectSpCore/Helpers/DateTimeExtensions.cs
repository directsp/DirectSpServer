using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Helpers
{
    public static class DateTimeExtensions
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static readonly double MaxUnixSeconds = (DateTime.MaxValue - UnixEpoch).TotalSeconds;

        public static double ToUnixDate(this DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
          UnixEpoch).TotalSeconds;
        }

        public static DateTime FromUnixDate(this DateTime dateTime, double unixDate)
        {
            return unixDate > MaxUnixSeconds
      ? UnixEpoch.AddMilliseconds(unixDate)
      : UnixEpoch.AddSeconds(unixDate);
        }
    }
}
