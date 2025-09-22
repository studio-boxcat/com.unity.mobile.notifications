#if UNITY_ANDROID || UNITY_EDITOR
using System;

namespace Unity.Notifications.Android
{
    /// <summary>
    /// The level of interruption of this notification channel.
    /// The importance of a notification is used to determine how much the notification should interrupt the user (visually and audibly).
    /// The higher the importance of a notification, the more interruptive the notification will be.
    /// </summary>
    /// <remarks>
    /// The exact behaviour of each importance level might vary depending on the device and OS version on devices running Android 7.1 or older.
    /// </remarks>
    public enum Importance
    {
        /// <summary>
        /// A notification with no importance: does not show in the shade.
        /// </summary>
        None = 0,

        /// <summary>
        /// Low importance, notification is shown everywhere, but is not intrusive.
        /// </summary>
        Low = 2,

        /// <summary>
        /// Default importance, notification is shown everywhere, makes noise, but does not intrude visually.
        /// </summary>
        Default = 3,

        /// <summary>
        /// High importance, notification is shown everywhere, makes noise and is shown on the screen.
        /// </summary>
        High = 4,
    }

    /// <summary>
    /// Determines whether notifications appear on the lock screen.
    /// </summary>
    public enum LockScreenVisibility
    {
        /// <summary>
        /// Do not reveal any part of this notification on a secure lock screen.
        /// </summary>
        Secret = -1,

        /// <summary>
        /// Show this notification on all lock screens, but conceal sensitive or private information on secure lock screens.
        /// </summary>
        Private = 0,

        /// <summary>
        /// Show this notification in its entirety on the lock screen.
        /// </summary>
        Public = 1,
    }

    /// <summary>
    /// The wrapper of the Android notification channel. Use this to group notifications by groups.
    /// </summary>
    public struct AndroidNotificationChannel
    {
        /// <summary>
        /// Notification channel identifier.
        /// Must be specified when scheduling notifications.
        /// </summary>
        public string Id { get; set; }
    }

    /// <summary>
    /// Notification channel group description.
    /// It is optional to put channels into groups, but looks nicer in Settings UI.
    /// </summary>
    public struct AndroidNotificationChannelGroup
    {
        /// <summary>
        /// A unique ID for this group. Will rename the group if already exists.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A user visible name for this group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description for this group.
        /// </summary>
        public string Description { get; set; }
    }
}
#endif
