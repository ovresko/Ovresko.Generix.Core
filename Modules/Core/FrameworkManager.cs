using AutoUpdaterDotNET;
using MongoDB.Driver;
using Newtonsoft.Json;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.LicenceManage;
using Ovresko.Generix.Utils;
using Portable.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Ovresko.Generix.Utils.Data.Import;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Unclassified.TxLib;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules.Core
{
    public static class FrameworkManager
    {
        public static string PrivateKey = "MIICITAjBgoqhkiG9w0BDAEDMBUEEFTGdTEj3djRVchSDv2vrBoCAQoEggH4ow+DkQNlHuB0whG0h51pCMcLuwTrjArybmVZM+PiTRbEnF65d9D4l8t276OEEOmUd07O30RTEEQnm+1+fv7eNmJ8xfzRXqS0/oCm2WjtGxlxQY7DlxctpSlJVLTgI27TuMN6tky6C8EGais35bZAJsbMCCcw6I1BCMUV6ZlCYgvTPSAOIg+Vh8LZUrRbxE/CJ6Ly+BPNlTCHzeqKIkBCoruxMXQEJuLqbbsP0HgiRKeDnowL2Xq/QzbXTi4BjUs26E5Rm2gOoZvf/BxnnPMtpxaRmL0QJ4aQKO1Jk/WkmO6YjG0pTi2XXn4u9cOmh/8q54CFfr7Y2Y3zDbBLeVYPrI8V1l6zcD0pi7S8VDgSpY1RkrAqgvTSlW7bERSAgW69kBRuG4SDZPesGj9AgRcqU8SKq8VSGXwTypv8yyHAkXKbEUdhFueHyRZhltqgjGMc90AYRjD4sjZ+zC6oM5fEGG/T0NB8/VodGyUwAbxttIwI820JoJBwhzQYMyqeICpIQb4O6DljCSXDuKxTHuG/0uaWnjAdT6GSydDvejkA9DVZMMyN0+Wl8l8+ywokwKsSm38WQ29PyAwXN8wBnLm8AYqxBi8fotj+b0wSLSXo6w5NCuKljgzBPXhHM4Ri4P2xa/9GJo/dOkLtbMoLPAUqFnMKkdO7HNuu";

        // Check if admin user exist in database
        public static bool AdminExists(bool AddIfNot = false)
        {
            var users = DS.db.GetAll<User>( a => true)  ;
            if (users == null || users?.Any() == false)
            {
                var Theadmin = users.FirstOrDefault(a => a.Libelle == "Admin");
                if (AddIfNot && Theadmin == null)
                {
                    User admin = new User()
                    {
                        Libelle = "Admin",
                        Password = "admin",
                        Fonction = "Administrateur systéme",
                        IsAdmin = true
                    };

                    admin.Save();
                }

                if (Theadmin != null && Theadmin.Password == "")
                {
                    Theadmin.Password = "admin";
                    Theadmin.Save();
                }
                return false;
            }
            return true;
        }

        public static void CheckUpdateFor(Assembly module, string link)
        {
            AutoUpdater.Start(link, module);
            AutoUpdater.DownloadPath = $"{ Environment.CurrentDirectory}/Plugins";
        }

        // Check if plan comptable exist
        public static bool ComptesExists(bool AddIfNot = false)
        {
            return true;
        }

        public static void CreateLicenceStandard(string username, string email)
        {
    //        var old = File.Exists("License.lic");
    //        if (old)
    //        {
    //            File.Delete("License.lic");
    //        }

    //        var license = License.New()
    //.As(LicenseType.Standard)
    //.LicensedTo(username, email)
    //.CreateAndSignWithPrivateKey(PrivateKey, "15021991");

    //        license.Customer.Email = email;
    //        license.Customer.Name = username;
    //        license.Sign(PrivateKey, "15021991");
    //        license.Type = LicenseType.Standard;

    //        var settings = ElvaSettings.getInstance();
    //        settings.UserName = username;
    //        settings.Email = email;
    //        settings.Save();
    //        File.WriteAllText("License.lic", license.ToString(), Encoding.UTF8);
        }

        public static void CreateLicenceTrial(string username, string email)
        {
    //        var old = File.Exists("License.lic");
    //        if (old)
    //        {
    //            File.Delete("License.lic");
    //        }

    //        var license = License.New()
    //.As(LicenseType.Trial)
    //.ExpiresAt(DateTime.Now.AddDays(14))
    //.LicensedTo(username, email)
    //.CreateAndSignWithPrivateKey(PrivateKey, "15021991");

    //        license.Expiration = DateTime.Now.AddDays(14);
    //        license.Customer.Email = email;
    //        license.Customer.Name = username;
    //        license.Sign(PrivateKey, "15021991");
    //        license.Type = LicenseType.Trial;
    //        var settings = ElvaSettings.getInstance();
    //        settings.UserName = username;
    //        settings.Email = email;

    //        settings.DemoUsed = true;
    //        settings.Save();

    //        File.WriteAllText("License.lic", license.ToString(), Encoding.UTF8);
        }

        public static void CreateSeries(ModuleErp module, long currentIndex, bool createAnyway = false)
        {


            if (!module.HasSeries && createAnyway == false)
                return;

            var serie = new SeriesName();
            serie.Libelle = _(module.Name);

            string suffix = "";
            string libelle = module.Libelle;

            var trim = libelle.Split(' ');
            if (trim.Length == 1)
            {
                var word = trim.FirstOrDefault();
                suffix += word.Substring(0, 2).ToUpper();
            }
            else
            {
                foreach (var item in trim)
                {
                    if (item.Length > 2)
                    {
                        var firstCharcter = new string(item.Take(1).ToArray());
                        suffix += firstCharcter.ToUpper();
                    }
                }
            }

            serie.Sufix = suffix + "-########";
            serie.Indexe = currentIndex++;
            serie.ForceIgniorValidatUnique = true;
            serie.Save();
            // module.SeriesNames = new List<SeriesName>();
            module.SeriesNames.Add(serie);
            if (module.SeriesDefault.IsValide() == false)
                module.SeriesDefault = serie.Id;
            module.ForceIgniorValidatUnique = true;
            module.Save();
        }

        // Check if series in database
        public static async void CreateSeries(bool DeleteOldSeries = true)
        {
            if (DeleteOldSeries)
            {
                var oldseries = DS.db.GetAll<SeriesName>(a => true) as IEnumerable<SeriesName>;
                DS.db.DeleteMany(oldseries);
            }

            var modulesWithSeries = DS.db.GetAll<ModuleErp>(a => true) as IEnumerable<ModuleErp>;
            if (modulesWithSeries != null)
            {
                foreach (var module in modulesWithSeries)
                {
                    try
                    {
                        var currentIndex = DS.Generic(module.ClassName)?.Count();//, true);
                        if (currentIndex.HasValue)
                            CreateSeries(module, currentIndex.Value);
                    }
                    catch (Exception s)
                    {
                        DataHelpers.Logger.LogError(s);
                        throw s;
                    }
                }
            }
        }

        public static void GenerateLicence()
        {
            var keyGenerator = Portable.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();
            var privateKey = keyPair.ToEncryptedPrivateKeyString("15021991");
            var publicKey = keyPair.ToPublicKeyString();

            using (var licence = new StreamWriter("public-lcs"))
            {
                licence.Write(publicKey);
            }

            using (var licence = new StreamWriter("private-lcs"))
            {
                licence.Write(privateKey);
            }
        }


        public enum InstallMode
        {
            Zip,
            Path
        }

        public static bool InstallPlugin(string fullPath,string filenameWithExtention, InstallMode mode)
        {
            switch (mode)
            {
                case InstallMode.Zip:
                    try
                    {

                        ZipArchive archive = ZipFile.OpenRead(Path.GetFullPath(fullPath));
                        string tempPath = $"temp/plugin_{DateTime.Now.Ticks}";
                        Directory.CreateDirectory(tempPath);
                        archive.ExtractToDirectory(Path.GetFullPath(tempPath), true);


                        var allfiles = Directory.EnumerateFiles(Path.GetFullPath($"{tempPath}/Plugins")).ToList();

                        var json = allfiles.FirstOrDefault(a => a.Contains(".json"));
                      
                        //string json = $"{tempPath}/{filenameWithExtention}.json";

                        if (!File.Exists(json))
                        {
                            DataHelpers.ShowMessage(_("Invalide package!"));
                            return false;
                        }

                        var jsonPlugin = File.ReadAllText(json);
                        PluginConfig configC = JsonConvert.DeserializeObject<PluginConfig>(jsonPlugin);

                        if (configC == null)
                        {
                            DataHelpers.ShowMessage(_("Invalide package!"));
                            return false;
                        }

                        PluginPackage pluginPackage = new PluginPackage();
                        pluginPackage.Libelle = configC?.Name;
                        pluginPackage.PluginVersion = configC?.Version;
                        pluginPackage.PluginDate = configC?.Release;
                        pluginPackage.PluginLien = configC?.Link;

                        archive.ExtractToDirectory(Environment.CurrentDirectory, true);

                        if (Directory.Exists($"{tempPath}/Plugins"))
                        {
                            var filesPlugins = Directory.EnumerateFiles($"{tempPath}/Plugins");
                            foreach (var item in filesPlugins)
                            {
                                pluginPackage.AllPluginFiles.Add(new PluginFile(item.Replace(tempPath,"")));
                            }
                        }

                        if (Directory.Exists($"{tempPath}/templates"))
                        {
                            var filesTemplates = Directory.EnumerateFiles($"{tempPath}/templates");
                            foreach (var item in filesTemplates)
                            {
                                pluginPackage.AllPluginFiles.Add(new PluginFile(item.Replace(tempPath, "")));
                            }
                        }
                        if (Directory.Exists($"{tempPath}/Languages"))
                        {
                            var filesLanguages = Directory.EnumerateFiles($"{tempPath}/Languages");
                            foreach (var item in filesLanguages)
                            {
                                pluginPackage.AllPluginFiles.Add(new PluginFile(item.Replace(tempPath, "")));
                            }
                        }
                        if (Directory.Exists($"{tempPath}/Reports"))
                        {
                            var filesReports = Directory.EnumerateFiles($"{tempPath}/Reports");
                            foreach (var item in filesReports)
                            {
                                pluginPackage.AllPluginFiles.Add(new PluginFile(item.Replace(tempPath, "")));
                            }
                        }

                        pluginPackage.Save();

                      
                        DataHelpers.ShowMessage(_("Plugin installé, redémarrer l'application"));
                    }
                    catch (Exception s)
                    {
                        DataHelpers.ShowMessageError(s);
                        return false;
                    }

                    break;
                case InstallMode.Path:
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(fullPath))
                        {
                            var file = Path.GetFullPath(fullPath);
                            var safe = Path.GetFileNameWithoutExtension(file);
                            var json = file.Replace(".dll", ".json");// Path.Combine(location, $"/{safe}.json");
                            if (File.Exists(json))
                            {
                             
                                var jsonContent = File.ReadAllText(json);
                                PluginConfig config = JsonConvert.DeserializeObject<PluginConfig>(jsonContent);

                                if (ModuleErp.PluginIsValide(config.Name, config.User, config.Key))
                                {
                                    // Check Dependencies
                                    var deps = config.Dependencies;

                                    
                                    File.Copy(file, $"Plugins/{safe}.dll");
                                    File.Copy(Path.GetFullPath(json), $"Plugins/{safe}.json");
                                    DataHelpers.ShowMessage("Fermer l'application pour que le systéme recharge cette extention");

                                    return true;
                                }
                                else
                                {
                                     DataHelpers.ShowMessage("Key config invalide");
                                    return false;
                                }
                            }
                            else
                            {
                                 DataHelpers.ShowMessage($"Le fichier de configuration {safe}.json est introuvable");
                                return false;
                            }
                        }
                    }
                    catch (Exception s)
                    {
                         DataHelpers.ShowMessage(s.Message);
                    }
                    break;
                default:
                    break;
            }
        

            return false;
        }

        // Check internet for feedback send
        public static bool InternetExists()
        {
            return true;
        }

        // Check if app licence exist
        public static bool LicenceExists()
        {
            return true;
        }

        // Check if class modules exist in the database
        public static bool ModulesExists(bool AddIfNot = false)
        {
            return true;
        }

        public static void ReloadModules()
        {
            // return;
            var modules = DS.db.GetAll<ModuleErp>();// DataHelpers.GetMongoDataSync("ModuleErp") as IEnumerable<ModuleErp>;
            DataHelpers.Modules = modules as IEnumerable<ModuleErp>;
            var collections = modules.Select(z => z.ClassName).ToList(); 
            string nspace = "Ovresko.Generix.Core.Modules";
            string plugins = "Ovresko.Generix.Extentions";
            var asm = AppDomain.CurrentDomain.GetAssemblies();
            var types = asm.SelectMany(a => a.GetTypes());

            //var q = from t in Assembly.GetExecutingAssembly().GetTypes()
            //        where t.IsClass && t.Namespace == nspace
            //        && (t.IsSubclassOf(typeof(IDocument))
            //        && !typeof(NoModule).IsAssignableFrom(t))
            //        select t;

            var pluginsType = types.Where(a => a.Namespace == plugins);
            var pluginsCore = types.Where(a => a.Namespace == nspace);

            var result = new List<Type>();
            result.AddRange(pluginsType);
            var names = result.Select(a => a.Name);
            foreach (var item in pluginsCore)
            {
                if (names.Contains(item.Name) == false)
                    result.Add(item);
            }

            var q = from t in result
                    where t.IsClass && (t.Namespace == nspace || t.Namespace == plugins)
                    && (typeof(Document).IsAssignableFrom(t)
                    && !typeof(NoModule).IsAssignableFrom(t))
                    select t;

            List<string> AssemblyCollection = new List<string>();
            q.ToList().ForEach(t =>
            {
                try
                    {
                    Console.Write(t.Name);
                    var instance = Activator.CreateInstance(t);
                    var CollectionName = t.GetProperty("CollectionName").GetValue(instance)?.ToString();
                    AssemblyCollection.Add(t.FullName);
                    //Install
                    IDocument extended = (instance as IDocument);
                    if (extended.MyModule() != null && (extended.MyModule() as ModuleErp).SeriesDefault.IsValide() == false)
                    {
                        FrameworkManager.CreateSeries(extended.MyModule() as ModuleErp, 1,true);
                    }


                    (instance as IDocument).InstallWithBootstrap();

                    if (!collections.Contains(t.FullName))
                    {
                        var moduleName = t.GetProperty("ModuleName").GetValue(instance)?.ToString();
                        var iconName = t.GetProperty("IconName").GetValue(instance)?.ToString();
                        var showInDesktop = t.GetProperty("ShowInDesktop").GetValue(instance) as bool?;
                        var isInstance = bool.Parse(t.GetProperty("IsInstance").GetValue(instance)?.ToString());
                        var indexMnu = int.Parse(t.GetProperty("MenuIndex").GetValue(instance)?.ToString());
                        if (!string.IsNullOrWhiteSpace(moduleName))
                        {
                            Console.Write("EXIT");
                        }
                        var moduleErp = new ModuleErp();
                        moduleErp.Libelle = CollectionName;
                        moduleErp.EstAcceRapide = showInDesktop.Value;
                        moduleErp.ClassName = t.FullName;// t.AssemblyQualifiedName;// $"{nspace}.{t.Name}";
                        moduleErp.GroupeModule = moduleName;
                        moduleErp.ModuleIcon = iconName;
                        moduleErp.ModuleSubmitable = (instance as IDocument).Submitable;
                        moduleErp.IsInstanceModule = isInstance;
                        moduleErp.ModuleMenuIndex = indexMnu;
                        moduleErp.Save();

                        try { DataHelpers.GetTypesModule.Add(CollectionName, t); } catch { }
                        try { DataHelpers.GetTypesModule.Add(moduleErp.ClassName, t); } catch { }
                        try { DataHelpers.GetTypesModule.Add(t.FullName, t); } catch { }
                        try { DataHelpers.GetTypesModule.Add(t.Name, t); } catch { }

                       
                    }
                }
                catch (Exception s)
                {
                    Console.WriteLine("ERROR\n");
                    Console.WriteLine(s.Message);
                }
            }
            );

            foreach (var item in modules)
            {
                if (!AssemblyCollection.Contains(item.ClassName))
                    item.Delete(false);
            }

            modules = DS.db.GetAll<ModuleErp>();// DataHelpers.GetMongoDataSync("ModuleErp") as IEnumerable<ModuleErp>;
            DataHelpers.Modules = modules;
        }

        // Check if app has minimum Rôles
        public static bool RolesExists(bool AddIfNot = false)
        {
            return true;
        }

        // Check if app has instance of ElvaSettings
        public static bool SettingsExists(bool AddIfNot = false)
        {
            return true;
        }

        public static void SetupAccounts()
        {
            //var accounts = DS.db.GetAll<CompteCompta>(a => true);
            //if (accounts == null || accounts.Any() == false)
            //{
            //    var response =  DataHelpers.ShowMessage("Voulez-vous charger le plan comptable?", "Comptes", MessageBoxButton.YesNo);
            //    if (response == MessageBoxResult.Yes)
            //    {
            //        string result = "Init/comptes.xls";
            //        var ovimport = new ExcelImport(new DynamicPath(result));

            //        var data = ovimport.ImportDataFromType(result, typeof(CompteCompta));

            //        if (data != null)
            //        {
            //            foreach (var item in data)
            //            {
            //                try
            //                {
            //                    item.AddedAtUtc = DateTime.Now;
            //                    item.ForceIgniorValidatUnique = true;
            //                    if (item.Save() && item.Submitable)
            //                    {
            //                        item.Submit();
            //                    }
            //                    // item.Save();
            //                }
            //                catch
            //                {
            //                    continue;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public static void SetupApp()
        {
            // Do all the nessesary job for initial setup;

            // 1- Check mode de paiement
            //var modePaiement = DS.db.GetAll<ModePaiement>(a => true);
            //if (!modePaiement.Any())
            //{
            //    //  var modeEspece = new ModePaiement { Libelle == "Espèces" }
            //}
        }

        // Check if app has taxes
        public static bool TaxesExists(bool AddIfNot = false)
        {
            return true;
        }

        public static void UpdateModules(bool DoUpdate = true)
        {
            var users = DS.db.GetAll<User>();
            var modules = DS.db.GetAll<ModuleErp>();
            DataHelpers.Modules = modules as IEnumerable<ModuleErp>;
            if (DoUpdate == false)
            {
                //check in db
                if (modules?.Count() < 1)
                    DoUpdate = true;
            }
            // deleteAll Modules

            if (!DoUpdate)
                return;

            foreach (var module in modules)
            {
                (module as IModel).Delete(false);
            }

            //Ovresko.Generix.Core.Modules.
            string nspace = "Ovresko.Generix.Core.Modules";
            string plugins = "Ovresko.Generix.Extentions";
            var asm = AppDomain.CurrentDomain.GetAssemblies();
            var types = asm.SelectMany(a => a.GetTypes());

            var pluginsType = types.Where(a => a.Namespace == plugins);
            var pluginsCore = types.Where(a => a.Namespace == nspace);

            var result = new List<Type>();
            result.AddRange(pluginsType);
            var names = result.Select(a => a.Name);
            foreach (var item in pluginsCore)
            {
                if (names.Contains(item.Name) == false)
                    result.Add(item);
            }

            var q = from t in result
                    where t.IsClass && (t.Namespace == nspace || t.Namespace == plugins)
                    && (typeof(Document).IsAssignableFrom(t)
                    && !typeof(NoModule).IsAssignableFrom(t))
                    select t;

            q.ToList().ForEach(t =>
                {
                    try
                    {
                        Console.Write(t.Name);
                        var instance = Activator.CreateInstance(t);
                        var collection = t.GetProperty("CollectionName").GetValue(instance)?.ToString();
                        var moduleName = t.GetProperty("ModuleName").GetValue(instance)?.ToString();
                        var iconName = t.GetProperty("IconName").GetValue(instance)?.ToString();
                        var showInDesktop = t.GetProperty("ShowInDesktop").GetValue(instance) as bool?;
                        var isInstance = bool.Parse(t.GetProperty("IsInstance").GetValue(instance)?.ToString());
                        var indexMnu = int.Parse(t.GetProperty("MenuIndex").GetValue(instance)?.ToString());
                        bool hasserie = false;
                        try { hasserie = (t.GetProperty("Series").GetValue(instance) != null); } catch { }
                       
                        if (!string.IsNullOrWhiteSpace(moduleName))
                        {
                            Console.Write("EXIT");
                        }
                        var moduleErp = new ModuleErp();
                        moduleErp.Libelle = collection;
                        moduleErp.EstAcceRapide = showInDesktop.Value;
                        moduleErp.ClassName = t.FullName;// t.AssemblyQualifiedName;// $"{nspace}.{t.Name}";
                        moduleErp.GroupeModule =  moduleName;
                        moduleErp.ModuleIcon = iconName;
                        moduleErp.ModuleSubmitable = (instance as IDocument).Submitable;
                        moduleErp.IsInstanceModule = isInstance;
                        moduleErp.ModuleMenuIndex = indexMnu;
                        moduleErp.HasSeries = hasserie;
                        moduleErp.Save();

                        try { DataHelpers.GetTypesModule.Add(collection, t); } catch { }
                        try { DataHelpers.GetTypesModule.Add(moduleErp.ClassName, t); } catch { }
                        try { DataHelpers.GetTypesModule.Add(t.FullName, t); } catch { }
                        try { DataHelpers.GetTypesModule.Add(t.Name, t); } catch { }
                    }
                    catch (Exception s)
                    {
                        Console.WriteLine("ERROR\n");
                        Console.WriteLine(s.Message);
                    }
                }
            );

            modules = DS.db.GetAll<ModuleErp>(a => true);
            DataHelpers.Modules = modules as IEnumerable<ModuleErp>;
        }
        public static bool ValidateAllPluins()
        {
            var plugins = Directory.EnumerateFiles("Plugins/");
            var jsonFiles = plugins.Where(a => a.Contains(".json"));
            bool result = false;
            foreach (var json in jsonFiles)
            {
                var content = File.ReadAllText(Path.GetFullPath(json));
                var config = JsonConvert.DeserializeObject<PluginConfig>(content);

                if (config == null)
                    throw new Exception($"Fichier configuration Module {json} invalide");
                var jsonIsValide = ModuleErp.PluginIsValide(config.Name, config.User, config.Key);
                if (jsonIsValide == false)
                {
                    throw new Exception($"Le module {json} n'est pas valide");
                }
                result = result && jsonIsValide;
            }

            return result;
        }

        public static bool ValidateLicence(string publicKey, string username, string email)
        {
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                 DataHelpers.ShowMessage("Vérifier les données insérées, Application non Enregistrée");
                return false;
            }

            var old = File.Exists("License.lic");
            if (old)
            {
                try
                {
                    using (var sr = (new StreamReader("License.lic")))
                    {
                        var license = License.Load(sr);
                        if (license.VerifySignature(publicKey))
                        {
                            if (license.Customer.Name != username || license.Customer.Email != email)
                            {
                                 DataHelpers.ShowMessage("Licence invalide! contactez votre fournisseur <0665 97 76 79> <contact@ovresko.com> <ovresko@gmail.com>");
                                return false;
                            }

                            // Licence existe
                            var licenceType = license.Type;
                            if (licenceType == LicenseType.Trial)
                            {
                                var date = DateTime.Now;
                                var started = license.Expiration;
                                if (date >= started)
                                {
                                    // Licence Dead
                                     DataHelpers.ShowMessage("Période d'éssai terminé, contactez votre fournisseur <0665 97 76 79> <contact@ovresko.com> <ovresko@gmail.com>");
                                    return false;
                                }
                                else
                                {
                                    DataHelpers.Settings.DemoUsed = true;
                                    DataHelpers.Settings.Save();
                                }
                            }
                            DataHelpers.LicenceType = licenceType.ToString();
                            DataHelpers.Expiration = license.Expiration;
                            DataHelpers.Customer = license.Customer.Name;

                            return true;
                        }
                        else
                        {
                            // Licence doesn't exist
                             DataHelpers.ShowMessage("Licence invalide! contactez votre fournisseur <0665 97 76 79> <contact@ovresko.com> <ovresko@gmail.com>");

                            return false;
                        }
                    }
                }
                catch (Exception s)
                {
                    try
                    {
                        File.Delete("License.lic");
                    }
                    catch
                    { }
                    try
                    {
                        File.Delete("public-lcs");
                    }
                    catch
                    { }
                }
            }

            return false;
        }

        internal static bool CanAdd(Type type)
        {
            if (type.Name == "ModuleErp")
                return true;

            // TODO: Remove if no user accept all
            if (DataHelpers.ConnectedUser == null)
                return true;

            var RulesForThisType = GetRulesForType(type);
            if (RulesForThisType.Any() && RulesForThisType != null)
            {
                var result = !RulesForThisType.Any(a => a.Creer == false);
                return result;
            }
            return true;
        }

        internal static bool CanCancel(Type type)
        {
            if (type.Name == "ModuleErp")
                return true;

            // TODO: Remove if no user accept all
            if (DataHelpers.ConnectedUser == null)
                return true;

            var RulesForThisType = GetRulesForType(type);
            if (RulesForThisType.Any() && RulesForThisType != null)
            {
                var result = !RulesForThisType.Any(a => a.CancelSubmit == false);
                return result;
            }
            return true;
        }

        internal static bool CanDelete(Type type)
        {
            if (type.Name == "ModuleErp")
                return true;

            // TODO: Remove if no user accept all
            if (DataHelpers.ConnectedUser == null)
                return true;

            var RulesForThisType = GetRulesForType(type);
            if (RulesForThisType.Any() && RulesForThisType != null)
            {
                var result = !RulesForThisType.Any(a => a.Supprimer == false);
                return result;
            }
            return true;
        }

        internal static bool CanSave(Type type)
        {
            if (type.Name == "ModuleErp")
                return true;

            // TODO: Remove if no user accept all
            if (DataHelpers.ConnectedUser == null)
                return true;

            var RulesForThisType = GetRulesForType(type);
            if (RulesForThisType.Any() && RulesForThisType != null)
            {
                var result = !RulesForThisType.Any(a => a.CanSave == false);
                return result;
            }
            return true;
        }

        internal static bool CanValidate(Type type)
        {
            if (type.Name == "ModuleErp")
                return true;

            // TODO: Remove if no user accept all
            if (DataHelpers.ConnectedUser == null)
                return true;

            var RulesForThisType = GetRulesForType(type);
            if (RulesForThisType.Any() && RulesForThisType != null)
            {
                var result = !RulesForThisType.Any(a => a.Valider == false);
                return result;
            }
            return true;
        }

        internal static bool CanView(Type type)
        {
            if (type.Name == "ModuleErp")
                return true;

            // TODO: Remove if no user accept all
            if (DataHelpers.ConnectedUser == null)
                return true;

            var RulesForThisType = GetRulesForType(type);
            if (RulesForThisType.Any() && RulesForThisType != null)
            {
                var result = !RulesForThisType.Any(a => a.Voir == false);
                return result;
            }
            return true;
        }

        internal static void CheckValidation()
        {
            //if (File.Exists("public-lcs"))
            //{
            //    var user = DataHelpers.Settings.UserName;
            //    var email = DataHelpers.Settings.Email;
            //    try
            //    {
            //        using (var publicK = new StreamReader("public-lcs"))
            //        {
            //            var result = ValidateLicence(publicK.ReadLine(), user, email);
            //            if (result)
            //            {
            //                return;
            //            }
            //        }
            //    }
            //    catch (Exception s)
            //    {
            //         DataHelpers.ShowMessage(s.Message);
            //        try
            //        {
            //            File.Delete("License.lic");
            //        }
            //        catch
            //        { }
            //        try
            //        {
            //            File.Delete("public-lcs");
            //        }
            //        catch
            //        { }
            //    }
            //}
            //var lm = new LicenceManagerViewModel();
            //DataHelpers.windowManager.ShowWindow(lm);
        }

        internal static void CleanFiles()
        {
            var files = Directory.EnumerateFiles("temp");
            foreach (var item in files)
            {
                try { File.Delete(item); } catch { Console.WriteLine("Can't delete...!"); }
            }
        }
        internal static void CreateCulture()
        {
            //var ci = new CultureInstaller.Program();
            //ci.Execute();
            //var all = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(a => a.Name);
            //foreach (var item in all)
            //{
            //    if (item.ToLower() == ("fr-alg"))//x-fr-fr-extend
            //        return;
            //}

            //Process.Start("CultureInstaller.exe");
        }
        private static IEnumerable<AccesRule> GetRulesForType(Type type)
        {
            var user = DataHelpers.ConnectedUser;

            var roles = user.Roles; // Manager CRM ...
            List<Role> Allroles = new List<Role>();
            roles.ForEach(a => Allroles.Add(a.GetObject("Role")));
            var Module = $"{type.Namespace}.{type.Name}";
            return Allroles.SelectMany(z => z.AccesRules).Where(a => (a.Module.GetObject("ModuleErp") as ModuleErp)?.ClassName == Module);
        }
    }
}