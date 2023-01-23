using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

using Classes;

namespace Services.OnePasswordCli.Update
{
    [SupportedOSPlatform("windows")]
    public static class Update
    {
        /// <summary> Processus de mise à jour </summary>
        /// <param name="version"> Numéro de version à installer </param>
        static void UpdateOp(string version = null)
        {
            // Vérifie que les sites web sont accessibles
            if (!OnePasswordCli.Online.IsReachable) { throw new TimeoutException("We have a connectivity problem with 1Password websites"); }

            // Relance le programme avec les privilèges Admin si elle ne les a pas.
            if (!Cmd.IsAdministrator()) { _ = Cmd.RunAs(Constants.Project.ExePath); }

            // Si auncun argument a été donné, il prend la dernière version disponible
            if (version != null) { version = OnePasswordCli.Online.Version; }

            // Récupère l'architecture du système
            //string arch = Constants.System.Arch;
            string arch = "x64";

            // Définit la variable d'architecture basé sur les valeurs d'architecture de 1Password CLI
            // et lève une exception si l'architecture ne correspond pas.
            string opArch;
            if (arch == "x64") { opArch = "amd64"; }
            else if (arch == "x86") { opArch = "386"; }
            else { throw new TargetException($"Sorry, your operating system architecture '{arch}' is unsupported"); }

            // Affiche dans la console l'architecture du système
            Console.WriteLine("[Info]\tSystem architecture: " + arch);

            // Définit le répertoire d'installation de 1Password CLI
            string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "1Password CLI2");

            // Si le programme est déjà installer, utiliser son répertoire
            //if (IsOpAvailable()) { installDir = Path.GetDirectoryName(Process.Start("op.exe").MainModule.FileName); }

            // Affiche dans la console le chemin du répertoire d'installation d'op
            Console.WriteLine("[Debug]\tPath 1Password CLI installation: " + installDir);

            // Définition de l'URL d'installation
            string url = OnePasswordCli.Online.UriDownload();

            // Installation du fichier ZIP
            Cmd.ExtractOnlineZip(url, installDir);

            // Récupère la valeur actuelle de la variable d'environnement PATH
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

            // Vérifie si le chemin C:\MonProgramme existe déjà dans PATH
            if (!path.Contains(installDir))
            {
                // Ajoute le chemin à PATH
                path += ";" + installDir;

                // Met à jour la valeur de PATH de manière permanente et qui s'applique à toutes les sessions
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
