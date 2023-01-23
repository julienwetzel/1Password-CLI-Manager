using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using System.Xml.Linq;

using HtmlAgilityPack;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32.TaskScheduler;

using Services.OnePasswordCli;

using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Classes
{
    public static class Cmd
    {
        /// <summary> Démarre un exécutable en mode admin </summary>
        /// <param name="path"> Le chemin d'accès à l'exécutable. </param>
        /// <param name="args"> L'argument à passer à l'éxecutable. </param>
        /// <returns> L'objet Process </returns>
        [SupportedOSPlatform("windows")]
        public static object RunAs(string path, string args = default)
        {
            // Message d'exception si le path n'est pas renseigné
            if (path == null) { throw new ArgumentNullException("path", "The path parameter can't be null"); }

            // Si un argument est passé pour la commande, l'ajouter dans le path
            if (args != null) { path = $"{path} {args}"; }

            ProcessStartInfo proc = new()
            {
                FileName = path,
                Arguments = "",
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };

            try
            {
                return Process.Start(proc);
            }
            catch (Exception ex)
            {
                throw new PrivilegeNotHeldException($"The following command failed to be executed as an administrator: \n{path} {args}", ex);
            }
        }

        /// <summary> Vérifie si la valeur reçue est une URL </summary>
        /// <param name="url"> L'url à tester. </param>
        /// <returns> true si vrai </returns>
        public static bool IsUrl(string url)
        {
            // Essayez de créer un Uri à partir de la chaîne de caractères
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary> Vérifie si la variable reçue est un chemin de dossier </summary>
        /// <param name="path"> Le chemin à tester </param>
        /// <returns> true si vrai </returns>
        public static bool IsPath(string path)
        {
            return Path.IsPathRooted(path) && !Path.GetInvalidPathChars().Any(path.Contains);
        }

        /// <summary> Vérifie si la valeur reçue est une version </summary>
        /// <param name="version"> Valeur à tester </param>
        /// <returns> true si vrai </returns>
        public static bool IsVersion(string version)
        {
            return Version.TryParse(version, out _);
        }

        /// <summary> Vérifie si le programme fonctionne avec une élévation de privilège </summary>
        /// <returns> true si vrai </returns>
        [SupportedOSPlatform("windows")]
        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary> Vérifie si le site web est accessible</summary>
        /// <returns> true si vrai</returns>
        public static bool IsWebAvailable(string link)
        {
            using (WebClient client = new())
            {
                try { client.DownloadData(link); }
                catch
                {
                    Console.WriteLine("[Error]\tError link : " + link);
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Vérifie si un fichier existe dans un des répertoires du PATH
        /// </summary>
        /// <param name="fileName">Nom du fichier à rechercher</param>
        /// <returns>true s'il existe</returns>
        public static bool ExistsOnPath(string fileName)
        {
            // Récupère les chemins du fichier
            (string fullPath, string dirPath) = GetPaths(fileName);

            // Vérifie si le chemin complet n'est pas nul
            return !string.IsNullOrEmpty(fullPath);
        }

        /// <summary>
        /// Cette méthode recherche le chemin complet et le chemin du répertoire d'un fichier
        /// </summary>
        /// <param name="fileName">Nom du fichier à rechercher</param>
        /// <returns>Chemin complet et chemin du répertoire du fichier</returns>
        public static (string fullPath, string dirPath) GetPaths(string fileName)
        {
            // Vérifie si le fichier existe dans le système de fichiers
            if (File.Exists(fileName))
            {
                // Retourne le chemin complet et le répertoire du fichier 
                return (Path.GetFullPath(fileName), Path.GetDirectoryName(fileName));
            }

            // Récupère les chemins de la variable d'environnement PATH
            string values = Environment.GetEnvironmentVariable("PATH");
            foreach (string path in values.Split(Path.PathSeparator))
            {
                // Construit le chemin complet du fichier
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    // Retourne le chemin complet et le répertoire du fichier
                    return (fullPath, path);
                }
            }
            // Retourne null si le fichier n'existe pas
            return (null, null);
        }

        /// <summary>
        /// Extraire le contenu d'un fichier ZIP sur le web dans un dossier local
        /// </summary>
        /// <param name="url">URL du fichier zip</param>
        /// <param name="path">Chemin d'accès du dossier d'extraction</param>
        public static void ExtractOnlineZip(string url, string path)
        {
            // Messages d'exception pour les arguments
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new UriFormatException("Le paramètre d'URL doit être une URL valide");
            }

            // Vérifie que le chemin spécifié est un chemin absolu valide.
            if (!Path.IsPathRooted(path) || path.Any(c => Path.GetInvalidPathChars().Contains(c)))
            {
                throw new UriFormatException("Le paramètre de chemin d'accès doit être une chaîne de chemin absolu valide");
            }


            // Créer le répertoire extraction s'il n'existe pas
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Ajouter le nom du fichier au répertoire d'extraction
            string zipFilePath = Path.Combine(path, "temp_op.zip");

            // Télécharger le fichier zip à l'aide du HttpClient
            using (HttpClient client = new())
            {
                using Stream stream = client.GetStreamAsync(url).Result;
                using FileStream fs = new(zipFilePath, FileMode.Create);
                stream.CopyTo(fs);
            }

            // Extraire le fichier zip dans le répertoire
            using (ZipArchive zip = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    entry.ExtractToFile(Path.Combine(path, entry.FullName), true);
                }
            }

            // Supprimer le fichier zip
            File.Delete(zipFilePath);
        }
    }
}
