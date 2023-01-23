using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
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

using Spectre.Console;

using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Path = System.IO.Path;

namespace opcli
{
    /// <summary>
    /// The program.
    /// </summary>S

    static internal class Program
    {
        /// <summary>
        /// TODO: Add Summary
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns><![CDATA[Task<int>]]></returns>
        static async Task<int> Main(string[] args)
        {
            Option<bool> cliVersionsOption = new(
                name: "--versions",
                description: "Displays the local and online version of 1Password CLI",
                parseArgument: result => { return true; }
            );

            Option<bool> cliConnectOption = new(
                name: "--connect",
                description: "Checks the connection with 1Password web services",
                parseArgument: result => { return true; }
            );

            Option<string> directoryOption = new(
                name: "--directory",
                description: "Optional option to specify a directory.",
                parseArgument: result => { return result.Tokens.Single().Value; }
            );

            Option<string> versionOption = new(
                name: "--version",
                description: "Optional option to specify a version.",
                parseArgument: result => { return result.Tokens.Single().Value; }
            );

            Command cliCommand = new("cli", "Interaction with 1Password CLI")
            {
                cliVersionsOption,
                cliConnectOption
            };

            Command updateCommand = new("update", "1Password CLI update") {
                directoryOption,
                versionOption
            };

            Command installCommand = new("install", "Install 1Password CLI") {
                directoryOption,
                versionOption
            };

            Command notifyCommand = new("notify", "Checks and sends a notification if a new version is available")
            {

            };

            // Brève description du programme
            string rootCommandText = $"{Constants.Project.Name}";

            // Définit les commandes à la racine
            RootCommand rootCommand = new(rootCommandText) {
                notifyCommand,
                cliCommand
            };

            // Ajoute la sous-commande update à cli
            cliCommand.AddCommand(updateCommand);
            // Ajoute la sous-commande install à cli
            cliCommand.AddCommand(installCommand);

            cliCommand.SetHandler((cliVersionsOptionValue, cliConnectOptionValue) =>
            {
                Console.WriteLine("Hello cli!");
                // Si l'option de versions est activée, afficher le tableau de versions
                if (cliVersionsOptionValue) { Console.WriteLine(OnePasswordCli.Print.Versions); }
                // Si l'option de connexion est activée, afficher le tableau des statuts de connexion
                if (cliConnectOptionValue) { Console.WriteLine(OnePasswordCli.Print.Network); }
            }, cliVersionsOption, cliConnectOption);

            updateCommand.SetHandler(() =>
            {
                Console.WriteLine("Hello update!");
            });

            installCommand.SetHandler(() =>
            {
                Console.WriteLine("Hello install!");
            });

            notifyCommand.SetHandler(() =>
            {
                if (OnePasswordCli.Online.IsUpdateAvailable)
                {
                    Console.WriteLine("[Info]\tStart notification to Windows desktop");
                    // Affiche une notification indiquant qu'une mise à jour est disponible et offrant la possibilité de mettre à jour
                    if (OperatingSystem.IsWindows()) // standard guard examples
                    {
                        Notification.ShowUpdate();
                        //ShowUpdateNotification();
                    }

                }
            });

            //Invoker la commande racine
            return await rootCommand.InvokeAsync(args);
        }



        //            case "--dir":
        //                string dir = args[i + 1];
        //                //match = Regex.IsMatch(dir, @"^(?:[a-zA-Z]:\\{2})(?:(?:\w+(?:\\{1,2})?))*$");
        //                //if (match) { directory = dir; } else { ShowHelp(); }
        //                break;

        //            case "--version":
        //                string ver = args[i + 1];
        //                //Console.WriteLine(" Project name: " + blabla + "\n Version: " + version);
        //                //if (match) { version = ver; } else { ShowHelp(); }
        //                break;


        //    // Écoute l'activation d'une notification
        //    ToastNotificationManagerCompat.OnActivated += toastArgs =>
        //    {
        //        ToastNotificationManagerCompat.Uninstall();
        //        // Obtient les arguments de la notification
        //        ToastArguments args = ToastArguments.Parse(toastArgs.Argument);


        //    // Vérifiez si une mise à jour est disponible
        //    if (localVersion != onlineVersion)
        //    {
        //        Console.WriteLine("[Info]\tStart notification to Windows desktop");
        //        // Affiche une notification indiquant qu'une mise à jour est disponible et offrant la possibilité de mettre à jour
        //        ShowUpdateNotification(localVersion, onlineVersion);
        //    }
        //}

        /// <summary> Affiche une erreur </summary>
        /// <param name="message"> Message de l'erreur à afficher </param>
        //[SupportedOSPlatform("windows")]
        //static void ShowErrorNotification(string message)
        //{
        //    ToastContent toastContent = new()
        //    {
        //        Visual = new ToastVisual()
        //        {
        //            BindingGeneric = new ToastBindingGeneric()
        //            {
        //                Children =
        //        {
        //            new AdaptiveText()
        //            {
        //                Text = message
        //            }
        //        }
        //            }
        //        }
        //    };

        //    //ToastNotification toast = new ToastNotification(toastContent.GetXml());
        //    //ToastNotificationManager.CreateToastNotifier().Show(toast);
        //}

        ///// <summary> Affiche une notification indiquant qu'une mise à jour est disponible </summary>
        ///// <param name="localVersion"> Valeur de de version local </param>
        ///// <param name="onlineVersion"> Valeur de la version online </param>
        //[SupportedOSPlatform("windows")]
        //static void ShowUpdateNotification()
        //{
        //    // Efface l'ancienne notification
        //    ToastNotificationManagerCompat.History.Clear();

        //    // Configuration de la notification
        //    new ToastContentBuilder()
        //        .AddHeader("1PCliUpdateNotification", "1Password CLI", "update")
        //        //.AddArgument("action", "viewConversation")
        //        //.AddArgument("conversationId", 9813234427)
        //        .AddText("New version are available")
        //        //.AddText("Local op = " + localVersion)
        //        //.AddText("Online op = " + onlineVersion)
        //        //.AddAppLogoOverride(Constants.Project.LogoDefaultPath)
        //        .AddButton("1pcUpdate", "Update", ToastActivationType.Background, "update")
        //        .SetToastScenario(ToastScenario.Default)
        //        .SetBackgroundActivation()

        //    // Envoie de la notification
        //    .Show();
        //}





    }
}