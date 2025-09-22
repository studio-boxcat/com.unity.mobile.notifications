#if UNITY_ANDROID || UNITY_EDITOR
using System;
using UnityEngine;

namespace Unity.Notifications.Android
{
    /// <summary>
    /// Allows applying a rich notification style to a notification.
    /// </summary>
    public enum NotificationStyle
    {
        /// <summary>
        /// Use the default style.
        /// </summary>
        None = 0,

        /// <summary>
        /// Generate a large-format notification centered around an image.
        /// </summary>
        BigPictureStyle = 1,

        /// <summary>
        /// Generate a large-format notification that includes a lot of text.
        /// </summary>
        BigTextStyle = 2
    }

    /// <summary>
    /// Data for setting up the big picture style notification.
    /// Properties that are not available in devices API level are ignored. See Android documentation for availibility.
    /// <see href="https://developer.android.com/reference/android/app/Notification.BigPictureStyle"/>
    /// </summary>
    public struct BigPictureStyle
    {
        /// <summary>
        /// The override for large icon (requirements are the same).
        /// </summary>
        /// <see cref="AndroidNotification.LargeIcon"/>
        public string LargeIcon { get; set; }

        /// <summary>
        /// The picture to be displayed.
        /// Can be resource name (like icon), file path or an URI supported by Android.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// The content title to be displayed in the notification.
        /// </summary>
        public string ContentTitle { get; set; }

        /// <summary>
        /// The content description to set.
        /// </summary>
        public string ContentDescription { get; set; }

        /// <summary>
        /// The summary text to be shown.
        /// </summary>
        public string SummaryText { get; set; }

        /// <summary>
        /// Whether to show big picture in place of large icon when collapsed.
        /// </summary>
        public bool ShowWhenCollapsed { get; set; }
    }

    /// <summary>
    /// The AndroidNotification is used schedule a local notification, which includes the content of the notification.
    /// </summary>
    public struct AndroidNotification
    {
        /// <summary>
        /// Notification title.
        /// Set the first line of text in the notification.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Notification body.
        /// Set the second line of text in the notification.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Notification small icon.
        /// It will be used to represent the notification in the status bar and content view (unless overridden there by a large icon)
        /// The icon has to be registered in Notification Settings or a PNG file has to be placed in the `res/drawable` folder of the Android library plugin
        /// and it's name has to be specified without the extension.
        /// Alternatively it can also be URI supported by the OS.
        /// </summary>
        public string SmallIcon { get; set; }

        /// <summary>
        /// The date and time when the notification should be delivered.
        /// </summary>
        public DateTime FireTime { get; set; }

        /// <summary>
        /// Notification large icon.
        /// Add a large icon to the notification content view. This image will be shown on the left of the notification view in place of the small icon (which will be placed in a small badge atop the large icon).
        /// The icon has to be registered in Notification Settings or a PNG file has to be placed in the `res/drawable` folder of the Android library plugin
        /// and it's name has to be specified without the extension.
        /// Alternatively it can be a file path or system supported URI.
        /// </summary>
        public string LargeIcon { get; set; }

        /// <summary>
        /// Apply a custom style to the notification.
        /// Currently only BigPicture and BigText styles are supported.
        /// </summary>
        public NotificationStyle Style { get; set; }

        /// <summary>
        /// Use this to save arbitrary string data related to the notification.
        /// </summary>
        public string IntentData { get; set; }

        /// <summary>
        /// Enable it to show a timestamp on the notification when it's delivered, unless the "CustomTimestamp" property is set "FireTime" will be shown.
        /// </summary>
        public bool ShowTimestamp { get; set; }

        /// <summary>
        /// Set this to show custom date instead of the notification's "FireTime" as the notification's timestamp'.
        /// </summary>
        public DateTime CustomTimestamp
        {
            get { return m_CustomTimestamp; }
            set
            {
                ShowCustomTimestamp = true;
                m_CustomTimestamp = value;
            }
        }

        /// <summary>
        /// Set this notification to be shown when app is in the foreground (default: true).
        /// </summary>
        public bool ShowInForeground
        {
            get => !m_SilentInForeground;
            set => m_SilentInForeground = !value;
        }

        /// <summary>
        /// The necessary properties for big picture style notification.
        /// For convenience, assigning this property will also set the Style property.
        /// </summary>
        public BigPictureStyle? BigPicture
        {
            get { return m_BigPictureStyle; }
            set
            {
                m_BigPictureStyle = value;
                if (m_BigPictureStyle.HasValue && Style == NotificationStyle.None)
                    Style = NotificationStyle.BigPictureStyle;
                else if (!m_BigPictureStyle.HasValue && Style == NotificationStyle.BigPictureStyle)
                    Style = NotificationStyle.None;
            }
        }

        internal bool ShowCustomTimestamp { get; set; }

        private DateTime m_CustomTimestamp;
        private bool m_SilentInForeground;
        private BigPictureStyle? m_BigPictureStyle;

        /// <summary>
        /// Create a notification struct with all optional fields set to default values.
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="text">Text to show on notification</param>
        /// <param name="fireTime">Date and time when to show, can be DateTime.Now to show right away</param>
        public AndroidNotification(string title, string text, DateTime fireTime)
        {
            Title = title;
            Text = text;
            FireTime = fireTime;

            SmallIcon = string.Empty;
            LargeIcon = string.Empty;
            Style = NotificationStyle.None;
            IntentData = string.Empty;
            ShowTimestamp = false;
            ShowCustomTimestamp = false;
            m_BigPictureStyle = null;

            m_CustomTimestamp = (-1L).ToDatetime();
            m_SilentInForeground = false;
        }

        /// <summary>
        /// Create a notification struct with a custom small icon and all optional fields set to default values.
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="text">Text to show on notification</param>
        /// <param name="fireTime">Date and time when to show, can be DateTime.Now to show right away</param>
        /// <param name="smallIcon">Name of the small icon to be shown on notification</param>
        public AndroidNotification(string title, string text, DateTime fireTime, string smallIcon)
            : this(title, text, fireTime)
        {
            SmallIcon = smallIcon;
        }
    }
}
#endif