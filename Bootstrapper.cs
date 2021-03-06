using System;
using Stylet;
using StyletIoC;
using Ovresko.Generix.Core.Pages;
using Ovresko.Generix.Core.Modules.Core.Data; 
using System.Threading;
using System.Windows; 
using System.Threading.Tasks; 
using System.Windows.Threading;  
using CrashReporterDotNET; 
using Ovresko.Generix.Core.Modules.Messaging;
using Ovresko.Generix.Core.Properties;
using Unclassified.TxLib;
using Ovresko.Generix.Core.Pages.Startup;
using System.Globalization;
using System.Windows.Markup;
using Ovresko.Generix.Datasource.Facade;
using Ovresko.Generix.Datasource.Services;

namespace Ovresko.Generix.Core
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        private ThreadStart th;
        private Thread t;
        private DataTypes dataTypes = DataTypes.MongoDb; 

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure Ioc 
            // Comment

            // In devlop
            builder.Bind<IMessagingService>().To<MessagingService>().InSingletonScope();

            bool UseMongoServer = Settings.Default.UseMongoServer;
            if (UseMongoServer == false)
                dataTypes = DataTypes.LiteDb;

            builder.Bind<IDataService>().ToFactory(c => new DataFacade(dataTypes).GetDataService(@"Database\OvreskoData.db","")).InSingletonScope();
             
            base.ConfigureIoC(builder);
        }
        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            SendReport(e.Exception);
            Environment.Exit(0);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SendReport(e.Exception);
            Environment.Exit(0);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SendReport((Exception)e.ExceptionObject);
            Environment.Exit(0);
        }

        public static void SendReport(Exception exception, string developerMessage = "")
        {
            try
            {
                var reportCrash = new ReportCrash("kimboox44@gmail.com")
                {
                    DeveloperMessage = developerMessage
                };
                reportCrash.Send(exception);
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage( s.Message);
                return;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {

            try
            {
             
                DataHelpers.mongod.Kill();
                DataHelpers.restheart.Kill();
            }
            catch (Exception s)
            {
                // DataHelpers.ShowMessage( s.Message);
            }
            base.OnExit(e);

        }


        
        protected override void OnStart()
        {  
            // Configure Crash handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;


            Tx.UseFileSystemWatcher = true;
            //Tx.LoadFromXmlFile(@"lang\languages.txd");
            Tx.LoadDirectory("Languages");

             
        }

        protected override void Configure()
        {
            base.Configure();
            DataHelpers.container = this.Container;

            Execute.DefaultPropertyChangedDispatcher = Execute.OnUIThread;
          //  ConvertisseurNombreEnLettre.Parametrage.ModifierLaVirgule("Dinar(s) et").ValiderLeParametrage();


            var CultureInfopaye = CultureSettings.Default.DefaultCultureName;//.DefaultLang;// Properties.Settings.Default.Culture;

            if(CultureInfo.CurrentUICulture.Name != CultureInfopaye)
            {
                if (string.IsNullOrWhiteSpace(CultureInfopaye))
                {
                    CultureInfopaye = CultureInfo.CurrentUICulture.Name;
                }
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
                    CultureInfo.CurrentUICulture.DateTimeFormat = DateTimeFormatInfo.InvariantInfo;//.DigitSubstitution = DigitShapes.NativeNational;

                    
                    // = { DigitSubstitution = DigitShapes.NativeNational }

                }
                catch
                { }
                Tx.SetCulture(CultureInfopaye);
            }
           


        }

        protected override void OnLaunch()
        {
           

            base.OnLaunch();
        }

        //private void AutoUpdater_ApplicationExitEvent()
        //{
        //    Thread.Sleep(5000);
        //    Environment.Exit(0);//.Exit();
        //}

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                DataHelpers.ShowMessage(e.Exception.Message);
                DataHelpers.Logger.LogError(e.Exception);
            }
            catch  
            {
            }
        }
    }
}
