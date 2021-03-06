using MaterialDesignThemes.Wpf;
using Ovresko.Generix.Core.Exceptions;
using Ovresko.Generix.Core.Framework;
using Ovresko.Generix.Core.Modules; 
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.AppAssistant;
using Ovresko.Generix.Core.Pages.Events;
using Ovresko.Generix.Core.Pages.Home;
using Ovresko.Generix.Core.Pages.LicenceManage;
using Ovresko.Generix.Core.Pages.ModulesManager;
using Ovresko.Generix.Core.Pages.Startup;
using Ovresko.Generix.Core.Pages.Template;
using Ovresko.Generix.Core.Properties;
using Ovresko.Generix.Core.SpreadSheet;
using Ovresko.Generix.Datasource.Models;
using Ovresko.Generix.Datasource.Services;
using Stylet;
using StyletIoC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using Unclassified.TxLib;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Pages
{
    public class ExpanderMenu : Expander
    {
        public ImageSource iconSource { get; set; }
    }
   
    public class ShellViewModel : Conductor<IScreen>.StackNavigation, IShell, IHandle<ModelChangeEvent>
    {
        
        public int MiniMenuWidth { get; set; } = 55;

        public void OpenMiniMenu()
        {
            MiniMenuWidth = 180;
            NotifyOfPropertyChange("MiniMenuWidth");
        }
        public void CloseMiniMenu()
        {
            MiniMenuWidth = 55;
            NotifyOfPropertyChange("MiniMenuWidth");
        }

        private IEventAggregator aggre;
        private IContainer container;
        private StackPanel parentStack;
        private IWindowManager windowManager;
        private IDataService dataService; 

        public ShellViewModel(IContainer container, IEventAggregator _aggre, IWindowManager windowManager, IDataService dataService)
        {

            this.dataService = dataService;
            this.windowManager = windowManager;
             




            SearchWorker = new System.ComponentModel.BackgroundWorker();
            SearchWorker.DoWork += SearchWorker_DoWork;
            SearchWorker.WorkerReportsProgress = true;
            SearchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
            SearchWorker.ProgressChanged += SearchWorker_ProgressChanged;


            DataHelpers.windowManager = this.windowManager;
            DataHelpers.Shell = this;
            DataHelpers.Aggre = _aggre;
            DataHelpers.container = container;






            aggre = _aggre;
            aggre.Subscribe(this);
            this.container = container;

            this.StateChanged += ShellViewModel_StateChanged;

            //for (int i = 0; i < 11; i++)
            //{
            //    months.Add(DateTime.Today.AddMonths(1 - i));
            //}
            //NotifyOfPropertyChange("months");
        

        }



        public void PreviousPage()
        {
            try
            {

                this.GoBack();
            }
            catch (Exception s)
            {
                windowManager.ShowMessageBox(s.Message);
                return;
            }
        }

        

        public static Func<object> NewItemFactory
        {
            get { return () => new HomeViewModel() { DisplayName = "Home" }; }
        }

        public void NotifyAppLogo()
        {
            NotifyOfPropertyChange("Applogo");
        }
        public string Applogo
        {
            get
            {
                if (DataHelpers.Settings.AppLogo != null)
                    return Path.GetFullPath(DataHelpers.Settings.AppLogo);
                return "";
            }
        }

        public string ConnectedUser
        {
            get
            {
                return DataHelpers.ConnectedUser?.Name;
            }
        }

        public string Customer { get { return DataHelpers.Customer; } }
        public bool FastSearchCheck { get; set; }
        public List<TreeViewItemExCategory> MainMenuCategories { get; set; } = new List<TreeViewItemExCategory>();
        public bool MenuIsExpanded { get; set; } = true;
        public int menuWidth { get; set; } = 200;
        public List<DateTime> months { get; set; } = new List<DateTime>();
        public List<MenuItem> NotificationItems { get; set; } = new List<MenuItem>();
        public bool SearchIsDropDownOpen { get; set; }
        public string SearchMenuText { get; set; }
        public List<SearchItem> SearchResults { get; set; } = new List<SearchItem>();
        public SearchItem SearchSelectedItem { get; set; }
        public DateTime? selectedMonth { get; set; } = DateTime.Today;
        public ScrollViewer sideMenu { get; set; } = new ScrollViewer();
        public StackPanel TopbarContent { get; set; } = new StackPanel();
       
         

        public void AddModule()
        {
            var modules = new ModulesManagerViewModel(this.windowManager);
            windowManager.ShowWindow(modules);
            //  ModuleErp.AddModules();
        }

        public void CloseApp()
        {
            this.RequestClose();
        }

        public void CloseMenu()
        {
        }

        public void CloseOthers()
        {
            //var actives = this.Items.ToList();
            //foreach (var item in actives)
            //{
            //    if (ActiveItem != item)
            //        item.RequestClose();
            //}
        }

        public void CloseScreen(IScreen screen)
        {
            DeactivateItem(screen);
          //  Items.Remove(screen);
            //Items.(screen);
        }

        public void ClosingTab()
        {
            menuWidth = 0;
            NotifyOfPropertyChange("menuWidth");
        }

        public async void OpenAllMenu()
        {
            SearchMenuTextSide = "";// "application";
            await SearchMenuTextChanged();
           ( this.View as ShellView).Toggle();
            return;
        }

        public void collapseMenu(bool open = false)
        {
            //if (open)

            //{
            //    (this.View as ShellView).OpenMenu();
            //    return;
            //}
           
              (this.View as ShellView).Toggle();
            return;
        }

        public void Handle(ModelChangeEvent message)
        {
            // SetupNotificationsItems();
        }

        public async void MenuItemChange(object sender, EventArgs args)
        {
            try
            {
                var nodeZ = (sender as TextBlock).GetBindingExpression(TextBlock.TextProperty).DataItem as TreeViewItemEx;//.NodeXml;
                await OpenMenuItem(nodeZ);
                (this.View as ShellView).CloseMenu();
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage($@"{_("home.error.contactprovider")}
                        {s.Message}", "Error !", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //collapseMenu();
        }

        public async void MenuItemChangeCat(object sender, EventArgs args)
        {
            var nodeZ = (sender as TextBlock).GetBindingExpression(TextBlock.TextProperty).DataItem as TreeViewItemExCategory;//.NodeXml;
            var name = nodeZ.Name;
        }

        public async Task OpenBaseType(Type type, string DisplayName)
        {
            var baseView = await DataHelpers.GetBaseViewModelScreen(type, aggre, false);
            // var control = await BaseViewModel.Create(Type.GetType(modelClass), this, aggre, windowManager);
            baseView.DisplayName = DisplayName;

           // Items.Add(baseView);
            ActivateItem(baseView);
             
        }

        
        //public void OpenReport()
        //{
        //    //var repor = new ReportViewModel(new SoldeStock());
        //    //this.Items.Add(repor);
        //    //ActivateItem(repor);
        //}
        public async Task OpenBaseType(string modelClass, string DisplayName)
        {
            try
            {
                Type type = DataHelpers.GetTypesModule.Resolve(modelClass);
                if (type == null)
                    return;


                var baseView = await DataHelpers.GetBaseViewModelScreen(type, aggre, false);
                // var control = await BaseViewModel.Create(Type.GetType(modelClass), this, aggre, windowManager);
                baseView.DisplayName = DisplayName;

                //   Items.Add(baseView);
                ActivateItem(baseView);
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }

        public void OpenHome()
        {
            var vm = new HomeViewModel();
            ActivateItem(vm);
            //var found = Items.FirstOrDefault(a => a.DisplayName == "Home");
            //if (found != null)
            //{
            //    ActivateItem(vm);
            //    return;
            //}

            //OpenScreen(vm, "Home");
        }

        //public async void OpenFacture()
        //{
        //    await DataHelpers.Shell.OpenBaseType("Facture", "Facture");
        //}
        public async void OpenInstance(IDocument doc)
        {
            this.OpenScreen(await DetailViewModel.Create(doc, doc.GetType(), aggre, this), $"{doc.CollectionName} - {doc.Name}");
        }

        public void OpenLicence()
        {
            var lm = new LicenceManagerViewModel();
            DataHelpers.windowManager.ShowWindow(lm);
        }

        public async Task OpenMenuItem(TreeViewItemEx nodeZ)
        {
            var node = nodeZ.NodeXml;
            var modelHeader = node.Attributes["header"].Value;
            var modelClass = node.Attributes["class"].Value;
            var modelIcon = node.Attributes["icon"].Value;
            var ins = node.Attributes["instance"];
            try
            {
               // var found = Items.FirstOrDefault(a => a.DisplayName == modelHeader);
                //if (found != null)
                //{
                //    ActivateItem(found);
                //}
                //else
                //{
                    if (ins == null)
                    {
                    try
                    {
                        Type type = DataHelpers.GetTypesModule.Resolve(modelClass);
                        if (type == null)
                            return;

                        var baseView = await DataHelpers.GetBaseViewModelScreen(type, aggre, false);
                        // var control = await BaseViewModel.Create(Type.GetType(modelClass), this, aggre, windowManager);
                        baseView.DisplayName = modelHeader;

                        //  Items.Add(baseView);
                        ActivateItem(baseView);
                    }
                    catch (Exception s)
                    {
                        DataHelpers.ShowMessageError(s);
                    }
                }
                    else
                    {
                        try
                        {
                            var isntance = ins.Value;
                        Type t = DataHelpers.GetTypesModule.Resolve(modelClass);
                        if (t == null)
                            return;

                        var one = Activator.CreateInstance(t);
                            var oneInstance = (IDocument)one.GetType()
                                .GetMethod(isntance).
                                Invoke(one, null);

                            if (oneInstance == null)
                                return;

                            this.OpenScreen(await DetailViewModel.Create(oneInstance, oneInstance.GetType(), aggre, this), $"{modelHeader}");
                        }
                        catch (Exception s)
                        {
                             DataHelpers.ShowMessageError(s);
                            return;
                        }
                    }
             //   }
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        /////// NOTIFICATIONS
        public void OpenModuleBuilder()
        {
            var ModuleBuimder = new ModuleBuilderViewModel();
            OpenScreen(ModuleBuimder, "Editor");
        }

        public async Task OpenModuleErp(ModuleErp module)
        {


            var displayname = module.Libelle;
            var className = module.ClassName;
            var iconImg = module.ModuleIcon;
            var instance = module.IsInstanceModule;
            var instanceFunction = module.InstanceFunction;

            //var found = Items.FirstOrDefault(a => a.DisplayName == displayname);
            //if (found != null)
            //{
            //    ActivateItem(found);
            //}
            //else
            //{
            if (instance == false)
            {
                //var type_ = Type.GetType(className);
                try
                {
                    Type atype = DataHelpers.GetTypesModule.Resolve(className);
                    if (atype == null)
                        return;
                    //  var fqn = type_.FullName;
                    var baseView = await DataHelpers.GetBaseViewModelScreen(atype, aggre, false);
                    baseView.DisplayName = displayname;
                    //     Items.Add(baseView);
                    ActivateItem(baseView);
                }
                catch (Exception s)
                {
                    DataHelpers.ShowMessageError(s);
                }
            }
            else
                {
                    try
                    {
                    Type type = DataHelpers.GetTypesModule.Resolve(className);
                 

                    var one = Activator.CreateInstance(type); //Activator.CreateInstance(Type.GetType(className));
                        var oneInstance = (IDocument)one.GetType()
                            .GetMethod(instanceFunction).
                            Invoke(one, null);

                        if (oneInstance == null)
                            return;

                        this.OpenScreen(await DetailViewModel.Create(oneInstance, oneInstance.GetType(), aggre, this), $"{displayname}");
                    }
                    catch (Exception s)
                    {
                         DataHelpers.ShowMessageError(s);
                        return;
                    }
            //    }
            }
        }

        public void OpenPos()
        {
            //var pos = new PointOfSaleViewModel();
            //DataHelpers.windowManager.ShowWindow(pos);
        }

        public void OpenScreen(IScreen screen, string title)
        {
            if (screen == null)
                return;
            try
            {
                screen.DisplayName = title;
             //   Items.Add(screen);
                ActivateItem(screen);
                ClosingTab();
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        public async void OpenScreenAttach(IDocument screen, string title)
        { 
            if (screen == null)
                return;
            try
            {
                var c = await DetailViewModel.Create(screen, screen.GetType(), aggre, this);

                c.DisplayName = $"{screen.CollectionName} - {title}";
              //  Items.Add(c);
                ActivateItem(c);
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
                return;
            }
        }

        public async Task OpenScreenDetach(IScreen screen, string title)
        {
            if (screen == null)
                return;
            try
            {
                var ioc = DataHelpers.container;
                var vm = ioc.Get<ViewManager>();
                // var c = await DetailViewModel.Create(screen, screen.GetType(), aggre, this);

                //c.DisplayName = screen.CollectionName;
                var content = vm.CreateAndBindViewForModelIfNecessary(screen);

                var cc = new ContentControl();
                cc.HorizontalAlignment = HorizontalAlignment.Stretch;
                cc.VerticalAlignment = VerticalAlignment.Stretch;
                cc.Content = content;

                GenericWindowViewModel gw = new GenericWindowViewModel(cc, title, title);
                windowManager.ShowDialog(gw);
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
                 DataHelpers.ShowMessage(s.Message);
                return;
            }
        }

        public async Task OpenScreenDetach(IDocument screen, string title)
        {
            if (screen == null)
                return;
            try
            {
                var ioc = DataHelpers.container;
                var vm = ioc.Get<ViewManager>();
                var c = await DetailViewModel.Create(screen, screen.GetType(), aggre, this);

                c.DisplayName = screen.CollectionName;
                var content = vm.CreateAndBindViewForModelIfNecessary(c);

                var cc = new ContentControl();
                cc.HorizontalAlignment = HorizontalAlignment.Stretch;
                cc.VerticalAlignment = VerticalAlignment.Stretch;
                cc.Content = content;

                GenericWindowViewModel gw = new GenericWindowViewModel(cc, c.DisplayName, screen.Name);

                windowManager.ShowDialog(gw);
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
                 DataHelpers.ShowMessage(s.Message);
                return;
            }
        }

        

        public async Task<IEnumerable<IDocument>> OpenScreenFind(Type docType, string displayName, 
            IEnumerable<IDocument> aList = null)
        {
            if (docType == null)
                return null;
            try
            {
                dynamic control = null;
                if (aList == null)
                { control = await DataHelpers.GetBaseViewModelScreen(docType, aggre, true); }
                else
                { control = DataHelpers.GetBaseViewModelScreen(docType, aggre, true, aList); }
                // var control = BaseViewModel.CreateSyncSelect(docType, this, aggre, windowManager);
                var ioc = DataHelpers.container;
                var vm = ioc.Get<ViewManager>();
                // var c = await DetailViewModel.Create(screen, screen.GetType(), aggre, this);
                control.DisplayName = displayName;
                var content = vm.CreateAndBindViewForModelIfNecessary(control);

                var cc = new ContentControl();
                cc.HorizontalAlignment = HorizontalAlignment.Stretch;
                cc.VerticalAlignment = VerticalAlignment.Stretch;
                cc.Content = content;

                GenericWindowViewModel gw = new GenericWindowViewModel(cc, displayName, "");
                var res = windowManager.ShowDialog(gw);
                var result = (control as IBaseViewModel).selectedList;

                return result;
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
                 DataHelpers.ShowMessage(s.Message);
                return null;
            }
        }

        public async void OpenScreenFindAttach(Type docType, string displayName, IEnumerable<IDocument> alist = null)
        {
            if (docType == null)
                return;
            try
            {
                if(alist != null)
                {
                    var control =   DataHelpers.GetBaseViewModelScreen(docType, aggre, false,alist);
                    control.DisplayName = displayName;
                    this.ActivateItem(control);
                }
                else
                {
                    var control = await DataHelpers.GetBaseViewModelScreen(docType, aggre, false);
                    control.DisplayName = displayName;
                    this.ActivateItem(control);
                }
            
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        public void OpenSettings()
        {
            var settings = ElvaSettings.getInstance();
            DataHelpers.Shell.OpenScreenAttach(settings, _("home.app.btn.settings"));
        }

        public async void OpenTest()
        {
            SearchMenuTextSide = "application";
            await SearchMenuTextChanged();

          //  var rep = new SpreadsheetViewModel();
          ////  this.Items.Add(rep);
          //  ActivateItem(rep);
        }

        //public async void OpenSoldeStock()
        //{
        //    await DataHelpers.Shell.OpenBaseType(typeof(SoldeStock_report), "Inventaire");
        //}

        public string MessageColor  { get {

                if (MessageCount > 0)
                    return "Red";
                return "#ECF0F1";
                        
            } }

        public string _notifPrefix { get {return _("home.app.btn.notification"); } }
       


        public long MessageCount { get;set;
        }


        public void UpdateNotificationsTitle()
        {
            NotifyOfPropertyChange("_notifPrefix");
            NotifyOfPropertyChange("MessageCount");
            NotifyOfPropertyChange("MessageColor");
            NotifyOfPropertyChange("NotificationsTitle");
        }
        public string NotificationsTitle { get
            {
                NotifyOfPropertyChange("_notifPrefix");
                return $"{MessageCount} {_notifPrefix}"; 
            }
        }


        public async void OpenNotificationsTitle()
        {
            CoreMessageBase.OpenListMessages(); 
        }

        private void SearchWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            

            NotifyOfPropertyChange("SearchResults");
          
            if (SearchResults.Count() == 1)
            {
                SearchMenuText = "";
                NotifyOfPropertyChange("SearchMenuText");
                var doc = SearchResults.FirstOrDefault();
                DataHelpers.Shell.OpenScreenAttach(doc.Doc, doc.Doc.Name);
            }

        }

        private void SearchWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SearchResults = new List<SearchItem>();
            var modules = DataHelpers.ModulesSearch;
            foreach (var item in modules)
            {
                try
                {
                    var className = item.ClassName;
                    var value = DS.Generic(className)?.Find("NameSearch", SearchMenuText, false);
                    if (value != null)
                    {
                        foreach (IDocument doc in value)
                        {
                            SearchResults.Add(new SearchItem(doc));
                            NotifyOfPropertyChange("SearchResults");
                        }
                    }

                }
                catch 
                {
                    continue; 
                }
                SearchWorker.ReportProgress(0);
            }
        }

        private void SearchWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            
        }

        public System.ComponentModel.BackgroundWorker SearchWorker { get; set; } 

        public async void SearchDocument()
        {
            if (string.IsNullOrWhiteSpace(SearchMenuText) || SearchMenuText.Length <= 2 || SearchWorker.IsBusy)
                return;
            SearchIsDropDownOpen = true;
            NotifyOfPropertyChange("SearchIsDropDownOpen");

            SearchWorker.RunWorkerAsync();

            //SearchResults = new List<SearchItem>();
            //var modules = DataHelpers.ModulesSearch;
            //foreach (var item in modules)
            //{
            //    var className = item.ClassName;//.Split('.').LastOrDefault();

            //    var value = DataHelpers.GetMongoData(className, "NameSearch", SearchMenuText, false);
            //    //var genericName = DataHelpers.GetMongoData(className, "GenericName", SearchMenuText, false);

            //    if (value != null)
            //    {
            //        foreach (var doc in value)
            //        {
            //            SearchResults.Add(new SearchItem(doc));
            //        }
            //    }

            //}


        }



        public string SearchMenuTextSide { get; set; }
        public async Task SearchMenuTextChanged()
        {
            //  if (FastSearchCheck == false)
              
                collapseMenu(true);
                await SetupSideMenu();

                List<TreeViewItemExCategory> newResult = new List<TreeViewItemExCategory>();

                var cats = MainMenuCategories.Where(a => a.Name.ContainsIgniorCase(SearchMenuTextSide));
                if(cats != null)
                {
                    newResult = cats.ToList();
                }
                //MainMenuCategories.ForEach(cat =>
                //{
                //    var toleave = cat.Items.Where(a => a.Name.ContainsIgniorCase(SearchMenuText));

                //    var newCat = new TreeViewItemExCategory(cat.Name);
                //    newCat.Items = new ObservableCollection<TreeViewItemEx>(toleave);
                //    if (newCat.Items.Any())
                //        newResult.Add(newCat);
                //});

                //if (newResult.Count == 1)
                //{
                //    // Open directly
                //    var item = newResult.FirstOrDefault();
                //    var lines = item.Items;

                //    if (lines != null && lines.Count == 1)
                //    {
                //        var line = lines.FirstOrDefault();
                //        await OpenMenuItem(line);
                //        return;
                //    }
                //}

                MenuIsExpanded = true;
                NotifyOfPropertyChange("MenuIsExpanded");
                MainMenuCategories = newResult;
                NotifyOfPropertyChange("MainMenuCategories");
          
        }

        public void SearchSelectedChange()
        {
            if (SearchSelectedItem != null)
            {
                SearchMenuTextSide = "";
                NotifyOfPropertyChange("SearchMenuTextSide");
                DataHelpers.Shell.OpenScreenAttach(SearchSelectedItem.Doc, SearchSelectedItem.Doc.Name);
            }
        }

        public StackPanel MenuPanel { get; set; } = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        public string searchbar { get
            {
                return _("home.app.search");
            }
        }
        public bool MenuPanelLoaded { get; set; } = false;
        public async Task SetupSideMenu()
        {
            MessageCount = DS.db.Count<CoreMessageBase>(a => a.DueDate <= DateTime.Now && a.StatusMessage == "En attente");

            // Init actions
            MainMenuCategories.Clear();

            //      sideMenu.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            // Load config models
            var models = new List<IDocument>();
            var config = new XmlDocument();
            config.Load("Modules/Models.xml");

            // get ModulesERP
            var modules = DataHelpers.Modules;// GetMongoDataSync("ModuleErp") as IEnumerable<ModuleErp>;
            config.DocumentElement.RemoveAll();

            var TopMenus = modules.Where(z => !string.IsNullOrEmpty(z.GroupeModule)).Select(a =>_(a.GroupeModule)).Distinct();

            if (MenuPanelLoaded == false)
                MenuPanel.Children.Clear();

            foreach (var groupe in TopMenus)
            {
                var moduleOfGroupe = modules.Where(a => _(a.GroupeModule) == groupe).OrderBy(a => a.ModuleMenuIndex);

                //(2) string.Empty makes cleaner code
                XmlElement section = config.CreateElement(string.Empty, "section", string.Empty);
                section.SetAttribute("header", groupe);
                section.SetAttribute("icon", "Desktop");

                if(MenuPanelLoaded == false)
                {
                    string _icon = "";
                    var _savedIcon = ModuleIcons.ModuleIcon.TryGetValue(groupe, out _icon);

                    //if (_savedIcon)
                    //{
                        // icon found
                        Button menubtn = new Button();
                        PackIcon pi = new PackIcon() { Kind = PackIconKind.CheckboxMarkedCircleOutline, Height=22,Width=22};
                        try
                        {
                            pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), _icon);
                            pi.Height = 22; pi.Width = 22;
                        }
                        catch { }

                        Border border = new Border();
                        border.BorderThickness = new Thickness(0);
                        border.Margin =new Thickness(0);
                        border.HorizontalAlignment = HorizontalAlignment.Left; 
                        border.Child = menubtn;
                       
                        var sp = new StackPanel { Orientation = Orientation.Horizontal };
                        sp.Children.Add(pi);
                        sp.Children.Add(new TextBlock { Text =  groupe ,Foreground = Brushes.White , Margin=new Thickness(10,0,10,0)});

                        menubtn.Content = sp;
                        menubtn.Click += Menubtn_Click;
                        menubtn.Tag = new ArrayList { groupe, pi };
                        ShadowAssist.SetShadowDepth(menubtn, ShadowDepth.Depth0);
                     //   menubtn.ToolTip = groupe;
                        // BorderThickness="0"  BorderBrush="#F5F6F7"
                        menubtn.BorderThickness = new Thickness(0);
                      //  menubtn.Width = 55;
                        menubtn.Height = 40;
                        menubtn.Style = App.Current.TryFindResource("MenuButtonHd") as Style;
                        border.Child = menubtn;
                        MenuPanel.Children.Add(border);
                        NotifyOfPropertyChange("MenuPanel");
                    //}
                }
                

                foreach (var module in moduleOfGroupe)
                {
                    XmlElement document = config.CreateElement(string.Empty, "item", string.Empty);
                    //header ="Tiers" icon="Users" description="" class="Ovresko.Generix.Core.Modules.Tier"
                    document.SetAttribute("header", module.Libelle);
                    document.SetAttribute("icon", "Users");
                    document.SetAttribute("class", module.ClassName);
                    if (module.IsInstanceModule)
                        document.SetAttribute("instance", "getInstance");
                    section.AppendChild(document);
                }
                config.DocumentElement.AppendChild(section);
            }
            MenuPanelLoaded = true;
            config.Save("Modules/Models.xml");
            config.Load("Modules/Models.xml");
            parentStack = new StackPanel();
            // Iteerate over items
           
            foreach (XmlNode node in config.DocumentElement.ChildNodes)
            {
                // iterate over sections
                Console.Write(node.ChildNodes);
                var header = node.Attributes["header"].Value;

                // ExpanderMenu expander = new ExpanderMenu();

                //   var nodeTree = new TreeViewItemExCategory(header, new Li);
                var Children = new List<TreeViewItemEx>();
                //
                var items = node.ChildNodes;

                foreach (XmlNode item in items)
                {
                    var modelHeader = item.Attributes["header"].Value;
                    var modelClass = item.Attributes["class"].Value;
                    var modelIcon = item.Attributes["icon"].Value;
                    var modelInstance = item.Attributes["instance"]?.Value;

                    var itemNode = new TreeViewItemEx(modelHeader, modelHeader);
                    itemNode.NodeXml = item;
                    //   itemNode.HeaderContent = new Button() { Content = modelHeader };

                    //var i = new FontAwesome.WPF.ImageAwesome();
                    //// i.Icon = (FontAwesomeIcon)Enum.Parse(typeof(FontAwesomeIcon), modelIcon);
                    //i.Icon = FontAwesomeIcon.Plus;
                    //i.Foreground = System.Windows.Media.Brushes.LightGray;
                    // itemNode.HeaderIcon = modelIcon;
                    Children.Add(itemNode);
                }
                var Category = new TreeViewItemExCategory(header, Children.ToArray());

                MainMenuCategories.Add(Category);
            }
            NotifyOfPropertyChange("sideMenu");
        }

        public PackIconKind BigMenuIcon { get; set; } 

        private async void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            var array = (sender as Button).Tag as ArrayList;
            //ArrayList { groupe, pi };
            var icon = array[1] as PackIcon;
            BigMenuIcon = icon.Kind;
            NotifyOfPropertyChange("BigMenuIcon");
            var groupe = array[0].ToString();

            SearchMenuTextSide = groupe;// "application";
            await SearchMenuTextChanged();
        }

        public void SetupTopBar()
        {
            TopbarContent = new StackPanel();
            TopbarContent.Orientation = Orientation.Horizontal;
            var modules = DataHelpers.Modules.Where(a => a.EstTopBar);

            foreach (var item in modules)
            {
                Button btn = new Button();
                btn.Tag = item;

                PackIcon pi = new PackIcon();
                try
                {
                    pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), item.ModuleIcon);
                }
                catch (Exception s)
                {
                    pi.Kind = PackIconKind.OpenInApp;
                }
                pi.Width = 23;
                pi.HorizontalAlignment = HorizontalAlignment.Center;
                pi.Height = 23;

                btn.Content = pi;
                btn.Click += Btn_Click;
                // btn.Foreground = Brushes.Black;
                btn.ToolTip = item.Libelle;
                ShadowAssist.SetShadowDepth(btn, ShadowDepth.Depth0);
                btn.BorderThickness =new Thickness(0);
                //btn.Background = Brushes.Transparent;
                //btn.Foreground = Brushes.
                 btn.Style = App.Current.TryFindResource("ToolbarButtonHd") as Style;
                TopbarContent.Children.Add(btn);
            }
            NotifyOfPropertyChange("TopbarContent");
        }

        public void showActiontemp()
        {
        }

        public void VisitWebsite()
        {
            System.Diagnostics.Process.Start("http://www.ovresko.com");
        }

        protected override void OnInitialActivate()
        { 

            var appStartup = new StartupViewModel(windowManager);
            windowManager.ShowDialog(appStartup);

            #region LOGIN

            var login = new LoginViewModel();
            var res = windowManager.ShowDialog(login);

            if (!res.HasValue || !res.Value)
            {
                try { showClose(); } catch { }
            }
            NotifyOfPropertyChange("ConnectedUser");

            #endregion LOGIN





            var setupside = SetupSideMenu();
            setupside.Wait();
            SetupTopBar();
            NotifyOfPropertyChange("MainMenuCategories");

            NotifyAppLogo();


            // If First launch start app assistant
            if (DataHelpers.Settings?.AppInitialized == false)
                StartAppAssistant();

            UpdateNotificationsTitle();
        }

        private void StartAppAssistant()
        {
            var assistant = new AppAssistantViewModel(windowManager);
            windowManager.ShowWindow(assistant);
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            OpenHome();
            //DataHelpers._ParentWindow = (this.View as ShellView).myMainWindow;//.TryFindParent<Window>();
            //  SetupNotificationsItems();
        }

        private async void Btn_Click(object sender, RoutedEventArgs e)
        {
            var module = (sender as Button).Tag;
            if (module != null)
            {
                await DataHelpers.Shell.OpenModuleErp(module as ModuleErp);
            }
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var allexpanders = parentStack.Children;

            foreach (Expander item in allexpanders)
            {
                if (item != (sender as Expander) && item.IsExpanded == true)
                    item.IsExpanded = false;
            }

            //if((sender as Expander).IsExpanded == false)
            //{
            //    (sender as Expander).IsExpanded = true;
            //}
        }

        private void ShellViewModel_StateChanged(object sender, ScreenStateChangedEventArgs e)
        {
        }

        public class SearchItem
        {
            public SearchItem(IDocument doc)
            {
                Doc = doc;
            }

            public IDocument Doc { get; set; }

            public string Name
            {
                get
                {
                    return $"{Doc.NameSearch}  ({Doc.CollectionName})";
                }
            }
        };

        /// <summary>
        /// Click pour ouvrir les listes
        /// </summary>

        #region SHOW COMMANDS

        public void showBulkEntryViewModel()
        {
            //var control = new BulkEntryViewModel(typeof(Collect), aggre, windowManager);
            //control.DisplayName = "Entrée en masse";
            //Items.Add(control);
            //ActivateItem(control);
            // DataHelpers.ShowMessage( "Sous développement");
            //return;
        }

        public void showClose()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void showUpdate()
        {
            //  AutoUpdater.Start("https://www.dl.dropboxusercontent.com/s/pwhbwgwvo6oku0i/Releases.xml");
        }


       
        #endregion SHOW COMMANDS
        //public void Search(object sender, KeyEventArgs e)
        //{
        //    if (!string.IsNullOrWhiteSpace(SearchMenuText))
        //    {
        //        e.Handled = true;
        //        return;
        //    }
        //        if (e.Key == Key.Back || e.Key == Key.Delete)
        //    {
        //        e.Handled = true;
        //    }
        //    Keyboard.Focus((this.View as ShellView).SearchBox);
        //}
    }

    public class TreeViewItemEx
    {
        public TreeViewItemEx(string name, string director)
        {
            Name = name;
            Director = director;
        }

        public string Director { get; }
        public string Name { get; }
        public XmlNode NodeXml { get; set; }
    }

    public class TreeViewItemExCategory
    {
        public TreeViewItemExCategory(string name)
        {
            Name = name;
        }

        public TreeViewItemExCategory(string name, params TreeViewItemEx[] items)
        {
            Name = name;
            Items = new ObservableCollection<TreeViewItemEx>(items);
        }

        public ObservableCollection<TreeViewItemEx> Items { get; set; }
        public string Name { get; }
    }
}