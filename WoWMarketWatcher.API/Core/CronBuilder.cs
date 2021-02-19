using System;

namespace WoWMarketWatcher.API.Core
{
    public static class CronBuilder
    {
        public static string AtEveryXHour(int everyXHour, int atMinute = 0)
        {
            if (everyXHour <= 0 || everyXHour > 23)
            {
                throw new ArgumentException($"{nameof(everyXHour)} must be greater than 0 and less than 24.");
            }

            if (atMinute < 0 || atMinute > 59)
            {
                throw new ArgumentException($"{nameof(atMinute)} must be greater than or equal to 0 and less than 60.");
            }

            return $"{atMinute} */{everyXHour} * * *";
        }
    }
}