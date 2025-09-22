#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using JniMethodID = System.IntPtr;
using JniFieldID = System.IntPtr;

namespace Unity.Notifications.Android
{
    /// <summary>
    /// Current status of a scheduled notification, can be queried using CheckScheduledNotificationStatus.
    /// </summary>
    public enum NotificationStatus
    {
        /// <summary>
        /// Status of a specified notification cannot be determined. This is only supported on Android Marshmallow (6.0) and above.
        /// </summary>
        Unavailable = -1,

        /// <summary>
        /// A notification with a specified id could not be found.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A notification with a specified id is scheduled but not yet delivered.
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// A notification with a specified id was already delivered (showing in status bar).
        /// </summary>
        Delivered = 2,
    }

    struct NotificationManagerJni
    {
        private AndroidJavaClass klass;
        private AndroidJavaObject self;

        public AndroidJavaObject KEY_FIRE_TIME;
        public AndroidJavaObject KEY_ID;
        public AndroidJavaObject KEY_INTENT_DATA;
        public AndroidJavaObject KEY_LARGE_ICON;
        public AndroidJavaObject KEY_SMALL_ICON;
        public AndroidJavaObject KEY_SHOW_IN_FOREGROUND;

        private JniMethodID getNotificationFromIntent;
        private JniMethodID setNotificationIcon;
        private JniMethodID getNotificationChannelId;
        private JniMethodID scheduleNotification;
        private JniMethodID createNotificationBuilder;


        public NotificationManagerJni(AndroidJavaClass clazz, AndroidJavaObject obj)
        {
            klass = clazz;
            self = obj;

            getNotificationFromIntent = default;
            setNotificationIcon = default;
            getNotificationChannelId = default;
            scheduleNotification = default;
            createNotificationBuilder = default;

            KEY_FIRE_TIME = clazz.GetStatic<AndroidJavaObject>("KEY_FIRE_TIME");
            KEY_ID = clazz.GetStatic<AndroidJavaObject>("KEY_ID");
            KEY_INTENT_DATA = clazz.GetStatic<AndroidJavaObject>("KEY_INTENT_DATA");
            KEY_LARGE_ICON = clazz.GetStatic<AndroidJavaObject>("KEY_LARGE_ICON");
            KEY_SMALL_ICON = clazz.GetStatic<AndroidJavaObject>("KEY_SMALL_ICON");
            KEY_SHOW_IN_FOREGROUND = clazz.GetStatic<AndroidJavaObject>("KEY_SHOW_IN_FOREGROUND");

            CollectMethods(clazz);
        }

        void CollectMethods(AndroidJavaClass clazz)
        {
            getNotificationFromIntent = JniApi.FindMethod(clazz, "getNotificationFromIntent", "(Landroid/content/Intent;)Landroid/app/Notification;", false);
            setNotificationIcon = JniApi.FindMethod(clazz, "setNotificationIcon", "(Landroid/app/Notification$Builder;Ljava/lang/String;Ljava/lang/String;)V", true);
            getNotificationChannelId = JniApi.FindMethod(clazz, "getNotificationChannelId", "(Landroid/app/Notification;)Ljava/lang/String;", true);
            scheduleNotification = JniApi.FindMethod(clazz, "scheduleNotification", "(Landroid/app/Notification$Builder;Z)I", false);
            createNotificationBuilder = JniApi.FindMethod(clazz, "createNotificationBuilder", "(Ljava/lang/String;)Landroid/app/Notification$Builder;", false);
        }

        public AndroidJavaObject GetNotificationFromIntent(AndroidJavaObject intent)
        {
            return self.Call<AndroidJavaObject>(getNotificationFromIntent, intent);
        }

        public void SetNotificationIcon(AndroidJavaObject builder, AndroidJavaObject keyName, string icon)
        {
            klass.CallStatic(setNotificationIcon, builder, keyName, icon);
        }

        public string GetNotificationChannelId(AndroidJavaObject notification)
        {
            return klass.CallStatic<string>(getNotificationChannelId, notification);
        }

        public void RegisterNotificationChannelGroup(AndroidNotificationChannelGroup group)
        {
            self.Call("registerNotificationChannelGroup", group.Id, group.Name, group.Description);
        }

        public void RegisterNotificationChannel(string id, string name, string desc)
        {
            self.Call("registerNotificationChannel",
                id,
                name,
                (int)Importance.High, // High importance, notification is shown everywhere, makes noise and is shown on the screen.
                desc,
                false, // EnableLights
                false, // EnableVibration
                // Whether or not notifications posted to this channel can bypass the Do Not Disturb.
                // This can be changed by users in the settings app.
                false, // CanBypassDnd
                true, // CanShowBadge
                null, // VibrationPattern
                (int)LockScreenVisibility.Public,
                null // Group
            );
        }

        public AndroidJavaObject[] GetNotificationChannels()
        {
            return self.Call<AndroidJavaObject[]>("getNotificationChannels");
        }

        public void DeleteNotificationChannelGroup(string id)
        {
            self.Call("deleteNotificationChannelGroup", id);
        }

        public void DeleteNotificationChannel(string channelId)
        {
            self.Call("deleteNotificationChannel", channelId);
        }

        public int ScheduleNotification(AndroidJavaObject notificationBuilder, bool customized)
        {
            return self.Call<int>(scheduleNotification, notificationBuilder, customized);
        }

        public bool CheckIfPendingNotificationIsRegistered(int id)
        {
            return self.Call<bool>("checkIfPendingNotificationIsRegistered", id);
        }

        public void CancelPendingNotification(int id)
        {
            self.Call("cancelPendingNotification", id);
        }

        public void CancelDisplayedNotification(int id)
        {
            self.Call("cancelDisplayedNotification", id);
        }

        public void CancelAllPendingNotificationIntents()
        {
            self.Call("cancelAllPendingNotificationIntents");
        }

        public void CancelAllNotifications()
        {
            self.Call("cancelAllNotifications");
        }

        public int CheckNotificationStatus(int id)
        {
            return self.Call<int>("checkNotificationStatus", id);
        }

        public void ShowNotificationSettings(string channelId)
        {
            self.Call("showNotificationSettings", channelId);
        }

        public AndroidJavaObject CreateNotificationBuilder(String channelId)
        {
            return self.Call<AndroidJavaObject>(createNotificationBuilder, channelId);
        }

        public void SetupBigPictureStyle(AndroidJavaObject builder, BigPictureStyle bigPicture)
        {
            self.Call("setupBigPictureStyle",
                builder,
                bigPicture.LargeIcon,
                bigPicture.Picture,
                bigPicture.ContentTitle,
                bigPicture.ContentDescription,
                bigPicture.SummaryText,
                bigPicture.ShowWhenCollapsed
            );
        }

        public PermissionStatus AreNotificationsEnabled()
        {
            return (PermissionStatus)self.Call<int>("areNotificationsEnabled");
        }
    }

    struct NotificationJni
    {
        JniFieldID extras;

        public void CollectJni()
        {
            using (var notificationClass = new AndroidJavaClass("android.app.Notification"))
            {
                CollectFields(notificationClass);
            }
        }

        void CollectFields(AndroidJavaClass clazz)
        {
            extras = JniApi.FindField(clazz, "extras", "Landroid/os/Bundle;", false);
        }

        public AndroidJavaObject Extras(AndroidJavaObject notification)
        {
            return notification.Get<AndroidJavaObject>(extras);
        }
    }

    struct NotificationBuilderJni
    {
        JniMethodID getExtras;
        JniMethodID setContentTitle;
        JniMethodID setContentText;
        JniMethodID setStyle;
        JniMethodID setWhen;
        JniMethodID setShowWhen;

        public void CollectJni()
        {
            using (var clazz = new AndroidJavaClass("android.app.Notification$Builder"))
            {
                getExtras = JniApi.FindMethod(clazz, "getExtras", "()Landroid/os/Bundle;", false);
                setContentTitle = JniApi.FindMethod(clazz, "setContentTitle", "(Ljava/lang/CharSequence;)Landroid/app/Notification$Builder;", false);
                setContentText = JniApi.FindMethod(clazz, "setContentText", "(Ljava/lang/CharSequence;)Landroid/app/Notification$Builder;", false);
                setStyle = JniApi.FindMethod(clazz, "setStyle", "(Landroid/app/Notification$Style;)Landroid/app/Notification$Builder;", false);
                setWhen = JniApi.FindMethod(clazz, "setWhen", "(J)Landroid/app/Notification$Builder;", false);
                setShowWhen = JniApi.FindMethod(clazz, "setShowWhen", "(Z)Landroid/app/Notification$Builder;", false);
            }
        }

        public AndroidJavaObject GetExtras(AndroidJavaObject builder)
        {
            return builder.Call<AndroidJavaObject>(getExtras);
        }

        public void SetContentTitle(AndroidJavaObject builder, string title)
        {
            builder.Call<AndroidJavaObject>(setContentTitle, title).Dispose();
        }

        public void SetContentText(AndroidJavaObject builder, string text)
        {
            builder.Call<AndroidJavaObject>(setContentText, text).Dispose();
        }

        public void SetStyle(AndroidJavaObject builder, AndroidJavaObject style)
        {
            builder.Call<AndroidJavaObject>(setStyle, style).Dispose();
        }

        public void SetWhen(AndroidJavaObject builder, long timestamp)
        {
            builder.Call<AndroidJavaObject>(setWhen, timestamp).Dispose();
        }

        public void SetShowWhen(AndroidJavaObject builder, bool showTimestamp)
        {
            builder.Call<AndroidJavaObject>(setShowWhen, showTimestamp).Dispose();
        }
    }

    struct BundleJni
    {
        JniMethodID containsKey;
        JniMethodID getBoolean;
        JniMethodID getInt;
        JniMethodID getLong;
        JniMethodID getString;
        JniMethodID putBoolean;
        JniMethodID putInt;
        JniMethodID putLong;
        JniMethodID putString;

        public void CollectJni()
        {
            using (var clazz = new AndroidJavaClass("android/os/Bundle"))
            {
                containsKey = JniApi.FindMethod(clazz, "containsKey", "(Ljava/lang/String;)Z", false);
                getBoolean = JniApi.FindMethod(clazz, "getBoolean", "(Ljava/lang/String;Z)Z", false);
                getInt = JniApi.FindMethod(clazz, "getInt", "(Ljava/lang/String;I)I", false);
                getLong = JniApi.FindMethod(clazz, "getLong", "(Ljava/lang/String;J)J", false);
                getString = JniApi.FindMethod(clazz, "getString", "(Ljava/lang/String;)Ljava/lang/String;", false);
                putBoolean = JniApi.FindMethod(clazz, "putBoolean", "(Ljava/lang/String;Z)V", false);
                putInt = JniApi.FindMethod(clazz, "putInt", "(Ljava/lang/String;I)V", false);
                putLong = JniApi.FindMethod(clazz, "putLong", "(Ljava/lang/String;J)V", false);
                putString = JniApi.FindMethod(clazz, "putString", "(Ljava/lang/String;Ljava/lang/String;)V", false);
            }
        }

        public bool ContainsKey(AndroidJavaObject bundle, AndroidJavaObject key)
        {
            return bundle.Call<bool>(containsKey, key);
        }

        public bool GetBoolean(AndroidJavaObject bundle, AndroidJavaObject key, bool defaultValue)
        {
            return bundle.Call<bool>(getBoolean, key, defaultValue);
        }

        public bool GetBoolean(AndroidJavaObject bundle, string key, bool defaultValue)
        {
            return bundle.Call<bool>(getBoolean, key, defaultValue);
        }

        public int GetInt(AndroidJavaObject bundle, AndroidJavaObject key, int defaultValue)
        {
            return bundle.Call<int>(getInt, key, defaultValue);
        }

        public long GetLong(AndroidJavaObject bundle, AndroidJavaObject key, long defaultValue)
        {
            return bundle.Call<long>(getLong, key, defaultValue);
        }

        public string GetString(AndroidJavaObject bundle, AndroidJavaObject key)
        {
            return bundle.Call<string>(getString, key);
        }

        public string GetString(AndroidJavaObject bundle, string key)
        {
            return bundle.Call<string>(getString, key);
        }

        public void PutBoolean(AndroidJavaObject bundle, AndroidJavaObject key, bool value)
        {
            bundle.Call(putBoolean, key, value);
        }

        public void PutInt(AndroidJavaObject bundle, AndroidJavaObject key, int value)
        {
            bundle.Call(putInt, key, value);
        }

        public void PutLong(AndroidJavaObject bundle, AndroidJavaObject key, long value)
        {
            bundle.Call(putLong, key, value);
        }

        public void PutString(AndroidJavaObject bundle, AndroidJavaObject key, string value)
        {
            bundle.Call(putString, key, value);
        }
    }

    struct JniApi
    {
        public NotificationManagerJni NotificationManager;
        public NotificationJni Notification;
        public NotificationBuilderJni NotificationBuilder;
        public BundleJni Bundle;

        public JniApi(AndroidJavaClass notificationManagerClass, AndroidJavaObject notificationManager)
        {
            NotificationManager = new NotificationManagerJni(notificationManagerClass, notificationManager);
            Notification = default;
            Notification.CollectJni();
            NotificationBuilder = default;
            NotificationBuilder.CollectJni();
            Bundle = default;
            Bundle.CollectJni();
        }

        public static JniFieldID FindField(AndroidJavaClass clazz, string name, string signature, bool isStatic)
        {
            var field = AndroidJNIHelper.GetFieldID(clazz.GetRawClass(), name, signature, isStatic);
            if (field == IntPtr.Zero)
                throw new Exception($"Field {name} with signature {signature} not found");
            return field;
        }

        public static JniMethodID FindMethod(AndroidJavaClass clazz, string name, string signature, bool isStatic)
        {
            var method = AndroidJNIHelper.GetMethodID(clazz.GetRawClass(), name, signature, isStatic);
            if (method == IntPtr.Zero)
                throw new Exception($"Method {name} with signature {signature} not found");
            return method;
        }
    }

    /// <summary>
    /// Use the AndroidNotificationCenter to register notification channels and schedule local notifications.
    /// </summary>
    public class AndroidNotificationCenter
    {
        private static int API_NOTIFICATIONS_CAN_BE_BLOCKED = 24;
        private static int API_POST_NOTIFICATIONS_PERMISSION_REQUIRED = 33;
        internal static string PERMISSION_POST_NOTIFICATIONS = "android.permission.POST_NOTIFICATIONS";

        /// <summary>
        /// A PlayerPrefs key used to save users reply to POST_NOTIFICATIONS request (integer value of the PermissionStatus).
        /// Value is one of <see cref="PermissionStatus"/>
        /// </summary>
        public static string SETTING_POST_NOTIFICATIONS_PERMISSION = "com.unity.androidnotifications.PostNotificationsPermission";

        /// <summary>
        /// The delegate type for the notification received callbacks.
        /// It is used in <see cref="AndroidNotificationCenter.OnNotificationReceived"/> event.
        /// </summary>
        public delegate void NotificationReceivedCallback(AndroidNotificationIntentData data);

        /// <summary>
        /// Subscribe to this event to receive callbacks whenever a scheduled notification is shown to the user.
        /// </summary>
        public static event NotificationReceivedCallback OnNotificationReceived = delegate { };

        private static AndroidJavaObject s_CurrentActivity;
        private static JniApi s_Jni;
        private static int s_DeviceApiLevel;
        private static bool s_Initialized = false;

        /// <summary>
        /// Initialize the AndroidNotificationCenter class.
        /// Can be safely called multiple times
        /// </summary>
        /// <returns>True if has been successfully initialized</returns>
        public static bool Initialize()
        {
            if (s_Initialized)
                return true;

            if (AndroidReceivedNotificationMainThreadDispatcher.GetInstance() == null)
            {
                var receivedNotificationDispatcher = new GameObject("AndroidReceivedNotificationMainThreadDispatcher");
                receivedNotificationDispatcher.AddComponent<AndroidReceivedNotificationMainThreadDispatcher>();
            }

            s_CurrentActivity = AndroidApplication.UnityActivity;

            var notificationManagerClass = new AndroidJavaClass("com.unity.androidnotifications.UnityNotificationManager");
            var notificationManager = notificationManagerClass.CallStatic<AndroidJavaObject>("getNotificationManagerImpl", s_CurrentActivity, new NotificationCallback());
            s_Jni = new JniApi(notificationManagerClass, notificationManager);

            using (var version = new AndroidJavaClass("android/os/Build$VERSION"))
                s_DeviceApiLevel = version.GetStatic<int>("SDK_INT");

            s_Initialized = true;
            return s_Initialized;
        }

        internal static void SetPostPermissionSetting(PermissionStatus status)
        {
            PlayerPrefs.SetInt(SETTING_POST_NOTIFICATIONS_PERMISSION, (int)status);
        }

        /// <summary>
        /// Has user given permission to post notifications.
        /// Before Android 13 (API 33) no permission is required, but user can disable notifications in the Settings since Android 7 (API 24).
        /// </summary>
        public static PermissionStatus UserPermissionToPost
        {
            get
            {
                if (!Initialize())
                    return PermissionStatus.Denied;
                if (s_DeviceApiLevel < API_NOTIFICATIONS_CAN_BE_BLOCKED)
                    return PermissionStatus.Allowed;

                var permissionStatus = (PermissionStatus)PlayerPrefs.GetInt(SETTING_POST_NOTIFICATIONS_PERMISSION, (int)PermissionStatus.NotRequested);
                var enableStatus = s_Jni.NotificationManager.AreNotificationsEnabled();
                if (enableStatus == PermissionStatus.Allowed)
                {
                    // only save to settings on devices where runtime permission exists
                    if (s_DeviceApiLevel >= API_POST_NOTIFICATIONS_PERMISSION_REQUIRED && permissionStatus != PermissionStatus.Allowed)
                        SetPostPermissionSetting(PermissionStatus.Allowed);
                    return PermissionStatus.Allowed;
                }
                else if (enableStatus == PermissionStatus.NotificationsBlockedForApp)
                    return enableStatus;

                switch (permissionStatus)
                {
                    case PermissionStatus.NotRequested:
                        break;
                    case PermissionStatus.Allowed:
                        permissionStatus = PermissionStatus.Denied;
                        SetPostPermissionSetting(permissionStatus);
                        break;
                    case PermissionStatus.DeniedDontAskAgain:  // no longer used, revert to Denied
                        permissionStatus = PermissionStatus.Denied;
                        break;
                }

                return permissionStatus;
            }
        }

        internal static bool CanRequestPermissionToPost
        {
            get
            {
                if (!Initialize())
                    return false;
                // on lower target SDK OS asks permission automatically, can't ask manually
                // return s_TargetApiLevel >= API_POST_NOTIFICATIONS_PERMISSION_REQUIRED;
                return true;
            }
        }

        /// <summary>
        /// Returns true if app should show UI explaining why it need permission to post notifications.
        /// The UI should be shown before requesting the permission.
        /// </summary>
        public static bool ShouldShowPermissionToPostRationale
        {
            get
            {
                if (!Initialize())
                    return false;

                if (CanRequestPermissionToPost)
                {
#if UNITY_2023_1_OR_NEWER
                    return Permission.ShouldShowRequestPermissionRationale(PERMISSION_POST_NOTIFICATIONS);
#else
                    return s_CurrentActivity.Call<bool>("shouldShowRequestPermissionRationale", PERMISSION_POST_NOTIFICATIONS);
#endif
                }

                return false;
            }
        }

        /// <summary>
        /// Register notification channel group.
        /// </summary>
        public static void RegisterNotificationChannelGroup(AndroidNotificationChannelGroup group)
        {
            if (!Initialize())
                return;

            if (string.IsNullOrEmpty(group.Id))
                throw new Exception("Notification channel group ID is not specified.");
            if (string.IsNullOrEmpty(group.Name))
                throw new Exception("Notification channel group name is not specified.");

            s_Jni.NotificationManager.RegisterNotificationChannelGroup(group);
        }

        /// <summary>
        /// Delete notification channel group and all the channels in it.
        /// </summary>
        /// <param name="id">The ID of the group.</param>
        public static void DeleteNotificationChannelGroup(string id)
        {
            if (Initialize())
                s_Jni.NotificationManager.DeleteNotificationChannelGroup(id);
        }

        /// <summary>
        ///  Creates a notification channel that notifications can be posted to.
        ///  Notification channel settings can be changed by users on devices running Android 8.0 and above.
        ///  On older Android versions settings set on the notification channel struct will still be applied to the notification
        ///  if they are supported to by the Android version the app is running on.
        /// </summary>
        /// <param name="channel">Channel parameters</param>
        /// <remarks>
        ///  When a channel is deleted and recreated, all of the previous settings are restored. In order to change any settings
        ///  besides the name or description an entirely new channel (with a different channel ID) must be created.
        /// </remarks>
        public static void RegisterNotificationChannel(string id, string name, string desc)
        {
            if (!Initialize())
                return;

            s_Jni.NotificationManager.RegisterNotificationChannel(id, name, desc);
        }

        public static bool HasNotificationChannel(string channelId)
        {
            if (!Initialize())
            {
                L.I("[AndroidNotificationCenter] Not initialized.");
                return false;
            }

            var androidChannels = s_Jni.NotificationManager.GetNotificationChannels();

            for (int i = 0; i < androidChannels.Length; ++i)
            {
                var channel = androidChannels[i];
                if (channel.Get<string>("id") == channelId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Delete the specified notification channel.
        /// </summary>
        /// <param name="channelId">ID of the channel to delete</param>
        public static void DeleteNotificationChannel(string channelId)
        {
            if (Initialize())
                s_Jni.NotificationManager.DeleteNotificationChannel(channelId);
        }

        /// <summary>
        /// Schedule a notification which will be shown at the time specified in the notification struct.
        /// The returned id can later be used to update the notification before it's triggered, it's current status can be tracked using CheckScheduledNotificationStatus.
        /// </summary>
        /// <param name="notification">Data for the notification</param>
        /// <param name="channelId">ID of the channel to send notification to</param>
        /// <returns>The generated ID for the notification</returns>
        public static int SendNotification(AndroidNotification notification, string channelId)
        {
            if (!Initialize())
                return -1;

            using (var builder = CreateNotificationBuilder(notification, channelId))
                return ScheduleNotification(builder, false);
        }

        /// <summary>
        /// Schedule a notification which will be shown at the time specified in the notification struct.
        /// The specified id can later be used to update the notification before it's triggered, it's current status can be tracked using CheckScheduledNotificationStatus.
        /// </summary>
        /// <param name="notification">Data for the notification</param>
        /// <param name="channelId">ID of the channel to send notification to</param>
        /// <param name="id">A unique ID for the notification</param>
        public static void SendNotificationWithExplicitID(AndroidNotification notification, string channelId, int id)
        {
            if (Initialize())
                using (var builder = CreateNotificationBuilder(id, notification, channelId))
                    ScheduleNotification(builder, false);
        }

        /// <summary>
        /// Schedule a notification created using the provided Notification.Builder object.
        /// Notification builder should be created by calling CreateNotificationBuilder.
        /// </summary>
        public static void SendNotification(AndroidJavaObject notificationBuilder)
        {
            if (Initialize())
                ScheduleNotification(notificationBuilder, true);
        }

        /// <summary>
        /// Schedule a notification created using the provided Notification.Builder object.
        /// Notification builder should be created by calling CreateNotificationBuilder.
        /// Stores the notification id to the second argument
        /// </summary>
        public static void SendNotification(AndroidJavaObject notificationBuilder, out int id)
        {
            id = -1;
            if (Initialize())
                id = ScheduleNotification(notificationBuilder, true);
        }

        static int ScheduleNotification(AndroidJavaObject notificationBuilder, bool customized)
        {
            return s_Jni.NotificationManager.ScheduleNotification(notificationBuilder, customized);
        }

        /// <summary>
        /// Update an already scheduled notification.
        /// If a notification with the specified id was already scheduled it will be overridden with the information from the passed notification struct.
        /// </summary>
        /// <param name="id">ID of the notification to update</param>
        /// <param name="notification">Data for the notification</param>
        /// <param name="channelId">ID of the channel to send notification to</param>
        public static void UpdateScheduledNotification(int id, AndroidNotification notification, string channelId)
        {
            if (!Initialize())
                return;

            if (s_Jni.NotificationManager.CheckIfPendingNotificationIsRegistered(id))
            {
                using (var builder = CreateNotificationBuilder(id, notification, channelId))
                {
                    ScheduleNotification(builder, false);
                }
            }
        }

        /// <summary>
        /// Cancel a scheduled or previously shown notification.
        /// The notification will no longer be displayed on it's scheduled time. If it's already delivered it will be removed from the status bar.
        /// </summary>
        /// <param name="id">ID of the notification to cancel</param>
        public static void CancelNotification(int id)
        {
            if (!Initialize())
                return;

            CancelScheduledNotification(id);
            CancelDisplayedNotification(id);
        }

        /// <summary>
        /// Cancel a scheduled notification.
        /// The notification will no longer be displayed on it's scheduled time. It it will not be removed from the status bar if it's already delivered.
        /// </summary>
        /// <param name="id">ID of the notification to cancel</param>
        public static void CancelScheduledNotification(int id)
        {
            if (Initialize())
                s_Jni.NotificationManager.CancelPendingNotification(id);
        }

        /// <summary>
        /// Cancel a previously shown notification.
        /// The notification will be removed from the status bar.
        /// </summary>
        /// <param name="id">ID of the notification to cancel</param>
        public static void CancelDisplayedNotification(int id)
        {
            if (Initialize())
                s_Jni.NotificationManager.CancelDisplayedNotification(id);
        }

        /// <summary>
        /// Cancel all notifications scheduled or previously shown by the app.
        /// All scheduled notifications will be canceled. All notifications shown by the app will be removed from the status bar.
        /// </summary>
        public static void CancelAllNotifications()
        {
            if (!Initialize())
                return;

            CancelAllScheduledNotifications();
            CancelAllDisplayedNotifications();
        }

        /// <summary>
        /// Cancel all notifications scheduled by the app.
        /// All scheduled notifications will be canceled. Notifications will not be removed from the status bar if they are already shown.
        /// </summary>
        public static void CancelAllScheduledNotifications()
        {
            if (Initialize())
                s_Jni.NotificationManager.CancelAllPendingNotificationIntents();
        }

        /// <summary>
        /// Cancel all previously shown notifications.
        /// All notifications shown by the app will be removed from the status bar. All scheduled notifications will still be shown on their scheduled time.
        /// </summary>
        public static void CancelAllDisplayedNotifications()
        {
            if (Initialize())
                s_Jni.NotificationManager.CancelAllNotifications();
        }

        /// <summary>
        /// Return the status of a scheduled notification.
        /// </summary>
        /// <param name="id">ID of the notification to check</param>
        /// <returns>The status of the notification</returns>
        public static NotificationStatus CheckScheduledNotificationStatus(int id)
        {
            if (!Initialize())
                return NotificationStatus.Unavailable;

            var status = s_Jni.NotificationManager.CheckNotificationStatus(id);
            return (NotificationStatus)status;
        }

        /// <summary>
        /// Allows retrieving the notification used to open the app. You can save arbitrary string data in the 'AndroidNotification.IntentData' field.
        /// </summary>
        /// <returns>
        /// Returns the AndroidNotification used to open the app, returns null if the app was not opened with a notification.
        /// </returns>
        public static AndroidNotificationIntentData GetLastNotificationIntent()
        {
            if (!Initialize())
                return null;

            var intent = s_CurrentActivity.Call<AndroidJavaObject>("getIntent");
            var notification = s_Jni.NotificationManager.GetNotificationFromIntent(intent);
            if (notification == null)
                return null;
            return GetNotificationData(notification);
        }

        /// <summary>
        /// Opens settings.
        /// On Android versions lower than 8.0 opens settings for the application.
        /// On Android 8.0 and later opens notification settings for the specified channel, or for the application, if channelId is null.
        /// Note, that opening settings will suspend the application and switch to settings app.
        /// </summary>
        /// <param name="channelId">ID for the channel to open or null to open notification settings for the application.</param>
        public static void OpenNotificationSettings(string channelId = null)
        {
            if (!Initialize())
                return;

            s_Jni.NotificationManager.ShowNotificationSettings(channelId);
        }

        /// <summary>
        /// Create Notification.Builder.
        /// Will automatically generate the ID for notification.
        /// <see cref="CreateNotificationBuilder(int, AndroidNotification, string)"/>
        /// </summary>
        public static AndroidJavaObject CreateNotificationBuilder(AndroidNotification notification, string channelId)
        {
            AndroidJavaObject builder, extras;
            CreateNotificationBuilder(notification, channelId, out builder, out extras);
            if (extras != null)
                extras.Dispose();
            return builder;
        }

        /// <summary>
        /// Create Notification.Builder object on Java side using privided AndroidNotification.
        /// </summary>
        /// <param name="id">ID for the notification</param>
        /// <param name="notification">Struct with notification data</param>
        /// <param name="channelId">Channel id</param>
        /// <returns>A proxy object for created Notification.Builder</returns>
        public static AndroidJavaObject CreateNotificationBuilder(int id, AndroidNotification notification, string channelId)
        {
            AndroidJavaObject builder, extras;
            CreateNotificationBuilder(notification, channelId, out builder, out extras);
            if (extras != null)
            {
                s_Jni.Bundle.PutInt(extras, s_Jni.NotificationManager.KEY_ID, id);
                extras.Dispose();
            }
            return builder;
        }

        static void CreateNotificationBuilder(AndroidNotification notification, string channelId, out AndroidJavaObject notificationBuilder, out AndroidJavaObject extras)
        {
            if (!Initialize())
            {
                notificationBuilder = extras = null;
                return;
            }

            long fireTime = notification.FireTime.ToLong();
            if (fireTime < 0L)
            {
                Debug.LogError("Failed to schedule notification, it did not contain a valid FireTime");
            }

            // NOTE: JNI calls are expensive, so we avoid calls that set something that is also a default

            notificationBuilder = s_Jni.NotificationManager.CreateNotificationBuilder(channelId);
            s_Jni.NotificationManager.SetNotificationIcon(notificationBuilder, s_Jni.NotificationManager.KEY_SMALL_ICON, notification.SmallIcon);
            if (!string.IsNullOrEmpty(notification.LargeIcon))
                s_Jni.NotificationManager.SetNotificationIcon(notificationBuilder, s_Jni.NotificationManager.KEY_LARGE_ICON, notification.LargeIcon);
            if (!string.IsNullOrEmpty(notification.Title))
                s_Jni.NotificationBuilder.SetContentTitle(notificationBuilder, notification.Title);
            if (!string.IsNullOrEmpty(notification.Text))
                s_Jni.NotificationBuilder.SetContentText(notificationBuilder, notification.Text);
            switch (notification.Style)
            {
                case NotificationStyle.None:
                    break;
                case NotificationStyle.BigPictureStyle:
                    if (notification.BigPicture.HasValue)
                    {
                        var bigPicture = notification.BigPicture.Value;
                        s_Jni.NotificationManager.SetupBigPictureStyle(notificationBuilder, bigPicture);
                    }
                    break;
                case NotificationStyle.BigTextStyle:
                    using (var style = new AndroidJavaObject("android.app.Notification$BigTextStyle"))
                    {
                        style.Call<AndroidJavaObject>("bigText", notification.Text).Dispose();
                        s_Jni.NotificationBuilder.SetStyle(notificationBuilder, style);
                    }
                    break;
            }
            long timestampValue = notification.ShowCustomTimestamp ? notification.CustomTimestamp.ToLong() : fireTime;
            s_Jni.NotificationBuilder.SetWhen(notificationBuilder, timestampValue);
            if (notification.ShowTimestamp)
                s_Jni.NotificationBuilder.SetShowWhen(notificationBuilder, notification.ShowTimestamp);

            extras = s_Jni.NotificationBuilder.GetExtras(notificationBuilder);
            s_Jni.Bundle.PutLong(extras, s_Jni.NotificationManager.KEY_FIRE_TIME, fireTime);
            s_Jni.Bundle.PutBoolean(extras, s_Jni.NotificationManager.KEY_SHOW_IN_FOREGROUND, notification.ShowInForeground);
            if (!string.IsNullOrEmpty(notification.IntentData))
                s_Jni.Bundle.PutString(extras, s_Jni.NotificationManager.KEY_INTENT_DATA, notification.IntentData);
        }

        internal static AndroidNotificationIntentData GetNotificationData(AndroidJavaObject notificationObj)
        {
            using (var extras = s_Jni.Notification.Extras(notificationObj))
            {
                var id = s_Jni.Bundle.GetInt(extras, s_Jni.NotificationManager.KEY_ID, -1);
                if (id == -1)
                    return null;

                var channelId = s_Jni.NotificationManager.GetNotificationChannelId(notificationObj);

                var data = new AndroidNotificationIntentData(id, channelId);
                data.NativeNotification = notificationObj;
                return data;
            }
        }

        internal static void ReceivedNotificationCallback(AndroidJavaObject notification)
        {
            var data = GetNotificationData(notification);
            OnNotificationReceived(data);
        }
    }
}

#endif