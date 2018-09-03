using AttributtedDataColumn;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MongoDB.Bson; 
using Ovresko.Generix.Core.Exceptions;
using Ovresko.Generix.Core.Modules.Core;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.Events;
using Ovresko.Generix.Core.Pages.PopupWait;
using Ovresko.Generix.Datasource.Models;
using Ovresko.Generix.Datasource.Services.Queries;
using Ovresko.Generix.Utils.Data.Export;
using Ovresko.Generix.Utils.Data.Import;
using Stylet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Pages.Template
{
    public class DetailViewModel : Screen, IDisposable, IHandle<DetailModelChangeEvent>
    {
        public WrapPanel spCard = new WrapPanel();

        private bool _CollapseAll = true;

        private LabelVisibilityConverter _LabelVisibilityConverter = new LabelVisibilityConverter();

        private UiVisibilityConverter _UiVisibilityConverter = new UiVisibilityConverter();

        private IEventAggregator aggre;

        private Border borderSeparation;

        private Button btnAddModel;

        private StackPanel cardSp = new StackPanel();

        private Expander expanderPref = new Expander();

        private bool isFreezed;

        private UniformGrid masterWrap;

        //public WrapPanel spCard2 = new WrapPanel();
        private UniformGrid masterWrapPref = new UniformGrid();

        private Thread notifyBaseThread;

        // private Binding myBinding;
        private Type type;

        protected async override void OnStateChanged(ScreenState previousState, ScreenState newState)
        {
            base.OnStateChanged(previousState, newState);

            if (model?.isLocal == false && previousState == ScreenState.Deactivated && newState == ScreenState.Active)
              await Setup(1000,true);
        }

        protected DetailViewModel(IDocument model, Type t, IEventAggregator _aggre, IShell shell)
        {
            saveWorker.DoWork += SaveWorker_DoWork;
            saveWorker.RunWorkerCompleted += SaveWorker_RunWorkerCompleted;
            saveWorker.WorkerSupportsCancellation = true;

            emailWorker.DoWork += EmailWorker_DoWork; ;
            emailWorker.RunWorkerCompleted += EmailWorker_RunWorkerCompleted; ;
            emailWorker.WorkerSupportsCancellation = true;
            // Change culture
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-DZ");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ar-DZ");

            if (!FrameworkManager.CanView(t))
            {
                throw new Exception(_("Vous n'etes pas autorise a visualiser ce document"));
            }

            this.shell = shell;
            aggre = _aggre;
            this.model = model; type = t;
            try { imgBg = System.IO.Path.GetFullPath(DataHelpers.imgBg); } catch { }
            DataHelpers.Aggre.Subscribe(this);
            //this.PropertyChanged += DetailViewModel_PropertyChanged;
            //StateChanged += DetailViewModel_StateChanged;

            borderSeparation = new Border();
            borderSeparation.Background = App.Current.FindResource("MaterialDesignDivider") as Brush;
            borderSeparation.Height = 1;
            borderSeparation.HorizontalAlignment = HorizontalAlignment.Stretch;
            borderSeparation.SnapsToDevicePixels = true;

            SetupDocCards();

            MaxLinesFetch = DataHelpers.Settings.MaxLinesFetch;
        }

        public string btnColor
        {
            get
            {
                var _model = (model as IDocument);

                if (_model.isLocal == true || IsEdited)
                {
                    return "#2196F3";//"Enregistrer";
                }
                if (_model.Submitable && _model.DocStatus == 0)
                {
                    return "#2FCC71";
                }
                else if (_model.Submitable && _model.DocStatus == 1)
                {
                    return "#FF3D00";
                }
                return "#2196F3";
            }
        }

        public string btnName
        {
            get
            {
                var _model = (model as IDocument);

                if (_model.isLocal == true || IsEdited)
                {
                    if (!_model.isLocal)
                        return _("Modifier");

                    return _("Enregistrer");
                }
                if (_model.Submitable && _model.DocStatus == 0)
                {
                    return _("Valider");
                }
                else if (_model.Submitable && _model.DocStatus == 1)
                {
                    return _("Annuler");
                }
                return _("Enregistrer");
            }
        }

        public bool CloseAfter { get; set; } = false;

        public bool CollapseAll
        {
            get { return _CollapseAll; }
            set
            {
                var elements = stackContent.FindChildren<Expander>();
                foreach (var item in elements)
                {
                    item.IsExpanded = !_CollapseAll;
                }
                _CollapseAll = value;
                //Action reload = async () =>
                //{
                //    await Setup(SetupBypass: true);
                //};

                //reload();
            }
        }

        public string CollectionTitle
        {
            get
            {
                return _((model as IDocument).CollectionName);
            }
        }

        public bool DashBoardLoaded { get; set; }

        public Card DocCardFor { get; set; }

        public Card DocCardOne { get; set; }

        public Card DocCardThree { get; set; }

        public Card DocCardTow { get; set; }

        public string DocForName { get; set; }

        public string DocForValue { get; set; }

        public StackPanel DocImageContent { get; set; }

        public string DocOneName { get; set; }

        public string DocOneValue { get; set; }

        public string DocThreeName { get; set; }

        public string DocThreeValue { get; set; }

        public string DocTowName { get; set; }

        public string DocTowValue { get; set; }

        public HashSet<string> ElementToHide
        {
            get
            {
                return (model as IDocument)?.PropertiesToHide;
            }
            set
            {
                if (model != null)
                    (model as IDocument).PropertiesToHide = value;
            }
        }

        public double elementWidth { get; set; } = 400;

        public BackgroundWorker emailWorker { get; set; } = new BackgroundWorker();

        //public Stopwatch stopwatch { get; set; } = new Stopwatch();
        public Dictionary<string, bool> ExpanderStatus { get; set; } = new Dictionary<string, bool>();

        public string FermerLabel
        {
            get
            {
                return _("Fermer");
            }
        }

        //350;
        public bool fermerVisible { get; set; } = true;

        public bool FinishLoaded { get; set; } = true;

        public string GlobalMsg { get; set; } = _("Nouveau(elle)");

        public string imgBg { get; set; }

        public bool IsEdited { get; set; } = false;

        public string LabelVisibility { get; set; }

        public CultureInfo LangName { get; set; }

        public WrapPanel linkButtons { get; set; }

        public StackPanel linkButtonsOps { get; set; }

        public Visibility LinksVisible { get; set; } = Visibility.Collapsed;

        public int MaxLinesFetch { get; set; } = 100;

        public SnackbarMessageQueue MessageQueue { get; set; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(1));

        //  public ScrollViewer masterStackContent { get; set; }
        public dynamic model { get; set; }

        public List<MenuItem> opeartionButtons { get; set; } = new List<MenuItem>();

        //  public double CurrentScrollPosition { get; private set; }
        // "#27a8f7";
        //public string DocStatus
        //{
        //    get
        //    {
        //        return model.isLocal == true ? _("Nouveau") : _("Enregistré");
        //    }
        //}
        public bool opsVisible
        {
            get
            {
                return opeartionButtons.Any();
            }
        }

        public bool ProgressValueVisible { get; set; }

        public List<string> propertiesOfDoc
        {
            get
            {
                return (model as IDocument).GetType().GetProperties().Select(z => z.Name).ToList();
            }
        }

        public bool SaveVisible
        {
            get
            {
                return (!model?.GetType().Name.Contains("_report") && ((model is IReportView) == false));
            }
        }

        public BackgroundWorker saveWorker { get; set; } = new BackgroundWorker();

        public int ScrollPosition { get; set; } = 0;

        public bool SetupDone { get; set; } = false;

        public IShell shell { get; set; }

        public StackPanel stackContent { get; set; }

        // private Binding bindVisibility;
        public Stopwatch stopwatch { get; set; } = new Stopwatch();

        public ComboBox Tablebox { get; private set; }

        public Guid tableModel { get; set; } = Guid.Empty;

        public Guid tableModelWeak { get; set; } = Guid.Empty;

        public string txtOuvrirTout { get { return _("Ouvrir tout"); } }

        public static async Task<DetailViewModel> Create(IDocument model, Type t, IEventAggregator _aggre, IShell shell)
        {
            DetailViewModel modelBase = new DetailViewModel(model, t, _aggre, shell);
            await modelBase.Setup();

            return modelBase;
        }

        public static DetailViewModel CreateSync(IDocument model, Type t, IEventAggregator _aggre, IShell shell)
        {
            DetailViewModel modelBase = new DetailViewModel(model, t, _aggre, shell);
            var s = modelBase.Setup();
            s.Wait();
            return modelBase;
        }

        public async Task Actualiser(int delay = 0)
        {
            //FinishLoaded = true;
            try
            {
                // NotifyOfPropertyChange("model");
                SetupDocCards();

                await Setup(delay);

                NotifyOfPropertyChange("btnName");
                NotifyOfPropertyChange("btnColor");
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        public override Task<bool> CanCloseAsync()
        {
            return ScreenCanClose();
        }

        public async void Close()
        {
            try
            {
                model.BeforeClose();
            }
            catch { }
            try
            {
                this.RequestClose();
            }
            catch
            {
                if (await this.ScreenCanClose())
                    this.View.GetParentObject()?.TryFindParent<Window>()?.Close();
            }
        }

        public async Task CreateDashBoard()
        {
            //cardSp = null;
          //  cardSp.Children.Clear();//= new StackPanel();
          //  var docType = (model as IDocument);

          // // var labelType = new TextBlock { Text = docType?.CollectionName.Split(' ')?[0], FontSize = 16, Foreground = Brushes.Black };

          //  // var names = docType?.NameSearch.Split(" - ");
          //  var labelName = new TextBlock { Text = docType?.NameSearch, FontWeight = FontWeights.Light, FontSize = 20, Foreground = Brushes.Black };

          //  var spTitl = new StackPanel();
          //  spTitl.Margin = new Thickness(25, 5, 0, 0);
          //  spTitl.Children.Add(labelName);
          ////  spTitl.Children.Add(labelType);

          //  masterWrapPref.Children.Add(spTitl);

          //  if (linkButtons != null)
          //  //    masterWrapPref.Children.Add(linkButtons);

          //  cardSp.Children.Add(masterWrapPref);
          //  if (spCard != null)
          //  {
          //     // cardSp.Children.Add(spCard);
          //      //     masterWrapPref.Children.Add(spCard);
          //  }

          //  if (DocImageContent != null)
          //  {
          //  //    masterWrapPref.Children.Add(DocImageContent);
          //  }
          //  //if (spCard2 != null)
          //  //{
          //  //    masterWrapPref.Children.Add(spCard2);
          //  //}

          //  // add master to expaner
          //  //   expanderPref.Content = masterWrapPref;

          //  expanderPref.Content = cardSp;
          //  expanderPref.Padding = new Thickness(0, 0, 0, 20);
          //  borderSeparation = new Border();
          //  borderSeparation.Background = App.Current.FindResource("MaterialDesignDivider") as Brush;
          //  borderSeparation.Height = 1;
          //  borderSeparation.HorizontalAlignment = HorizontalAlignment.Stretch;
          //  //  borderSeparation.SnapsToDevicePixels = true;

          //  await Execute.PostToUIThreadAsync(() =>
          //  {
          //      stackContent.Children.Insert(0, expanderPref);
          //      stackContent.Children.Insert(1, borderSeparation);
          //      NotifyOfPropertyChange("stackContent");
          //  });

          //  DashBoardLoaded = true;
        }

        public async void CreerNouveau()
        {
            var selected = (IDocument)Activator.CreateInstance(type);
            shell.OpenScreen(await DetailViewModel.Create(selected, selected.GetType(), aggre, shell), $"{_("Nouveau(elle)")} {selected.CollectionName}");
        }

        public void Delete()
        {
            try
            {
                var origin = model as IDocument;
                if (origin.Submitable && origin.DocStatus == 0 && origin.isLocal == false)
                {
                    if ((model as IModel).Delete())
                        DataHelpers.Shell.CloseScreen(this);
                }
                else if (origin.isLocal == true)
                {
                    throw new Exception(_("Vous ne pouvez pas supprimer un document non enregistré"));
                }
                else if (origin.Submitable && origin.DocStatus == 1 && origin.isLocal == false)
                {
                    throw new Exception(_("Vous devez annuler le document avant de le supprimer"));
                }
                else
                {
                    if ((model as IModel).Delete())
                        DataHelpers.Shell.CloseScreen(this);
                }

                var filtre = new PopupWaitViewModel(_("Veuillez patienter"));
                var restulDialog = DataHelpers.windowManager.ShowDialog(filtre);

                this.aggre.Publish(new ModelChangeEvent(this.model.GetType()));
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        public void Dispose()
        {
        }

        public async void Dupliquer()
        {
            var selected = (IDocument)Activator.CreateInstance(type);

            var parentProperties = model.GetType().GetProperties();
            // var childProperties = child.GetType().GetProperties();

            foreach (PropertyInfo parentProperty in parentProperties)
            {
                if (parentProperty.CanWrite)
                {
                    try
                    {
                        parentProperty.SetValue(selected, parentProperty.GetValue(model));
                        selected.isLocal = true;
                        selected.DocStatus = 0;
                        selected.Id = Guid.Empty;
                    }
                    catch (Exception s)
                    {
                        DataHelpers.Logger.LogError(s);
                         DataHelpers.ShowMessage(s.Message);
                    }
                }
            }

            shell.OpenScreen(await DetailViewModel.Create(selected, selected.GetType(), aggre, shell), $"{_("Nouveau(elle)")} {selected.CollectionName}");
        }

        public async void EditTemplate()
        {
            Ovresko.Generix.Utils.PdfModelExport pdf = new Ovresko.Generix.Utils.PdfModelExport(model.GetType(), model.GetType().Name, model, DataHelpers.GetTypesModule);
            pdf.EditTemplate();
        }

        public void Handle(DetailModelChangeEvent message)
        {
            if (message.doc != null && message.type != null && this.model.Id == message.id)
            {
                this.type = message.type;
                this.model = message.doc;

                Actualiser();
            }
        }

        public async void Notify(Object source)
        {
            if (FinishLoaded)
            {
                await Setup();
            }
        }

        public void NotifySpecificProperty(string propertyName)
        {
            (model as IModel).NotifyUpdates(propertyName);
            NotifyOfPropertyChange("ElementToHide");
        }

        public override void RequestClose(bool? dialogResult = null)
        {
            Console.WriteLine();
            base.RequestClose(dialogResult);
        }

        public async void Save()
        {
            ProgressValueVisible = true;
            NotifyOfPropertyChange("ProgressValueVisible");

            if (saveWorker.IsBusy)
            {
                MessageQueue.Enqueue(_("Veuillez patienter"));
            }
            else
            {
                saveWorker.RunWorkerAsync();
            }

            //stopwatch.Restart();

            // DataHelpers.ShowMessage( $"Temp d'execution SAVE {stopwatch.ElapsedMilliseconds} ms");
        }

        public async Task<bool> ScreenCanClose()
        {
            bool result = true;
            await Execute.OnUIThreadAsync(() =>
            { 
                if((model is IReportView) == true)
                {
                    result = true;
                }
                else if ((model as IDocument).isLocal)
                {
                    var response =  DataHelpers.ShowMessage(_("Ignorer les modifications?")+$" {_("pour")} {this.model?.CollectionName}", _("Confirmation"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.No)
                    {
                        result = true;
                        // try { model?.Open(); } catch { }
                    }
                    else
                    {
                        result = model.Save();
                      //  result = true;
                    }
                }
                else
                {
                    var original = DS.Generic(type)?.GetById(model.Id);
                    if (original != null) //&& ((model as IDocument).DocStatus != 1
                    {
                        var ins = (original as IModel);
                        var fields = (ins.GetType().GetProperties());
                        foreach (var item in fields)
                        {
                            try
                            {

                                //if ((item.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType == ModelFieldType.WeakTable)
                                //    continue;

                                if (item.Name == "AddedAtUtc"
                                    || item.Name == "EditedAtUtc"
                                     || item.Name == "MyModule"
                                    || item.Name == "CollectionName" || item.Name == "Index")
                                    continue;

                                string modelvalue = "";
                                string originalValue = "";
                                try
                                {
                                    modelvalue = item.GetValue(model)?.ToString();
                                    originalValue = item.GetValue(original)?.ToString();
                                }
                                catch
                                {
                                    // DataHelpers.ShowMessage(item?.Name);
                                    continue;
                                }
                                if (modelvalue != originalValue)
                                {
                                    var response = DataHelpers.ShowMessage(_("Ignorer les modifications?") + $" pour {this.model?.CollectionName}", _("Confirmation"), MessageBoxButton.YesNo);
                                    if (response == MessageBoxResult.No)
                                    {
                                        result = true;
                                        //try { model?.Open(); } catch { }
                                        break;
                                    }
                                    else
                                    {
                                        result = model.Save();
                                        //result = true;
                                        break;
                                    }
                                }
                            }
                            catch (Exception s)
                            {
                                DataHelpers.ShowMessage(s.StackTrace);
                                continue;
                            }
                        }
                    }
                }

                
            });

            return result;
        }

        public void ShowProperties()
        {
            UserControl control = new UserControl();
            Window win = new Window();
            var scroll = new ScrollViewer();
            scroll.HorizontalAlignment = HorizontalAlignment.Stretch;
            scroll.VerticalAlignment = VerticalAlignment.Stretch;
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            control.Background = Brushes.WhiteSmoke;
            // props with displayname
            var props = (model as IDocument).GetType().GetProperties().Where(a =>
            a.GetCustomAttribute(typeof(DisplayNameAttribute)) != null
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.Separation
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.BaseButton
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.Button
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.OpsButton
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.LienButton
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.ImageSide
            && (a.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)?.FieldType != ModelFieldType.Image);

            var stack = new StackPanel() { Orientation = Orientation.Vertical };

            var stackLin = new StackPanel() { Orientation = Orientation.Horizontal };
            var line = new TextBlock()
            {
                Text = _($"Nom de champ"),
                Width = 200,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };

            var value = new TextBlock()
            {
                Text = _($"Codes"),
                Width = 200,

                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Margin = new Thickness(5)
            };
            stackLin.Children.Add(line);
            stackLin.Children.Add(value);

            stack.Children.Add(stackLin);

            foreach (var item in props)
            {
                var dispaly = item.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;

                stackLin = new StackPanel() { Orientation = Orientation.Horizontal };

                line = new TextBlock()
                {
                    Text = $"{dispaly.DisplayName}",
                    Width = 200,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };

                var valueTb = new TextBox()
                {
                    Text = $"{item.Name}",
                    Width = 200,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };
                stackLin.Children.Add(line);
                stackLin.Children.Add(valueTb);

                stack.Children.Add(stackLin);
            }
            stack.Margin = new Thickness(20);
            stack.HorizontalAlignment = HorizontalAlignment.Stretch;
            stack.VerticalAlignment = VerticalAlignment.Stretch;
            scroll.Content = stack;
            control.Content = scroll;
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.VerticalAlignment = VerticalAlignment.Stretch;
            win.WindowState = WindowState.Normal;
            win.WindowStyle = WindowStyle.ToolWindow;
            win.Content = control;
            win.ShowDialog();
        }

        public void UserControl_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.F1)
            {
                Save();
            }
            else if (args.Key == Key.F2)
            {
                Close();
            }
        }

        protected async override void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
        {
            base.NotifyOfPropertyChange(propertyName);
            if (propertyName == "model")
            {
                await (model as IModel).NotifyUpdates();
                NotifyOfPropertyChange("ElementToHide");
            }
        }

        protected override void OnClose()
        {
            //var _model = (model as IDocument);
            //if (_model.Id != Guid.Empty && !_model.isLocal  && ((_model.Submitable && _model.DocStatus == 0) || !_model.Submitable))
            //{
            //        // saved
            //        var e = DataHelpers.GetById(_model.GetType().Name, _model.Id);
            //        if (!e.Equals(_model))
            //        {
            //            var res =  DataHelpers.ShowMessage( "Vos modifications ne sont enregistrées, voulez-vous quitter?", "Modifications non enregistrées", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //            if (res == MessageBoxResult.No)
            //                return;

            //        }

            //}

            base.OnClose();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }
        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            // Execute code after view opened
            // To be run inside ModelBase document
            (model as IDocument).AfterOpen();
        }

        private async void Actualiser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ProgressValueVisible)
                { ProgressValueVisible = true;
                    NotifyOfPropertyChange("ProgressValueVisible");
                }

                var nmodel = (model as IDocument);
                if (!nmodel.isLocal)
                {
                    try
                    {
                        var newVersion = DS.Generic(type.Name)?.GetById(model.Id);// (model as IDocument).Id.GetObject(type.Name);
                        model = newVersion;
                    }
                    catch (Exception s)
                    {
                         DataHelpers.ShowMessage(s.Message);
                    }
                }
                SetupDone = false;
                //SetupDocCards();
                await Actualiser();
                await Task.Delay(1005);
                MessageQueue.Enqueue(_("Données actualisées"));
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        private async void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            // add iamge
            var Nmodel = (IDocument)model;
            var propName = (sender as Button).Tag.ToString();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = _("Selectionner une image");
            ofd.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphics (*.png)|*.png";
            BitmapImage img;
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var file = System.IO.Path.GetFullPath(ofd.FileName);
                    var imageLink = $"Database/images/{propName}_{DateTime.Now.Year}_{DateTime.Now.DayOfYear}_{DateTime.Now.Millisecond}_{System.IO.Path.GetExtension(ofd.FileName)}";

                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        // var img = new BitmapImage(new Uri(varimgPath));
                        var Bitm = new System.Drawing.Bitmap(file);
                        ImageTools.Save(Bitm, 150, 200, imageLink);
                    }

                    // File.Copy(file, imageLink, true);
                    img = new BitmapImage(new Uri(System.IO.Path.GetFullPath(imageLink)));

                    Nmodel.GetType().GetProperty(propName).SetValue(model, imageLink);
                    NotifyOfPropertyChange("model");
                    await Setup(SetupBypass: true);
                }
                catch (Exception s)
                {
                     DataHelpers.ShowMessage(s.Message);
                }
            }
        }

        // Table add button
        private void AddBtnTable_Click(object sender, RoutedEventArgs e)
        {
            var Nmodel = (IDocument)model;

            // Model name (RepasSimple)
            var modelType = (sender as Button).Tag as Type;

            var s = Activator.CreateInstance(modelType) as IModel;
            var propertyName = modelType.Name;
            var initValues = Nmodel.GetType().GetProperty(propertyName).GetValue(model);
            // initValues.Add(s);

            Type genericListType = typeof(List<>).MakeGenericType(type);
            var newValues = (IList)Activator.CreateInstance(genericListType);
            newValues = initValues;

            newValues.Add(s);
            Nmodel.GetType().GetProperty(propertyName).SetValue(model, newValues);
            NotifyOfPropertyChange($"model");
        }

        private async Task AddItemToTable(DataGrid table, string source, string mapFunction)
        {
            var afterMap = (table.Tag as AfterMapMethodAttribute)?.MethodName;
            var selected = table.GetValue(DataGrid.ItemsSourceProperty);

            var doc = (IDocument) DS.Generic(source)?.GetById(tableModel)   ;
            // DataHelpers.GetById(source, tableModel);
            var mapped = doc.Map(mapFunction);

            if (afterMap != null)
                mapped = (model as IDocument).GetType().GetMethod(afterMap)?.Invoke(model, new[] { mapped });

            (selected as IList).Add(mapped);
        }

        private async Task AddItemToTable(DataGrid table, IDocument item, string mapFunction)
        {
            var afterMap = (table.Tag as AfterMapMethodAttribute)?.MethodName;
            var selected = table.GetValue(DataGrid.ItemsSourceProperty);

            var doc = item;
            var mapped = doc.Map(mapFunction);

            if (afterMap != null)
            {
                try
                {
                    mapped = (model as IDocument).GetType().GetMethod(afterMap).Invoke(model, new[] { mapped });
                }
                catch (Exception s)
                {
                    MessageQueue.Enqueue(s.Message + " AddItemToTable");
                    DataHelpers.Logger.LogError(s);
                }
            }

            (selected as IList).Add(mapped);
        }

        private async void Box_DropDownClosed(object sender, EventArgs e)
        {
            await Setup();
        }

        private void Box_KeyUp(object sender, KeyEventArgs e)
        {
            //var box = (sender as ComboBox).Tag as ComboBox;
            //if(box != null)
            //{
            //    var ds = box.Items;
            //    var text = box.Text;

            //    var result = ds.fi
            //}
        }

        private void Box_KeyUp1(object sender, KeyEventArgs e)
        {
            var box = (sender as ComboBox);
            if (e.Key == Key.Back || string.IsNullOrWhiteSpace(box.Text))
            {
                if (box.SelectedItem != null)
                {
                    var txt = box.Text;
                    box.SelectedItem = null;
                    box.Text = txt;
                }
                //return;
            }

            box.Items.Filter = ((a) =>
            {
                if ((a as IDocument)?.NameSearch.ToLower().Contains(box.Text.ToLower()) == true)
                    return true;
                return false;
            });
        }

        private async void Box_LostFocus(object sender, RoutedEventArgs e)
        {
            await Setup();
        }

        private async void Box_LostFocus1(object sender, RoutedEventArgs e)
        {
            // throw new NotImplementedException();
            //await Actualiser();
        }

        private async void Box_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // new ArrayList() { optionLien, prop.Name };
            var data = (sender as ComboBox).Tag as ArrayList;

            var link = data[0]?.ToString();
            var prop = data[1]?.ToString();

            Guid valueId = (model as IDocument).GetType().GetProperty(prop).GetValue(model);

            if (valueId != null && valueId != Guid.Empty)
            {
                var Concrete = valueId.GetObject(link) as IDocument;
                if (Concrete != null)
                {
                    await DataHelpers.Shell.OpenScreenDetach(Concrete, Concrete.Name);
                    await Setup();
                }
            }
        }

        private async void Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NotifySpecificProperty((sender as ComboBox).Name);
            await Setup();
        }

        private async void Box_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            // await Actualiser();
            try
            {
                NotifySpecificProperty((sender as ComboBox).Name);
                //var array = (sender as ComboBox).Tag as ArrayList;
                //if(array != null && array.Count == 2)
                //{
                //    var propertyname = array[1] as string;

                //}
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = (sender as Button);
                if (btn.Tag == null)
                    return;
                var data = (btn.Tag as ArrayList);
                Type type = DataHelpers.GetTypesModule.Resolve(data[0]?.ToString());
                if (type == null)
                    return;

                var selected = (IDocument)Activator.CreateInstance(type);
                await DataHelpers.Shell.OpenScreenDetach(selected, selected.Name);
                (model as IDocument).GetType().GetProperty(data[1].ToString()).SetValue(model, selected.Id);
                await Setup(SetupBypass: true);
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }

        public FlowDirection GetFlowDirection
        {
            get
            {
                return DataHelpers.GetFlowDirection;
            }
        }

        private async void BtnAddLien_Click(object sender, RoutedEventArgs e)
        {
            var Nmodel = (IDocument)model;
            var canAdd = true;
            if (Nmodel.Submitable)
            {
                canAdd = Nmodel.EnsureIsSavedSubmit();
            }
            else
            {
                canAdd = Nmodel.EnsureIsSaved();
            }

            if (Nmodel != null && canAdd)
            {
                try
                { // header[0] -> lTier
                    // header[1] -> Facture
                    // header = Facture
                    // property name is the link

                    var item = (Button)sender;
                    var header = (ArrayList)item.Tag;

                    var property = header[0].ToString(); // lTier
                    var link = header[1].ToString(); // Facture

                    Type type = DataHelpers.GetTypesModule.Resolve(link);
                    if (type == null)
                        return;
                    // Type.GetType($"Ovresko.Generix.Core.Modules.{link}");
                    var selected = (IDocument)Activator.CreateInstance(type);

                    selected.GetType().GetProperty(property).SetValue(selected, model.Id);
                    selected = DataHelpers.MapProperties(selected, model);

                    if (selected.DocOpenMod == OpenMode.Detach)
                    {
                        await DataHelpers.Shell.OpenScreenDetach(selected, selected.CollectionName);
                    }
                    else
                    {
                        DataHelpers.Shell.OpenScreenAttach(selected, selected.CollectionName);
                    }
                }
                catch (Exception s)
                {
                     DataHelpers.ShowMessageError(s);
                    return;
                }
            }
        }

        private async void Btnaddline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = (sender as Button).Tag as DataGrid;
                var selected = s.GetValue(DataGrid.ItemsSourceProperty);
                (selected as IList).Clear();
            }
            catch (Exception s)
            {
                MessageQueue.Enqueue(s.Message + " Btnaddline_Click");
                GlobalMsg = $"{_("Erreur")}: {s.Message}";
                NotifyOfPropertyChange("GlobalMsg");
            }
            await Setup();
        }

        private async void BtnAddline_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var s = (sender as Button).Tag as DataGrid;
            //    var selected = s.GetValue(DataGrid.ItemsSourceProperty);
            //    s.GetValue(DataGrid.);
            //    var clone = (selected as IList)[0].;
            //    (selected as IList).Add(clone);
            //}
            //catch (Exception s)
            //{
            //     DataHelpers.ShowMessage( s.Message, "Erreur");
            //}
            //await Setup();
        }

        private async void BtnAddModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dic = (ArrayList)(sender as Button).Tag;
                if (tableModel == Guid.Empty || dic.Count < 3)
                {
                     DataHelpers.ShowMessage(_("Selectionner une ligne à ajoutée, ou vérifier la déclaration"));
                    return;
                }

                var data = dic;
                var s = data[0] as DataGrid;

                await AddItemToTable(s, data[1].ToString(), data[2].ToString());

                await Setup();
            }
            catch (Exception s)
            {
                MessageQueue.Enqueue(s.Message + " BtnAddModel_Click");
                GlobalMsg = $"{_("Erreur")}: {s.Message}";
                NotifyOfPropertyChange("GlobalMsg");
                return;
            }
        }

        private async void BtnDOWN_Click(object sender, RoutedEventArgs e)
        {
            var but = (sender as Button);
            var table = but.Tag as DataGrid;

            if (table != null)
            {
                var selected = table.SelectedItem;
                if (selected != null)
                {
                    var source = table.GetValue(DataGrid.ItemsSourceProperty) as IList;

                    var index = source.IndexOf(selected);
                    var removedItem = source[index];

                    if (index - 1 >= 0 && (index - 1) <= source.Count)
                    {
                        source.RemoveAt(index);
                        source.Insert((index - 1), removedItem);
                    }

                    await Setup();
                }
            }
        }

        private async void BtnNewModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = (sender as Button);
                if (btn.Tag == null)
                    return;
                var data = (btn.Tag as ArrayList);
                Type type= DataHelpers.GetTypesModule.Resolve(data[0].ToString());
                if (type == null)
                    return;

                var grid = data[2] as DataGrid;
                var selected = (IDocument)Activator.CreateInstance(type);

                await DataHelpers.Shell.OpenScreenDetach(selected, selected.Name);

                tableModel = selected.Id;
                //if  (tableModel.IsValide())
                //{
                //    var article = tableModel.GetObject(selected.GetType().Name);
                //    if(article != null && grid != null)
                //    {
                //        (grid.GetValue(DataGrid.ItemsSourceProperty) as IList).Add(article);
                //    }
                //}

                await Setup(SetupBypass: true);
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            // { box, modelValuesId, mytypeAttr ,}
            try
            {
                var data = (sender as Button).Tag as ArrayList;

                dynamic box = data[0];
                dynamic optionLienField = data[1];
                dynamic mytypeAttr = data[2];
                dynamic allatt = data[3];
                string propName = data[4] as string;

                dynamic modelValuesId = null;
                if (optionLienField.Contains("()"))
                {
                    modelValuesId = model.GetType().
                  GetMethod(optionLienField.Replace("()", ""))?.Invoke(model, null);
                }
                else
                {
                    modelValuesId = model.GetType().
                  GetProperty(optionLienField)?.
                  GetValue(model);
                }

                if (mytypeAttr != null && modelValuesId != null)
                {
                    var sourcProp = allatt[1];

                    dynamic sourceEntity = null;
                    if (modelValuesId.GetType() == typeof(Guid))
                    {
                        //  var instance = Activator.CreateInstance(mytypeAttr.type);
                        //sourceEntity = await (instance as ModelBase<>).GetById( Getd();
                        // sourceEntity = DataHelpers.GetById(mytypeAttr.type.Name, modelValuesId);
                        Guid _id = modelValuesId;
                        //sourceEntity = DataHelpers.GetDataStatic(mytypeAttr.type, a => (a as IDocument).Id == _id);
                        //IGenericData ds = DataHelpers.GetGenericData(mytypeAttr.type);

                        sourceEntity = DS.Generic(mytypeAttr.type)?.GetById(_id);
                    }
                    else
                    {
                        sourceEntity = modelValuesId;
                    }
                    // DataHelpers.GetById(mytypeAttr.type.Name, modelValuesId);
                    if (sourceEntity != null)
                    {
                        var modelValues = sourceEntity.GetType().GetProperty(sourcProp)
                          .GetValue(sourceEntity);
                        if (modelValues != null)
                        {
                            box.SetBinding(ComboBox.ItemsSourceProperty,
                                new Binding
                                {
                                    Source = modelValues,
                                    Mode = BindingMode.OneWay
                                });

                            box.SetBinding(ComboBox.SelectedValueProperty, new Binding
                            {
                                Source = model,
                                Path = new PropertyPath(propName),
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                Mode = BindingMode.TwoWay
                            });

                            box.SetBinding(ComboBox.DisplayMemberPathProperty,
                                new Binding { Source = $"NameSearch" });
                            box.SelectedValuePath = "Id";
                        }
                    }
                }
                else if (modelValuesId != null)
                {
                    var sourceEntity = modelValuesId;

                    if (sourceEntity != null)
                    {
                        box.SetBinding(ComboBox.ItemsSourceProperty,
                            new Binding
                            {
                                Source = sourceEntity,
                                Mode = BindingMode.OneWay
                            });

                        box.SetBinding(ComboBox.SelectedValueProperty, new Binding
                        {
                            Source = model,
                            Path = new PropertyPath(propName),
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Mode = BindingMode.TwoWay
                        });

                        box.SetBinding(ComboBox.DisplayMemberPathProperty,
                            new Binding { Source = $"NameSearch" });
                        box.SelectedValuePath = "Id";
                    }
                }

            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s, "Reload");
            }
            await Setup();
        }

        private async void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dic = (ArrayList)(sender as Button).Tag;
                if (dic.Count < 3)
                {
                     DataHelpers.ShowMessage(_("Selectionner une ligne à ajoutée, ou vérifier la déclaration"));
                    return;
                }

                var data = dic;
                var s = data[0] as DataGrid;
                var selected = s.GetValue(DataGrid.ItemsSourceProperty);

                Type type  = DataHelpers.GetTypesModule.Resolve(data[1].ToString());
                if (type == null)
                    return;

                var doc = await DataHelpers.Shell.OpenScreenFind(type, $"{_("Selectioner")}...");

                if (doc != null)
                {
                    var list = doc as IEnumerable<IDocument>;
                    foreach (var item in list)
                    {
                        //    var mapped = item.Map(data[2].ToString());
                        //    (selected as IList).Add(mapped);
                        await AddItemToTable(s, item, data[2].ToString());
                    }

                    await Setup();
                }
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }

            //

            //
        }

        private async void BtnUP_Click(object sender, RoutedEventArgs e)
        {
            var but = (sender as Button);
            var table = but.Tag as DataGrid;

            if (table != null)
            {
                var selected = table.SelectedItem;
                if (selected != null)
                {
                    var source = table.GetValue(DataGrid.ItemsSourceProperty) as IList;

                    var index = source.IndexOf(selected);
                    var removedItem = source[index];

                    if (index + 1 > 0 && (index + 1) < source.Count)
                    {
                        source.RemoveAt(index);
                        source.Insert((index + 1), removedItem);
                    }

                    await Setup();
                }
            }
        }

        private async void BtnView_Click(object sender, RoutedEventArgs e)
        {
            var btn = (sender as Button);
            if (btn.Tag == null)
                return;

            var combo = (ComboBox)btn.Tag;
            var selected = (IDocument)combo.SelectedItem;

            if (selected != null)
            {
                shell.OpenScreenDetach(selected, $"{selected.Name}");
                await Setup();
            }
        }

        private async void BtnView_Click1(object sender, RoutedEventArgs e)
        {
            try
            {
                var dic = (ArrayList)(sender as Button).Tag;
                if (dic.Count < 3)
                {
                     DataHelpers.ShowMessage(_("Selectionner une ligne à ajoutée, ou vérifier la déclaration"));
                    return;
                }

                var data = dic;
                var s = data[0] as ComboBox;

                Type type  = DataHelpers.GetTypesModule.Resolve(data[1].ToString());
                if (type == null)
                    return;

                var docList = await DataHelpers.Shell.OpenScreenFind(type, _("Selectioner document"));
                var doc = docList?.FirstOrDefault();
                (this.model as IDocument).GetType().GetProperty(dic[2].ToString()).SetValue(this.model, doc.Id);
                s.SelectedItem = doc.Id;
                await Setup();
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }

        private void BtnViewDate_Click(object sender, RoutedEventArgs e)
        {
            var btn = (sender as Button);
            if (btn.Tag == null)
                return;

            var combo = (Xceed.Wpf.Toolkit.DateTimePicker)btn.Tag;

            if (combo != null)
            {
                combo.Value = DateTime.Now;
            }
        }

        private async void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            // await Task.Delay(900);
            NotifySpecificProperty((sender as CheckBox).Name);
            await Setup();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void DatePicker_SelectedDateChanged1(object sender, TimePickerBaseSelectionChangedEventArgs<DateTime?> e)
        {
            await Setup();
        }

        private async void DatePicker_SelectedDateChanged2(object sender, SelectionChangedEventArgs e)
        {
          
        }

        private void DatePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //var result =  DataHelpers.ShowMessage( "Voulez-vous supprimer le document?", "Voulez-vous supprimer le document?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if(result == MessageBoxResult.Yes)
            Delete();
        }

        private async void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = (sender as MenuItem).Tag as DataGrid;
                var selected = s.GetValue(DataGrid.ItemsSourceProperty);
                (selected as IList).Clear();
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message, _("Erreur"));
            }
            await Setup();
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = (sender as MenuItem).Tag as DataGrid;
                var selected = s.GetValue(DataGrid.ItemsSourceProperty);
                (selected as IList).Remove(s.SelectedItem);
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message, _("Erreur"));
            }
            await Setup();
        }

        private void DetailViewModel_CloseEvent(object sender, EventArgs e)
        {
            Close();
        }

        private void DetailViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "select" || e.PropertyName == "date")
            //{
            //    NotifyOfPropertyChange("model");
            //}
            //Console.WriteLine(e.PropertyName);
        }

        private async void DetailViewModel_StateChanged(object sender, ScreenStateChangedEventArgs e)
        {
            // try { await Setup(); } catch { }
        }

        private async void Email_Click(object sender, RoutedEventArgs e)
        {
            ProgressValueVisible = true;
            NotifyOfPropertyChange("ProgressValueVisible");
            if (emailWorker.IsBusy)
            {
                MessageQueue.Enqueue("Patientez SVP");
                return;
            }
            else
            {
                emailWorker.RunWorkerAsync();
            }
        }

        private void EmailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var emailHost = DataHelpers.Settings.EmailHost;
                var emailPort = DataHelpers.Settings.EmailPort;
                var pwd = DataHelpers.Settings.EmailPwd;
                var from = DataHelpers.Settings.EmailFrom;
                //var tier = Tier.GetObject<Tier>();
                //var to = tier.Email;

                var validated = DataHelpers.GetEmptyStrings(new Dictionary<string, string> {
                    { _("Adresse serveur SMTP"),emailHost },
                    { _("Port serveur SMTP"),emailPort },
                    {_("Mot de passe email d'envoi"), pwd },
                    {_("Adresse email d'envoi"), from },
                 });

                if (validated.Any())
                {
                    foreach (var item in validated)
                    {
                         DataHelpers.ShowMessage($"{item.Key} {_("est invalide ou vide")}");
                    }
                    return;
                }

                ArrayList attache = new ArrayList();
                if (!string.IsNullOrWhiteSpace(model.DefaultTemplate))
                {
                    var canaddFiles =  DataHelpers.ShowMessage($"{_("Voulez-vous Attacher le document")} {model.DefaultTemplate}.pdf", _("Confirmation"), MessageBoxButton.YesNo);
                    if (canaddFiles == MessageBoxResult.Yes)
                    {
                        var filess = model.ExportPDF(model.GetType(), model.DefaultTemplate, true);
                        attache = new ArrayList { Path.GetFullPath(filess) };
                    }
                }

                if (!string.IsNullOrWhiteSpace(pwd)
                    && !string.IsNullOrWhiteSpace(emailHost))
                {
                    Execute.OnUIThread(() =>
                    {
                        EmailModelExport.SendEmail(pwd, from, model.EmailRecepiant, $"", $@"

{model.EmailMessage}

_________________________________________________________________________

Sent by www.ovresko.com
",

    emailHost, emailPort, attache, Sujet: $"{model.CollectionName} {this.model.Name}");
                    });
                }
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }

        private void EmailWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressValueVisible = false;
            NotifyOfPropertyChange("ProgressValueVisible");
            MessageQueue.Enqueue(_("Email envoyé"));
        }

        private void Exporter_Click(object sender, RoutedEventArgs e)
        {
            //DocEngine.Generate(model as IDocument);
            var dest = (sender as Button).Tag.ToString();

            try
            {
                if (dest == "PDF")
                {
                    (model as IDocument).ExportPDF(type);
                    //OvExport.PdfModelExport pdf = new OvExport.PdfModelExport(model.GetType(), model.GetType().Name, model);
                    //var file = pdf.GeneratePdf();
                    //if (!string.IsNullOrWhiteSpace(file))
                    //{
                    //    Thread.Sleep(2000);
                    //    Process.Start(file);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Go Create template! By!!");
                    //}
                }
                else if (dest == "OFFICE")
                {
                    (model as IDocument).ExportWORD(type);
                    //OvExport.PdfModelExport pdf = new OvExport.PdfModelExport(model.GetType(), model.GetType().Name, model);
                    //var file = pdf.GenerateOffice();
                    //if (!string.IsNullOrWhiteSpace(file))
                    //{
                    //    Thread.Sleep(2000);
                    //    Process.Start(file);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Go Create template! By!!");
                    //}
                }
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessage(s.Message);
                return;
            }

            //var rep = new ObjReportViewModel(model);
            //shell.OpenScreen(rep, "Report");
        }

        private async void ItemBox_Checked(object sender, RoutedEventArgs e)
        {
            var box = (sender as CheckBox);
            var data = box.Tag as ArrayList;
            // { prop.Name, item
            var propName = data[0].ToString();
            // item is ID of accesrule
            var item = data[1];

            if (box.IsChecked == true)
            {
                if (item != null)
                {
                    ((model as IDocument).GetType().GetProperty(propName).GetValue(model) as IList).Add(item);
                }
            }
            else
            {
                if (item != null)
                {
                    ((model as IDocument).GetType().GetProperty(propName).GetValue(model) as IList).Remove(item);
                }
            }

            await Setup();
        }

        private void Listview_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private async void Listview_KeyUp(object sender, KeyEventArgs e)

        {
            //var dg = (sender as DataGrid);
            //if (e.Key == Key.Delete)
            //   await Setup();
        }

        private async void Listview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Table double click
            if (isFreezed)
                return;
            var table = (sender as DataGrid);
            table.CancelEdit();
            table.CancelEdit(DataGridEditingUnit.Row);
            table.CancelEdit(DataGridEditingUnit.Cell);
            var dg = table.SelectedItem as IDocument;
            try
            {
                var selected = (sender as DataGrid).SelectedItem as IDocument;

                var ioc = DataHelpers.container;
                var vm = ioc.Get<ViewManager>();
                var c = await DetailViewModel.Create(selected, selected.GetType(), aggre, shell);
                c.CloseAfter = true;
                var content = vm.CreateAndBindViewForModelIfNecessary(c);

                var cc = new ContentControl();
                cc.HorizontalAlignment = HorizontalAlignment.Stretch;
                cc.VerticalAlignment = VerticalAlignment.Stretch;
                cc.Content = content;

                GenericWindowViewModel gw = new GenericWindowViewModel(cc, $"{_("Modifier")} {selected.CollectionName}", selected.Name);

                DataHelpers.windowManager.ShowDialog(gw);
                await Setup();
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessage(s.Message, _("Erreur"));
                return;
            }
        }

        private async void Listview_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                var grid = (DataGrid)sender;
                if (grid.SelectedItem != null)
                {
                    grid.RowEditEnding -= Listview_RowEditEnding;
                    grid.CommitEdit();
                    grid.Items.Refresh();
                    grid.RowEditEnding += Listview_RowEditEnding;
                }

                await Setup();

                NotifySpecificProperty((sender as DataGrid).Name);
                return;
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
            }
        }

        private void Listview_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //  (sender as DataGrid).CommitEdit(DataGridEditingUnit.Cell, false);
                // (sender as DataGrid).CommitEdit(DataGridEditingUnit.Row, false);
                //await Setup(500);
            }
            catch (Exception s)
            {
                Console.WriteLine("Listview_RowEditEnding");
                MessageQueue.Enqueue(s.Message + " Listview_Unloaded");
            }
        }

        private async void NewMenu_Click(object sender, RoutedEventArgs e)
        {
            var Nmodel = (IDocument)model;
            if (Nmodel != null && Nmodel.EnsureIsSaved())
            {
                try
                {
                    // header[0] -> lTier
                    // header[1] -> Facture
                    // header = Facture
                    // property name is the link

                    var item = (Button)sender;
                    var header = (ArrayList)item.Tag;

                    var link = header[0].ToString();

                    IEnumerable<IDocument> relatedItems;// = new List<IDocument>();
                    if (header[2] != null)
                    {
                        var value = header[2];
                       // relatedItems = new List<IDocument>() { DS.Generic(header[1].ToString())?.GetById(GuidParser.Convert(value)) as IDocument };// as List<IDocument>;
                        relatedItems = DS.Generic(header[1].ToString())?.Find("_id", GuidParser.Convert(value?.ToString()), true);

                    }
                    else
                    { 
                        relatedItems = DS.Generic(header[1].ToString())?.Find(link, Nmodel.Id, true);
                    }
                    if (relatedItems != null && relatedItems.Count() == 1)
                    {
                        var first = relatedItems.FirstOrDefault();
                        await DataHelpers.Shell.OpenScreenDetach(first as IDocument, first.Name);
                    }
                    else
                    {
                        var linkType = DataHelpers.GetTypesModule.Resolve(header[1].ToString());// relatedItems.First().GetType();
                        var view = DataHelpers.GetBaseViewModelScreen(linkType, aggre, false, relatedItems );
                        view.DisplayName = $"{item.Content.ToString()} / {model.Name}";
                        shell.OpenScreen(view, view.DisplayName);
                    }
                }
                catch (Exception s)
                {
                     DataHelpers.ShowMessageError(s);
                    return;
                }
            }
        }

        // CUSTOM BUTTON
        private async void NewMenuButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string methodName = (sender as Button).Tag.ToString();
                var Nmodel = (IDocument)model;
                Nmodel.DoFunction(methodName);
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
            await Setup(SetupBypass:true);
        }

        // CUSTOM BUTTON OPS
        private async void NewOps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string methodName = (sender as MenuItem).Tag.ToString();
                var Nmodel = (IDocument)model;
                Nmodel.DoFunction(methodName);
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessageError(s,"Operation");
            }
            await Actualiser();
        }

        private void RefreshAll()
        {
            var props = (model as IDocument).GetType().GetProperties();

            foreach (var prop in props)
            {
                NotifyOfPropertyChange(prop.Name);
            }
        }

        private async Task RefreshAllTAbles()
        {
            //stopwatch.Restart();
            NotifyOfPropertyChange("tableModel");
            var tables = this.View.FindChildren<DataGrid>();

            if (tables != null)
            {
                foreach (var item in tables)
                {
                    item.RowEditEnding -= Listview_RowEditEnding;
                    item.CommitEdit();
                    item.Items.Refresh();

                    item.RowEditEnding += Listview_RowEditEnding;
                }
            }
            //stopwatch.Stop();

            // DataHelpers.ShowMessage( $"RefreshAllTAbles : {stopwatch.ElapsedMilliseconds} ms");
        }

        //private void Rtb_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    (sender as Xceed.Wpf.Toolkit.MultiLineTextEditor).IsOpen = true;
        //}

        //private void Rtb_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    (sender as Xceed.Wpf.Toolkit.MultiLineTextEditor).IsOpen = false;
        //}

        private void Rtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotifySpecificProperty((sender as TextBox).Name);
        }

        private async void SaveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SetupDone = false;
                var origin = model as IDocument;
                if (origin.Submitable && origin.DocStatus == 0 && origin.isLocal == false && IsEdited == false)
                {
                    if ((model as IModel).Submit())
                        IsEdited = false;
                    //await Setup();
                }
                else if (origin.Submitable && origin.DocStatus == 1 && origin.isLocal == false)
                {
                    Execute.OnUIThreadSync(async () =>
                    {
                        var s =  DataHelpers.ShowMessage(_("Si vous annuler, vous devez modifier les changements effectués"), _("Confirmation"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (s == MessageBoxResult.Cancel)
                        {
                            saveWorker.CancelAsync();
                        }
                        else
                        {
                            if ((model as IModel).Cancel())
                            {
                                IsEdited = false;
                                await Actualiser();
                                return;
                            }
                        }
                    });

                    // return;
                }
                else
                {
                    if ((model as IModel).Save())
                        IsEdited = false;
                    // await Setup();
                }

           
                if (CloseAfter)
                {
                    saveWorker.CancelAsync();
                    Execute.OnUIThread(() =>
                    {
                        Close();
                    });
                }
            }
            catch (Exception s)
            {
                Execute.OnUIThread(() =>
                {
                    DataHelpers.ShowMessage(s.Message);

                });
            }

            this.aggre.Publish(new ModelChangeEvent(this.model.GetType()));
            //   FinishLoaded = true;
        }

        private async void SaveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Media.SystemSounds.Exclamation.Play();
            if (!CloseAfter && model.Submitable && model.DocStatus == 1)
            {
                await Actualiser(2000);
            }
            else
            {
                await Actualiser();
            }
            // MessageQueue.Enqueue(_("Document enregistré"));
        }

        private async Task Setup(int delay = 0, bool? SetupBypass = null)
        {
            //stopwatch.Restart();
            this.NotifyOfPropertyChange("LabelVisibility");
            if (!FinishLoaded)
            {
                return;
            }

            var _model = (model as IDocument);
            ElementToHide = new HashSet<string>();
            this.NotifyOfPropertyChange("model");
            this.NotifyOfPropertyChange("ElementToHide");
            FinishLoaded = false;
            await RefreshAllTAbles();

            if (SetupDone && SetupBypass != true)
            {
                // IF Doc is validated and trying do edit

                if (_model.DocStatus == 0 &&
                     //  _model.Submitable &&
                     IsEdited == false &&
                   _model.isLocal == false)
                {
                    IsEdited = true;
                    NotifyOfPropertyChange("btnColor");
                    NotifyOfPropertyChange("btnName");
                }
                else
                {
                    //IsEdited = false;
                    FinishLoaded = true;
                    NotifyOfPropertyChange("btnColor");
                    NotifyOfPropertyChange("btnName");
                    return;
                }
            }

            if (delay > 0)
                await Task.Delay(delay);

            _model.IsSelectedd = false;

            if (model == null)
                throw new Exception(_("Model vide"));

            // Execute before refresh
            _model.BeforRefresh();

            isFreezed = false;
            //  MessageQueue.Enqueue(model.Status);
            GlobalMsg = model.Status;
            NotifyOfPropertyChange("GlobalMsg");
            linkButtons = new WrapPanel();
            linkButtons.Orientation = Orientation.Vertical;
            // linkButtons.MaxHeight = 130;
            linkButtons.Margin = new Thickness(0, 5, 0, 0);
            if (model.DocOpenMod == OpenMode.Detach)
            {
                //    fermerVisible = false;
            }

            // Opération BUtton

            #region SETUP

            if (stackContent != null && stackContent.Children.Count > 1)
            {
                ExpanderStatus.Clear();
                var expanders = stackContent.FindChildren<Expander>();
                foreach (var item in expanders)
                {
                    ExpanderStatus.Add(item.Header.ToString(), item.IsExpanded);
                }
            }

            opeartionButtons = new List<MenuItem>();
            //opeartionButtons.HorizontalAlignment = HorizontalAlignment.Left;
            //opeartionButtons.Orientation = Orientation.Horizontal;

            linkButtonsOps = new StackPanel();

            linkButtonsOps.Orientation = Orientation.Vertical;
            Button actualiser = new Button();
            actualiser.ToolTip = _("Actualiser");
            actualiser.Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new PackIcon() { Kind = PackIconKind.Refresh, Width = 30, Height = 30 }
                },
                VerticalAlignment = VerticalAlignment.Center,
            };
            actualiser.Style = App.Current.FindResource("DetailMenuItems") as Style;
            actualiser.Click += Actualiser_Click;
            actualiser.TouchDown += Actualiser_Click;

            Button Exporter = new Button();
            Exporter.Tag = "PDF";
            Exporter.ToolTip = _("Exporter/Imprimer PDF");
            Exporter.Click += Exporter_Click; ;
            Exporter.TouchDown += Exporter_Click; ;
            Exporter.Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new PackIcon() { Kind = PackIconKind.FilePdf, Width = 30, Height = 30 }
                },
                VerticalAlignment = VerticalAlignment.Center,
            };
            Exporter.Style = App.Current.FindResource("DetailMenuItems") as Style;

            Button ExporterOffice = new Button();
            ExporterOffice.ToolTip = _("Exporter/Imprimer Document Word");
            ExporterOffice.Tag = "OFFICE";
            ExporterOffice.Click += Exporter_Click; ;
            ExporterOffice.TouchDown += Exporter_Click; ;
            ExporterOffice.Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new PackIcon() { Kind = PackIconKind.FileWordBox, Width = 30, Height = 30 }
                },
                VerticalAlignment = VerticalAlignment.Center,
            };
            ExporterOffice.Style = App.Current.FindResource("DetailMenuItems") as Style;

            Button delete = new Button();
            delete.ToolTip = _("Supprimer le document");
            delete.Click += Delete_Click; ;
            delete.TouchDown += Delete_Click; ;
            delete.Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new PackIcon() { Kind = PackIconKind.DeleteForever, Width = 30, Height = 30 }
                    //,
                    //new TextBlock(){Text = "Supprimer", VerticalAlignment = VerticalAlignment.Center,}
                },
                VerticalAlignment = VerticalAlignment.Center,
            };
            delete.Style = App.Current.FindResource("DetailMenuItems") as Style;

            // Send email

            Button email = new Button();
            email.Click += Email_Click;
            email.ToolTip = _("Envoyer par email");
            email.TouchDown += Email_Click; ;
            email.Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new PackIcon() { Kind = PackIconKind.EmailOutline, Width = 30, Height = 30 }
                    //,
                    //new TextBlock(){Text = "Supprimer", VerticalAlignment = VerticalAlignment.Center,}
                },
                VerticalAlignment = VerticalAlignment.Center,
            };
            email.Style = App.Current.FindResource("DetailMenuItems") as Style;

            Separator sep = new Separator();
            sep.BorderThickness = new Thickness(1);
            sep.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#D3D3D3");

            if((model is IReportView) == false)
            {
                linkButtonsOps.Children.Add(email);
                linkButtonsOps.Children.Add(delete);
                linkButtonsOps.Children.Add(actualiser);
                linkButtonsOps.Children.Add(Exporter);
                linkButtonsOps.Children.Add(ExporterOffice);
            }
          

            linkButtonsOps.Children.Add(sep);

            masterWrap = new UniformGrid();
            masterWrap.Margin = new Thickness(10, 5, 10, 5);
            masterWrap.Columns = 2;
            // masterWrap.MaxHeight = 750;
            masterWrap.VerticalAlignment = VerticalAlignment.Stretch;
            masterWrap.HorizontalAlignment = HorizontalAlignment.Left;
            masterWrap.Width = 900;
            masterWrap.Height = Double.NaN;

            stackContent = new StackPanel();
            stackContent.Margin = new Thickness(0);
            stackContent.Orientation = Orientation.Vertical;
            //  stackContent.MaxHeight = 750;
            stackContent.VerticalAlignment = VerticalAlignment.Stretch;
            stackContent.HorizontalAlignment = HorizontalAlignment.Stretch;

            var properties = type.GetProperties();
            //reposition image site
            //try
            //{
            //    var index = properties.FindIndex(x => x.Name.Contains("TopItem"));
            //    var TopItem = properties[index];
            //    properties.RemoveAt(index);
            //    properties.Insert(0, TopItem);
            //}
            //catch { }

            var submitable = (model as IDocument);
            if (submitable.Submitable)
            {
                if (submitable.isLocal == false && submitable.DocStatus == 0 && IsEdited == false)
                {
                    //btnName = "Valider";
                    //btnColor = "#2FCC71";
                    NotifyOfPropertyChange("btnName");
                    NotifyOfPropertyChange("btnColor");
                }
                else if (submitable.isLocal == false && submitable.DocStatus == 1 && IsEdited == false)
                {
                    //btnName = "Annuler";
                    //btnColor = "#FF3D00"//"#2FCC71";
                    NotifyOfPropertyChange("btnName");
                    NotifyOfPropertyChange("btnColor");
                    isFreezed = true;
                }
                else
                {
                    //btnName = "Enregistrer";
                    //btnColor = "#2196F3";
                    NotifyOfPropertyChange("btnName");
                    NotifyOfPropertyChange("btnColor");
                }
            }

            //MethodInfo method = typeof(DataHelpers).GetMethod("GetMongoData");
            //MethodInfo generic = method.MakeGenericMethod(type.Assembly.GetType());
            //generic.Invoke(null, null);

            dynamic concrete = model;
            bool addToPanel = true;

            int indexRox = 0;

            Expander expander = new Expander();
            expander.Header = _("Général");
            expander.IsExpanded = true;
            expander.HorizontalAlignment = HorizontalAlignment.Stretch;
            // expander.Width = 900;
            expander.Padding = new Thickness(0, 0, 0, 20);
            expander.BorderThickness = new Thickness(0, 0, 1, 0);
            expander.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#D3D3D3");
            expander.FlowDirection = DataHelpers.GetFlowDirection;

            // Préférence expander
            expanderPref.Content = null;
            expanderPref = new Expander();
            expanderPref.Header = _("Tableau de Bord");
            expanderPref.IsExpanded = CollapseAll;
            expanderPref.Padding = new Thickness(0, 0, 0, 20);
            expanderPref.BorderThickness = new Thickness(0, 0, 1, 0);
            expanderPref.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#f9fafc");
            expanderPref.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#f9fafc"); //Brushes.LightYellow;
            expanderPref.Margin = new Thickness(-1, 0, 0, 0);
            // expanderPref.Foreground = Brushes.White;

            masterWrapPref.Children.Clear();
            masterWrapPref = new UniformGrid();
            masterWrapPref.Margin = new Thickness(10, 5, 10, 5);
            masterWrapPref.Columns = 4;
            masterWrapPref.MaxHeight = 750;
            masterWrapPref.VerticalAlignment = VerticalAlignment.Stretch;
            masterWrapPref.HorizontalAlignment = HorizontalAlignment.Left;
            masterWrapPref.Height = Double.NaN;
            masterWrapPref.Width = 900;
            //cardSp = new StackPanel();
            //cardSp.Children.Clear();

            // ADD CONTROL PANEL AT THE TOP

            #endregion SETUP

            foreach (var prop in properties)
            {
                addToPanel = true;
                string name = "";
                var dd = prop.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
                if (dd != null)
                {
                    name = dd.DisplayName;
                }

                bool DoRefresh = true;
                //var refresh = prop.GetCustomAttribute(typeof(RefreshViewAttribute)) as RefreshViewAttribute;
                //if (refresh != null)
                //    DoRefresh = true;

                var dontShow = prop.GetCustomAttribute(typeof(DontShowInDetailAttribute)) as DontShowInDetailAttribute;
                var isBold = prop.GetCustomAttribute(typeof(IsBoldAttribute)) as IsBoldAttribute;

                if (!String.IsNullOrWhiteSpace(name) && dontShow == null)
                {
                    //if ((model as IDocument).PropertiesToHide.Contains(prop.Name))
                    //    continue;

                    Label label = new Label();
                    label.Content = name;
                    label.Margin = new Thickness(3);
                    label.Padding = new Thickness(2);
                    label.HorizontalAlignment = HorizontalAlignment.Left;
                    label.FontSize = 13;
                    label.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#535864");
                    Grid.SetColumn(label, 0);
                    Border border = new Border()
                    {
                        CornerRadius = new CornerRadius(2),
                        BorderThickness = new Thickness(1),
                        BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#eceff1")
                    };
                    border.Width = elementWidth;
                    border.Height = 28;
                    border.Padding = new Thickness(0);
                    border.Margin = new Thickness(3);
                    border.HorizontalAlignment = HorizontalAlignment.Left;

                    StackPanel sp = new StackPanel();
                    sp.Width = elementWidth;
                    sp.Height = 70;
                    sp.VerticalAlignment = VerticalAlignment.Bottom;
                    sp.Margin = new Thickness(20, 3, 3, 0);
                    sp.Orientation = Orientation.Vertical;
                    sp.HorizontalAlignment = HorizontalAlignment.Left;
                    var attributes = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                    var mytypeAttr = prop.GetCustomAttribute(typeof(myTypeAttribute)) as myTypeAttribute;
                    var setColumn = prop.GetCustomAttribute<SetColumnAttribute>();
                    if (attributes != null)
                    {
                        if (setColumn?.column == 1)
                        {
                            if (masterWrap.Children.Count % 2 != 0)
                                masterWrap.Children.Add(new StackPanel());
                        }
                        if (setColumn?.column == 2)
                        {
                            if (masterWrap.Children.Count % 2 == 0)
                                masterWrap.Children.Add(new StackPanel());
                        }

                        switch (attributes.FieldType)
                        {
                            case ModelFieldType.Text:
                                try
                                {

                                    var longDescription = prop.GetCustomAttribute(typeof(LongDescriptionAttribute)) as LongDescriptionAttribute;
                                    TextBox tb = new TextBox();
                                    tb.Name = prop.Name;
                                    var bindVisibility = new Binding("ElementToHide");
                                    bindVisibility.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibility.Converter = _UiVisibilityConverter;
                                    bindVisibility.Source = this;
                                    bindVisibility.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibility.ConverterParameter = prop.Name;
                                    tb.SetBinding(TextBox.VisibilityProperty, bindVisibility);

                                    HintAssist.SetHint(tb, name);
                                    //HintAssist.SetFloatingScale(tb, 1);
                                    tb.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxWhite") as Style;
                                    tb.Width = elementWidth;
                                    var myBinding = new Binding(prop.Name);
                                    myBinding.Source = model;
                                    myBinding.Mode = BindingMode.TwoWay;
                                    myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    tb.SetBinding(TextBox.TextProperty, myBinding);
                                    if (DoRefresh)
                                    {
                                        tb.KeyUp += Tb_KeyUp;
                                    }
                                    if (isBold?.IsBod == true)
                                        tb.FontWeight = FontWeights.Bold;

                                    if (longDescription != null)
                                    {
                                        var text = longDescription.text;
                                        TextBlock tbLong = new TextBlock();
                                        tbLong.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0");
                                        tbLong.TextWrapping = TextWrapping.Wrap;
                                        tbLong.Width = elementWidth;
                                        tbLong.Text = text;
                                        sp.Children.Add(tbLong);
                                        sp.Height = double.NaN;
                                    }
                                    sp.Children.Add(tb);

                                    masterWrap.Children.Add(sp);
                                    //  expander.IsExpanded = CollapseAll;
                                    indexRox++;

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Date:
                                try
                                {
                                    var optionDate = attributes.Options;
                                    var datePicker = new Xceed.Wpf.Toolkit.DateTimePicker();//..f DateTimePicker();
                                    datePicker.AutoCloseCalendar = true;
                                    if (CultureInfo.CurrentCulture.Name.Contains("ar"))
                                    {
                                        datePicker.CultureInfo = CultureInfo.InvariantCulture;
                                        datePicker.Language = XmlLanguage.GetLanguage("en-US");
                                    }
                                    else if (CultureInfo.CurrentCulture.Name.Contains("fr"))
                                    {
                                        datePicker.CultureInfo = new CultureInfo("fr-FR");
                                        datePicker.Language = XmlLanguage.GetLanguage("fr-FR");// CultureInfo;
                                    }
                                    if (CultureInfo.CurrentCulture.Name.Contains("en"))
                                    {
                                        datePicker.CultureInfo = new CultureInfo("en-US");
                                        datePicker.Language = XmlLanguage.GetLanguage("en-US");// CultureInfo;
                                    }

                                    datePicker.HorizontalContentAlignment = HorizontalAlignment.Left;
                                    datePicker.CalendarWidth = 250;

                                    datePicker.TextAlignment = TextAlignment.Left;

                                    datePicker.Format = Xceed.Wpf.Toolkit.DateTimeFormat.LongDate;
                                    datePicker.Watermark = _("Select date");
                                    datePicker.Name = prop.Name;
                                    if (DoRefresh)
                                    {
                                        datePicker.LostFocus += DatePicker_LostFocus;//= DatePicker_ValueChanged1;// ;= DatePicker_ValueChanged; ;
                                    }
                                    Border brDate = new Border();
                                    brDate.BorderThickness = new Thickness(1);
                                    brDate.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0");
                                    brDate.CornerRadius = new CornerRadius(5);
                                    brDate.Padding = new Thickness(1);
                                    brDate.Background = Brushes.White;
                                    brDate.Width = elementWidth;

                                    datePicker.Style = App.Current.FindResource("MaterialDesignFloatingHintDateTimePickerEx") as Style;

                                    var myBindingDate = new Binding(prop.Name);
                                    myBindingDate.Source = model;
                                    myBindingDate.Mode = BindingMode.TwoWay;
                                    myBindingDate.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    datePicker.SetBinding(Xceed.Wpf.Toolkit.DateTimePicker.ValueProperty, myBindingDate);

                                    //HintAssist.SetHint(datePicker, name);
                                    if (isBold?.IsBod == true)
                                        datePicker.FontWeight = FontWeights.Bold;

                                    Button btnViewDate = new Button();
                                    btnViewDate.Content = new PackIcon() { Kind = PackIconKind.Update };
                                    //btnView.Style = App.Current.FindResource("MaterialDesignFloatingActionMiniDarkButton") as Style;
                                    btnViewDate.Style = App.Current.FindResource("ToolButton") as Style;

                                    btnViewDate.Margin = new Thickness(2);
                                    btnViewDate.Padding = new Thickness(2);
                                    btnViewDate.HorizontalAlignment = HorizontalAlignment.Right;

                                    btnViewDate.Tag = datePicker;
                                    btnViewDate.Click += BtnViewDate_Click;
                                    btnViewDate.TouchDown += BtnViewDate_Click;

                                    datePicker.Width = elementWidth - 55;
                                    datePicker.VerticalAlignment = VerticalAlignment.Bottom;
                                    datePicker.VerticalContentAlignment = VerticalAlignment.Bottom;
                                    StackPanel sp1date = new StackPanel();
                                    //  sp1date.Margin = new Thickness(25,15,10,0);
                                    sp1date.Orientation = Orientation.Horizontal;
                                    sp1date.Children.Add(datePicker);
                                    sp1date.Children.Add(btnViewDate);
                                    brDate.Child = sp1date;

                                    var bindVisibilityDate = new Binding("ElementToHide");
                                    bindVisibilityDate.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityDate.Converter = _UiVisibilityConverter;
                                    bindVisibilityDate.Source = this;
                                    bindVisibilityDate.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityDate.ConverterParameter = prop.Name;
                                    datePicker.SetBinding(Xceed.Wpf.Toolkit.DateTimePicker.VisibilityProperty, bindVisibilityDate);

                                    //if (datePicker.Text == "")
                                    //    label.Content = "";

                                    var bindVisibilityDatelabel = new Binding("LabelVisibility");
                                    bindVisibilityDatelabel.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityDatelabel.Converter = _LabelVisibilityConverter;
                                    bindVisibilityDatelabel.Source = this;
                                    bindVisibilityDatelabel.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityDatelabel.ConverterParameter = datePicker;
                                    label.SetBinding(Label.VisibilityProperty, bindVisibilityDate);

                                    sp.Children.Add(label);
                                    sp.Children.Add(brDate);
                                    sp.Width = elementWidth;

                                    masterWrap.Children.Add(sp);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                    break;

                            case ModelFieldType.Devise:
                                try
                                {

                                    TextBox tbDevise = new TextBox();
                                    tbDevise.Name = prop.Name;
                                    // tbDevise.MouseDown += TbDevise_MouseDown;
                                    // tbDevise.MouseLeftButtonUp += TbDevise_MouseDown;
                                    HintAssist.SetHint(tbDevise, name);
                                    // HintAssist.SetFloatingScale(tbDevise, 0.8);
                                    if (isBold?.IsBod == true)
                                        tbDevise.FontWeight = FontWeights.Bold;
                                    // tb.Style = App.Current.FindResource("MaterialDesignTextFieldBoxTextBoxExtra") as Style;
                                    tbDevise.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxWhite") as Style;
                                    tbDevise.MouseLeftButtonUp += TbDevise_MouseDown1;
                                    tbDevise.StylusButtonUp += TbDevise_StylusButtonUp; ;
                                    tbDevise.MouseDown += (a, z) => { (a as TextBox).SelectAll(); };
                                    tbDevise.GotMouseCapture += (a, z) => { (a as TextBox).SelectAll(); };

                                    tbDevise.Width = elementWidth;

                                    var bindVisibilityDevis = new Binding("ElementToHide");
                                    bindVisibilityDevis.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityDevis.Converter = _UiVisibilityConverter;
                                    bindVisibilityDevis.Source = this;
                                    bindVisibilityDevis.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityDevis.ConverterParameter = prop.Name;
                                    tbDevise.SetBinding(TextBox.VisibilityProperty, bindVisibilityDevis);

                                    //  tbDevise.Height = 20;
                                    var myBindingDevise = new Binding(prop.Name);
                                    myBindingDevise.Source = model;

                                    myBindingDevise.StringFormat = "N";//!string.IsNullOrWhiteSpace(DataHelpers.Settings.FormatDevis) ? DataHelpers.Settings.FormatDevis : "N";
                                    myBindingDevise.Mode = BindingMode.TwoWay;
                                    myBindingDevise.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    tbDevise.SetBinding(TextBox.TextProperty, myBindingDevise);
                                    //tbDevise.Margin = new Thickness(0, 0, 3, 0);
                                    //tbDevise.Padding = new Thickness(7, -1, 0, 0);
                                    //tbDevise.BorderThickness = new Thickness(0);
                                    if (DoRefresh)
                                    {
                                        tbDevise.KeyUp += Tb_KeyUp;
                                    }
                                    //  border.Child = tbDevise;

                                    // sp.Children.Add(label);
                                    sp.Children.Add(tbDevise);
                                    //if (indexRox >= grid.RowDefinitions.Count)
                                    //{
                                    //    RowDefinition gridRowDevise = new RowDefinition();
                                    //    gridRowDevise.Height = new GridLength(35);
                                    //    grid.RowDefinitions.Add(gridRowDevise);
                                    //}
                                    //Grid.SetColumn(sp, indexColumn);
                                    //Grid.SetRow(sp, indexRox);
                                    // grid.Children.Add(sp);
                                    // indexRox++;

                                    masterWrap.Children.Add(sp);
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Numero:
                                try
                                {

                                    TextBox tbNumero = new TextBox();
                                    tbNumero.Name = prop.Name;
                                    HintAssist.SetHint(tbNumero, name);
                                    // HintAssist.SetFloatingScale(tbNumero, 0.8);
                                    tbNumero.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxWhite") as Style;
                                    tbNumero.Width = elementWidth;

                                    var bindVisibilityNumero = new Binding("ElementToHide");
                                    bindVisibilityNumero.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityNumero.Converter = _UiVisibilityConverter;
                                    bindVisibilityNumero.Source = this;
                                    bindVisibilityNumero.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityNumero.ConverterParameter = prop.Name;
                                    tbNumero.SetBinding(TextBox.VisibilityProperty, bindVisibilityNumero);
                                    tbNumero.GotMouseCapture += (a, z) => { (a as TextBox).SelectAll(); };
                                    tbNumero.MouseLeftButtonUp += (a, z) => { (a as TextBox).SelectAll(); };
                                    tbNumero.MouseRightButtonUp += (a, z) => { (a as TextBox).SelectAll(); };

                                    var myBindingNumero = new Binding(prop.Name);
                                    myBindingNumero.Source = model;
                                    myBindingNumero.StringFormat = attributes.Options;
                                    myBindingNumero.Mode = BindingMode.TwoWay;
                                    myBindingNumero.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    tbNumero.SetBinding(TextBox.TextProperty, myBindingNumero);
                                    if (isBold?.IsBod == true)
                                        tbNumero.FontWeight = FontWeights.Bold;
                                    //border.Child = tbNumero;
                                    tbNumero.KeyUp += Tb_KeyUp;
                                    // sp.Children.Add(label);
                                    sp.Children.Add(tbNumero);

                                    masterWrap.Children.Add(sp);
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Separation:
                                try
                                {

                                    if (sp.Children.Count > 0)
                                        masterWrap.Children.Add(sp);
                                    expander.Content = masterWrap;
                                    stackContent.Children.Add(expander);
                                    masterWrap = new UniformGrid();
                                    masterWrap.Margin = new Thickness(10, 5, 10, 5);
                                    masterWrap.Columns = 2;
                                    masterWrap.MaxHeight = 750;
                                    masterWrap.VerticalAlignment = VerticalAlignment.Stretch;
                                    masterWrap.HorizontalAlignment = HorizontalAlignment.Left;
                                    //  masterWrap.Width = Double.NaN;
                                    masterWrap.Height = Double.NaN;
                                    masterWrap.Width = 900;

                                    borderSeparation = new Border();
                                    borderSeparation.Background = App.Current.FindResource("MaterialDesignDivider") as Brush;
                                    borderSeparation.Height = 1;
                                    borderSeparation.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    borderSeparation.SnapsToDevicePixels = true;
                                    stackContent.Children.Add(borderSeparation);

                                    expander = new Expander();
                                    expander.Header = name.ToUpper();
                                    expander.FlowDirection = DataHelpers.GetFlowDirection;
                                    bool expanded = CollapseAll;
                                    try { expanded = bool.Parse(attributes.Options); } catch { }
                                    expander.IsExpanded = expanded;
                                    expander.Padding = new Thickness(0, 0, 0, 20);
                                    expander.BorderThickness = new Thickness(0, 0, 1, 0);
                                    expander.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#D3D3D3");

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Select:
                                try
                                {

                                    var optionSelect = attributes.Options;

                                    List<string> dataSelect = new List<string>();

                                    if (optionSelect.Contains("this"))
                                    {
                                        var secondParam = optionSelect.Split('>');
                                        var options = model.GetType().GetProperty(secondParam[1].ToString()).GetValue(model) as List<string>;
                                        dataSelect = options?.ToList();
                                    }
                                    else
                                    {
                                        dataSelect = DataHelpers.GetSelectData(optionSelect);
                                    }

                                    Border brSelect = new Border();
                                    brSelect.BorderThickness = new Thickness(1);
                                    brSelect.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0");
                                    brSelect.CornerRadius = new CornerRadius(5);
                                    brSelect.Padding = new Thickness(1);
                                    brSelect.Background = Brushes.White;

                                    ComboBox boxSelect = new ComboBox();
                                    boxSelect.Name = prop.Name;
                                    var bindVisibilitySelect = new Binding("ElementToHide");
                                    bindVisibilitySelect.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilitySelect.Converter = _UiVisibilityConverter;
                                    bindVisibilitySelect.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    boxSelect.ItemsPanel = (ItemsPanelTemplate)Application.Current.FindResource("VSP");
                                    boxSelect.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                                    boxSelect.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                                    bindVisibilitySelect.Source = this;
                                    bindVisibilitySelect.ConverterParameter = prop.Name;
                                    boxSelect.SetBinding(ComboBox.VisibilityProperty, bindVisibilitySelect);

                                    HintAssist.SetHint(boxSelect, name);
                                    //  HintAssist.SetFloatingScale(box, 0.8)MaterialDesignFloatingHintComboBoxWhite;MaterialDesignFloatingHintComboBox
                                    boxSelect.Style = App.Current.FindResource("MaterialDesignFloatingHintComboBoxWhite") as Style;
                                    // box.Margin = new Thickness(0);
                                    if (DoRefresh)
                                    {
                                        boxSelect.SelectionChanged += Box_SelectionChanged;
                                    }
                                    if (isBold?.IsBod == true)
                                        boxSelect.FontWeight = FontWeights.Bold;
                                    boxSelect.Width = elementWidth - 20;

                                    var pathSelect = $"{optionSelect}";
                                    var myBindingSelect = new Binding($"{prop.Name}");
                                    myBindingSelect.Source = model;
                                    myBindingSelect.Mode = BindingMode.TwoWay;
                                    myBindingSelect.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    boxSelect.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = dataSelect, Mode = BindingMode.OneTime });
                                    boxSelect.SetBinding(ComboBox.SelectedItemProperty, new Binding { Source = model, Path = new PropertyPath(prop.Name), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });

                                    //if (boxSelect.Text == "")
                                    //    label.Content = "";

                                    var bindVisibilitySelectLabel = new Binding("LabelVisibility");
                                    bindVisibilitySelectLabel.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilitySelectLabel.Converter = _LabelVisibilityConverter;
                                    bindVisibilitySelectLabel.Source = this;
                                    bindVisibilitySelectLabel.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                                    bindVisibilitySelectLabel.ConverterParameter = boxSelect;
                                    label.SetBinding(Label.VisibilityProperty, bindVisibilitySelectLabel);

                                    brSelect.Child = boxSelect;
                                    sp.Width = elementWidth;
                                    sp.Children.Add(label);
                                    sp.Children.Add(brSelect);
                                    masterWrap.Children.Add(sp);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                // box.SetBinding(ComboBox.DisplayMemberPathProperty, new Binding { Source = $"Name" });
                                // box.SelectedValuePath = "Id";
                                // border.Height = 40;
                                // X
                                // box.Padding = new Thickness(7, -1, 0, 0);
                                // border.Child = box;
                                //// sp.Children.Add(label);
                                // sp.Children.Add(border);

                                // masterWrap.Children.Add(sp);
                                break;

                            case ModelFieldType.Check:
                                try
                                {

                                    CheckBox checkbox = new CheckBox();
                                    checkbox.Name = prop.Name;
                                    var optioncheck = attributes.Options;

                                    var bindVisibilityCheck = new Binding("ElementToHide");
                                    bindVisibilityCheck.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityCheck.Converter = _UiVisibilityConverter;
                                    bindVisibilityCheck.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                                    bindVisibilityCheck.Source = this;
                                    bindVisibilityCheck.ConverterParameter = prop.Name;
                                    checkbox.SetBinding(CheckBox.VisibilityProperty, bindVisibilityCheck);

                                    checkbox.Content = name;

                                    checkbox.Width = elementWidth;
                                    checkbox.Height = 20;
                                    var myBindingCheck = new Binding(prop.Name);
                                    myBindingCheck.Source = model;
                                    myBindingCheck.Mode = BindingMode.TwoWay;
                                    myBindingCheck.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    checkbox.SetBinding(CheckBox.IsCheckedProperty, myBindingCheck);
                                    checkbox.Margin = new Thickness(0, 25, 0, 0);
                                    //label.Margin = new Thickness(50, 0, 10, 0);
                                    // checkbox.Padding = new Thickness(7, -1, 0, 0);
                                    checkbox.BorderThickness = new Thickness(1);
                                    //border.BorderThickness = new Thickness(0);
                                    //border.Child = checkbox;
                                    if (DoRefresh)
                                    {
                                        checkbox.Click += Checkbox_Click;
                                        checkbox.TouchDown += Checkbox_Click;
                                    }
                                    label.FontSize = 12;
                                    label.Content = _(optioncheck);
                                    label.FontWeight = FontWeights.Normal;
                                    sp.Children.Add(checkbox);
                                    sp.Children.Add(label);
                                    //if (indexRox >= grid.RowDefinitions.Count)
                                    //{
                                    //    RowDefinition gridRowCheck = new RowDefinition();
                                    //    gridRowCheck.Height = new GridLength(35);
                                    //    grid.RowDefinitions.Add(gridRowCheck);
                                    //}
                                    //Grid.SetColumn(sp, indexColumn);
                                    //Grid.SetRow(sp, indexRox);
                                    // grid.Children.Add(sp);
                                    // indexRox++;

                                    masterWrap.Children.Add(sp);
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.TextLarge:
                                try
                                {

                                    if (sp.Children.Count > 0)
                                        masterWrap.Children.Add(sp);

                                    bool expandedTextLarge = CollapseAll;
                                    try { expandedTextLarge = bool.Parse(attributes.Options); } catch { }

                                    expander.Content = masterWrap;
                                    stackContent.Children.Add(expander);
                                    masterWrap = new UniformGrid();
                                    masterWrap.Margin = new Thickness(10, 5, 10, 5);
                                    masterWrap.Columns = 1;
                                    masterWrap.MaxHeight = elementWidth * 2 + 50;
                                    masterWrap.VerticalAlignment = VerticalAlignment.Top;
                                    masterWrap.HorizontalAlignment = HorizontalAlignment.Left;
                                    // masterWrap.Width = Double.NaN;
                                    masterWrap.Height = 100;
                                    masterWrap.Width = 900;

                                    expander = new Expander();
                                    expander.FlowDirection = DataHelpers.GetFlowDirection;
                                    expander.Header = name.ToUpper();
                                    expander.IsExpanded = expandedTextLarge;
                                    expander.Padding = new Thickness(0, 0, 0, 20);
                                    expander.BorderThickness = new Thickness(0, 0, 1, 0);
                                    expander.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#D3D3D3");

                                    borderSeparation = new Border();
                                    borderSeparation.Background = App.Current.FindResource("MaterialDesignDivider") as Brush;
                                    borderSeparation.Height = 1;
                                    borderSeparation.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    borderSeparation.SnapsToDevicePixels = true;
                                    stackContent.Children.Add(borderSeparation);

                                    label.VerticalAlignment = VerticalAlignment.Top;
                                    TextBox rtb = new TextBox();
                                    rtb.Name = prop.Name;
                                    rtb.KeyUp += Tb_KeyUp;
                                    var bindVisibilityTextLarge = new Binding("ElementToHide");
                                    bindVisibilityTextLarge.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityTextLarge.Converter = _UiVisibilityConverter;
                                    bindVisibilityTextLarge.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                                    bindVisibilityTextLarge.Source = this;
                                    bindVisibilityTextLarge.ConverterParameter = prop.Name;
                                    rtb.SetBinding(TextBox.VisibilityProperty, bindVisibilityTextLarge);

                                    //  rtb.Margin = new Thickness(0);
                                    //  tb.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxWhite") as Style;
                                    rtb.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxLarge") as Style;
                                    rtb.Height = 100;
                                    rtb.Width = elementWidth * 2 + 50;

                                    rtb.VerticalAlignment = VerticalAlignment.Top;
                                    HintAssist.SetHint(rtb, $"{name}...");

                                    rtb.AcceptsReturn = true;
                                    rtb.TextWrapping = TextWrapping.Wrap;
                                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                    rtb.Margin = new Thickness(0);
                                    rtb.Padding = new Thickness(4);
                                    var myBindingTextLarge = new Binding(prop.Name);
                                    myBindingTextLarge.Source = model;
                                    myBindingTextLarge.Mode = BindingMode.TwoWay;
                                    myBindingTextLarge.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    rtb.SetBinding(TextBox.TextProperty, myBindingTextLarge);
                                    rtb.Background = Brushes.White;

                                    masterWrap.Children.Add(rtb);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Lien:

                                #region Champ lien
                                try
                                {

                                    var optionLien = attributes.Options;
                                    ComboBox box = new ComboBox();
                                    box.Name = prop.Name;
                                    var bindVisibilityLien = new Binding("ElementToHide");
                                    bindVisibilityLien.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityLien.Converter = _UiVisibilityConverter;
                                    bindVisibilityLien.Source = this;

                                    bindVisibilityLien.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityLien.ConverterParameter = prop.Name;
                                    box.SetBinding(ComboBox.VisibilityProperty, bindVisibilityLien);

                                    Border br = new Border();
                                    br.BorderThickness = new Thickness(1);
                                    br.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0");
                                    br.CornerRadius = new CornerRadius(5);
                                    br.Padding = new Thickness(1);
                                    br.Background = Brushes.White;
                                    HintAssist.SetHint(box, name);
                                    //HintAssist.SetFloatingScale(box, 1);
                                    //HintAssist.SetFloatingOffset(box, new Point(0,-10));

                                    box.Style = App.Current.FindResource("MaterialDesignFloatingHintComboBoxWhite") as Style;
                                    // box.Style = App.Current.FindResource("MaterialDesignFloatingHintComboBox") as Style;
                                    box.ItemsPanel = (ItemsPanelTemplate)Application.Current.FindResource("VSP");
                                    box.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                                    box.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                                    if (isBold?.IsBod == true)
                                        box.FontWeight = FontWeights.Bold;
                                    //  box.Margin = new Thickness(0);
                                    if (DoRefresh)
                                    {
                                        box.SelectionChanged += Box_SelectionChanged1; ;
                                        box.DropDownClosed += Box_DropDownClosed;
                                        box.MouseLeftButtonUp += Box_LostFocus1;
                                    }
                                    box.IsTextSearchEnabled = false;
                                    box.IsTextSearchCaseSensitive = false;

                                    // box.Tag = box;

                                    //box.Padding = new Thickness(5, 2, 0, 0);
                                    // box.BorderThickness = new Thickness(0.5);
                                    // box.SelectionChanged += Box_LostFocus;
                                    box.IsEditable = true;
                                    //MaxLinesFetch
                                    var dataLien = DS.Generic(optionLien)?.GetAll();
                                    var property = prop.Name;
                                    var pathLien = property;// $"l{optionLien}";
                                                            //var myBindingLien = new Binding($"{pathLien}");
                                                            // myBindingLien.Source = model;
                                                            // myBindingLien.Mode = BindingMode.TwoWay;
                                                            // myBindingLien.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    box.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = dataLien, Mode = BindingMode.OneTime });
                                    box.SetBinding(ComboBox.SelectedValueProperty, new Binding { Source = model, Path = new PropertyPath(pathLien), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });
                                    box.SetBinding(ComboBox.DisplayMemberPathProperty, new Binding { Source = $"NameSearch" });
                                    box.SelectedValuePath = "Id";
                                    //  box.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    box.Width = elementWidth - 62;

                                    Button btnView = new Button();
                                    btnView.Content = new PackIcon() { Kind = PackIconKind.DatabaseSearch };
                                    btnView.ToolTip = _("Chercher");
                                    //btnView.Style = App.Current.FindResource("MaterialDesignFloatingActionMiniDarkButton") as Style;
                                    btnView.Style = App.Current.FindResource("ToolButton") as Style;
                                    var clsLien = new ArrayList() { box, optionLien, prop.Name };

                                    var clsBox = new ArrayList() { optionLien, prop.Name };
                                    box.Tag = clsBox;
                                    box.MouseDoubleClick += Box_MouseDoubleClick;

                                    btnView.Tag = clsLien;
                                    btnView.Click += BtnView_Click1;
                                    btnView.TouchDown += BtnView_Click1;

                                    btnView.Margin = new Thickness(2);
                                    btnView.Padding = new Thickness(2);
                                    btnView.HorizontalAlignment = HorizontalAlignment.Right;

                                    Button btnAdd = new Button();
                                    btnAdd.Content = new PackIcon() { Kind = PackIconKind.Plus };
                                    btnAdd.ToolTip = _("Créer nouveau");
                                    btnAdd.Style = App.Current.FindResource("ToolButton") as Style;
                                    //  btnAdd.Style = App.Current.FindResource("MaterialDesignFloatingActionMiniDarkButton") as Style;
                                    btnAdd.Tag = new ArrayList() { optionLien, prop.Name };
                                    btnAdd.Click += BtnAdd_Click;
                                    btnAdd.TouchDown += BtnAdd_Click;
                                    btnAdd.Margin = new Thickness(2);
                                    btnAdd.Padding = new Thickness(2);
                                    btnAdd.HorizontalAlignment = HorizontalAlignment.Right;

                                    StackPanel sp1 = new StackPanel();
                                    sp1.Margin = new Thickness(0);
                                    sp1.Background = Brushes.White;
                                    sp1.Orientation = Orientation.Horizontal;
                                    sp1.Children.Add(box);
                                    sp1.Children.Add(btnAdd);
                                    sp1.Children.Add(btnView);
                                    // sp.Children.Add(label);
                                    br.Child = sp1;

                                    //if (box.Text == "")
                                    //    label.Content = "";

                                    var bindVisibilitylabel = new Binding("LabelVisibility");
                                    bindVisibilitylabel.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilitylabel.Converter = _LabelVisibilityConverter;
                                    bindVisibilitylabel.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                                    bindVisibilitylabel.Source = this;
                                    bindVisibilitylabel.ConverterParameter = box;
                                    label.SetBinding(Label.VisibilityProperty, bindVisibilitylabel);

                                    sp.Children.Add(label);
                                    sp.Children.Add(br);
                                    sp.Width = elementWidth;
                                    //if (indexRox >= grid.RowDefinitions.Count)
                                    //{
                                    //    RowDefinition gridRowLien = new RowDefinition();
                                    //    gridRowLien.Height = new GridLength(35);
                                    //    grid.RowDefinitions.Add(gridRowLien);
                                    //}
                                    //Grid.SetColumn(sp, indexColumn);
                                    //Grid.SetRow(sp, indexRox);
                                    // grid.Children.Add(sp);
                                    // indexRox++;

                                    masterWrap.Children.Add(sp);
                                    box.Items.IsLiveFiltering = true;

                                    box.KeyUp += Box_KeyUp1;
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name); 
                                }
                                break;

                            #endregion Champ lien

                            case ModelFieldType.ReadOnly:
                                try
                                {

                                    #region Champ read only

                                    var format = attributes.Options;
                                    TextBox tbReadOnly = new TextBox();
                                    tbReadOnly.Name = prop.Name;
                                    tbReadOnly.KeyUp += Tb_KeyUp;
                                    tbReadOnly.Width = elementWidth;
                                    HintAssist.SetHint(tbReadOnly, name);
                                    // HintAssist.SetFloatingScale(tbReadOnly, 0.8);
                                    tbReadOnly.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxWhite") as Style;
                                    if (isBold?.IsBod == true)
                                        tbReadOnly.FontWeight = FontWeights.Bold;

                                    tbReadOnly.Background = Brushes.WhiteSmoke;
                                    //  tbReadOnly.Height = 20;
                                   var myBindingReadOnly = new Binding(prop.Name);
                                    myBindingReadOnly.Source = model;
                                    myBindingReadOnly.Mode = BindingMode.OneWay;
                                    //myBinding.ConverterCulture = new System.Globalization.CultureInfo("ar-DZ");
                                    myBindingReadOnly.StringFormat = format;
                                    myBindingReadOnly.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    tbReadOnly.SetBinding(TextBox.TextProperty, myBindingReadOnly);
                                    //tbReadOnly.Margin = new Thickness(0);
                                    //tbReadOnly.Padding = new Thickness(7, -1, 0, 0);
                                    //tbReadOnly.BorderThickness = new Thickness(0);
                                    tbReadOnly.IsReadOnly = true;
                                    //  border.Child = tbReadOnly;

                                    // sp.Children.Add(label);
                                    sp.Children.Add(tbReadOnly);

                                    var bindVisibilityReadOnly = new Binding("ElementToHide");
                                    bindVisibilityReadOnly.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityReadOnly.Converter = _UiVisibilityConverter;
                                    bindVisibilityReadOnly.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityReadOnly.Source = this;
                                    bindVisibilityReadOnly.ConverterParameter = prop.Name;
                                    tbReadOnly.SetBinding(TextBox.VisibilityProperty, bindVisibilityReadOnly);

                                    masterWrap.Children.Add(sp);
                                   

                                    #endregion Champ read only
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;
                            case ModelFieldType.LienButton:

                                #region Champ Lien Button
                                try
                                {

                                    Button newMenu = new Button();
                                    newMenu.Name = prop.Name;
                                    var optionLienButton = attributes.Options; // model name
                                    var propName = "";
                                    dynamic propValue = null;
                                    if (optionLienButton.Contains(">"))
                                    {
                                        // property name in options
                                        var spilts = optionLienButton.Split('>');
                                        propName = spilts[1].ToString();
                                        optionLienButton = spilts[0].ToString();
                                    }
                                    else if (optionLienButton.Contains("<"))
                                    {
                                        var spilts = optionLienButton.Split('<');
                                        propName = spilts[1].ToString();
                                        propValue = (model as IDocument).GetType()?.GetProperty(propName)?.GetValue(model);
                                        optionLienButton = spilts[0].ToString();
                                    }
                                    else
                                    {
                                        propName = prop.Name;
                                        optionLienButton = model.GetType().Name;
                                    }

                                        int? items = 0;
                                    
                                        // the code that you want to measure comes here

                                        // Try get count
                                        if (propValue != null)
                                        {
                                            items = (DS.Generic(optionLienButton)?.Find("_id", GuidParser.Convert( propValue), true))?.Count;
                                        }
                                        else
                                        {
                                            items = (DS.Generic(optionLienButton)?.Find(propName, model.Id, true))?.Count;
                                        }
                                   
                                    if (items > 0)
                                    {
                                        newMenu.Content = $"{name} ({items})";
                                    }
                                    else
                                    {
                                        newMenu.Content = $"{name}";
                                    }

                                    newMenu.Style = App.Current.FindResource("LinkButton") as Style;

                                    var tagLien = new ArrayList() { propName, optionLienButton, propValue };
                                    newMenu.Tag = tagLien;
                                    newMenu.Click += NewMenu_Click;
                                    newMenu.TouchDown += NewMenu_Click;
                                    newMenu.HorizontalAlignment = HorizontalAlignment.Left;
                                    newMenu.HorizontalContentAlignment = HorizontalAlignment.Left;

                                    var stackLienContent = new StackPanel();
                                    stackLienContent.Orientation = Orientation.Horizontal;
                                    stackLienContent.HorizontalAlignment = HorizontalAlignment.Left;
                                    stackLienContent.Margin = new Thickness(2, 5, 0, 0);

                                    stackLienContent.Children.Add(newMenu);

                                    if (!prop.Name.Contains("NOBTN"))
                                    {
                                        Button btnAddLien = new Button();
                                        btnAddLien.Content = new PackIcon() { Kind = PackIconKind.Plus, Width = 16 };
                                        btnAddLien.Style = App.Current.FindResource("LinkButtonPlus") as Style;
                                        btnAddLien.Tag = tagLien;
                                        btnAddLien.Click += BtnAddLien_Click;
                                        btnAddLien.TouchDown += BtnAddLien_Click;

                                        stackLienContent.Children.Add(btnAddLien);
                                    }

                                    linkButtons.Children.Add(stackLienContent);
                                    if (!(model as IDocument).isLocal)
                                        LinksVisible = Visibility.Visible;
                                    NotifyOfPropertyChange("LinksVisible");
                                    NotifyOfPropertyChange("linkButtons");

                                    //addToPanel = false;
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                    continue;
                                } 
                                break;

                            #endregion Champ Lien Button

                            case ModelFieldType.Image:

                                #region Champ image
                                try
                                {

                                    var optionImage = attributes.Options;
                                    sp.Height = 220;
                                    Image img = new Image();
                                    img.Name = prop.Name;
                                    img.Width = elementWidth - 62;
                                    img.Height = 170;
                                    var value = model.GetType().GetProperty(prop.Name).GetValue(model, null);
                                    var imgPath = "";
                                    if (value != null)
                                    {
                                        imgPath = System.IO.Path.GetFullPath(value);
                                    }

                                    img.SetBinding(Image.SourceProperty, new Binding() { Source = imgPath, Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                                    // img.Margin = new Thickness(25,0,0,10);
                                    Border b = new Border();
                                    b.Width = elementWidth;
                                    b.BorderThickness = new Thickness(1);
                                    b.CornerRadius = new CornerRadius(3, 3, 0, 0);
                                    b.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0");
                                    Button addBtn = new Button();
                                    addBtn.Name = prop.Name;
                                    addBtn.Tag = prop.Name;
                                    var content = new StackPanel();
                                    content.Orientation = Orientation.Horizontal;
                                    content.Children.Add(new PackIcon() { Kind = PackIconKind.PlusBox, Width = 15, Height = 15 });
                                    content.Children.Add(new TextBlock() { Text = name });
                                    addBtn.Content = content;
                                    addBtn.Click += AddBtn_Click;
                                    addBtn.TouchDown += AddBtn_Click;
                                    addBtn.VerticalAlignment = VerticalAlignment.Bottom;
                                    addBtn.Height = 26;
                                    addBtn.Width = elementWidth;
                                    StackPanel sps = new StackPanel();
                                    sps.Orientation = Orientation.Vertical;
                                    sps.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    sps.VerticalAlignment = VerticalAlignment.Stretch;
                                    b.Child = img;
                                    sps.Children.Add(b);
                                    sps.Children.Add(addBtn);
                                    sps.Margin = new Thickness(25, 10, 0, 10);

                                    //border.Height = 200;
                                    //border.Child = sps;

                                    //sp.Children.Add(border);

                                    masterWrap.Children.Add(sps);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.ImageSide:
                                try
                                {

                                    var optionImageSide = attributes.Options;
                                    //  sp.Height = 220;
                                    Image imgSide = new Image();
                                    imgSide.Name = prop.Name;
                                    ShadowAssist.SetShadowDepth(imgSide, ShadowDepth.Depth1);
                                    imgSide.Width = 140;
                                    //imgSide.Height = 200;
                                    var valueSide = model.GetType().GetProperty(prop.Name).GetValue(model, null);
                                    var imgPathSide = "";
                                    if (valueSide != null)
                                    {
                                        imgPathSide = System.IO.Path.GetFullPath(valueSide);
                                    }

                                    imgSide.SetBinding(Image.SourceProperty, new Binding()
                                    {
                                        Source = imgPathSide,
                                        Mode = BindingMode.OneWay,
                                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                    });
                                    imgSide.Margin = new Thickness(0);

                                    Button addBtnSide = new Button();
                                    addBtnSide.Style = App.Current.FindResource("ImportButtonStyle") as Style;
                                    addBtnSide.Tag = prop.Name;
                                    addBtnSide.Content = optionImageSide;
                                    addBtnSide.Click += AddBtn_Click;
                                    addBtnSide.TouchDown += AddBtn_Click;
                                    addBtnSide.VerticalAlignment = VerticalAlignment.Bottom;
                                    addBtnSide.Height = 26;
                                    addBtnSide.Width = 180;

                                    //var spImg = new StackPanel();
                                    //spImg.Orientation = Orientation.Vertical;

                                    //spImg.Children.Add(imgSide);
                                    //spImg.Children.Add(addBtnSide);

                                    //masterWrapPref.Children.Add(spImg);

                                    DocImageContent = new StackPanel();
                                    DocImageContent.Margin = new Thickness(0, 0, 10, 0);
                                    DocImageContent.Orientation = Orientation.Vertical;
                                    DocImageContent.Height = double.NaN;
                                    DocImageContent.HorizontalAlignment = HorizontalAlignment.Left  ;
                                   
                                    DocImageContent.Children.Add(imgSide);
                                    DocImageContent.Children.Add(addBtnSide);
                                    NotifyOfPropertyChange("DocImageContent");

                                    //var spsImage  = new StackPanel();
                                    //spsImage.Margin = new Thickness(10, 0, 10, 0);
                                    //spsImage.Orientation = Orientation.Vertical;
                                    //spsImage.Height = double.NaN;

                                    //spsImage.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    //spsImage.VerticalAlignment = VerticalAlignment.Stretch;
                                    //spsImage.Children.Add(imgSide);
                                    //spsImage.Children.Add(addBtnSide);

                                    //masterWrap.Children.Insert(0,spsImage);

                                    //border.Height = 200;
                                    //border.Child = spsSide;

                                    //sp.Children.Add(border);

                                    // masterWrap.Children.Add(sp);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            #endregion Champ image

                            case ModelFieldType.WeakTable:
                                try
                                {

                                    // AccessRule
                                    var optionWeak = attributes.Options;

                                    WrapPanel wrap = new WrapPanel();
                                    // List<AccessRule>
                                    List<IDocument> sourceData;
                                    if (optionWeak.Contains("this") == true)
                                    {
                                        var secondParam = optionWeak.Split('>');
                                        var options = model.GetType().GetProperty(secondParam[1].ToString()).GetValue(model) as IEnumerable<IDocument>;
                                          
                                        sourceData = options?.ToList();
                                    }
                                    else
                                    {
                                        var documents = DS.Generic(optionWeak)?.GetAll() ;//as IEnumerable<IDocument>;
                                        sourceData = documents;
                                    }
                                    foreach (var item in sourceData)
                                    {
                                        CheckBox itemBox = new CheckBox();
                                        itemBox.Name = prop.Name;
                                        itemBox.Margin = new Thickness(20, 5, 5, 5);
                                        itemBox.Content = item.NameSearch;
                                        if ((model as IDocument).GetType().GetProperty(prop.Name).GetValue(model)?.Contains(item.Id))
                                            itemBox.IsChecked = true;
                                        itemBox.Tag = new ArrayList() { prop.Name, item.Id };
                                        itemBox.Checked += ItemBox_Checked;
                                        itemBox.Unchecked += ItemBox_Checked;

                                        wrap.Children.Add(itemBox);
                                    }
                                    masterWrap.Children.Add(wrap);
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Table:

                                #region Champs table
                                try
                                {

                                    if (masterWrap.Children.Count > 0)
                                    {
                                        // NO SEPARATION
                                        if (sp.Children.Count > 0)
                                            masterWrap.Children.Add(sp);
                                        expander.Content = masterWrap;
                                        stackContent.Children.Add(expander);
                                        masterWrap = new UniformGrid();
                                        masterWrap.Margin = new Thickness(10, 5, 10, 5);
                                        masterWrap.Columns = 1;

                                        masterWrap.VerticalAlignment = VerticalAlignment.Top;
                                        masterWrap.HorizontalAlignment = HorizontalAlignment.Left;
                                        // masterWrap.Width = Double.NaN;
                                        // masterWrap.Height = 100;
                                        masterWrap.Width = 900;

                                        expander = new Expander();
                                        expander.FlowDirection = DataHelpers.GetFlowDirection;
                                        expander.Header = name.ToUpper();
                                        expander.IsExpanded = CollapseAll;
                                        expander.Padding = new Thickness(0, 0, 0, 20);
                                        expander.BorderThickness = new Thickness(0, 0, 1, 0);
                                        expander.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#D3D3D3");

                                        borderSeparation = new Border();
                                        borderSeparation.Background = App.Current.FindResource("MaterialDesignDivider") as Brush;
                                        borderSeparation.Height = 1;
                                        borderSeparation.HorizontalAlignment = HorizontalAlignment.Stretch;
                                        borderSeparation.SnapsToDevicePixels = true;
                                        stackContent.Children.Add(borderSeparation);
                                    }

                                    var optionTable = attributes.Options;
                                    var type = mytypeAttr?.type;
                                    if (type == null)
                                    {
                                         type = DataHelpers.GetTypesModule.Resolve(optionTable);
                                        if (type == null)
                                            break ;
                                    }
                                    var afterMapMethod = prop.GetCustomAttribute(typeof(AfterMapMethodAttribute)) as AfterMapMethodAttribute;

                                    DataGrid listview = new DataGrid();
                                    listview.SelectionUnit = DataGridSelectionUnit.FullRow;
                                    listview.Name = prop.Name;
                                    ContextMenu cm = new ContextMenu();
                                    MenuItem deleteItem = new MenuItem();
                                    deleteItem.Header = _("Supprimer");
                                    deleteItem.Click += DeleteItem_Click;
                                    deleteItem.TouchDown += DeleteItem_Click;
                                    deleteItem.IsEnabled = !isFreezed;
                                    deleteItem.Tag = listview;
                                    cm.Items.Add(deleteItem);

                                    MenuItem deleteAll = new MenuItem();
                                    deleteAll.Header = _("Supprimer tout");
                                    deleteAll.Click += DeleteAll_Click; ;
                                    deleteAll.TouchDown += DeleteAll_Click; ;
                                    deleteAll.Tag = listview;
                                    deleteAll.IsEnabled = !isFreezed;
                                    cm.Items.Add(deleteAll);

                                    var bindVisibilityTable = new Binding("ElementToHide");
                                    bindVisibilityTable.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityTable.Converter = _UiVisibilityConverter;
                                    bindVisibilityTable.Source = this;
                                    bindVisibilityTable.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityTable.ConverterParameter = prop.Name;
                                    listview.SetBinding(DataGrid.VisibilityProperty, bindVisibilityTable);
                                    listview.FlowDirection = DataHelpers.GetFlowDirection;
                                    listview.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0"); ;
                                    listview.BorderThickness = new Thickness(1);
                                    listview.FontSize = 14;
                                    listview.ContextMenu = cm;
                                    listview.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    listview.VerticalAlignment = VerticalAlignment.Stretch;
                                    listview.AutoGenerateColumns = true;
                                    listview.Background = Brushes.WhiteSmoke;
                                    listview.RowBackground = Brushes.White;
                                    listview.RowHeight = double.NaN;
                                    listview.Foreground = Brushes.Black;
                                    listview.Tag = afterMapMethod;
                                    // listview.CanUserAddRows = !isFreezed;
                                    listview.CanUserAddRows = false;
                                    listview.CanUserDeleteRows = !isFreezed;
                                    // listview.IsReadOnly = true;
                                    listview.RowEditEnding += Listview_RowEditEnding;
                                    listview.KeyUp += Listview_KeyUp;
                                    listview.Unloaded += Listview_Unloaded;
                                    listview.MouseDoubleClick += Listview_MouseDoubleClick;
                                    var myBindingTable = new Binding(prop.Name);
                                    myBindingTable.Source = model;
                                    //myBindingTable.IsAsync = true;
                                    myBindingTable.Mode = BindingMode.TwoWay;
                                    myBindingTable.NotifyOnSourceUpdated = true;

                                    myBindingTable.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    listview.AutoGeneratingColumn += ColumnHeaderBehavior.OnAutoGeneratingColumn;
                                    listview.MinHeight = 150;
                                    listview.MinWidth = 655;
                                    listview.Margin = new Thickness(25, 0, 20, 20);
                                    label.Visibility = Visibility.Hidden;
                                    listview.SetBinding(DataGrid.ItemsSourceProperty, myBindingTable);
                                    DataGridAssist.SetCellPadding(listview, new Thickness(4, 2, 2, 2));
                                    DataGridAssist.SetColumnHeaderPadding(listview, new Thickness(4, 2, 2, 2));
                                    label.Width = 0;
                                    Grid spsTable = new Grid();
                                    var clmTable = new ColumnDefinition()
                                    {
                                        Width = new GridLength(1, GridUnitType.Star)
                                    };
                                    var rowTable = new RowDefinition()
                                    {
                                        Height = new GridLength(1, GridUnitType.Star),
                                    };
                                    var rowTable2 = new RowDefinition()
                                    {
                                        Height = new GridLength(1, GridUnitType.Star),
                                    };
                                    spsTable.ColumnDefinitions.Add(clmTable);
                                    spsTable.RowDefinitions.Add(rowTable);
                                    spsTable.RowDefinitions.Add(rowTable2);

                                    Grid.SetColumn(listview, 0);
                                    Grid.SetRow(listview, 1);

                                    // Add buttons in stackpanel
                                    var stackForButtons = new StackPanel();
                                    stackForButtons.Orientation = Orientation.Horizontal;
                                    stackForButtons.Margin = new Thickness(25, 10, 10, 10);
                                    // COmbo select

                                    // DataHelpers.instanc().GetMongoDataRange(optionLien,100)
                                    //var boxdataLien = await DataHelpers.instanc().GetMongoDataRange(type.Name, MaxLinesFetch);
                                    bool addCommandButtons = true;
                                    List<IDocument> boxdataLien = new List<IDocument>();
                                    if (typeof(IDocument).IsAssignableFrom(type) == false)
                                        addCommandButtons = false;

                                    if (addCommandButtons)
                                    {
 

                                    Tablebox = new ComboBox();
                                    HintAssist.SetHint(Tablebox, $"{_("Chercher")} {name}");
                                    //HintAssist.SetFloatingScale(Tablebox, 0.8);
                                    Tablebox.Style = App.Current.FindResource("MaterialDesignFloatingHintComboBoxWhiteTable") as Style;
                                    Tablebox.ItemsPanel = (ItemsPanelTemplate)Application.Current.FindResource("VSP");
                                    Tablebox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                                    Tablebox.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                                    Tablebox.Margin = new Thickness(0, 0, 10, 0);
                                    Tablebox.IsEditable = true;
                                    var tagTable = new ArrayList() { listview, type.Name, optionTable };
                                    Tablebox.Tag = tagTable;
                                    Tablebox.KeyUp += Tablebox_KeyUp;
                                        
                                        boxdataLien = DS.Generic(type.Name)?.GetAll()  ;//, MaxLinesFetch); IEn

                                    Tablebox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = boxdataLien, Mode = BindingMode.OneTime, IsAsync = true });
                                    Tablebox.SetBinding(ComboBox.SelectedValueProperty, new Binding { Source = this, Path = new PropertyPath("tableModel"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });
                                    Tablebox.SetBinding(ComboBox.DisplayMemberPathProperty, new Binding { Source = $"NameSearch" });

                                    Tablebox.SelectedValuePath = "Id";
                                    Tablebox.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    Tablebox.Width = 400;
                                    Tablebox.SelectionChanged += Tablebox_SelectionChanged;
                                    Tablebox.IsTextSearchEnabled = false;
                                    // btn Find or select
                                    Button btnSelect = new Button();
                                    btnSelect.Style = App.Current.FindResource("ToolButton") as Style;
                                    var stp = new StackPanel();
                                    stp.Orientation = Orientation.Horizontal;
                                    var icn = new PackIcon() { Kind = PackIconKind.DatabaseSearch };
                                    var textChercher = new TextBlock { Text = _("Chercher") };
                                    stp.Children.Add(icn);
                                    stp.Children.Add(textChercher);
                                    btnSelect.Content = stp;
                                    btnSelect.Margin = new Thickness(2);
                                    btnSelect.Padding = new Thickness(2);
                                    btnSelect.HorizontalAlignment = HorizontalAlignment.Right;
                                    var cls = new ArrayList() { listview, type.Name, optionTable };
                                    btnSelect.Tag = cls;
                                    btnSelect.IsEnabled = !isFreezed;
                                    btnSelect.Click += BtnSelect_Click; ;
                                    //  btnSelect.TouchDown += BtnSelect_Click; ;

                                    // btn créer
                                    Button btnNewModel = new Button();
                                    btnNewModel.Style = App.Current.FindResource("ToolButton") as Style;
                                    btnNewModel.Content = new PackIcon() { Kind = PackIconKind.Plus };
                                    btnNewModel.Margin = new Thickness(2);
                                    btnNewModel.Padding = new Thickness(2);
                                    btnNewModel.HorizontalAlignment = HorizontalAlignment.Right;
                                    btnNewModel.IsEnabled = !isFreezed;
                                    btnNewModel.Tag = new ArrayList() { type.Name, prop.Name, listview };
                                    btnNewModel.Click += BtnNewModel_Click; ;
                                    btnNewModel.TouchDown += BtnNewModel_Click; ;

                                    // Button add
                                    btnAddModel = new Button();
                                    btnAddModel.Style = App.Current.FindResource("ToolButton") as Style;
                                    btnAddModel.Content = new PackIcon() { Kind = PackIconKind.CheckCircle };
                                    btnAddModel.Margin = new Thickness(2);
                                    btnAddModel.Padding = new Thickness(2);
                                    btnAddModel.HorizontalAlignment = HorizontalAlignment.Right;
                                    var tag2 = new ArrayList() { listview, type.Name, optionTable };
                                    btnAddModel.IsEnabled = !isFreezed;
                                    btnAddModel.Tag = tag2;
                                    btnAddModel.Click += BtnAddModel_Click;
                                    btnAddModel.TouchDown += BtnAddModel_Click;

                                    // Button delete all
                                    Button btnDeleteall = new Button();
                                    btnDeleteall.Content = new PackIcon() { Kind = PackIconKind.DeleteForever };
                                    btnDeleteall.Style = App.Current.FindResource("ToolButton") as Style;
                                    btnDeleteall.Margin = new Thickness(2);
                                    btnDeleteall.Padding = new Thickness(2);
                                    btnDeleteall.HorizontalAlignment = HorizontalAlignment.Right;
                                    btnDeleteall.Tag = listview;
                                    btnDeleteall.IsEnabled = !isFreezed;

                                    // Button UP
                                    Button btnUP = new Button();
                                    btnUP.Content = new PackIcon() { Kind = PackIconKind.ArrowUpBoldCircle };
                                    btnUP.Style = App.Current.FindResource("ToolButton") as Style;
                                    btnUP.Margin = new Thickness(2);
                                    btnUP.Padding = new Thickness(2);
                                    btnUP.HorizontalAlignment = HorizontalAlignment.Right;
                                    btnUP.Tag = listview;
                                    btnUP.IsEnabled = !isFreezed;
                                    btnUP.Click += BtnDOWN_Click;
                                    btnUP.TouchDown += BtnDOWN_Click;

                                    // Button DOWN
                                    Button btnDOWN = new Button();
                                    btnDOWN.Content = new PackIcon() { Kind = PackIconKind.ArrowDownBoldCircle };
                                    btnDOWN.Style = App.Current.FindResource("ToolButton") as Style;
                                    btnDOWN.Margin = new Thickness(2);
                                    btnDOWN.Padding = new Thickness(2);
                                    btnDOWN.HorizontalAlignment = HorizontalAlignment.Right;
                                    btnDOWN.Tag = listview;
                                    btnDOWN.IsEnabled = !isFreezed;
                                    btnDOWN.Click += BtnUP_Click;
                                    btnDOWN.TouchDown += BtnUP_Click;

                                    stackForButtons.Children.Add(Tablebox);

                                    stackForButtons.Children.Add(btnAddModel);
                                    stackForButtons.Children.Add(btnSelect);
                                    stackForButtons.Children.Add(btnNewModel);
                                    stackForButtons.Children.Add(btnDeleteall);
                                    stackForButtons.Children.Add(btnUP);
                                    stackForButtons.Children.Add(btnDOWN);

                                        btnDeleteall.Click += Btnaddline_Click;
                                        btnDeleteall.TouchDown += Btnaddline_Click;

                                    }

                                    Grid.SetColumn(stackForButtons, 0);
                                    Grid.SetRow(stackForButtons, 0);
                                  
                                    spsTable.Children.Add(listview);
                                    spsTable.Children.Add(stackForButtons);
                                    spsTable.Margin = new Thickness(0);
                                    masterWrap.Children.Clear();
                                    masterWrap.Columns = 1;

                                    masterWrap.Children.Add(spsTable);
                                    //masterWrap.Children.Add(btnaddline);

                                    if (isFreezed)
                                    {
                                        listview.IsReadOnly = true;
                                    }
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            #endregion Champs table

                            //case ModelFieldType.LienFetch:
                            //    #region Champ read only

                            //    var allattLienFetch = attributes.Options.Split('>');
                            //    var propertyLienFetch = allattLienFetch[0];
                            //    var valueLienFetch = allattLienFetch[1];

                            //    TextBox tbLienFetch = new TextBox();

                            //    tbLienFetch.IsReadOnly = true;
                            //    tbLienFetch.Width = elementWidth;
                            //    HintAssist.SetHint(tbLienFetch, name);
                            //    tbLienFetch.Style = App.Current.FindResource("MaterialDesignFloatingHintTextBoxWhite") as Style;
                            //    if (isBold?.IsBod == true)
                            //        tbLienFetch.FontWeight = FontWeights.Bold;

                            //    tbLienFetch.Background = Brushes.WhiteSmoke;
                            //    myBinding = new Binding(prop.Name);
                            //    myBinding.Source = model;
                            //     myBinding.Mode = BindingMode.OneWayToSource;
                            //    myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                            //    tbLienFetch.SetBinding(TextBox.TextProperty, myBinding);

                            //    var id = model.GetType().GetProperty(propertyLienFetch)?.GetValue(model); // <= ObjecId
                            //    var sources = DataHelpers.GetById(mytypeAttr.type.Name, id);
                            //    var valueFetched = (sources as IDocument)?.GetType()?.GetProperty(valueLienFetch)?.GetValue(sources);
                            //    if(valueFetched != null)
                            //    {
                            //        tbLienFetch.Text = valueFetched?.ToString();

                            //    }
                            //    else
                            //    {
                            //        tbLienFetch.Text ="";
                            //    }
                            //    sp.Children.Add(tbLienFetch);
                            //    masterWrap.Children.Add(sp);
                            //    break;

                            //    #endregion
                            //    break;
                            case ModelFieldType.LienField:

                                #region Champs lien Field
                                try
                                {

                                    //optionLienField == lTierCommandeVente
                                    // lTierCommandeVente =model.lTierCommandeVente => Tier

                                    Border brField = new Border();
                                    brField.BorderThickness = new Thickness(1);
                                    brField.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AFBFC0");
                                    brField.CornerRadius = new CornerRadius(5);
                                    brField.Padding = new Thickness(1);
                                    brField.Background = Brushes.White;

                                    var allatt = attributes.Options.Split('>');
                                    var optionLienField = allatt[0];

                                    var boxLienField = new ComboBox();
                                    boxLienField.Name = prop.Name;
                                    HintAssist.SetHint(boxLienField, name);
                                    // HintAssist.SetFloatingScale(box, 0.8);

                                    boxLienField.Style = App.Current.FindResource("MaterialDesignFloatingHintComboBoxWhite") as Style;
                                    boxLienField.ItemsPanel = (ItemsPanelTemplate)Application.Current.FindResource("VSP");
                                    boxLienField.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                                    boxLienField.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                                    if (isBold?.IsBod == true)
                                        boxLienField.FontWeight = FontWeights.Bold;
                                    //  boxLienField.Margin = new Thickness(0);
                                    if (DoRefresh)
                                    {
                                        boxLienField.SelectionChanged += Box_SelectionChanged1;
                                    }
                                    //boxLienField.Padding = new Thickness(5, 2, 0, 0);
                                    // boxLienField.BorderThickness = new Thickness(0.5);
                                    // boxLienField.SelectionChanged += Box_LostFocus;
                                    boxLienField.IsEditable = true;

                                    Button btnReload = new Button();
                                    btnReload.Content = new PackIcon() { Kind = PackIconKind.Reload };
                                    //btnView.Style = App.Current.FindResource("MaterialDesignFloatingActionMiniDarkButton") as Style;
                                    btnReload.Style = App.Current.FindResource("ToolButton") as Style;
                                    btnReload.ToolTip = _("Recharger");
                                    btnReload.Margin = new Thickness(2);
                                    btnReload.Padding = new Thickness(2);
                                    btnReload.HorizontalAlignment = HorizontalAlignment.Right;
                                    btnReload.TouchDown += async (d, s) => { await Setup(SetupBypass: true); };

                                    // GOES HERE

                                    btnReload.Tag = new ArrayList { boxLienField, optionLienField, mytypeAttr, allatt, prop.Name };
                                    btnReload.Click += BtnReload_Click;

                                    brField.Width = elementWidth;
                                    boxLienField.Width = elementWidth - 35;

                                    StackPanel sp1Field = new StackPanel();
                                    sp1Field.Orientation = Orientation.Horizontal;

                                    sp1Field.Children.Add(boxLienField);
                                    sp1Field.Children.Add(btnReload);

                                    brField.Child = sp1Field;
                                    //if (boxLienField.SelectedItem == null)
                                    //    label.Content = "";

                                    var bindVisibilityLienFieldLabel = new Binding("LabelVisibility");
                                    bindVisibilityLienFieldLabel.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityLienFieldLabel.Converter = _LabelVisibilityConverter;
                                    bindVisibilityLienFieldLabel.Source = this;
                                    bindVisibilityLienFieldLabel.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                    bindVisibilityLienFieldLabel.ConverterParameter = boxLienField;
                                    label.SetBinding(Label.VisibilityProperty, bindVisibilityLienFieldLabel);

                                    sp.Children.Add(label);
                                    sp.Children.Add(brField);
                                    sp.Width = elementWidth;
                                    masterWrap.Children.Add(sp);
                                    BtnReload_Click(btnReload, null);

                                    var bindVisibilityLienField = new Binding("ElementToHide");
                                    bindVisibilityLienField.Mode = System.Windows.Data.BindingMode.OneWay;
                                    bindVisibilityLienField.Converter = _UiVisibilityConverter;
                                    bindVisibilityLienField.Source = this;
                                    bindVisibilityLienField.ConverterParameter = prop.Name;
                                    boxLienField.SetBinding(ComboBox.VisibilityProperty, bindVisibilityLienField);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                #endregion Champs lien Field

                                break;

                            case ModelFieldType.OpsButton:
                                try
                                {
                                    var longDescription = prop.GetCustomAttribute(typeof(LongDescriptionAttribute)) as LongDescriptionAttribute;

                                    MenuItem newOps = new MenuItem();
                                    newOps.Name = prop.Name;
                                    var optionnewOps = attributes.Options; // model name
                                    newOps.Height = 55;
                                    // newOps.Style = App.Current.FindResource("ToolBarAction") as Style;
                                    newOps.Background = Brushes.White;
                                    newOps.Icon = new PackIcon() { Kind = PackIconKind.CheckboxMarkedCircleOutline };
                                    if (longDescription != null)
                                    {
                                        StackPanel spops = new StackPanel();
                                        spops.Children.Add(new TextBlock() { Text = name, FontSize = 16,Foreground= (SolidColorBrush)new BrushConverter().ConvertFromString("#304ffe"),FontWeight=FontWeights   .DemiBold});
                                        spops.Children.Add(new TextBlock() { Text = longDescription.text, FontSize = 12, Foreground=Brushes.LightGray });
                                        newOps.Header = spops;
                                    }
                                    else
                                    {
                                        StackPanel spops = new StackPanel();
                                        spops.Children.Add(new TextBlock() { Text = name, FontSize = 16, Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#304ffe"), FontWeight = FontWeights.DemiBold });
                                        newOps.Header = spops;
                                       
                                    }

                                    
                                    newOps.Tag = optionnewOps;
                                    newOps.Click += NewOps_Click;
                                    newOps.TouchDown += NewOps_Click;
                                    // opeartionButtons.Orientation = Orientation.Horizontal;
                                    opeartionButtons.Add(newOps);
                                    addToPanel = false;
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.Button:
                                try
                                {

                                    Button newMenuButton = new Button();
                                    newMenuButton.Name = prop.Name;
                                    var optionButtonn = attributes.Options; // model name
                                    newMenuButton.Content = name;
                                    newMenuButton.Style = App.Current.FindResource("DetailButton") as Style;
                                    newMenuButton.Tag = optionButtonn;
                                    newMenuButton.Click += NewMenuButton_Click; ;
                                    newMenuButton.TouchDown += NewMenuButton_Click; ;
                                    border.Child = newMenuButton;
                                    sp.Children.Add(label);
                                    sp.Children.Add(border); 
                                    masterWrap.Children.Add(sp);

                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }
                                break;

                            case ModelFieldType.ReportControl:

                                try
                                {
                                    Grid spsTable = new Grid();
                                    var clmTable = new ColumnDefinition()
                                    {
                                        Width = new GridLength(1, GridUnitType.Star)
                                    };
                                    var rowTable = new RowDefinition()
                                    {
                                        Height = new GridLength(1, GridUnitType.Star),
                                    };
                                    //var rowTable2 = new RowDefinition()
                                    //{
                                    //    Height = new GridLength(1, GridUnitType.Star),
                                    //};
                                    spsTable.ColumnDefinitions.Add(clmTable);
                                    spsTable.RowDefinitions.Add(rowTable);
                                  //  spsTable.RowDefinitions.Add(rowTable2);

                                    ContentControl contentControl = new ContentControl();

                                    var reportControl = prop.GetValue(model) as UserControl;
                                    reportControl.DataContext = model;
                                    contentControl.Content = reportControl;

                                    Grid.SetColumn(contentControl, 0);
                                    Grid.SetRow(contentControl, 0);

                                   // spsTable.Children.Add(new TextBlock() { Text = name });
                                    spsTable.Children.Add(contentControl);

                                    masterWrap.Columns = 1;
                                    masterWrap.Children.Add(spsTable);
                                }
                                catch (Exception s)
                                {
                                    DataHelpers.ShowMessageError(s, name);
                                }

                                break;
                            default:
                                break;
                        }
                    }

                    if (addToPanel && (dontShow == null))
                    {
                        //if (isFreezed)
                        //{
                        //    foreach (UIElement item in masterWrap.Children)
                        //    {
                        //       var texts = item.FindChildren<TextBox>();
                        //        foreach (var t in texts)
                        //        {
                        //            var propertyElement = item.GetType().GetProperty("IsReadOnlyProperty");
                        //            if (propertyElement != null)
                        //            {
                        //                propertyElement.SetValue(item, true);
                        //            }
                        //        }
                        //    }
                        //}

                        //sp.Children.Add(label);

                        //sp.Children.Add(border);
                        //stackContent.Children.Add(sp);
                    }
                }
            }

            //  stackContent.Children.Add(grid);
            // stackContent.Children.Add(masterWrap);

            expander.Content = masterWrap;
            expander.FlowDirection = DataHelpers.GetFlowDirection;
            stackContent.Children.Add(expander);
            masterWrap = new UniformGrid();
            masterWrap.Margin = new Thickness(10, 5, 10, 5);
            masterWrap.Columns = 2;
            masterWrap.MaxHeight = 750;
            masterWrap.VerticalAlignment = VerticalAlignment.Stretch;
            masterWrap.HorizontalAlignment = HorizontalAlignment.Stretch;
            // masterWrap.Width = Double.NaN;
            masterWrap.Height = Double.NaN;
            masterWrap.Width = 900;

            if (isFreezed)
            {
                var elements = stackContent.FindChildren<UIElement>();
                foreach (var item in elements)
                {
                    if (item.GetType() == typeof(Expander))
                        continue;
                    if (item.GetType() == typeof(DataGrid))
                    {
                        (item as DataGrid).IsReadOnly = true;
                        continue;
                    }
                    item.IsHitTestVisible = false;
                    item.Focusable = false;
                }
            }

            expander = new Expander();
            expander.Padding = new Thickness(0, 0, 0, 0);
            expander.HorizontalAlignment = HorizontalAlignment.Stretch;
            expander.BorderThickness = new Thickness(0, 0, 1, 0);
            expander.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#D3D3D3");
            expander.IsExpanded = CollapseAll;
            expander.FlowDirection = DataHelpers.GetFlowDirection;
             
            if ((LinksVisible == Visibility.Visible || DocImageContent !=null) && (model as IDocument).EnsureIsSavedSubmit(true))
            {
                await CreateDashBoard();
            }

            NotifyOfPropertyChange("stackContent");
            NotifyOfPropertyChange("opeartionButtons");
            NotifyOfPropertyChange("DocStatus");
            NotifyOfPropertyChange("linkButtonsOps");
            NotifyOfPropertyChange("linkButtons");

            NotifyOfPropertyChange("model");
            this.NotifyOfPropertyChange("ElementToHide");

            FinishLoaded = true;
            SetupDone = true;
            //  _model.DoRefresh = false;
            (model as IDocument).AfterRefresh();

            ProgressValueVisible = false;
            NotifyOfPropertyChange("ProgressValueVisible");

            if (stackContent != null && stackContent.Children.Count > 1)
            {
                //ExpanderStatus.Clear();
                var expanders = stackContent.FindChildren<Expander>();
                foreach (var item in expanders)
                {
                    try { item.IsExpanded = ExpanderStatus[item.Header.ToString()]; } catch { }
                }
            }
            // stopwatch.Stop();
            //  DataHelpers.ShowMessage( $"Temp d'execution SETUP : {stopwatch.ElapsedMilliseconds} ms");
        }

        private async void DatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            NotifySpecificProperty((sender as Xceed.Wpf.Toolkit.DateTimePicker).Name);
            await Setup(500);
        }

        private async void DatePicker_ValueChanged1(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
         
        }

        private  void DatePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
           // NotifySpecificProperty((sender as Xceed.Wpf.Toolkit.DateTimePicker).Name);
            //  Setup().Wait();
          
        }

        public List<MenuItem> InfoCards { get; set; }

        private void SetupDocCards()
        {
            InfoCards = new List<MenuItem>();
            var cards = (model as IDocument).InfoCards;

            if(cards != null && cards .Any())
            {
                foreach (var item in cards)
                {
                    var sp = new StackPanel ();
                    sp.HorizontalAlignment = HorizontalAlignment.Left;
                   
                    sp.Children.Add(new TextBlock { Text = _(item.Key), FontSize= 16, Foreground=Brushes.Blue, FontWeight = FontWeights.Bold });
                    sp.Children.Add(new TextBlock { Text = item.Value, FontSize = 14 });

                    InfoCards.Add(new MenuItem() { Header = sp,Height = 55 });
                }

                NotifyOfPropertyChange("InfoCards");
            }

            //spCard.Children.Clear();// = new WrapPanel();

            //spCard.Orientation = Orientation.Horizontal;
            //spCard.Margin = new Thickness(0, 15, 0, 0);
            //spCard.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#EBEDEE");
            ////     spCard.FlowDirection = FlowDirection.RightToLeft;
            //// spCard2 = new WrapPanel();
            ////SETUP CARD ONE

            //if ((model as IDocument)?.DocCardOne != null)
            //{
            //    LinksVisible = Visibility.Visible;
            //    DocCardOne = new Card();
            //    DocCardOne.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#EBEDEE");
            //    var carOneContent = new StackPanel();
            //    carOneContent.Orientation = Orientation.Horizontal;

            //    // icon
            //    PackIcon pi = new PackIcon();
            //    try
            //    {
            //        pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), (model as IDocument).DocCardOne.BulletIcon);
            //    }
            //    catch (Exception s)
            //    {
            //        DataHelpers.Logger.LogError(s.Message);
            //        pi.Kind = PackIconKind.Exclamation;
            //    }
            //    pi.Width = 28;
            //    // pi.HorizontalAlignment = HorizontalAlignment.Center;
            //    pi.Height = 28;

            //    // pi.HorizontalContentAlignment = HorizontalAlignment.Center;
            //    pi.VerticalAlignment = VerticalAlignment.Center;
            //    pi.VerticalContentAlignment = VerticalAlignment.Center;
            //    carOneContent.HorizontalAlignment = HorizontalAlignment.Left;

            //    carOneContent.Children.Add(pi);

            //    var st = new StackPanel();
            //    carOneContent.VerticalAlignment = VerticalAlignment.Center;
            //    st.Orientation = Orientation.Vertical;

            //    DocOneName = (model as IDocument).DocCardOne.BulletTitle;
            //    DocOneValue = (model as IDocument).DocCardOne.BulletValue;
            //    var tbValue = new TextBlock() { Text = DocOneValue, FontSize = 14 };
            //    tbValue.SetBinding(TextBlock.TextProperty, new Binding("DocOneValue"));
            //    var tbName = new TextBlock() { Text = DocOneName, FontSize = 10 };
            //    tbName.SetBinding(TextBlock.TextProperty, new Binding("DocOneName"));

            //    st.Children.Add(tbValue);
            //    st.Children.Add(tbName);

            //    st.Margin = new Thickness(10, 2, 2, 2);
            //    st.Name = "stnale";
            //    //st.UpdateLayout();
            //    carOneContent.Children.Add(st);
            //    carOneContent.Background = Brushes.Transparent;
            //    DocCardOne.Width = 200;

            //    DocCardOne.Margin = new Thickness(15, 0, 0, 0);
            //    DocCardOne.Content = carOneContent;
            //    ShadowAssist.SetShadowDepth(DocCardOne, ShadowDepth.Depth0);
            //    ShadowAssist.SetShadowEdges(DocCardOne, ShadowEdges.All);
            //    DocCardOne.Padding = new Thickness(5);
            //    // DocCardOne.UpdateLayout();
            //    spCard.Children.Add(DocCardOne);
            //    NotifyOfPropertyChange("DocCardOne");
            //    NotifyOfPropertyChange("DocOneName");
            //    NotifyOfPropertyChange("DocOneValue");
            //}

            //// Card Tow
            ////SETUP CARD ONE

            //if ((model as IDocument)?.DocCardTow != null)
            //{
            //    LinksVisible = Visibility.Visible;
            //    DocCardTow = new Card();
            //    DocCardTow.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#EBEDEE");
            //    var carTowContent = new StackPanel();
            //    carTowContent.Orientation = Orientation.Horizontal;

            //    // icon
            //    PackIcon pi = new PackIcon();
            //    try
            //    {
            //        pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), (model as IDocument).DocCardTow.BulletIcon);
            //    }
            //    catch (Exception s)
            //    {
            //        DataHelpers.Logger.LogError(s.Message);
            //        pi.Kind = PackIconKind.Exclamation;
            //    }
            //    pi.Width = 28;
            //    pi.HorizontalAlignment = HorizontalAlignment.Center;
            //    pi.Height = 28;

            //    //  pi.HorizontalContentAlignment = HorizontalAlignment.Center;
            //    pi.VerticalAlignment = VerticalAlignment.Center;
            //    pi.VerticalContentAlignment = VerticalAlignment.Center;
            //    // carTowContent.HorizontalAlignment = HorizontalAlignment.Center;

            //    carTowContent.Children.Add(pi);

            //    var st = new StackPanel();
            //    st.Margin = new Thickness(10, 2, 2, 2);
            //    carTowContent.VerticalAlignment = VerticalAlignment.Center;
            //    st.Orientation = Orientation.Vertical;

            //    DocTowName = (model as IDocument).DocCardTow.BulletTitle;
            //    DocTowValue = (model as IDocument).DocCardTow.BulletValue;
            //    var tbValue = new TextBlock() { Text = DocTowValue, FontSize = 14 };
            //    tbValue.SetBinding(TextBlock.TextProperty, new Binding("DocTowValue"));
            //    var tbName = new TextBlock() { Text = DocTowName, FontSize = 10 };
            //    tbName.SetBinding(TextBlock.TextProperty, new Binding("DocTowName"));

            //    st.Children.Add(tbValue);
            //    st.Children.Add(tbName);

            //    carTowContent.Children.Add(st);
            //    carTowContent.Background = Brushes.Transparent;
            //    DocCardTow.Content = carTowContent;
            //    ShadowAssist.SetShadowDepth(DocCardTow, ShadowDepth.Depth0);
            //    ShadowAssist.SetShadowEdges(DocCardTow, ShadowEdges.All);
            //    DocCardTow.Padding = new Thickness(5);
            //    DocCardTow.Width = 200;
            //    DocCardTow.Margin = new Thickness(0);
            //    spCard.Children.Add(DocCardTow);
            //    NotifyOfPropertyChange("DocCardTow");
            //    NotifyOfPropertyChange("DocTowName");
            //    NotifyOfPropertyChange("DocTowValue");
            //}

            //// Card Tow
            ////SETUP CARD THREE

            //if ((model as IDocument)?.DocCardThree != null)
            //{
            //    LinksVisible = Visibility.Visible;
            //    DocCardThree = new Card();
            //    DocCardThree.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#EBEDEE");
            //    var carThreeContent = new StackPanel();
            //    carThreeContent.Orientation = Orientation.Horizontal;

            //    // icon
            //    PackIcon pi = new PackIcon();
            //    try
            //    {
            //        pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind),
            //            (model as IDocument).DocCardThree.BulletIcon);
            //    }
            //    catch (Exception s)
            //    {
            //        DataHelpers.Logger.LogError(s.Message);
            //        pi.Kind = PackIconKind.Exclamation;
            //    }
            //    pi.Width = 28;
            //    pi.HorizontalAlignment = HorizontalAlignment.Center;
            //    pi.Height = 28;

            //    //  pi.HorizontalContentAlignment = HorizontalAlignment.Center;
            //    pi.VerticalAlignment = VerticalAlignment.Center;
            //    pi.VerticalContentAlignment = VerticalAlignment.Center;
            //    // carThreeContent.HorizontalAlignment = HorizontalAlignment.Center;

            //    carThreeContent.Children.Add(pi);

            //    var st = new StackPanel();
            //    st.Margin = new Thickness(10, 2, 2, 2);
            //    carThreeContent.VerticalAlignment = VerticalAlignment.Center;
            //    st.Orientation = Orientation.Vertical;

            //    DocThreeName = (model as IDocument).DocCardThree.BulletTitle;
            //    DocThreeValue = (model as IDocument).DocCardThree.BulletValue;
            //    var tbValue = new TextBlock() { Text = DocThreeValue, FontSize = 14 };
            //    tbValue.SetBinding(TextBlock.TextProperty, new Binding("DocThreeValue"));
            //    var tbName = new TextBlock() { Text = DocThreeName, FontSize = 10 };
            //    tbName.SetBinding(TextBlock.TextProperty, new Binding("DocThreeName"));

            //    st.Children.Add(tbValue);
            //    st.Children.Add(tbName);

            //    //st.Children.Add(new TextBlock() { Text = (model as IDocument).DocCardThree.BulletValue, FontSize = 14 });
            //    //st.Children.Add(new TextBlock() { Text = (model as IDocument).DocCardThree.BulletTitle, FontSize = 10 });

            //    carThreeContent.Children.Add(st);
            //    carThreeContent.Background = Brushes.Transparent;
            //    DocCardThree.Content = carThreeContent;
            //    ShadowAssist.SetShadowDepth(DocCardThree, ShadowDepth.Depth0);
            //    ShadowAssist.SetShadowEdges(DocCardThree, ShadowEdges.All);
            //    DocCardThree.Padding = new Thickness(5);
            //    DocCardThree.Width = 200;
            //    DocCardThree.Margin = new Thickness(0);
            //    spCard.Children.Add(DocCardThree);
            //    NotifyOfPropertyChange("DocCardThree");
            //    NotifyOfPropertyChange("DocThreeValue");
            //    NotifyOfPropertyChange("DocThreeName");
            //}

            //// Card Tow
            ////SETUP CARD ONE

            //if ((model as IDocument)?.DocCardFor != null)
            //{
            //    LinksVisible = Visibility.Visible;
            //    DocCardFor = new Card();
            //    DocCardFor.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#EBEDEE");
            //    var carForContent = new StackPanel();
            //    carForContent.Orientation = Orientation.Horizontal;

            //    // icon
            //    PackIcon pi = new PackIcon();
            //    try
            //    {
            //        pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), (model as IDocument).DocCardFor.BulletIcon);
            //    }
            //    catch (Exception s)
            //    {
            //        DataHelpers.Logger.LogError(s.Message);
            //        pi.Kind = PackIconKind.Exclamation;
            //    }
            //    pi.Width = 28;
            //    pi.HorizontalAlignment = HorizontalAlignment.Center;
            //    pi.Height = 28;

            //    //  pi.HorizontalContentAlignment = HorizontalAlignment.Center;
            //    pi.VerticalAlignment = VerticalAlignment.Center;
            //    pi.VerticalContentAlignment = VerticalAlignment.Center;
            //    // carForContent.HorizontalAlignment = HorizontalAlignment.Center;

            //    carForContent.Children.Add(pi);

            //    var st = new StackPanel();
            //    st.Margin = new Thickness(10, 2, 2, 2);
            //    carForContent.VerticalAlignment = VerticalAlignment.Center;
            //    st.Orientation = Orientation.Vertical;

            //    DocForName = (model as IDocument).DocCardFor.BulletTitle;
            //    DocForValue = (model as IDocument).DocCardFor.BulletValue;
            //    var tbValue = new TextBlock() { Text = DocForValue, FontSize = 14 };
            //    tbValue.SetBinding(TextBlock.TextProperty, new Binding("DocForValue"));
            //    var tbName = new TextBlock() { Text = DocForName, FontSize = 10 };
            //    tbName.SetBinding(TextBlock.TextProperty, new Binding("DocForName"));

            //    st.Children.Add(tbValue);
            //    st.Children.Add(tbName);

            //    carForContent.Children.Add(st);
            //    carForContent.Background = Brushes.Transparent;
            //    DocCardFor.Content = carForContent;
            //    ShadowAssist.SetShadowDepth(DocCardFor, ShadowDepth.Depth0);
            //    ShadowAssist.SetShadowEdges(DocCardFor, ShadowEdges.All);
            //    DocCardFor.Padding = new Thickness(5);
            //    DocCardFor.Width = 200;
            //    DocCardFor.Margin = new Thickness(0);
            //    spCard.Children.Add(DocCardFor);
            //    NotifyOfPropertyChange("DocCardFor");

            //    NotifyOfPropertyChange("DocForValue");
            //    NotifyOfPropertyChange("DocForName");
            //}
        }
        private async void Tablebox_KeyUp(object sender, KeyEventArgs e)
        {
            if (isFreezed)
                return;

            if (e.Key == Key.Enter)
            {
                try
                {
                    var dic = (ArrayList)(sender as ComboBox).Tag;
                    if (tableModel == Guid.Empty || dic.Count < 3)
                    {
                         DataHelpers.ShowMessage(_("Selectionner une ligne à ajoutée, ou vérifier la déclaration"));
                        return;
                    }

                    var data = dic;
                    var s = data[0] as DataGrid;
                    await AddItemToTable(s, data[1].ToString(), data[2].ToString());
                    //var selected = s.GetValue(DataGrid.ItemsSourceProperty);

                    //var doc = DataHelpers.GetById(data[1].ToString(), tableModel);
                    //var mapped = doc.Map(data[2].ToString());

                    // Do AfterMap Function if exist

                    //(selected as IList).Add(mapped);

                    await Setup();
                }
                catch (Exception s)
                {
                    MessageQueue.Enqueue(s.Message + " Tablebox_KeyUp");
                    GlobalMsg = $"{_("Erreur")}: {s.Message}";
                    NotifyOfPropertyChange("GlobalMsg");
                    return;
                }
            }
            else
            {
                if (!(sender as ComboBox).IsDropDownOpen)
                    (sender as ComboBox).IsDropDownOpen = true;

                if (e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.Up && e.Key != Key.Down)
                {
                    var box = (sender as ComboBox);
                    var text = box.Text;

                    box.Items.Filter = ((a) =>
                    {
                        if (string.IsNullOrWhiteSpace(text))
                            return true;
                        if ((a as IDocument)?.NameSearch?.ToLower().Contains(box.Text?.ToLower()) == true)
                            return true;
                        return false;
                    });
                }

                return;
            }
            //var text = (sender as ComboBox).Text;
            ////(sender as ComboBox).IsDropDownOpen = true;

            //(sender as ComboBox).FindChild<TextBox>("PART_EditableTextBox").CaretIndex = text.Length;
            //CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView((sender as ComboBox).ItemsSource);

            //itemsViewOriginal.Filter = ((o) =>
            //{
            //    if (String.IsNullOrEmpty(text)) return true;
            //    else
            //    {
            //        if (((IDocument)o).Name.ToLower().Contains(text.ToLower())) return true;
            //        else return false;
            //    }
            //});

            //itemsViewOriginal.Refresh();
        }

        private void Tablebox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // DataHelpers.ShowMessage( "Valeur : "+tableModel);
        }

        private async void Tb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //TODO replace with actualiser
                NotifySpecificProperty((sender as TextBox).Name);
                await Setup();
            }
        }

        private void TbDevise_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
        }

        private void TbDevise_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void TbDevise_MouseDown1(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void TbDevise_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void TbDevise_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void TbReadOnly_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotifySpecificProperty((sender as TextBox).Name);
        }
    }

    internal class LabelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uielement = (parameter as Control);

            if (uielement != null)
            {
                if (uielement.GetType().GetProperties().Any(x => x.Name == "Text"))
                {
                    var valueur = uielement.GetType().GetProperty("Text").GetValue(uielement);
                    if (valueur == null || string.IsNullOrEmpty(valueur as string))
                        return Visibility.Hidden;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    internal class UiVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value : List<string> -> list properties to hide
            // param : property name

            var result = Visibility.Visible;
            var properties = value as HashSet<string>;
            var propertyName = parameter as string;
            if (properties != null)
            {
                if (properties.Contains(propertyName))
                {
                    result = Visibility.Hidden;
                }
            }

            //return visibility
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}