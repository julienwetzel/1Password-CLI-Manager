using System;
using System.Collections.Generic;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Classes;

public sealed class Constants
{
    // La classe de Constants est marquée avec le mot-clé 'sealed' 
    // signifiant que l'on ne peut pas hériter de cette classe. 



    // Variables système
    // cette classe contient des variables système typiques tel que Arch.
    // La classe de Constants est marquée avec le mot-clé 'sealed' 
    // signifiant que l'on ne peut pas hériter de cette classe. 

    // Variables système
    // cette classe contient des variables système typiques tel que Arch.
    public static class System
    {
        // Arch stocke des informations sur l'architecture du système d'exploitation (exemple : x64)
        public static string Arch => RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
    }
    // Variables de projet
    // Cette classe stocke des informations sur le projet 
    // telles que le nom de l'application, sa version et son chemin absolu.
    public static class Project
    {
        // Name stocke le nom de l'application
        //((AssemblyTitleAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title
        public static string Name => AccessAssembly.ProductTitle;
        // Version stocke la version de l'application
        public static string Version => AccessAssembly.ProductVersion;
        // ExePath stocke le chemin absolu de l'exécution de l'assembly
        public static string ExePath => Assembly.GetEntryAssembly().Location;
        // DirPath stocke le chemin absolu du dossier d'exécution de l'assembly
        public static string DirPath => AppDomain.CurrentDomain.BaseDirectory;
        // LogoDefaultPath stocke le chemin par défaut du logo de l'application
        public static string LogoDefaultPath => Path.Combine(Environment.CurrentDirectory, "Images", "logo192.png");
    }

    // La méthode Instance est utilisé pour retourner la seule instance 
    // de l'objet créé à l'aide du constructeur par défaut
    //public static Constants Instance => new();
}

