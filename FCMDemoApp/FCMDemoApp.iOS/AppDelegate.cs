using System;
using Foundation;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Firebase.CloudMessaging;

namespace FCMDemoApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            try
            {
                Firebase.Core.App.Configure();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            LoadApplication(new App());
            try
            {
                // Register your app for remote notifications.
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {

                    // For iOS 10 display notification (sent via APNS)
                    UNUserNotificationCenter.Current.Delegate = this;

                    var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                    UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) => {
                        Console.WriteLine(granted);
                    });
                }
                else
                {
                    // iOS 9 or before
                    var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                    var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                    UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
                }

                UIApplication.SharedApplication.RegisterForRemoteNotifications();
                // To connect with FCM. FCM manages the connection, closing it
                // when your app goes into the background and reopening it 
                // whenever the app is foregrounded.
                Messaging.SharedInstance.ShouldEstablishDirectChannel = true;
                Messaging.SharedInstance.Delegate = this;
                //Messaging.SharedInstance.SubscribeAsync("news");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return base.FinishedLaunching(app, options);
        }
        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            System.Diagnostics.Debug.WriteLine($"Livecycle check: DidRefreshRegistrationToken FCM Token: {fcmToken}");
            //Do something with the fcmToken
            Messaging.SharedInstance.SubscribeAsync("news");
        }
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary message, Action<UIBackgroundFetchResult> completionHandler)
        {
            System.Diagnostics.Debug.WriteLine($"Livecycle check: DidReceiveRemoteNotification: {message}");
            HandleMessage(message);
        }
        [Export("messaging:didReceiveMessage:")]
        public void DidReceiveMessage(Messaging messaging, RemoteMessage remoteMessage)
        {
            System.Diagnostics.Debug.WriteLine($"Livecycle check: DidReceiveMessage: {remoteMessage.AppData.ToString()}");
        }

        private void HandleMessage(NSDictionary message)
        {
            NSDictionary aps = message.ObjectForKey(new NSString("aps")) as NSDictionary;

            string alert = string.Empty;
            if (aps.ContainsKey(new NSString("alert")))
            {
                NSDictionary alertDic = (aps[new NSString("alert")] as NSDictionary);
                if (alertDic.ContainsKey(new NSString("body")))
                {
                    alert = (alertDic[new NSString("body")] as NSString).ToString();
                }
            }

            //Do something with the alert
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            Messaging.SharedInstance.ApnsToken = deviceToken;
        }
    }
}
