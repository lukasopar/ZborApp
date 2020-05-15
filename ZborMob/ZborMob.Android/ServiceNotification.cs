using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Microsoft.AspNetCore.SignalR.Client;
using ZborDataStandard.Model;

namespace ZborMob.Droid
{
    [Service]
    public class ServiceNotification : Service
    {
        // A notification requires an id that is unique to the application.
        const int NOTIFICATION_ID = 9000;
        private HubConnection hubConnection;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // Code omitted for clarity - here is where the service would do something.
            var uri = $"{App.BackendUrl}/" + "chatHub";

            hubConnection = new HubConnectionBuilder()
                .WithUrl(uri, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(App.Token);
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // bypass SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };
                })
                .Build();
            hubConnection.On<Poruka, string>("ReceiveMessageMob", async (poruka, ime) =>
            {
                Notification.Builder notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.icon)
                .SetContentTitle("Zboris")
                .SetContentText("Nove poruke");

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(poruka.IdRazgovor.GetHashCode(), notificationBuilder.Build());
            });
            hubConnection.On<Razgovor, string>("ReceiveNewConversationMob", async (razg, ime) =>
            {
                Notification.Builder notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.icon)
                .SetContentTitle("Zboris")
                .SetContentText("Nove poruke");

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(razg.Id.GetHashCode(), notificationBuilder.Build());

            });
            hubConnection.StartAsync();


            string NOTIFICATION_CHANNEL_ID = "12334";
            string channelName = "ServiceHub";
            NotificationChannel chan = new NotificationChannel(NOTIFICATION_CHANNEL_ID, channelName, NotificationManager.ImportanceNone);
            NotificationManager manager = global::Android.App.Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            manager.CreateNotificationChannel(chan);

            NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);
            Notification notification = notificationBuilder.SetOngoing(true)
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetContentTitle("App is running in background")
                        .SetPriority((int)NotificationPriority.High)
                    .Build();
            StartForeground(2, notification);





            return StartCommandResult.Sticky;
            // Work has finished, now dispatch anotification to let the user know.

        }
    }
}