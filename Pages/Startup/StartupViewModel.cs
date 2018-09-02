using AutoUpdaterDotNET;
using LiteDB;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.AppAssistant;
using Ovresko.Generix.Core.Pages.LanuageChooser;
using Ovresko.Generix.Core.Properties;
using Ovresko.Generix.Datasource.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Unclassified.TxLib;
using static Ovresko.Generix.Core.Translations.OvTranslate; 

namespace Ovresko.Generix.Core.Pages.Startup
{
    public enum StartupMode
    {
        ALL,
        LAUNCH
    }

    internal class StartupViewModel : Screen
    {
        IWindowManager _windowManager;
        private ElvaSettings setting;

        public StartupViewModel(IWindowManager windowManager)
        {
            
            _windowManager = windowManager;
               Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
          
        }

        protected override void OnViewLoaded()
        {

            base.OnViewLoaded();
            // launch
            Worker.RunWorkerAsync();
        }

        
        public string Error { get; set; }

        public string Message { get; set; }

        public StartupMode Mode { get; set; }

        public int Progress { get; set; }

        public BackgroundWorker Worker { get; set; }

        public void CheckForUpdates()
        {
            try
            {
                // Check For Updates
                AutoUpdater.Mandatory = true;
                AutoUpdater.AppTitle = "Ovresko Commerciale Pro";

                AutoUpdater.Start("https://ovresko.com/OvreskoUpdates/OvreskoGenerixCore.xml");
                AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        public void InitSeries()
        {
            try
            {
                // setup series
                FrameworkManager.CreateSeries();
                //var setting = ElvaSettings.getInstance();
                //setting.AppInitialized = true;
                //setting.Save();
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        public void SetDefaultCultureOnThread()
        {
            try
            { 
                setting = ElvaSettings.getInstance();  
                if (!string.IsNullOrWhiteSpace(setting?.Pays))
                {
                    ElvaSettings.SetGlobalCulture(setting?.Pays); 
                } 

            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        private   bool EnsureServerIsRunning()
        {

            return true;
            //var isAlive = DS.db.Context.Database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(15000);
 
            //return isAlive;
        }

        public void InstallDefaultDatabase()
        {
            try
            {  // Prepare database
                var adr = Properties.Settings.Default.MongoServerSettings;
                var db = Properties.Settings.Default.dbUrl;

                bool addDefault = false;
                DbSourceLink DefaultDbSource = new DbSourceLink();
                // CHeck if DefaultDB exist
                var dblinks = DS.db.GetAll<DbSourceLink>(a => true) as IEnumerable<DbSourceLink>;
                if (dblinks != null)
                {
                    if (!dblinks.Select(a => a.DbName).Contains("Default"))
                    {
                        DefaultDbSource = new DbSourceLink();
                        DefaultDbSource.DbName = "Default";
                        DefaultDbSource.SourceIp = adr;
                        addDefault = true;
                    }
                }
                setting = ElvaSettings.getInstance();
                if (setting != null)
                {
                    if (addDefault)
                        DefaultDbSource.Save();

                    if (setting.DbSourceLink.IsValide())
                    {
                        // i have db set
                        var DbSource = DS.db.GetOne<DbSourceLink>(a => a.Id == setting.DbSourceLink) as DbSourceLink;
                        if (DbSource != null)
                        {
                            Properties.Settings.Default.MongoServerSettings = DbSource.SourceIp;
                            Properties.Settings.Default.dbUrl = DbSource.DbName;
                            DataHelpers.DbAdresse = DbSource.SourceIp;
                            DataHelpers.DbName = DbSource.DbName;
                            DS._db = null;
                        }
                        else
                        {
                            SetDefaultDB();
                        }
                    }
                    else
                    {
                        SetDefaultDB();
                    }
                }
                else
                {
                    SetDefaultDB();
                }
            }
            catch (Exception s)
            {
                SetError(s.Message);
                Console.Write(s.Message);
                SetDefaultDB();
            }
        }

        public void InstallDefaultUser()
        {
            try
            {
                FrameworkManager.AdminExists(true);
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        public void LoadModulesDll()
        {
            try
            {
                var path = "Plugins";
                foreach (var item in Directory.EnumerateFiles(path))
                {
                    if (item.Contains(".dll") && !item.Contains(".config"))
                    {
                        Console.WriteLine(item);
                        var Dllpath = Path.GetFullPath(item);
                        Assembly.LoadFrom(Dllpath);
                        //var json = Dllpath.Replace(".dll", ".json");
                        //if (File.Exists(json))
                        //{
                        //    var jsonContent = File.ReadAllText(json);
                        //    PluginConfig config = JsonConvert.DeserializeObject<PluginConfig>(jsonContent);
                        //    if (ModuleErp.PluginIsValide(config?.Name, config?.User, config?.Key))
                        //    {
                        //        Assembly.LoadFrom(Dllpath);
                        //    }
                        //}
                        //else
                        //{
                        //   // SetError($"Le fichier de configuration: {json}\n est introuvable");
                        //}
                    }
                }
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        public void OpenAppAssistant()
        {
            var assistant = new AppAssistantViewModel(_windowManager);
            _windowManager.ShowWindow(assistant);
        }

        public void ReloadAllModules()
        {
            try
            {
                var updateModules = Properties.Settings.Default.UpdateModules;
                FrameworkManager.UpdateModules(updateModules);
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        public void RestoreDatabaseDump()
        {
            try
            {
                if (Settings.Default.UseMongoServer)
                {
                    ElvaSettings.getInstance().RestorData();
                    Thread.Sleep(10000);
                }
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
            //var result =  DataHelpers.ShowMessage("Voulez-vous installer les données initiales (Configuration, Taxes, Paramétres achats & ventes...etc)", "Demo Data", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            //    ElvaSettings.getInstance().RestorData();
            //}
            //else
            //{
            //    // Comptes
            //    FrameworkManager.SetupAccounts();
            //}
        }

        public void SetDefaultDB()
        {
            try
            {
                var adr = Properties.Settings.Default.MongoServerSettings;
                var db = Properties.Settings.Default.dbUrl;

                DataHelpers.DbAdresse = adr;
                DataHelpers.DbName = db;
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }
        bool haserrors = false;
        public void SetError(string error)
        {
            haserrors = true;
            Error = error;
            NotifyOfPropertyChange("Error");
           
        }

        public void StartServerApi()
        {
            try
            {
                //starting the mongod server (when app starts)
                ProcessStartInfo startRestHeart = new ProcessStartInfo();
                startRestHeart.FileName = @"java";
                startRestHeart.WindowStyle = ProcessWindowStyle.Hidden;
                // set UseShellExecute to false
                startRestHeart.UseShellExecute = false;

                //@"" prevents need for backslashes
                //var dbPaht = Path.GetFullPath("dbs/db");
                startRestHeart.Arguments = $"-jar Restheart/restheart.jar";

                DataHelpers.restheart = Process.Start(startRestHeart);
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        public void StartServerDatabase()
        {
            if (Settings.Default.UseMongoServer == false)
            {
                
                //var mapper = BsonMapper.Global;
                //mapper.Entity<IDocument>().Ignore(a => a.Submitable)
                //    .Ignore(a => a.CollectionName)
                //    .Ignore(a => a.ModuleName)
                //    .Ignore(a => a.SubModule)
                //    .Ignore(a => a.DocOpenMod)
                //    .Ignore(a => a.IconName)
                //    .Ignore(a => a.ShowInDesktop)
                //    .Ignore(a => a.MenuIndex);

               // mapper.Entity<ModelBase<IDocument>>().Ignore(a => a.NameField);

                return;
            }
            try
            {
                // Prepare database
                var adr = Properties.Settings.Default.MongoServerSettings;
                var db = Properties.Settings.Default.dbUrl;

                if(Directory.Exists(Path.GetFullPath("Database/x86")) == false)
                    Directory.CreateDirectory("Database/x86");

                if (Directory.Exists(Path.GetFullPath("Database/x64")) == false)
                    Directory.CreateDirectory("Database/x64");


                var lockFiles = "Database/x86";
                var fls = Directory.EnumerateFiles(lockFiles);

                var lock64 = "Database/x64";
                var fls64 = Directory.EnumerateDirectories(lock64);

                var files64 = fls64.Where(a => a.Contains("lock"));
                foreach (var item in files64)
                {
                    try { File.Delete(Path.GetFullPath(item)); } catch { }
                }

                var locks = fls.Where(a => a.Contains("lock"));
              
                foreach (var item in locks)
                {
                    try  { File.Delete(Path.GetFullPath(item)); } catch (Exception s) { Console.Write(s.Message); }
                }

                var dbsourceOrigin = Path.GetFullPath("Database"); 
                var folders = Directory.EnumerateDirectories(dbsourceOrigin).ToList(); 
                var dbfolder = Path.GetFullPath("Database/x86");

                //dbs/64/App/mongod.exe
                bool is64 = false;

                if (Environment.Is64BitOperatingSystem)
                    is64 = true;

                string mongoProcess = "";
                string storageEngine = "--storageEngine=mmapv1";
                if (is64 && Directory.Exists("Database/data64"))
                { 
                    mongoProcess = @"Database/data64/bin/mongod.exe";
                    dbfolder = Path.GetFullPath("Database/x64");
                    storageEngine = "";
                }
                else if(Directory.Exists("Database/data86"))
                {
                    mongoProcess = @"Database/data86/bin/mongod.exe";
                }
                else
                {
                    // Not using embedded DB
                    return;
                }
                //starting the mongod server (when app starts)
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = Path.GetFullPath(mongoProcess); 
                start.WindowStyle = ProcessWindowStyle.Minimized; 
                start.Arguments = $"--dbpath=\"{dbfolder}\" {storageEngine}  --bind_ip=0.0.0.0"; 
                start.CreateNoWindow = true;
                DataHelpers.mongod = Process.Start(start);
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

       
        public static void AutoUpdater_ApplicationExitEvent()
        {
            Thread.Sleep(5000);
            Environment.Exit(0);// DataHelpers.Shell.showClose();//.Exit();//.Exit();
        }



        private void CheckForNewModules()
        {
            try
            {
                // Reload new modules
                FrameworkManager.ReloadModules();
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        private void CleanTempFiles()
        {
            var folders = Directory.EnumerateDirectories("temp");
            if(folders != null)
            {
                foreach (var item in folders)
                {
                    try { Directory.Delete(Path.GetFullPath(item)); } catch { }
                }
            }

            var files = Directory.EnumerateFiles("temp");
            foreach (var item in files)
            {
                try { File.Delete(item); } catch { SetError("Impossible de supprimer un fichier /temp"); }
            }
        }

        private void LoadSettingsInDatahelpers()
        {
            try
            {
                var setting = ElvaSettings.getInstance();
                DataHelpers.Settings = setting;

                DataHelpers.NotAllowedProperties = typeof(ModelBase<>).GetProperties().Select(z => z.Name).ToList();
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        private void LoadTypesToDatahelper()
        {
            try
            {
                // Update types in DataHelp
                var asm = AppDomain.CurrentDomain.GetAssemblies();
                var types = asm.SelectMany(a => a.GetTypes());
                string nspace = "Ovresko.Generix.Core.Modules";
                string plugins = "Ovresko.Generix.Extentions";

                var q = from t in types
                        where t.IsClass && (t.Namespace == nspace || t.Namespace == plugins)
                        && (typeof(IDocument).IsAssignableFrom(t)
                        && !typeof(NoModule).IsAssignableFrom(t))
                        select t;
                // Type DataHelpers
                foreach (var t in q)
                {
                    var instance = Activator.CreateInstance(t);
                    (instance as IDocument).RunWithBootstrap();                    

                    var collection = t.GetProperty("CollectionName").GetValue(instance)?.ToString();


                    try { DataHelpers.GetTypesModule.Add(t.Name, t); } catch { }
                    try { DataHelpers.GetTypesModule.Add(collection, t); } catch { }
                    try { DataHelpers.GetTypesModule.Add(t.AssemblyQualifiedName, t); } catch { }
                    try { DataHelpers.GetTypesModule.Add(t.FullName, t); } catch { }

                  
                }
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        private void SetDefaultLang()
        {
            //try
            //{
            //    var defaultLanguage = ElvaSettings.getInstance().LangeDefault;
            //    if (!string.IsNullOrWhiteSpace(defaultLanguage))
            //    {
            //        DataHelpers.LangeDefault = defaultLanguage;
            //    }
            //}
            //catch (Exception s)
            //{
            //    SetError(s.Message);
            //}
        }

        private void SetNote(string note)
        {
            Message = _(note);
            NotifyOfPropertyChange("Message");
        }

        private void StartLogger()
        {
            try
            {
                Stylet.Logging.LogManager.Enabled = true;
                var myLogger = new LLibrary.L(new LLibrary.LConfiguration
                {
                    DeleteOldFiles = TimeSpan.FromDays(5),
                });
                DataHelpers.Logger = myLogger;
            }
            catch (Exception s)
            {
                SetError(s.Message);
            }
        }

        private  void DoCheckApplicationFolders()
        {
            if (!Directory.Exists("Database/images"))
                Directory.CreateDirectory("Database/images");
            if (!Directory.Exists("Database/init"))
                Directory.CreateDirectory("Database/init");
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");
            if (!Directory.Exists("templates"))
                Directory.CreateDirectory("templates");
            if (!Directory.Exists("Reports"))
                Directory.CreateDirectory("Reports");
            if (!Directory.Exists("Plugins"))
                Directory.CreateDirectory("Plugins");
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {

            DoCheckApplicationFolders();

            SetNote("startup.label.loadingdb");
            StartServerDatabase();
            Worker.ReportProgress(10);

           // SetNote("startup.label.checkingdb");
           //if( EnsureServerIsRunning() == false)
           // {
           //     SetError("Server Off!");
           //     return;
           // }
            

            SetNote("startup.label.loadingmodule");
            LoadModulesDll();
            Worker.ReportProgress(20);

            SetNote("startup.label.logging");
            StartLogger();

            
            SetNote("startup.label.data");
            LoadTypesToDatahelper();

            SetNote("startup.label.initdb");
            InstallDefaultDatabase();
            Worker.ReportProgress(30);

            SetNote("startup.label.cultures");
            SetDefaultCultureOnThread();
            Worker.ReportProgress(40);

            SetNote("startup.label.checkmodules");
            ReloadAllModules();

         
             
            var setting = ElvaSettings.getInstance();
            if (setting.IsRestheatUsed)
                StartServerApi();

            if (setting.AppInitialized == false)
            {
                Mode = StartupMode.ALL;
            }
            else
            {
                Mode = StartupMode.LAUNCH;
            }

            if (Mode == StartupMode.ALL)
            { 
                SetNote("startup.label.init");
                RestoreDatabaseDump();
                Worker.ReportProgress(50);

                SetNote("startup.label.data");
                FrameworkManager.UpdateModules();
                Worker.ReportProgress(60);

                SetNote("startup.label.series");
                InitSeries();
                Worker.ReportProgress(70);

               // SetNote("Lancement assistant");
               //Execute.PostToUIThread(() => OpenAppAssistant());
               // Worker.ReportProgress(80);

               //Execute.OnUIThread(() => { _windowManager.ShowMessageBox("Vous devez relancer l'application aprés cette configuration");  });
            }
            else
            {
              
                SetNote("startup.label.lang");
                SetDefaultLang();
                Worker.ReportProgress(50);
            }

            Worker.ReportProgress(60);
            SetNote("startup.label.checkadmin");
            InstallDefaultUser();

            SetNote("startup.label.data");
            ReloadAllModules();
            Worker.ReportProgress(60);

            SetNote("startup.label.temp");
            CleanTempFiles();
            Worker.ReportProgress(70);

            SetNote("startup.label.loadingmodule");
            CheckForNewModules();
            Worker.ReportProgress(80);

            LoadSettingsInDatahelpers();
            Worker.ReportProgress(90);

            SetNote("startup.label.updates");
            CheckForUpdates();
            Worker.ReportProgress(95);

            SetNote("startup.label.lang");
            CheckDefaultLang();
            Worker.ReportProgress(100);

          
        }

        private void Restart()
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }


        public bool? RestartAtFinish { get; set; } = false;
        public void CheckDefaultLang()
        {
            var setting = ElvaSettings.getInstance();
            if(string.IsNullOrWhiteSpace( setting?.Pays))
            {
                Execute.OnUIThread(() =>
                {
                    try
                    {
                        
                        var langChoose = new LangViewModel();
                        RestartAtFinish =  DataHelpers.windowManager.ShowDialog(langChoose);
                    }
                    catch (Exception s)
                    {
                        DataHelpers.ShowMessageError(s);
                        return;
                    }
                    
                });
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            NotifyOfPropertyChange("Progress");
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(haserrors)
            {

                var res = DataHelpers.ShowMessage(_("Un probleme est survenu lors de demarrage, voulez-vous continuer à ouvrir l'application?"), _("Erreur"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                if(res == MessageBoxResult.Yes)
                {
                    this.RequestClose(true);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                this.RequestClose(true);
            }
          
        }
    }
}