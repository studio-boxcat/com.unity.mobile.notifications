#if UNITY_IOS || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

#pragma warning disable 162

namespace Unity.Notifications.iOS
{
    internal struct iOSNotificationWithUserInfo
    {
        internal iOSNotificationData data;
        internal Dictionary<string, string> userInfo;
        internal List<iOSNotificationAttachment> attachments;
    }

    internal static class iOSNotificationsWrapper
    {
#if DEVELOPMENT_BUILD
        [DllImport("__Internal")]
        private static extern int _NativeSizeof_iOSNotificationAuthorizationData();

        [DllImport("__Internal")]
        private static extern int _NativeSizeof_iOSNotificationData();

        [DllImport("__Internal")]
        private static extern int _NativeSizeof_NotificationSettingsData();
#endif

        [DllImport("__Internal")]
        private static extern void _RequestAuthorization(IntPtr request, Int32 options, bool registerForRemote);

        [DllImport("__Internal")]
        private static extern int _RegisteredForRemoteNotifications();

        [DllImport("__Internal")]
        private static extern void _UnregisterForRemoteNotifications();

        [DllImport("__Internal")]
        private static extern void _ScheduleLocalNotification(iOSNotificationData data);

        [DllImport("__Internal")]
        private static extern void _SetNotificationReceivedDelegate(NotificationReceivedCallback callback);

        [DllImport("__Internal")]
        private static extern void _SetRemoteNotificationReceivedDelegate(NotificationReceivedCallback callback);

        [DllImport("__Internal")]
        private static extern void _SetAuthorizationRequestReceivedDelegate(AuthorizationRequestCallback callback);

        [DllImport("__Internal")]
        private static extern iOSNotificationSettings _GetNotificationSettings();

        [DllImport("__Internal")]
        private static extern IntPtr _GetScheduledNotificationDataArray(out Int32 count);

        [DllImport("__Internal")]
        private static extern IntPtr _GetDeliveredNotificationDataArray(out Int32 count);

        [DllImport("__Internal")]
        internal static extern void _RemoveScheduledNotification(string identifier);

        [DllImport("__Internal")]
        internal static extern void _RemoveAllScheduledNotifications();

        [DllImport("__Internal")]
        internal static extern void _RemoveDeliveredNotification(string identifier);

        [DllImport("__Internal")]
        private static extern void _SetApplicationBadge(Int32 badge);

        [DllImport("__Internal")]
        private static extern Int32 _GetApplicationBadge();

        [DllImport("__Internal")]
        private static extern bool _GetAppOpenedUsingNotification();

        [DllImport("__Internal")]
        internal static extern void _RemoveAllDeliveredNotifications();

        [DllImport("__Internal")]
        private static extern IntPtr _GetLastNotificationData();

        [DllImport("__Internal")]
        private static extern string _GetLastRespondedNotificationAction();

        [DllImport("__Internal")]
        private static extern string _GetLastRespondedNotificationUserText();

        [DllImport("__Internal")]
        private static extern void _FreeUnmanagediOSNotificationDataArray(IntPtr ptr, int count);

        [DllImport("__Internal")]
        internal static extern IntPtr _AddItemToNSDictionary(IntPtr dict, string key, string value);

        [DllImport("__Internal")]
        internal static extern IntPtr _AddAttachmentToNSArray(IntPtr atts, string id, string url, out IntPtr error);

        [DllImport("__Internal")]
        private static extern void _ReadNSDictionary(IntPtr handle, IntPtr nsDict, ReceiveNSDictionaryKeyValueCallback callback);

        [DllImport("__Internal")]
        private static extern void _ReadAttachmentsNSArray(IntPtr handle, IntPtr nsArray, ReceiveUNNotificationAttachmentCallback callback);

        [DllImport("__Internal")]
        internal static extern IntPtr _CreateUNNotificationAction(string id, string title, int options, int iconType, string icon);

        [DllImport("__Internal")]
        internal static extern IntPtr _CreateUNTextInputNotificationAction(string id, string title, int options, int iconType, string icon, string buttonTitle, string placeholder);

        [DllImport("__Internal")]
        private static extern void _ReleaseNSObject(IntPtr obj);

        [DllImport("__Internal")]
        private static extern string _NSErrorToMessage(IntPtr error);

        [DllImport("__Internal")]
        private static extern IntPtr _AddActionToNSArray(IntPtr actions, IntPtr action, int capacity);

        [DllImport("__Internal")]
        private static extern IntPtr _CreateUNNotificationCategory(string id, string hiddenPreviewsBodyPlaceholder, string summaryFormat, int options, IntPtr actions, IntPtr intentIdentifiers);

        [DllImport("__Internal")]
        private static extern IntPtr _AddCategoryToCategorySet(IntPtr categorySet, IntPtr category);

        [DllImport("__Internal")]
        private static extern void _SetNotificationCategories(IntPtr categorySet);

        [DllImport("__Internal")]
        private static extern IntPtr _AddStringToNSArray(IntPtr array, string str, int capacity);

        [DllImport("__Internal")]
        internal static extern void _OpenNotificationSettings();

        private delegate void AuthorizationRequestCallback(IntPtr request, iOSAuthorizationRequestData data);
        private delegate void NotificationReceivedCallback(iOSNotificationData notificationData);
        private delegate void ReceiveNSDictionaryKeyValueCallback(IntPtr dict, string key, string value);
        private delegate void ReceiveUNNotificationAttachmentCallback(IntPtr array, string id, string url);

#if UNITY_IOS && !UNITY_EDITOR && DEVELOPMENT_BUILD
        static iOSNotificationsWrapper()
        {
            VerifyNativeManagedSize(_NativeSizeof_iOSNotificationAuthorizationData(), typeof(iOSAuthorizationRequestData));
            VerifyNativeManagedSize(_NativeSizeof_iOSNotificationData(), typeof(iOSNotificationData));
            VerifyNativeManagedSize(_NativeSizeof_NotificationSettingsData(), typeof(iOSNotificationSettings));
        }

        static void VerifyNativeManagedSize(int nativeSize, Type managedType)
        {
            var managedSize = Marshal.SizeOf(managedType);
            if (nativeSize != managedSize)
                throw new Exception(string.Format("Native/managed struct size missmatch: {0} vs {1}", nativeSize, managedSize));
        }

#endif

        public static void RegisterAuthorizationRequestCallback()
        {
            _SetAuthorizationRequestReceivedDelegate(AuthorizationRequestReceived);
        }

        public static void RegisterOnReceivedRemoteNotificationCallback()
        {
            _SetRemoteNotificationReceivedDelegate(RemoteNotificationReceived);
        }

        public static void RegisterOnReceivedCallback()
        {
            _SetNotificationReceivedDelegate(NotificationReceived);
        }

        [MonoPInvokeCallback(typeof(AuthorizationRequestCallback))]
        public static void AuthorizationRequestReceived(IntPtr request, iOSAuthorizationRequestData data)
        {
            AuthorizationRequest.OnAuthorizationRequestCompleted(request, data);
        }

        [MonoPInvokeCallback(typeof(NotificationReceivedCallback))]
        public static void RemoteNotificationReceived(iOSNotificationData data)
        {
            iOSNotificationCenter.OnReceivedRemoteNotification(NotificationDataToDataWithUserInfo(data));
        }

        [MonoPInvokeCallback(typeof(NotificationReceivedCallback))]
        public static void NotificationReceived(iOSNotificationData data)
        {
            iOSNotificationCenter.OnSentNotification(NotificationDataToDataWithUserInfo(data));
        }

        static iOSNotificationWithUserInfo NotificationDataToDataWithUserInfo(iOSNotificationData data)
        {
            iOSNotificationWithUserInfo ret;
            ret.data = data;
            ret.data.userInfo = IntPtr.Zero;
            ret.userInfo = NSDictionaryToCs(data.userInfo);
            ret.attachments = AttachmentsNSArrayToCs(data.attachments);
            return ret;
        }

        [MonoPInvokeCallback(typeof(ReceiveNSDictionaryKeyValueCallback))]
        private static void ReceiveNSDictionaryKeyValue(IntPtr dict, string key, string value)
        {
            GCHandle handle = GCHandle.FromIntPtr(dict);
            var dictionary = (Dictionary<string, string>)handle.Target;
            if (dictionary == null)
                return;
            dictionary[key] = value;
        }

        [MonoPInvokeCallback(typeof(ReceiveUNNotificationAttachmentCallback))]
        private static void ReceiveUNNotificationAttachment(IntPtr array, string id, string url)
        {
            GCHandle handle = GCHandle.FromIntPtr(array);
            var list = (List<iOSNotificationAttachment>)handle.Target;
            if (list == null)
                return;
            list.Add(new iOSNotificationAttachment()
            {
                Id = id,
                Url = url,
            });
        }

        public static void RequestAuthorization(IntPtr request, int options, bool registerRemote)
        {
            _RequestAuthorization(request, options, registerRemote);
        }

        public static bool RegisteredForRemoteNotifications()
        {
            return _RegisteredForRemoteNotifications() != 0;
        }

        public static void UnregisterForRemoteNotifications()
        {
            _UnregisterForRemoteNotifications();
        }

        public static iOSNotificationSettings GetNotificationSettings()
        {
            return _GetNotificationSettings();
        }

        public static void ScheduleLocalNotification(iOSNotificationWithUserInfo data)
        {
            data.data.userInfo = iOSNotificationsWrapper.CsDictionaryToObjC(data.userInfo);
            data.data.attachments = iOSNotificationsWrapper.CsAttachmentsToObjc(data.attachments);
            _ScheduleLocalNotification(data.data);
        }

        public static iOSNotificationWithUserInfo[] GetDeliveredNotificationData()
        {
            int count;
            var ptr = _GetDeliveredNotificationDataArray(out count);
            return MarshalAndFreeNotificationDataArray(ptr, count);
        }

        public static string GetLastRespondedNotificationAction()
        {
            return _GetLastRespondedNotificationAction();
        }

        public static string GetLastRespondedNotificationUserText()
        {
            return _GetLastRespondedNotificationUserText();
        }

        public static iOSNotificationWithUserInfo[] GetScheduledNotificationData()
        {
            int count;
            var ptr = _GetScheduledNotificationDataArray(out count);
            return MarshalAndFreeNotificationDataArray(ptr, count);
        }

        static iOSNotificationWithUserInfo[] MarshalAndFreeNotificationDataArray(IntPtr ptr, int count)
        {
            if (count == 0 || ptr == IntPtr.Zero)
                return null;

            var dataArray = new iOSNotificationWithUserInfo[count];
            var structSize = Marshal.SizeOf(typeof(iOSNotificationData));
            var next = ptr;
            for (var i = 0; i < count; ++i)
            {
                dataArray[i].data = (iOSNotificationData)Marshal.PtrToStructure(next, typeof(iOSNotificationData));
                dataArray[i].userInfo = NSDictionaryToCs(dataArray[i].data.userInfo);
                dataArray[i].attachments = AttachmentsNSArrayToCs(dataArray[i].data.attachments);
                next = next + structSize;
            }
            _FreeUnmanagediOSNotificationDataArray(ptr, count);

            return dataArray;
        }

        public static IntPtr CsDictionaryToObjC(Dictionary<string, string> userInfo)
        {
            if (userInfo == null)
                return IntPtr.Zero;

            IntPtr dict = IntPtr.Zero;
            foreach (var item in userInfo)
                dict = _AddItemToNSDictionary(dict, item.Key, item.Value);
            return dict;
        }

        public static IntPtr CsAttachmentsToObjc(List<iOSNotificationAttachment> attachments)
        {
            if (attachments == null || attachments.Count == 0)
                return IntPtr.Zero;

            var atts = IntPtr.Zero;
            foreach (var attachment in attachments)
            {
                IntPtr error;
                atts = _AddAttachmentToNSArray(atts, attachment.Id, attachment.Url, out error);
                if (error != IntPtr.Zero)
                {
                    if (atts != IntPtr.Zero)
                        _ReleaseNSObject(atts);
                    var msg = _NSErrorToMessage(error);
                    throw new Exception(msg);
                }
            }

            return atts;
        }

        public static Dictionary<string, string> NSDictionaryToCs(IntPtr dict)
        {
            var ret = new Dictionary<string, string>();
            var handle = GCHandle.Alloc(ret);
            _ReadNSDictionary(GCHandle.ToIntPtr(handle), dict, ReceiveNSDictionaryKeyValue);
            handle.Free();
            return ret;
        }

        public static List<iOSNotificationAttachment> AttachmentsNSArrayToCs(IntPtr array)
        {
            if (array == IntPtr.Zero)
                return null;
            var ret = new List<iOSNotificationAttachment>();
            var handle = GCHandle.Alloc(ret);
            _ReadAttachmentsNSArray(GCHandle.ToIntPtr(handle), array, ReceiveUNNotificationAttachment);
            handle.Free();
            return ret;
        }

        public static void SetApplicationBadge(int badge)
        {
            _SetApplicationBadge(badge);
        }

        public static int GetApplicationBadge()
        {
            return _GetApplicationBadge();
        }

        public static bool GetAppOpenedUsingNotification()
        {
            return _GetAppOpenedUsingNotification();
        }

        public static iOSNotificationWithUserInfo? GetLastNotificationData()
        {
            if (_GetAppOpenedUsingNotification())
            {
                IntPtr ptr = _GetLastNotificationData();

                if (ptr != IntPtr.Zero)
                {
                    iOSNotificationWithUserInfo data;
                    data.data = (iOSNotificationData)Marshal.PtrToStructure(ptr, typeof(iOSNotificationData));
                    data.userInfo = NSDictionaryToCs(data.data.userInfo);
                    data.data.userInfo = IntPtr.Zero;
                    data.attachments = AttachmentsNSArrayToCs(data.data.attachments);
                    data.data.attachments = IntPtr.Zero;
                    _FreeUnmanagediOSNotificationDataArray(ptr, 1);
                    return data;
                }
            }
            return null;
        }

        public static void SetNotificationCategories(IEnumerable<iOSNotificationCategory> categories)
        {
            var allActions = new Dictionary<string, IntPtr>();
            foreach (var category in categories)
            {
                foreach (var action in category.Actions)
                {
                    if (string.IsNullOrEmpty(action.Id))
                        throw new ArgumentException("Action must have a valid and unique ID");
                    if (!allActions.ContainsKey(action.Id))
                        allActions[action.Id] = action.CreateUNNotificationAction();
                }
            }

            IntPtr categorySet = IntPtr.Zero;
            foreach (var category in categories)
            {
                IntPtr actions = IntPtr.Zero;
                int count = category.Actions.Length;
                foreach (var action in category.Actions)
                    actions = _AddActionToNSArray(actions, allActions[action.Id], count);
                IntPtr intentIdentifiers = IntPtr.Zero;
                count = category.IntentIdentifiers.Length;
                foreach (var idr in category.IntentIdentifiers)
                    intentIdentifiers = _AddStringToNSArray(intentIdentifiers, idr, count);
                var cat = _CreateUNNotificationCategory(category.Id, category.HiddenPreviewsBodyPlaceholder, category.SummaryFormat, (int)category.Options,
                    actions, intentIdentifiers);
                categorySet = _AddCategoryToCategorySet(categorySet, cat);
            }

            _SetNotificationCategories(categorySet);

            foreach (var act in allActions)
                _ReleaseNSObject(act.Value);
        }
    }
}
#pragma warning restore 162
#endif
