#if UNITY_ANDROID || UNITY_EDITOR
using UnityEngine;

namespace Unity.Notifications.Android
{
    /// <summary>
    /// Wrapper for the AndroidNotification. Contains the notification's id and channel.
    /// </summary>
    public class AndroidNotificationIntentData
    {
        /// <summary>
        /// The id of the notification.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The channel id that the notification was sent to.
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// Returns the  proxy to the Android Java instance of Notification class.
        /// </summary>
        public AndroidJavaObject NativeNotification { get; internal set; }

        /// <summary>
        /// Create an AndroidNotificationIntentData with AndroidNotification, id, and channel id.
        /// </summary>
        /// <param name="id">Notification id</param>
        /// <param name="channelId">ID of the notification channel</param>
        public AndroidNotificationIntentData(int id, string channelId)
        {
            Id = id;
            Channel = channelId;
        }
    }
}
#endif
