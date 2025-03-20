using Unity.Notifications.iOS;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Notifications
{
    /// <summary>
    /// Class used to access notification settings for a specific platform.
    /// </summary>
    public class NotificationSettings
    {
        private static NotificationSetting GetSetting(BuildTargetGroup target, string key)
        {
            var manager = NotificationSettingsManager.Initialize();

            NotificationSetting setting = null;
            Assert.AreEqual(target, BuildTargetGroup.Android, "Only Android platform is supported");
            setting = manager.AndroidNotificationSettingsFlat.Find(i => i.Key == key);

            return setting;
        }

        private static void SetSettingValue<T>(BuildTargetGroup target, string key, T value)
        {
            var manager = NotificationSettingsManager.Initialize();

            NotificationSetting setting = GetSetting(target, key);
            if (setting != null)
            {
                setting.Value = value;
                manager.SaveSetting(setting);
            }
        }

        private static T GetSettingValue<T>(BuildTargetGroup target, string key)
        {
            var setting = GetSetting(target, key);
            return (T)setting.Value;
        }

        /// <summary>
        /// Class used to access Android-specific notification settings.
        /// </summary>
        public static class AndroidSettings
        {
            internal static readonly string RESCHEDULE_ON_RESTART = "UnityNotificationAndroidRescheduleOnDeviceRestart";
            internal static readonly string EXACT_ALARM = "UnityNotificationAndroidScheduleExactAlarms";
            internal static readonly string CUSTOM_ACTIVITY_CLASS = "UnityNotificationAndroidCustomActivityString";

            /// <summary>
            /// By default AndroidSettings removes all scheduled notifications when the device is restarted. Enable this to automatically reschedule all non expired notifications when the device is turned back on.
            /// </summary>
            public static bool RescheduleOnDeviceRestart
            {
                get
                {
                    return GetSettingValue<bool>(BuildTargetGroup.Android, RESCHEDULE_ON_RESTART);
                }
                set
                {
                    SetSettingValue<bool>(BuildTargetGroup.Android, RESCHEDULE_ON_RESTART, value);
                }
            }

            /// <summary>
            /// The full class name of the activity that you wish to be assigned to the notification.
            /// </summary>
            public static string CustomActivityString
            {
                get
                {
                    return GetSettingValue<string>(BuildTargetGroup.Android, CUSTOM_ACTIVITY_CLASS);
                }
                set
                {
                    SetSettingValue<string>(BuildTargetGroup.Android, CUSTOM_ACTIVITY_CLASS, value);
                }
            }

            /// <summary>
            /// A set of flags indicating whether to use exact scheduling and add supporting permissions.
            /// </summary>
            public static AndroidExactSchedulingOption ExactSchedulingOption
            {
                get
                {
                    return GetSettingValue<AndroidExactSchedulingOption>(BuildTargetGroup.Android, EXACT_ALARM);
                }
                set
                {
                    SetSettingValue<AndroidExactSchedulingOption>(BuildTargetGroup.Android, EXACT_ALARM, value);
                }
            }

            /// <summary>
            /// Add image to notification settings.
            /// </summary>
            /// <param name="id">Image identifier</param>
            /// <param name="image">Image texture, must be obtained from asset database</param>
            /// <param name="type">Image type</param>
            public static void AddDrawableResource(string id, Texture2D image, NotificationIconType type)
            {
                var manager = NotificationSettingsManager.Initialize();
                manager.AddDrawableResource(id, image, type);
                SettingsService.RepaintAllSettingsWindow();
            }

            /// <summary>
            /// Remove icon at given index from notification settings.
            /// </summary>
            /// <param name="index">Index of image to remove</param>
            public static void RemoveDrawableResource(int index)
            {
                var manager = NotificationSettingsManager.Initialize();
                manager.RemoveDrawableResourceByIndex(index);
                SettingsService.RepaintAllSettingsWindow();
            }

            /// <summary>
            /// Remove icon with given identifier from notification settings.
            /// </summary>
            /// <param name="id">ID of the image to remove</param>
            public static void RemoveDrawableResource(string id)
            {
                var manager = NotificationSettingsManager.Initialize();
                manager.RemoveDrawableResourceById(id);
                SettingsService.RepaintAllSettingsWindow();
            }

            /// <summary>
            /// Remove all images from notification settings.
            /// </summary>
            public static void ClearDrawableResources()
            {
                var manager = NotificationSettingsManager.Initialize();
                manager.ClearDrawableResources();
                SettingsService.RepaintAllSettingsWindow();
            }


        }
    }
}
