using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ovresko.Generix.Utils.Data.Sources;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Ovresko.Generix.Core.Translations;
using System.IO;
using System.Globalization;
using Ovresko.Generix.Core.Pages.AppAssistant;
using Unclassified.TxLib;
using System.Threading;
using System.Windows.Markup;
using Ovresko.Generix.Core.Properties;
using LiteDB;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules
{
   public class ElvaSettings : ModelBase<ElvaSettings>
    {
        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Application";
        public override string CollectionName { get; } = _("Paramétres");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string IconName { get; set; } = "Settings";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "Libelle";

        public override bool IsInstance { get; set; } = true;

        #endregion


        public ElvaSettings()
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                Printers.Add(printer);
            }

            //var lsngs = Properties.Settings.Default.LANGS;
            //foreach (var item in lsngs)
            //{
            //    Langs.Add(item);
            //}

            // Langs = Tx.AvailableCultureNames.ToList();
            // Load Cultures
            LoadCultureInfos();
        }



        #region PROPERTIES

        //[BsonIgnore]
        //public List<string> Langs { get; set; } = new List<string>();

      

        [IsBoldAttribute(false)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Nom d'entreprise")]
        public string Societe { get; set; }

        [IsBoldAttribute(false)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Adresse")]
        public string Adresse { get; set; }


        //[ExDisplayName("Nom d'utilisateur")]
        //[Column(ModelFieldType.Text, "")]
        //public string UserName { get; set; }


        [ExDisplayName("N° Télephone")]
        [Column(ModelFieldType.Text, "")]
        public string NumTel { get; set; }


        [ExDisplayName("E-Mail")]
        [Column(ModelFieldType.Text, "")]
        public string Email { get; set; }

        [BsonIgnore]
        [ExDisplayName("Affichage")]
        [Column(ModelFieldType.Separation,"")]
        public string sepChamp { get; set; }

        //[ColumnAttribute(ModelFieldType.Select, "this>Langs")]
        //[ExDisplayName("Langue par default")]
        //public string LangeDefault { get; set; }  

        [ColumnAttribute(ModelFieldType.Select, "this>PaysList")]
        [ExDisplayName("PaysLang")]
        public string Pays { get; set; }

        [BsonIgnore]
        public List<string> PaysList { get; set; } = new List<string>();
        /// <summary>
        /// Load Cultures From curretn machine and add it
        /// </summary>
        private void LoadCultureInfos()
        {
            PaysList = new List<string>();
            var inMachine = Tx.AvailableCultureNames.ToList();
            if (inMachine != null)
            {
                foreach (var item in inMachine)
                {
                    PaysList.Add(item);
                }
                // Notify
                NotifyOfPropertyChange("PaysList");
            }

        }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Format devise")]
        public string FormatDevis { get; set; } = "N";

        [BsonIgnore]
        [ExDisplayName("Paramétres E-mail")]
        [Column(ModelFieldType.Separation,"")]
        public string sepEmail { get; set; }

        [ExDisplayName("Serveur SMTP")]
        [Column(ModelFieldType.Text, "")]
        public string EmailHost { get; set; }

        [ExDisplayName("Port serveur SMTP")]
        [Column(ModelFieldType.Text, "")]
        public string EmailPort{ get; set; }

        [ExDisplayName("Email d'envois")]
        [Column(ModelFieldType.Text, "")]
        public string EmailFrom { get; set; }


        [ExDisplayName("Mots de passe email")]
        [Column(ModelFieldType.Text, "")]
        public string EmailPwd { get; set; }

        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Base des données")]
        public string sepVente { get; set; }
        

        [ShowInTableAttribute(false)]
        [ExDisplayName("Source base des données")]
        [ColumnAttribute(ModelFieldType.Lien, "DbSourceLink")]
        public Guid DbSourceLink { get; set; } = Guid.Empty;
         
        [IsBoldAttribute(false)]
        [ColumnAttribute(ModelFieldType.ReadOnly, "")]
        [ExDisplayName("Base des données en ligne")]
        public string ActiveDB { get; set; }

        //[ShowInTableAttribute(false)]
        //[IsBold(false)]
        //[ColumnAttribute(ModelFieldType.OpsButton, "SubmitDB")]
        //[ExDisplayName("Transférer DB")]
        //public double AddFactureBtn { get; set; }

        [ColumnAttribute(ModelFieldType.Check, "Utiliser BD embarqué")]
        [ExDisplayName("Utiliser BD locale")]
        [SetColumn(1)]
        public bool LocalBd { get; set; }

        [SetColumn(1)]
        [ColumnAttribute(ModelFieldType.Check, "Restheart API")]
        [ExDisplayName("Lancer serveur API")]
        public bool IsRestheatUsed { get; set; }


        [BsonIgnore]
        [ColumnAttribute(ModelFieldType.OpsButton, "ClearDataAll")]
        [ExDisplayName("Vider la base des données")]
        public string ClearDataAllBtn { get; set; }

        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Logo application")]
        public string sepLogo { get; set; }

        [ColumnAttribute(ModelFieldType.Image, "param")] 
        [ExDisplayName("Logo application")]
        public string AppLogo { get; set; }

        //[ColumnAttribute(ModelFieldType.Image, "")]
        //[ExDisplayName("Arriére plan")]
        //public string AppBackground { get; set; }


        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Impression")]
        public string sepPrinting { get; set; }

        [ColumnAttribute(ModelFieldType.Select, "this>Printers")]
        [ExDisplayName("Imprimante par default")]
        public string ImprimanteDefault{ get; set; }

        [BsonIgnore]
        public List<string> Printers { get; set; } = new List<string>();

        [ExDisplayName("Modifier modéle d'impression")]
        [Column(ModelFieldType.Button,"OpenTemplate")]
        public string BtnOpenTempalate { get; set; }

        public void OpenTemplate()
        {
            var file = "template.docx";
            Process.Start(file);
        }

       

      

        private void SubmitDB()
        {

            var db = DbSourceLink.GetObject("DbSourceLink") as DbSourceLink;
            if (db != null)
            {
                Properties.Settings.Default.MongoServerSettings = db.SourceIp;
                Properties.Settings.Default.dbUrl = db.DbName;
                this.ActiveDB = db.DbName;
               
                if (this.Save())
                {
                     
                    DataHelpers.ShowMessage( $"{_("Source DB à été changée vers")}: {db.DbName}");
                }
                return;
            }
            else
            {
                DataHelpers.ShowMessage( _("Aucune source DB sélectionnée"));
                return;
            }
        }

        ////////////////// ACTION

        public static void SetGlobalCulture(string CultureInfopaye)
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureInfopaye);
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfopaye);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureInfopaye);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(CultureInfopaye);
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
            XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

                CultureInfo.CurrentCulture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;
                CultureInfo.CurrentCulture.DateTimeFormat = DateTimeFormatInfo.InvariantInfo;
                CultureInfo.CurrentCulture.NumberFormat = NumberFormatInfo.InvariantInfo;// = DateTimeFormatInfo.InvariantInfo;
                CultureInfo.CurrentUICulture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;
                CultureInfo.CurrentUICulture.DateTimeFormat = DateTimeFormatInfo.InvariantInfo;
                

            }
            catch
            { }

            Tx.SetCulture(CultureInfopaye);

             CultureSettings.Default.DefaultCultureName = CultureInfopaye;
            CultureSettings.Default.Save();
        }

        public override bool Save()
        {
            base.Save();
            isLocal = false; 
            //var adr = Properties.Settings.Default.MongoServerSettings;
            //DataSource defaulDb = new DataSource("Default",adr);
            //defaulDb.UpdateOne(this);
            DataHelpers.Settings = this;
            //DataHelpers.LangeDefault = this.LangeDefault;

            SetGlobalCulture(this.Pays);
            //Tx.SetCulture(this.LangeDefault);

            DataHelpers.Shell.MenuPanelLoaded = false;
            return true;
        }



        public void ClearDataAll()
        {
            var confirm = DataHelpers.ShowMessage( _("Voulez-vous supprimer tout les données enregistrées?"), "Confirmation", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {

                DS.db.DropAllCollections();
                DataHelpers.ShowMessage( _("Base des données éffacés")); 
            }
        }


        public static  ElvaSettings getInstance()
        {
            //var adr = Properties.Settings.Default.MongoServerSettings;
            //DataSource defaulDb = new DataSource( "Default", adr);
            var instance =  DS.db.GetOne<ElvaSettings>(a => true);
            if (instance != null )
            {
                return instance as ElvaSettings;
            }
            else
            {
                var setting = new ElvaSettings() { isLocal = false };
                // try find default db
                try
                {
                    var Dbsources = DS.db.GetOne<DbSourceLink>( a => a.DbName == "Default");// as IEnumerable<DbSourceLink>;
                    if(Dbsources != null)
                    {
                        var def = Dbsources;//.Where(a => a.DbName == "Default").FirstOrDefault();
                         setting.DbSourceLink = def.Id;
                        
                    }
                }
                catch { }
            

                DS.db.AddOne<ElvaSettings>(setting);
                return DS.db.GetOne<ElvaSettings>(a => true);
            }
        }

        

        public override string Name
        {
            get
            {
                return _("Configuration");
            }
            set => base.Name = value;
        }
        #endregion


        public override void AfterEdit()
        {
            base.AfterEdit();
        }

        public bool AppInitialized { get; set; } = false;

        public bool DemoUsed { get;  set; }
        public int MaxLinesFetch { get; internal set; } = 100;


        //[LongDescription("Exporter la base des données")]
        //[ExDisplayName("Exporter")]
        //[Column(ModelFieldType.OpsButton, "ExportData")]
        //public string ExportDataBtn { get; set; }


        //[ExDisplayName("Restaurer la base des données")]
        //[Column(ModelFieldType.OpsButton, "RestorData")]
        //public string RestorDataBtn { get; set; }


        public void ExportData()
        {
            //% mongodump - h ds012345.mongolab.com:56789 - d dbname - u dbuser - p dbpassword - o dumpdir
            string mongoProcess = "";
            if (Environment.Is64BitOperatingSystem)
            {
                mongoProcess = @"Database/data64/bin/mongodump.exe";

            }
            else
            {
                mongoProcess = @"Database/data86/bin/mongodump.exe";
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Path.GetFullPath(mongoProcess);// @"dbs/bin/mongod.exe";
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.UseShellExecute = true;

            //@"" prevents need for backslashes
            //var dbPaht = Path.GetFullPath("dbs/db");
            //  start.Arguments = $"-h {this.SourceIp} -d {DbName}  -o {folderName}";

            Process.Start(start);
        }



        void CleanBeforeRestorData()
        {

            //DS.db.Context.DropCollection<CompteSettings>();
            //DS.db.Context.DropCollection<VenteSettings>();
            //DS.db.Context.DropCollection<PosSettings>(); 

        }


        public void RestorData()
        {
            try
            {






                string mongoProcess = "";
                if (Environment.Is64BitOperatingSystem)
                {
                    mongoProcess = @"Database/data64/bin/mongorestore.exe";

                }
                else
                {
                    mongoProcess = @"Database/data86/bin/mongorestore.exe";
                }

                // Clean Compte settings & vente settin
                CleanBeforeRestorData();



                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = Path.GetFullPath(mongoProcess);// @"dbs/bin/mongod.exe";
                start.WindowStyle = ProcessWindowStyle.Normal;
                start.UseShellExecute = true;
                Process.Start(start);

                //foreach (var item in DataHelpers.Modules)
                //{
                //    var cls = DataHelpers.GetTypes[item.ClassName];//.Split('.').LastOrDefault();
                //    try
                //    {
                //        DataHelpers.GetGenericData(cls)?.Clear();
                //    }
                //    catch (Exception s)
                //    {
                //        throw s;
                //    }
                //}

              //   DataHelpers.ShowMessage("Terminé!");

            }
            catch (Exception s)
            {
                throw s;
            }
        }


        [ExDisplayName("Ouvrir assistant")]
        [Column(ModelFieldType.OpsButton, "OpenAssistant")]
        public string OpenAssistantaBtn { get; set; }

        public void OpenAssistant()
        {
            AppAssistantViewModel appAssistant = new AppAssistantViewModel(DataHelpers.windowManager);
            appAssistant.Open();
        }
    }


    
}
