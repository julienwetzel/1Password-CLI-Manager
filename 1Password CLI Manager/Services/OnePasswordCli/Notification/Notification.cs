using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using HtmlAgilityPack;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32.TaskScheduler;

using Services.OnePasswordCli;
using Services.OnePasswordCli.Notification;

using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Path = System.IO.Path;

namespace Services.OnePasswordCli.Notification
{
    /// <summary>
    /// The notification.
    /// </summary>
    static public class Notification
    {
        /// <summary> Affiche une erreur </summary>
        /// <param name="message"> Message de l'erreur à afficher </param>
        [SupportedOSPlatform("windows")]
        static void ShowErrorNotification(string message)
        {
            ToastContent toastContent = new()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                {
                    new AdaptiveText()
                    {
                        Text = message
                    }
                }
                    }
                }
            };

            //ToastNotification toast = new ToastNotification(toastContent.GetXml());
            //ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        /// <summary>
        /// Shows the update notification.
        /// </summary>
        [SupportedOSPlatform("windows")]
        static public void ShowUpdate()
        {
            Console.WriteLine("Notify");
            // Efface l'ancienne notification
            ToastNotificationManagerCompat.History.Clear();

            string arguments = "update";

            // Configuration de la notification
            new ToastContentBuilder()
                .AddHeader("OnePasswordCliUpdateNotification", "1Password CLI", arguments)
                .AddText("New version are available")
                //.AddArgument("action", "viewConversation")
                //.AddArgument("conversationId", 9813234427)
                //.AddText(OnePasswordCli.Print.Versions)
                //.AddAppLogoOverride(Constants.Project.LogoDefaultPath)
                .AddButton("btnUpdate", "Update", ToastActivationType.Background, arguments)
                .SetToastScenario(ToastScenario.Default)
                .SetBackgroundActivation()

            // Envoie de la notification
            .Show();
        }
    }
}
