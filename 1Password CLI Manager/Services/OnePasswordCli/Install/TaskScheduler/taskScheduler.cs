using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32.TaskScheduler;

using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Services.Op.Install.TaskScheduler
{
    public static class TaskScheduler
    {
        // Cette fonction ajoute une entrée dans le planificateur de tâche 'Task Scheduler' 
        // pour vérifier régulièrement et automatiquement les mises à jour
        // 
        // Retourne: N/A
        [SupportedOSPlatform("windows")]
        static void TaskSchedulerOp()
        {
            // Output all the tasks in the root folder with their triggers and actions
            TaskFolder tf = TaskService.Instance.RootFolder;
            Console.WriteLine("\nLa liste des tâches dans le dossier racine: ({0})", tf.Tasks.Count);

            foreach (Task t in tf.Tasks)
            {
                try
                {
                    Console.WriteLine("+ {0}, {1} ({2})", t.Name,
                       t.Definition.RegistrationInfo.Author, t.State);
                    foreach (Trigger trg in t.Definition.Triggers)
                    {
                        Console.WriteLine(" + {0}", trg);
                    }

                    foreach (Microsoft.Win32.TaskScheduler.Action act in t.Definition.Actions)
                    {
                        Console.WriteLine(" = {0}", act);
                    }
                }
                catch { Console.WriteLine("[Error]\tErreur lors de la récupération de toutes les tâches"); }
            }
            Console.WriteLine("\n_____Vérification de la création de la tâche_____");

            try
            {
                using TaskService ts = new();
                // Affiche la version maximum supportée
                Console.WriteLine("Version maximale prise en charge des tâches planifiées: " + ts.HighestSupportedVersion);

                // Obtenir le chemin d'accès complet de l'application courante
                string executablePath = Assembly.GetExecutingAssembly().Location;

                // Créer une nouvelle tâche dans le planificateur de tâches
                TaskDefinition td = ts.NewTask();
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.RegistrationInfo.Source = "Mise à jour du CLI 1Password";
                td.RegistrationInfo.Version = new Version(0, 1);
                td.RegistrationInfo.Author = "Julien Wetzel";
                td.RegistrationInfo.Description = "Lancement au démarrage et toutes les 3 heures";
                td.Settings.AllowDemandStart = true;
                td.Settings.AllowHardTerminate = true;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.Enabled = true;
                td.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(10);
                td.Settings.Hidden = false;
                td.Settings.RunOnlyIfIdle = false;
                td.Settings.RunOnlyIfNetworkAvailable = false;
                td.Settings.RunOnlyIfLoggedOn = true;
                td.Settings.Priority = ProcessPriorityClass.Normal;
                td.Settings.Compatibility = TaskCompatibility.V2;
                td.Settings.MultipleInstances = TaskInstancesPolicy.StopExisting;
                td.Settings.StartWhenAvailable = true;
                td.Settings.WakeToRun = false;
                td.Settings.RestartCount = 5;
                td.Settings.RestartInterval = TimeSpan.FromSeconds(100);

                td.Actions.Add(executablePath, null, null);
                td.Triggers.Add(new RegistrationTrigger { Delay = TimeSpan.FromMinutes(3) });
                td.Triggers.Add(new LogonTrigger { Delay = TimeSpan.FromMinutes(3) });

                // Création d'un trigger de répétition toutes les 3 heures indéfiniment
                TimeTrigger tTrigger = td.Triggers.Add(new TimeTrigger());
                tTrigger.StartBoundary = DateTime.Now + TimeSpan.FromMinutes(1);
                tTrigger.ExecutionTimeLimit = TimeSpan.FromSeconds(30);
                tTrigger.Id = "3hRep";
                tTrigger.Repetition.Duration = TimeSpan.FromHours(4);
                tTrigger.Repetition.Interval = TimeSpan.FromHours(3);
                tTrigger.Repetition.StopAtDurationEnd = true;

                // Enregistrement de la tâche
                const string taskName = "Mise à jour du CLI 1Password";
                ts.RootFolder.RegisterTaskDefinition(taskName, td);
                ts.RootFolder.DeleteTask("Elevate", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // Affichage des informations sur la nouvelle tâche créée
            Task runningTask = tf.Tasks["Mise à jour du CLI 1Password"];
            Console.WriteLine("\nLa nouvelle tâche sera lancée à " + runningTask.NextRunTime);
            Console.WriteLine("\nLes déclenchements de la nouvelle tâche:");
            for (int i = 0; i < runningTask.Definition.Triggers.Count; i++)
            {
                Console.WriteLine("  {0}: {1}", i, runningTask.Definition.Triggers[i]);
            }

            Console.WriteLine("\nLes actions de la nouvelle tâche:");
            for (int i = 0; i < runningTask.Definition.Actions.Count; i++)
            {
                Console.WriteLine("  {0}: {1}", i, runningTask.Definition.Actions[i]);
            }
        }
    }
}
