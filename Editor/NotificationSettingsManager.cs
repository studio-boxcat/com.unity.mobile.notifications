using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Notifications
{
    [HelpURL("Packages/com.unity.mobile.notifications/documentation.html")]
    internal class NotificationSettingsManager : ScriptableObject
    {
        internal static readonly string k_SettingsPath = "ProjectSettings/NotificationsSettings.asset";

        private static NotificationSettingsManager s_SettingsManager;

        public List<NotificationSetting> AndroidNotificationSettings;

        [SerializeField]
        [FormerlySerializedAs("AndroidNotificationEditorSettingsValues")]
        private NotificationSettingsCollection m_AndroidNotificationSettingsValues;

        [FormerlySerializedAs("TrackedResourceAssets")]
        public List<DrawableResourceData> DrawableResources = new List<DrawableResourceData>();

        public List<NotificationSetting> AndroidNotificationSettingsFlat
        {
            get
            {
                var target = new List<NotificationSetting>();
                FlattenList(AndroidNotificationSettings, target);
                return target;
            }
        }

        private void FlattenList(List<NotificationSetting> source, List<NotificationSetting> target)
        {
            foreach (var setting in source)
            {
                target.Add(setting);

                if (setting.Dependencies != null)
                {
                    FlattenList(setting.Dependencies, target);
                }
            }
        }

        public static NotificationSettingsManager Initialize()
        {
            if (s_SettingsManager != null)
                return s_SettingsManager;

            var settingsManager = CreateInstance<NotificationSettingsManager>();
            bool dirty = false;

            if (File.Exists(k_SettingsPath))
            {
                var settingsJson = File.ReadAllText(k_SettingsPath);
                if (!string.IsNullOrEmpty(settingsJson))
                    EditorJsonUtility.FromJsonOverwrite(settingsJson, settingsManager);
            }

            if (settingsManager.m_AndroidNotificationSettingsValues == null)
            {
                settingsManager.m_AndroidNotificationSettingsValues = new NotificationSettingsCollection();
                dirty = true;
            }

            // Create the settings for Android.
            settingsManager.AndroidNotificationSettings = new List<NotificationSetting>()
            {
                new NotificationSetting(
                    NotificationSettings.AndroidSettings.RESCHEDULE_ON_RESTART,
                    "Reschedule on Device Restart",
                    "Enable this to automatically reschedule all non-expired notifications after device restart. By default AndroidSettings removes all scheduled notifications after restarting.",
                    settingsManager.GetOrAddNotificationSettingValue(NotificationSettings.AndroidSettings.RESCHEDULE_ON_RESTART, false)),
                new NotificationSetting(
                    NotificationSettings.AndroidSettings.EXACT_ALARM,
                    "Schedule at exact time",
                    "Whether notifications should appear at exact time or approximate",
                    settingsManager.GetOrAddNotificationSettingValue(NotificationSettings.AndroidSettings.EXACT_ALARM, (AndroidExactSchedulingOption)0)),
                new NotificationSetting(
                    NotificationSettings.AndroidSettings.CUSTOM_ACTIVITY_CLASS,
                    "Custom Activity Name",
                    "The full class name of the activity which will be assigned to the notification.",
                    settingsManager.GetOrAddNotificationSettingValue(NotificationSettings.AndroidSettings.CUSTOM_ACTIVITY_CLASS, "com.unity3d.player.UnityPlayerActivity")),
            };

            settingsManager.SaveSettings(dirty);

            s_SettingsManager = settingsManager;
            return s_SettingsManager;
        }

        private T GetOrAddNotificationSettingValue<T>(string key, T defaultValue)
        {
            var collection = m_AndroidNotificationSettingsValues;

            try
            {
                var value = collection[key];
                if (value != null)
                    return (T)value;
            }
            catch (InvalidCastException)
            {
                Debug.LogWarning("Failed loading : " + key + " for type:" + defaultValue.GetType() + " Expected : " + collection[key].GetType());
                //Just return default value if it's a new setting that was not yet serialized.
            }

            collection[key] = defaultValue;
            return defaultValue;
        }

        public void SaveSetting(NotificationSetting setting)
        {
            var collection = m_AndroidNotificationSettingsValues;

            if (!collection.Contains(setting.Key) || collection[setting.Key].ToString() != setting.Value.ToString())
            {
                collection[setting.Key] = setting.Value;
                SaveSettings();
            }
        }

        public void SaveSettings(bool forceSave = true)
        {
            if (!forceSave && File.Exists(k_SettingsPath))
                return;

            if (AssetDatabase.MakeEditable(k_SettingsPath))
                File.WriteAllText(k_SettingsPath, EditorJsonUtility.ToJson(this, true));
            else
                Debug.LogError($"Failed to make file {k_SettingsPath} editable");
        }

        public void AddDrawableResource(string id, Texture2D image, NotificationIconType type)
        {
            /* commenting out for now, since you can have same Id's in editor
            foreach (var drawable in DrawableResources)
            {
                if (drawable.Id == id)
                {
                    Debug.LogWarning("Drawable with Id"+id+" already exists, please assign another Id");
                    return;
                }
            } */
            var drawableResource = new DrawableResourceData();
            drawableResource.Id = id;
            drawableResource.Type = type;
            drawableResource.Asset = image;

            DrawableResources.Add(drawableResource);
            SaveSettings();
        }

        public void RemoveDrawableResourceByIndex(int index)
        {
            if (index < DrawableResources.Count && index >= 0)
            {
                DrawableResources.RemoveAt(index);
                SaveSettings();
            }
            else
            {
                Debug.LogWarning("Invalid drawable index provided, drawable not removed.");
            }
        }

        public void RemoveDrawableResourceById(string id)
        {
            DrawableResourceData DrawableRes = null;
            foreach (var drawable in DrawableResources)
            {
                if (drawable.Id == id)
                {
                    DrawableRes = drawable;
                    break;
                }
            }
            if (DrawableRes == null)
            {
                Debug.LogWarning("Drawable with Id " + id + " not found. Drawable not removed.");
            }
            else
            {
                DrawableResources.Remove(DrawableRes);
                SaveSettings();
            }
        }

        public void ClearDrawableResources()
        {
            DrawableResources.Clear();
            SaveSettings();
        }

        public Dictionary<string, byte[]> GenerateDrawableResourcesForExport()
        {
            var icons = new Dictionary<string, byte[]>();
            foreach (var drawableResource in DrawableResources)
            {
                if (!drawableResource.Verify())
                {
                    Debug.LogWarning(string.Format("Failed exporting: '{0}' AndroidSettings notification icon because:\n {1} ",
                        drawableResource.Id,
                        drawableResource.GenerateErrorString()));
                    continue;
                }

                var texture = TextureAssetUtils.ProcessTextureForType(drawableResource.Asset, drawableResource.Type);

                var scale = drawableResource.Type == NotificationIconType.Small ? 0.375f : 1;

                var textXhdpi = TextureAssetUtils.ScaleTexture(texture, (int)(128 * scale), (int)(128 * scale));
                var textHdpi = TextureAssetUtils.ScaleTexture(texture, (int)(96 * scale), (int)(96 * scale));
                var textMdpi = TextureAssetUtils.ScaleTexture(texture, (int)(64 * scale), (int)(64 * scale));
                var textLdpi = TextureAssetUtils.ScaleTexture(texture, (int)(48 * scale), (int)(48 * scale));

                icons[string.Format("drawable-xhdpi-v11/{0}.png", drawableResource.Id)] = textXhdpi.EncodeToPNG();
                icons[string.Format("drawable-hdpi-v11/{0}.png", drawableResource.Id)] = textHdpi.EncodeToPNG();
                icons[string.Format("drawable-mdpi-v11/{0}.png", drawableResource.Id)] = textMdpi.EncodeToPNG();
                icons[string.Format("drawable-ldpi-v11/{0}.png", drawableResource.Id)] = textLdpi.EncodeToPNG();

                if (drawableResource.Type == NotificationIconType.Large)
                {
                    var textXxhdpi = TextureAssetUtils.ScaleTexture(texture, (int)(192 * scale), (int)(192 * scale));
                    icons[string.Format("drawable-xxhdpi-v11/{0}.png", drawableResource.Id)] = textXxhdpi.EncodeToPNG();
                }
            }

            return icons;
        }
    }
}
