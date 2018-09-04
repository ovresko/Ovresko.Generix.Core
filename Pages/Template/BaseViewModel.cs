using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.Events;
using Ovresko.Generix.Core.Pages.MassEdit;
using Ovresko.Generix.Datasource.Models;
using Ovresko.Generix.Utils;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Unclassified.TxLib;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Pages.Template
{
    public class BaseViewModel<T> : Screen, IDisposable, IHandle<ModelChangeEvent>, IBaseViewModel where T : Document, new()
    {
        public IWindowManager windowManager;
        private List<T> _list = new List<T>();
        private IEventAggregator aggre;
        private bool fromFiltre = false;
        public BaseViewModel()
        {
        }

        public string ElementsCount
        {
            get
            {
                return $"{Items.Count * PageNumber} / {TotalCount} {_("eléments")} ";
            }
        }

        public int PagesNumber
        {
            get
            {
                if (TotalCount <= PageCount)
                    return 1;

                try { return ((int)TotalCount / PageCount) + 1; } catch { }
                return 1;
            }
        }

        public BaseViewModel(IEventAggregator _aggre, bool ForSelectOnly)
        {
            this.ForSelectOnly = ForSelectOnly;
            shell = DataHelpers.Shell;
            windowManager = DataHelpers.windowManager;
            aggre = _aggre;

            if (SaveVisible == false)
            {
                var s = new T();
                var i = DS.db.GetAll<T>();// s.GetList();
                this._list = i;
                PAGE_MODE = PAGE_MODES.LIST;
                PageCount = 200;
                RowHeight = 25;
                CellPadding = 4;
                DataGridTag = "report";
                Task.Run(async () => await NextPage());
            }
            else
            {
                Task.Run(async () => await NextPage());
            }

            SetupOperationButtons();
        }

        public BaseViewModel(IEventAggregator _aggre, bool ForSelectOnly, IEnumerable<IDocument> _list)
        {
            if (this._list == null)
                this._list = new List<T>();

            foreach (var item in _list)
            {
                this._list.Add( (T)item);
            }
            //this._list = _list;// as IEnumerable<T>;
            TotalCount = (long)_list?.Count();

            PAGE_MODE = PAGE_MODES.LIST;
            fromFiltre = true;
            this.ForSelectOnly = ForSelectOnly;
            shell = DataHelpers.Shell;
            windowManager = DataHelpers.windowManager;
            aggre = _aggre;

            Task.Run(async () => await NextPage());
            SetupOperationButtons();

        }

        public string btnFermerText
        {
            get
            {
                return _("Fermer");
            }
        }

        public string AddButton
        {
            get
            {
                if (!ForSelectOnly)
                    return _("base.button.add");

                return _("Selectionner");
            }
        }

        public string btnNouveauHint
        {
            get
            {
                return _("Créer un nouveau document!");
            }
        }

        public string btnNouveauText
        {
            get
            {
                return _("Nouveau");
            }
        }

        public int CellPadding { get; set; } = 6;
        public string ChercherHint
        {
            get
            {
                return _("Chercher");
            }
        }

        public string DataGridTag { get; set; } = "";
        public DataHelpers datahelper { get; set; } = new DataHelpers();
        public WrapPanel Filtres { get; set; } = new WrapPanel();
        public bool FiltreVisible { get; set; } = false;
        public bool ForSelectOnly { get; set; } = false;
        public FlowDirection GetFlowDirection
        {
            get
            {
                return DataHelpers.GetFlowDirection;
            }
        }

        public bool IsRunning { get; set; }
        public BindableCollection<T> Items { get; set; } = new BindableCollection<T>();
        public string LblAddBureau
        {
            get
            {
                return _("Ajouter au bureau");
            }
        }

        public string LblEditionMasse
        {
            get
            {
                return _("Édition en masse");
            }
        }

        public string LblExporter
        {
            get
            {
                return _("Exporter");
            }
        }

        public string LblExportertemplate
        {
            get
            {
                return _("Exporter modéle");
            }
        }

        public string LblImporter
        {
            get
            {
                return _("Importer");
            }
        }

        public string LblModifierModel
        {
            get
            {
                return _("Modifier module");
            }
        }

        public string LblOutils
        {
            get
            {
                return _("Outils");
            }
        }

        public string LblSupprimer
        {
            get
            {
                return _("Supprimer");
            }
        }

        public SnackbarMessageQueue MessageQueue { get; set; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(1));
        public List<MenuItem> opeartionButtons { get; set; }
        public int PageCount { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
        public int RowHeight { get; set; } = 30;
        public IDocument selected { get; set; }
        // private T Instance;
        public IEnumerable<IDocument> selectedList { get; set; }

        public IShell shell { get; set; }
        public List<int> ShowCounts
        {
            get
            {
                return new List<int>()
                {   20,
                    50,
                    100,
                    200,
                    500
                };
            }
        }

        public Type type
        {
            get
            {
                return typeof(T);
            }
        }
        public static void Outside()
        {
        }

        public async Task Actualiser()
        {
            if (!fromFiltre && SaveVisible == true)
                PAGE_MODE = PAGE_MODES.ALL;

            if (SaveVisible == false)
                PAGE_MODE = PAGE_MODES.LIST;

            NameSearch = "";
            await NextPage();
            NotifyOfPropertyChange("NameSearch");
            NotifyOfPropertyChange("PageNumber");
            MessageQueue.Enqueue(_("Données actualisées"));
        }

        public async void Add()
        {
            if (ForSelectOnly)
            {
                selectedList = Items.Where(a => (a as IDocument).IsSelectedd);
                ForSelectOnly = false;
                CloseWindows();
            }
            else
            {
                try
                {
                    selected = new T();

                    if (selected.DocOpenMod == OpenMode.Attach && ForSelectOnly == false)
                    {
                        //selected.CollectionName = displayName;
                        shell.OpenScreen(await DetailViewModel.Create(selected, selected.GetType(), aggre, shell), $"{this.DisplayName} | {selected.Name}");
                    }
                    else
                    {
                        var ioc = DataHelpers.container;
                        var vm = ioc.Get<ViewManager>();
                        var c = await DetailViewModel.Create(selected, selected.GetType(), aggre, shell);

                        await shell.OpenScreenDetach(selected, selected.Name);

                    }
                }
                catch (Exception s)
                {
                    DataHelpers.ShowMessage(s.Message + " " + s.StackTrace);
                    return;
                }
            }


        }

        public void AjouterAuBureau()
        {
            var instance = Activator.CreateInstance<T>();
            var module = this.type.GetMethod("MyModule")?.Invoke(instance, null); //
            if (module != null)
            {
                // Activate show
                (module as ModuleErp).EstAcceRapide = true;
                (module as ModuleErp).Save();
                DataHelpers.ShowMessage($"Icon {_(instance.CollectionName)},{_("ajoutée au bureau")}");
                return;
            }
        }

        public async void DeleteAll()
        {
            var selected = Items.Where(a => a.IsSelectedd).ToList();
            var confirm = DataHelpers.ShowMessage($"{_("Voulez-vous supprimer ces")} {selected.Count} {_("documents?")}", _("Confirmation!"), MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.No)
                return;

            if (selected != null && selected.Any())
            {
                foreach (IModel item in selected)
                {
                    try
                    {
                        if (!item.Delete(false))
                            continue;
                    }
                    catch (Exception s)
                    {
                        DataHelpers.ShowMessage(s.Message);
                        continue;
                    }
                }
                await Actualiser();
            }
        }

        public void Dispose()
        {
        }

        public async void ExporterPDF()
        {
            if (SearchResul?.Any() == true)
            {
                windowManager.ShowWindow(new PrintWindowViewModel(SearchResul));
            }
            else
            {
                var response = DataHelpers.ShowMessage(_("Voulez-vous exporter tous les documents?"), _("Confirmation"), MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    if (SaveVisible == false)
                    {
                        SearchResul = _list.ToList<T>();
                        windowManager.ShowWindow(new PrintWindowViewModel(SearchResul));
                    }
                    else
                    {
                        SearchResul = DS.db.GetAll<T>();// await datahelper.GetData<T>(a => true);
                        windowManager.ShowWindow(new PrintWindowViewModel(SearchResul));
                    }
                }
            }
        }

        public void ExportTemplate()
        {
            var dialog = new SaveFileDialog();
            var file = dialog.ShowDialog();
            if (file == true)
            {
                var result = dialog.FileName;
                if (!result.Contains("xls"))
                    result += ".xls";

                var dp = new DynamicPath(result);
                var ovimport = new ExcelImport(dp);

                ovimport.ExportTemplate(result, type);
                Process.Start(result);
            }
        }

        public void GridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ouvrirItem();
            }
        }

        public void Handle(ModelChangeEvent message)
        {
            if (message.type == this.type)
            {
                Execute.OnUIThreadAsync(async () =>
                {
                    await Actualiser();
                });
            }
        }

        public void ImportData()
        {
            try
            {
                var dialog = new OpenFileDialog();
                var file = dialog.ShowDialog();
                if (file == true)
                {
                    var result = dialog.FileName;
                    var ovimport = new ExcelImport(new DynamicPath(result));

                    var data = ovimport.ImportDataFromType(result, type);

                    if (data != null)
                    {
                        int? count = null;
                        try { count = data.Count(); } catch { }
                        var confirmation = DataHelpers.ShowMessage($"{count} {_("documents trouvés")}, {_("voulez-vous continuer!")}", _("Confirmation"), MessageBoxButton.YesNo);
                        if (confirmation == MessageBoxResult.Yes)
                        {
                            foreach (dynamic item in data)
                            {


                                item.AddedAtUtc = DateTime.Now;
                                //     item.isLocal = false;
                                try
                                {
                                    // item.Series = item.MyModule()?.Id;

                                    item.ForceIgniorValidatUnique = true;
                                    if ((item as IModel).Save())
                                    {
                                        (item as IModel).Submit();
                                    }
                                    else
                                    {
                                        throw new Exception(_("une erreur s'est produite!")); 
                                    }
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s);
                                    var shouldStop = DataHelpers.ShowMessage(_("une erreur s'est produite voulez-vous annuler?"), _("Erreur"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                                    if (shouldStop == MessageBoxResult.Yes)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                // item.Save();
                            }
                            //var mi = typeof(BaseMongoRepository).GetMethod("AddMany");
                            //var gen = mi.MakeGenericMethod(type);
                            //gen.Invoke(DS.db, new object[] { data });
                            //  DS.db.AddMany<t>(data);
                            DataHelpers.ShowMessage(_("Terminé"));
                            Actualiser();
                        }
                    }
                    else
                    {
                        DataHelpers.ShowMessage(_("Aucun document trouvé"));
                        return;
                    }
                }
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessage(s.Message);
                return;
            }
        }

        public void MassEdit()
        {
            if (SearchResul != null && SearchResul.Any())
            {
                var editMass = new MassEditViewModel(type, (IEnumerable<object>)SearchResul);
                //var view = DataHelpers.container.Get<ViewManager>();
                //var bind = view.CreateAndBindViewForModelIfNecessary(editMass);
                DataHelpers.windowManager.ShowWindow(editMass);
            }
            else
            {
                DataHelpers.ShowMessage(_("Filter les résultats d'abord"));
                return;
            }
        }

        public async void ModifierModule()
        {
            try
            {
                var instance = Activator.CreateInstance<T>();
                ModuleErp module = this.type.GetMethod("MyModule")?.Invoke(instance, null) as ModuleErp; //
                if (module != null)
                {
                    await DataHelpers.Shell.OpenScreenDetach(module, module.CollectionName);
                }
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
                throw;
            }
        }

        public async void nextPage()
        {
            PageNumber++;

            await NextPage();
            NotifyOfPropertyChange("PageNumber");
            NotifyOfPropertyChange("CurrentPage");
        }

        public async Task NextPage()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            switch (PAGE_MODE)
            {
                case PAGE_MODES.ALL:

                    SearchResul = new List<T>();
                    TotalCount = DS.db.Count<T>();// await datahelper.GetMongoDataCount<T>();
                    Items.Clear();
                    //Items.AddRange(await datahelper.GetMongoDataPaged<T>(PageNumber, PageCount));
                    Items.AddRange( DS.db.GetPage<T>(PageNumber, PageCount));

                    break;

                case PAGE_MODES.FILTER_TEXT:
                    if (SaveVisible == false)
                    {
                        //TotalCount = await datahelper.GetMongoDataCount<T>(a => a.NameSearch.Contains(NameSearch.ToLower()));
                        Items.Clear();
                        var result = _list.Where(a => a.Name.ToLower().Contains(NameSearch.ToLower()));
                        Items.AddRange(result.Skip((PageNumber - 1) * PageCount).Take(PageCount));
                    }
                    else
                    {
                        TotalCount = DS.db.Count<T>(a => a.NameSearch.Contains(NameSearch.ToLower()));
                            //await datahelper.GetMongoDataCount<T>(a => a.NameSearch.Contains(NameSearch.ToLower()));
                        Items.Clear();
                        Items.AddRange( DS.db.GetPage<T>( a=> a.NameSearch.ContainsIgniorCase(NameSearch.ToLower()), PageNumber, PageCount));
                       // Items.AddRange(await datahelper.GetMongoDataFilterPaged<T>(NameSearch.ToLower(), PageNumber, PageCount));

                    }
                    break;

                case PAGE_MODES.FILTER_BOX:
                    TotalCount = SearchResul.Count;
                    Items.Clear();
                    Items.AddRange(SearchResul.Skip((PageNumber - 1) * PageCount).Take(PageCount));
                    break;

                case PAGE_MODES.LIST:
                    TotalCount = _list.Count();
                    Items.Clear();
                    Items.AddRange(_list.Skip((PageNumber - 1) * PageCount).Take(PageCount));
                    break;

                default:
                    break;
            }

            Items.Refresh();
            NotifyOfPropertyChange("Items");
            NotifyOfPropertyChange("ElementsCount");
            NotifyOfPropertyChange("PagesNumber");

            IsRunning = false;
        }

        public async void ouvrirItem()
        {
            try
            {
                if (selected != null)
                {
                    // Show for select item  only
                    if (ForSelectOnly)
                    {
                        selectedList = Items.Where(a => a.IsSelectedd);
                        ForSelectOnly = false;
                        CloseWindows();
                    }
                    else
                    {
                        if (selected.DocOpenMod == OpenMode.Attach)
                        {
                            // selected.dis = displayName;
                            shell.OpenScreen(await DetailViewModel.Create(selected, selected.GetType(), aggre, shell), $"{selected.CollectionName} - {selected.Name}");
                        }
                        else
                        {
                            var ioc = DataHelpers.container;
                            var vm = ioc.Get<ViewManager>();
                            var c = await DetailViewModel.Create(selected, selected.GetType(), aggre, shell);
                            c.DisplayName = selected.CollectionName;
                            var content = vm.CreateAndBindViewForModelIfNecessary(c);

                            var cc = new ContentControl();
                            cc.HorizontalAlignment = HorizontalAlignment.Stretch;
                            cc.VerticalAlignment = VerticalAlignment.Stretch;
                            cc.Content = content;

                            GenericWindowViewModel gw = new GenericWindowViewModel(cc, displayName, selected.Name);
                            windowManager.ShowWindow(gw);
                        }
                    }
                }
            }
            catch (Exception s)
            {
                MessageQueue.Enqueue(s.Message);
                return;
            }
        }

        public async void prevPage()
        {
            PageNumber--;
            if (PageNumber <= 0)
            {
                PageNumber = 1;
                return;
            }
            // await GetPages();
            await NextPage();
            NotifyOfPropertyChange("PageNumber");
        }
        public bool actionsVisible { get; set; } = false;
        public void SetupOperationButtons()
        {
            Execute.OnUIThreadAsync(() =>
            {
                var sourceType = type;

                #region Filtres

                Filtres = new WrapPanel();

                var baseFilter = type.GetProperties().Where(a => (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute) != null
                && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute).FieldType == ModelFieldType.BaseFilter);

                foreach (var filter in baseFilter)
                {
                    FiltreVisible = true;
                    var attrib = (filter.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute);
                    var attribDisplay = (filter.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute);

                    Button newOps = new Button();
                    newOps.Content = attribDisplay.DisplayName;
                    newOps.Click += NewOps_Click1;
                    newOps.Style = App.Current.FindResource("MaterialDesignFlatButton") as Style;
                 //   newOps.Foreground = Brushes.#3F51B5;
                    newOps.Margin = new Thickness(2, 5, 0, 0);
                    newOps.Tag = attrib.Options;  // <= the name of the function
                    Filtres.Children.Add(newOps);
                }
                NotifyOfPropertyChange("FiltreVisible");

                #endregion Filtres

                opeartionButtons = new List<MenuItem>();

                var baseAction = type.GetProperties().Where(a => (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute) != null
                && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute).FieldType == ModelFieldType.BaseButton);

                foreach (var action in baseAction)
                {
                    actionsVisible = true;
                    var attrib = (action.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute);
                    var attribDisplay = (action.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute);
                    MenuItem newOps = new MenuItem();
                    newOps.Header = attribDisplay.DisplayName;
                    newOps.Click += NewOps_Click;
                    newOps.TouchDown += NewOps_Click;
                    //  newOps.Style = App.Current.FindResource("MaterialDesignFlatButton") as Style;
                    newOps.Tag = attrib.Options;  // <= the name of the function
                    opeartionButtons.Add(newOps);
                }

                //opeartionButtons.Orientation = Orientation.Horizontal;
            });
        }
        public async void ValidateAll()
        {
            var selected = Items.Where(a => a.IsSelectedd);
            if (selected != null && selected.Any())
            {
                foreach (IModel item in selected)
                {
                    try
                    {
                        item.Submit();
                    }
                    catch (Exception s)
                    {
                        DataHelpers.ShowMessage($"{s.Message}\n{((IDocument)item).Name}");
                        continue;
                    }
                }
                await Actualiser();
            }
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            InitContextMenu();
            aggre.Subscribe(this);
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }

        private async void NewOps_Click(object sender, RoutedEventArgs e)
        {
            var method = (sender as MenuItem).Tag.ToString();
            var instance = new T();

            (instance as IDocument).DoFunction(method);
            await Actualiser();
        }

        private async void NewOps_Click1(object sender, RoutedEventArgs e)
        {
            var method = (sender as Button).Tag.ToString();
            var instance = new T();
            List<T> allData;
            if (SaveVisible)
            {
                allData = DS.db.GetAll<T>();// await datahelper.GetMongoDataAll<T>();
            }
            else
            {
                allData = _list.ToList();
            }
            var neitems = (instance as IDocument).DoFunction(method, new object[] { allData });
            _list = (neitems as IEnumerable<T>).ToList();
            PAGE_MODE = PAGE_MODES.LIST;
            var originalePage = PageCount;
            PageCount = 1000;
            await NextPage();
            _list = allData;
            PageCount = originalePage;
        }
        // public ListCollectionView CollectionItems { get; set; } = new ListCollectionView(new List<T>());
        #region TEMP

        //public IWindowManager windowManager;
        private string _NameSearch;

        public enum PAGE_MODES
        {
            ALL,
            LIST,
            FILTER_TEXT,
            FILTER_BOX
        }

        public string ActionsLbl
        {
            get
            {
                return _("Actions");
            }
        }

        public string displayName
        {
            get
            {
                return _(this.DisplayName);
            }
        }



        public int fontSize { get; set; } = 12;


        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public string NameSearch
        {
            get { return _NameSearch; }
            set
            {
                _NameSearch = value;
            }
        }

        public PAGE_MODES PAGE_MODE { get; set; } = PAGE_MODES.ALL;

        //public Visibility ShowButtonAjouter { get; set; } = Visibility.Visible;
        public bool SaveVisible
        {
            get
            {
                return !type.Name.Contains("_report");
            }
        }

        public List<T> SearchResul { get; set; } = new List<T>();
        public string StatusLabel { get; set; } = "...";

        //public string CurrentPage
        //{
        //    get
        //    {
        //        return $"{PageNumber * SelectedCOunt} /";
        //    }
        //}
        public long TotalCount { get; set; }

        public void BigFont()
        {
            fontSize++;
            NotifyOfPropertyChange("fontSize");
        }

        public void CloseWindows()
        {
            try
            {
                this.RequestClose();
            }
            catch
            {
                // DataHelpers.ShowMessage( this.View.GetParentObject().TryFindParent<Window>().GetType().ToString());
                this.View.GetParentObject().TryFindParent<Window>().Close();
            }
        }

        public async void doFiltrer()
        {
            List<T> allData = new List<T>();
            if (SaveVisible)
            {
                allData = DS.db.GetAll<T>();// await datahelper.GetMongoDataAll<T>();
            }
            else
            {
                allData = _list.ToList();
            }

            var filtre = new FiltreViewModel<T>(Filtres);
            DataHelpers.container.Get<ViewManager>().BindViewToModel(new FiltreView(), filtre);
            filtre.SetInputs(allData);
            var restulDialog = windowManager.ShowDialog(filtre);
            if (restulDialog == true)
            {
                PAGE_MODE = PAGE_MODES.FILTER_BOX;
                SearchResul = filtre.Result.ConvertAll<T>(a => (T)a);

                //   SelectedCOunt = Items.Count;
                //  ItemsSource = new PagingCollectionView(Items, SelectedCOunt);
                PageNumber = 1;
                await NextPage();
            }
        }

        public async void DoSearchKey()
        {
            await FilterThread();
        }
        ////}
        public async Task FilterThread()
        {
            StatusLabel = _("Recherche en cours");
            NotifyOfPropertyChange("StatusLabel");
            PAGE_MODE = PAGE_MODES.FILTER_TEXT;
            PageNumber = 1;

            await NextPage();
            StatusLabel = _("Terminé");
            NotifyOfPropertyChange("StatusLabel");
        }

        public void InitContextMenu()
        {
            var menuOpen = new MenuItem();
            menuOpen.Header = _("Ouvrir");
            menuOpen.Click += MenuOpen_Click;
            menuOpen.TouchDown += MenuOpen_Click;

            var menuDelete = new MenuItem();
            menuDelete.Header = _("Supprimer");
            menuDelete.Click += MenuDelete_Click; ;
            menuDelete.TouchDown += MenuDelete_Click; ;

            MenuItems.Add(menuOpen);
            MenuItems.Add(menuDelete);

            NotifyOfPropertyChange("MenuItems");
        }

        //public async void prevPage()
        //{
        //    PageNumber--;
        //    if (PageNumber <= 0)
        //    {
        //        PageNumber = 1;
        //        return;
        //    }
        //    // await GetPages();
        //    await Next();
        //    NotifyOfPropertyChange("PageNumber");
        //    NotifyOfPropertyChange("CurrentPage");
        //}
        //Thread t ;
        public async void SearchKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try { await FilterThread(); }
                catch (Exception s)
                {
                    DataHelpers.ShowMessage(s.Message);
                    return;
                }
            }
        }

        public void SelectAll()
        {
            foreach (IDocument item in Items)
            {
                item.IsSelectedd = !item.IsSelectedd;
            }
            NotifyOfPropertyChange("Items");
        }

        public void SmallFont()
        {
            fontSize--;
            NotifyOfPropertyChange("fontSize");
        }
        public void UserControl_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.F1)
            {
                Add();
            }
            else if (args.Key == Key.F2)
            {
                CloseWindows();
            }
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                selected.IsSelectedd = true;
                DeleteAll();
            }
            else
            {
                DataHelpers.ShowMessage(_("Selectionner une ligne!"));
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            ouvrirItem();
        }


        #endregion TEMP
    }
}