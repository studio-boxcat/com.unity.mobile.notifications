# https://github.com/Unity-Technologies/com.unity.mobile.notifications/blob/master/com.unity.mobile.notifications/Runtime/Android/Plugins/mobilenotifications.androidlib/proguard-rules.pro
# Accessed from C# code
-keep class com.unity.androidnotifications.UnityNotificationManager { public *; }
-keep class com.unity.androidnotifications.NotificationChannelWrapper { public *; }
-keep interface com.unity.androidnotifications.NotificationCallback { *; }