using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ABI.Windows.UI;

using BetterConsoles.Colors.Extensions;

using BetterConsoleTables;

using Classes;

using HtmlAgilityPack;

using Microsoft.AspNetCore.Http.Features;

using Windows.System;
using Windows.UI;

using Color = System.Drawing.Color;

namespace Services.OnePasswordCli
{
    /// <summary>
    /// The OnePasswordCli class is a static class for the OnePassword command line
    /// tool.
    /// </summary>
    public static class OnePasswordCli
    {
        /// <summary>
        /// The ExecutableName property is a string that contains the name of the 
        /// executable.
        /// </summary>
        /// <value>A string.</value>
        public static string ExecutableName => "op.exe";

        /// <summary>
        /// The Local subclass contains properties and methods related of the 
        /// OnePassword tool.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         To access the information, simply enter the following code to access the property values 
        ///         <code>OnePasswordCli.Local.&lt;Property&gt;</code>
        ///         <br/>Properties:
        ///         <list type="bullet">
        ///             <item>ProgramDirectory</item>
        ///             <item>AbsolutePath</item>
        ///             <item>Version</item>
        ///             <item>IsAvailable</item>
        ///         </list>
        ///     </para>
        /// </remarks>
        public static class Local
        {
            /// <summary>
            /// The ProgramDirectory property is a string that contains the path 
            /// to the local program directory.
            /// </summary>
            /// <value>A string.</value>
            public static string ProgramDirectory => GetLocalProgramDirectory();

            /// <summary>
            /// The AbsolutePath property is a string that contains the absolute path 
            /// of the OnePassword tool.
            /// </summary>
            /// <value>A string.</value>
            public static string AbsolutePath => GetLocalAbsolutePath();

            /// <summary>
            /// The Version property is a string that contains the version of the local
            /// version of the OnePassword tool.
            /// </summary>
            /// <value>A string.</value>
            public static string Version => GetLocalVersion();

            /// <summary>
            /// The IsAvailable property is a bool that determines if the OnePassword 
            /// tool is installed.
            /// </summary>
            /// <value>A bool.</value>
            public static bool IsAvailable => IsLocalAvailablePath();
        }

        /// <summary>
        /// The Online subclass contains properties and methods related to the online 
        /// version of the OnePassword tool.
        /// </summary>
        public static class Online
        {
            /// <summary>
            /// The Version property is a string that contains the version of the online 
            /// version of the OnePassword tool.
            /// </summary>
            /// <value>A string.</value>
            public static string Version => GetOnlineVersion();

            /// <summary>
            /// The IsReachable property is a bool that determines if the online version 
            /// of the OnePassword tool is reachable.
            /// </summary>
            /// <value>A bool.</value>
            public static bool IsReachable => IsOnlineReachable();

            /// <summary>
            /// The IsUpdateAvailable property is a bool that determines if a new version is 
            /// available
            /// </summary>
            /// <value>A bool.</value>
            public static bool IsUpdateAvailable => IsOnlineUpdateAvailable();

            /// <summary>
            /// The UriDownload method a string that contains the path to download the 
            /// online version of the OnePassword tool.
            /// </summary>
            /// <param name="v">The v.</param>
            /// <value>A string.</value>
            public static string UriDownload(string v = default)
            {
                return GetUriDownload(v);
            }

            /// <summary>
            /// The UriVersion property is a string that contains the URI to get the 
            /// online version of the OnePassword tool.
            /// </summary>
            /// <value>A string.</value>
            public static string UriVersion => "https://app-updates.agilebits.com/product_history/CLI2";

            /// <summary>
            /// The UriDomainDownload property is a string that contains the URI for the 
            /// domain download for the online version of the OnePassword tool.
            /// </summary>
            /// <value>A string.</value>
            public static string UriDomainDownload => "https://cache.agilebits.com";
        }

        /// <summary>
        /// The Print subclass contains properties and methods related to printing 
        /// information about the OnePassword tool.
        /// </summary>
        public static class Print
        {
            /// <summary>
            /// The Versions property is a string that contains the formatting for the 
            /// versions table.
            /// </summary>
            /// <value>A string.</value>
            public static string Versions => GetVersionsTableFormat();

            //Etat du réseau
            /// <summary>
            /// The Network property is a string that contains the network status of the 
            /// online version of the OnePassword tool.
            /// </summary>
            /// <value>A string.</value>
            public static string Network => GetOnlineWebStatus();
        }

        /// <summary>
        /// Récupération du répertoire local du programme 
        /// </summary>
        /// <returns> Chemin en string </returns>
        private static string GetLocalProgramDirectory()
        {
            // Utilisation de Cmd.GetPaths pour récupérer le chemin
            (_, string dirPath) = Cmd.GetPaths(ExecutableName);

            // Retour du chemin du répertoire
            return dirPath;
        }

        /// <summary>
        /// Récupération du chemin absolu du programme
        /// </summary>
        /// <returns> Chemin en string </returns>
        private static string GetLocalAbsolutePath()
        {
            // Utilisation de Cmd.GetPaths pour récupérer le chemin absolu
            (string fullPath, _) = Cmd.GetPaths(ExecutableName);

            // Retour du chemin absolu
            return fullPath;
        }

        /// <summary>
        /// Récupération de la version locale
        /// </summary>
        /// <returns> version en string </returns>
        private static string GetLocalVersion()
        {
            // Déclaration de la variable version
            string version;

            try
            {
                // Définissez la commande "op" avec l'option "--version"
                Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "op.exe",
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                // Exécutez la commande "op"
                process.Start();

                // Récupérez la sortie de la commande "op" et stockez-le dans la variable version
                version = process.StandardOutput.ReadToEnd().Trim();
            }
            catch
            {
                // Si une exception est levée, on retourne null
                return null;
            }

            // On retourne le résultat de la version
            return version;
        }

        /// <summary>
        /// Vérifie si le chemin vers le programme est disponible
        /// </summary>
        /// <returns> true si oui </returns>
        private static bool IsLocalAvailablePath()
        {
            // Vérifier si le programme est disponible dans le PATH
            return Cmd.ExistsOnPath("op.exe");
        }

        /// <summary>
        /// Méthode pour récupérer la version sur le web
        /// </summary>
        /// <returns> Numéro de version en string </returns>
        private static string GetOnlineVersionOnWeb()
        {
            // Récupérer le contenu HTML à partir de l'URL UriVersion
            string responseBody = null;
            using (HttpClient client = new())
            {
                HttpResponseMessage response = client.GetAsync(Online.UriVersion).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    HttpContent responseContent = response.Content;
                    responseBody = responseContent.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }

            // Charger la réponse HTML dans htmlDoc
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(responseBody);

            // Sélectionne le premier tag HTML h3 de la page
            HtmlNode h3Node = htmlDoc.DocumentNode.SelectSingleNode("//h3");

            // Sélectionne le premier tag span enfant du tag h3
            HtmlNode spanNode = h3Node.SelectSingleNode("./span");

            // Supprimer le contenu du tag span
            spanNode.Remove();

            // Obtenir le contenu à l'intérieur du tag h3
            string versionContent = h3Node.InnerText.Trim();

            // Essayer de valider si le contenu est un numéro de version
            bool validVersion = Version.TryParse(versionContent, out Version onlineVersion);

            //Lancer une exception si la valeur reçue d'Internet
            // n'est pas un numéro de version
            if (!validVersion)
            {
                throw new InvalidDataException($"The value returned is not a version: \n\"{versionContent}\"");
            }

            // Retourner la version à l'intérieur du tag h3
            return onlineVersion.ToString();
        }

        /// <summary>
        /// Méthode pour récupérer la version sur le programme
        /// </summary>
        /// <returns> Numéro de version en string </returns>
        private static string GetOnlineVersionOnCli()
        {
            // Exécuter la commande "op" avec l'option "--version"
            Process process = new();
            process.StartInfo.FileName = "op.exe";
            process.StartInfo.Arguments = "update";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();

            // Répondre négativement à la question de mise à jour
            StreamWriter writer = process.StandardInput;
            writer.WriteLine("n");
            writer.Dispose();

            // Récupérer la version en ligne indiquée dans la commande op
            string opOutput = process.StandardOutput.ReadToEnd();

            // Créer une expression régulière qui recherche le texte "Version" suivi d'un espace et d'un numéro de version valide
            Regex opRegex = new(@"Version\s+(\d+\.\d+\.\d+)");

            // Appliquer l'expression régulière à la sortie de la commande
            Match match = opRegex.Match(opOutput);

            // Exécution de la commande 'op' avec l'option '--version'
            // Réponse négative à la question de mise à jour
            // Récupération de la version en ligne indiquée par la commande 'op'
            // Création d'une expression régulière pour rechercher le texte 'Version' suivi d'un espace et d'un numéro de version valide
            // Application de l'expression régulière à la sortie de la commande

            // Si l'expression régulière n'a pas trouvé de correspondance ou que la valeur n'a pas le format de version valide 
            if (!match.Success || !Version.TryParse(match.Groups[1].Value, out _)) { return null; }

            // Si l'expression régulière a trouvé une correspondance et que la valeur a un format de version valide 
            // Retourne le numéro de version
            return match.Groups[1].Value.Trim();
        }

        /// <summary>
        /// Méthode pour récupérer la version en ligne
        /// </summary>
        /// <returns> Numéro de version en string </returns>
        private static string GetOnlineVersion()
        {
            //Vérification de la disponibilité du programme localement et connexion à l'adresse UriVersion pour récupérer la version
            if (Local.IsAvailable && !string.IsNullOrEmpty(GetOnlineVersionOnCli()))
            {
                return GetOnlineVersionOnCli();
            }

            //Vérification de la disponibilité du programme localement et msg si la version n'est pas récupérée
            if (Local.IsAvailable && string.IsNullOrEmpty(GetOnlineVersionOnCli()))
            {
                Console.WriteLine($"[WARN]\t{ExecutableName} is available " +
            "but no longer returns the online version.");
            }

            //Vérification de la version en ligne et retour de son valeur si non nul
            if (!string.IsNullOrEmpty(GetOnlineVersionOnWeb()))
            {
                return GetOnlineVersionOnWeb();
            }

            //Lancement d'une exception si aucune version en ligne n'est récupérée
            if (string.IsNullOrEmpty(GetOnlineVersionOnWeb()))
            {
                throw new NullReferenceException($"An error s'est produit lors de la récupération de la version {ExecutableName} à partir du web.");
            }

            //Renvoi null si aucune version n'est récupérée
            return null;
        }

        /// <summary>
        /// Cette méthode retourne un tableau sur les statuts de connexion
        /// </summary>
        /// <returns> Tableau en string </returns>
        private static string GetOnlineWebStatus()
        {
            // Détermine si la connexion à l'URI de téléchargement est disponible
            bool isDownladReachable = Cmd.IsWebAvailable(Online.UriDomainDownload);

            // Détermine si la connexion à l'URI de version est disponible
            bool isVersionReachable = Cmd.IsWebAvailable(Online.UriVersion);

            // Affecte le statut "OK" à la chaîne pour le téléchargement si la connexion est disponible, sinon "FAIL"
            string downloadStatus = isDownladReachable ? "OK" : "FAIL";

            // Affecte le statut "OK" à la chaîne pour la version si la connexion est disponible, sinon "FAIL"
            string versionStatus = isVersionReachable ? "OK" : "FAIL";

            // Crée le tableau avec les colonnes "URI" et "Statut"
            Table table = new("URI", "Status");

            // Ajoute une ligne à la table avec le statut du lien de téléchargement
            table.AddRow("Download", downloadStatus)
            // Ajoute une ligne à la table avec le statut du lien des versionsS
                 .AddRow("Version", versionStatus);

            // Configure le tableau
            table.Config = TableConfiguration.Unicode();

            // Retourne une chaîne représentant le tableau
            return table.ToString();
        }

        /// <summary>
        /// Cette méthode retourne une valeur booléenne selon si les sites web de OnePassword sont disponibles
        /// ou non
        /// </summary>
        /// <returns> 
        ///     <para>
        ///         <b>True</b>&#160;&#160;Websites are reachable<br/>
        ///         <b>False</b> Websites not reachable
        ///     </para> 
        /// </returns>
        private static bool IsOnlineReachable()
        {
            // Vérifie si les URI sont disponibles
            return Cmd.IsWebAvailable(Online.UriDomainDownload) && Cmd.IsWebAvailable(Online.UriVersion);
        }

        /// <summary> 
        /// Cette méthode retourne l'URI de téléchargement du programme spécifié
        /// Utilise l'architecture spécifiée et la version en ligne
        /// </summary>
        /// <param name="version"> Version de l'application </param>
        /// <returns> Lien de téléchargement </returns>
        private static string GetUriDownload(string version)
        {
            // Récupère l'architecture de la machine
            string arch = Constants.System.Arch ?? throw new NullReferenceException("The value to the architecture cannot be null.");

            // Utilise la version en ligne si pas spécifiée
            version ??= Online.Version ?? throw new NullReferenceException("The value for the version cannot be null.");

            // Retourne l'URI générée
            return $"{Online.UriDomainDownload}/dist/1P/op2/pkg/v{version}/op_windows_{arch}_v{version}.zip";
        }

        /// <summary> 
        /// Cette méthode retourne un tableau avec la version Online
        /// disponible et la version locale actuel
        /// </summary>
        /// <returns> Tableau en string </returns>
        private static string GetVersionsTableFormat()
        {
            // Récupère la version locale
            string versionLocal = Local.Version;

            // Récupère la version en ligne
            string versionOnline = Online.Version;

            // Crée le tableau avec les colonnes "Version" et "Statut"  
            Table table = new("Version", "Status");

            // Ajoute une ligne à la table avec la version locale 
            table.AddRow("Local", versionLocal)

            // Ajoute une ligne à la table avec la version en ligne	
                 .AddRow("Online", versionOnline);

            // Configure le tableau 
            table.Config = TableConfiguration.Unicode();

            // Retourne une chaîne représentant le tableau
            return table.ToString();
        }

        /// <summary>
        /// Cette méthode retourne une valeur booléenne
        /// </summary>
        /// <returns>A bool.</returns>
        private static bool IsOnlineUpdateAvailable()
        {
            return OnePasswordCli.Local.Version != OnePasswordCli.Online.Version;
        }
    }
}
