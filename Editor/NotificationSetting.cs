using System.Collections.Generic;

namespace Unity.Notifications
{
    internal class NotificationSetting
    {
        public string Key;
        public string Label;
        public string Tooltip;
        public object Value;

        public List<NotificationSetting> Dependencies;

        public NotificationSetting(string key, string label, string tooltip, object value,
                                   List<NotificationSetting> dependencies = null)
        {
            this.Key = key;
            this.Label = label;
            this.Tooltip = tooltip;
            this.Value = value;
            this.Dependencies = dependencies;
        }
    }
}
