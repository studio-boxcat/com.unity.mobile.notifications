#if UNITY_ANDROID || UNITY_EDITOR
using System;

namespace Unity.Notifications.Android
{
    internal static class AndroidNotificationExtensions
    {
        public static long ToLong(this DateTime dateTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = dateTime.ToUniversalTime() - origin;

            return (long)Math.Floor(diff.TotalMilliseconds);
        }

        public static DateTime ToDatetime(this long dateTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddMilliseconds(dateTime).ToLocalTime();
        }
    }
}
#endif
