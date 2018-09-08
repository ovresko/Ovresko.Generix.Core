
using Ovresko.Generix.Core.Modules.Core.Module;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks; 
using Ovresko.Generix.Core.Pages;
using MongoDB.Bson;
using Ovresko.Generix.Core.Modules;
using StyletIoC;
using System.Linq.Expressions;

using Ovresko.Generix.Core.Pages.Template;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

using static Ovresko.Generix.Core.Translations.OvTranslate;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;
using LLibrary; 
using Ovresko.Generix.Core.Exceptions;
using System.Windows;
using Unclassified.TxLib;
using Ovresko.Generix.Datasource.Models;
using Ovresko.Generix.Datasource.Services.Queries;

namespace Ovresko.Generix.Core.Modules.Core.Data
{
    public class DataHelpers
    {

        //public static Window ParentWindow { get; set; }

        public static FlowDirection GetFlowDirection
        {
            get
            {
                if (Tx.CurrentThreadCulture?.Contains("ar") == true)
                    return FlowDirection.RightToLeft; 
                return FlowDirection.LeftToRight; 
            }
        }

        public static string LicenceType { get; set; }
        public static DateTime? Expiration { get; set; }
        public static string Customer { get; set; }

        public static ModuleErp GetModule(IDocument type)
        {
          
            return Modules?.FirstOrDefault(a => a.Libelle == type.CollectionName);
        }

        public static BackgroundWorker worker { get; set; } = new BackgroundWorker();


        //public static dynamic GetGenericData(Type type)
        //{
        //    try
        //    {
        //        Type d1 = typeof(GenericData<>);

                
        //        Type[] typeArgs = { type };               
        //        Type makeme = d1.MakeGenericType(typeArgs);
        //        dynamic generic = Activator.CreateInstance(makeme, null);

        //        return generic;
        //    }
        //    catch (Exception s)
        //    {
        //         DataHelpers.ShowMessage( s.Message);
        //        return null;

        //    }
        //}

        //public static dynamic GetGenericData(string concrete)
        //{
        //    //try
        //    //{

        //    //var type = Type.GetType($"Ovresko.Generix.Core.Modules.{concrete}");
        //    Type type;
        //    try
        //    {
        //        type = DataHelpers.GetTypesModule.Resolve(concrete];
        //    }
        //    catch
        //    {
        //        throw new ModuleClassNotFoundException(concrete);
        //    }


        //    Type d1 = typeof(GenericData<>);
        //        Type[] typeArgs = { type };
        //        Type makeme = d1.MakeGenericType(typeArgs);
        //        dynamic generic = Activator.CreateInstance(makeme, null) ;

        //        return generic;
        //    //}
        //    //catch (Exception s)
        //    //{
        //    //     DataHelpers.ShowMessageError( s,concrete);
        //    //    return null;
        //    //}
        //}

        

        public static GlobalTypes GetTypesModule = new GlobalTypes();

        public static dynamic CreateObject(string type)
        {
            Type t = null; 
            t = GetTypesModule.Resolve(type); 
            return Activator.CreateInstance(t); 
        }

        public async static  Task<IScreen> GetBaseViewModelScreen(Type type, IEventAggregator aggre, bool ForSelectOnly)
        {
            IScreen control = null;
           
              await  Execute.OnUIThreadAsync(() =>
                {
                    Type d1 = typeof(BaseViewModel<>);
                    Type[] typeArgs = { type };
                    Type makeme = d1.MakeGenericType(typeArgs);
                    dynamic baseViewModel = Activator.CreateInstance(makeme, new object[] { aggre, ForSelectOnly });

                    control = baseViewModel;
                    container.Get<ViewManager>().BindViewToModel(new BaseView(), control);
                    //var screnn = control as IScreen;
                    //screnn.AttachView(new BaseView());
                });
         
                
            return control;
          
          
        }

        public static IScreen GetBaseViewModelScreen(Type type, IEventAggregator aggre, bool ForSelectOnly, IEnumerable<IDocument> _list)
        {
            IScreen control = null;
            Execute.OnUIThreadAsync(() =>
            {
                Type d1 = typeof(BaseViewModel<>);
                var d = d1.GetConstructors();
                Type[] typeArgs = { type };
                Type makeme = d1.MakeGenericType(typeArgs);
                dynamic baseViewModel = Activator.CreateInstance(makeme, new object[] { aggre, ForSelectOnly, _list });

                control = baseViewModel;
                container.Get<ViewManager>().BindViewToModel(new BaseView(), control);
                

            });
            return control;
        }

        public static User ConnectedUser { get; set; }
        public static ShellViewModel Shell { get;  set; }
        public static IEventAggregator Aggre { get;  set; }

        

        public static ElvaSettings Settings
        {get;set;}

        public static string DbAdresse { get;  set; }
        public static string DbName { get;  set; }

        //public static IEnumerable<T> GetMongoData<T>(T collectionName)
        //{
        //    var tier = new MongoTier(Properties.Settings.Default.MongoServerSettings);
        //    var MongoDB = tier.GetDatabase("ElvaData");
        //    var collection = MongoDB.GetCollection<T>(collectionName.GetType().Name);

        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static IWindowManager windowManager;

        public static MessageBoxResult ShowMessage(string message,string caption = "",MessageBoxButton boxButton = MessageBoxButton.OK, MessageBoxImage img = MessageBoxImage.Information)
        {

            MessageBoxResult boxResult = MessageBoxResult.OK;

            if (windowManager == null)
                windowManager = container.Get<WindowManager>();


            Execute.OnUIThreadSync(() =>
            {
                boxResult = windowManager.ShowMessageBox(message, caption,boxButton, img);
            });

            return boxResult;
        }
        public static MessageBoxResult ShowMessageError(Exception message,string caption = "Erreur")
        {
            if (message?.GetType()?.IsAssignableFrom(typeof(ModuleClassNotFoundException)) == true)
            {

                //what you want to do when ThreadAbortException occurs   
                return MessageBoxResult.OK;
            }
            else
            {
                //do when other exceptions occur
                string captionValue = "Erreur";
                if (!string.IsNullOrEmpty(caption))
                    captionValue = caption;

                if (windowManager == null)
                    windowManager = container.Get<WindowManager>();


                MessageBoxResult boxResult = MessageBoxResult.OK;
                Execute.OnUIThreadSync(() =>
                {
                    boxResult = windowManager.ShowMessageBox($@"{message?.Message}
_______________
{message?.InnerException?.Message}", captionValue);
                });

                return boxResult;
            }
          
        }
        public static string imgBg;

        private static DataHelpers _instance { get; set; }

        private static IEnumerable<ModuleErp> _Modules;

        public static IEnumerable<ModuleErp> Modules
        {
            get { return _Modules; }
            set { _Modules = value;
                ModulesSearch = value.Where(a => !a.ClassName.Contains(_("Traduction")));
            }
        }

       // public static IEnumerable<ModuleErp> Modules { get; set; } = new List<ModuleErp>();



        public static IEnumerable<ModuleErp> ModulesSearch { get; set; } = new List<ModuleErp>();





        public static L Logger { get;  set; }
        public static CultureInfo CurrentCulture { get;  set; }
        public static CultureInfo CurrentUICulture { get;  set; }
        //public static string LangeDefault { get; set; }
        public static List<string> NotAllowedProperties { get;  set; }
        // public static ServiceContainer Ioc { get; internal set; }

        public static DataHelpers instanc()
        {
            if (_instance == null)
                _instance = new DataHelpers();
            return _instance;
        }

        public static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate must be greater than or equal to startDate");

            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }
        public static IEnumerable<DateTime> GetDateRangeByMonth(DateTime startDate, DateTime endDate)
        { 
            if (endDate < startDate)
                throw new ArgumentException("endDate must be greater than or equal to startDate");

            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddMonths(1);
            }
        }
        //public static void ClearData(string collection = null)
        //{
        //    try
        //    {

        //        if (collection == null)
        //        {
        //            DS.db.DropAllCollections(DbName);
        //            return;
        //        }

        //        DS.db.DropCollection(collection);
        //        return;
        //    }
        //    catch (Exception s)
        //    {
        //        Logger.LogError(s);
        //        throw s;
        //    }
        //}

        //public static async Task<IDocument> GetByIdV2(Type type, Guid id)
        //{

        //    MethodInfo method = typeof(DataHelpers).GetMethod("GetByIdAsync");
        //    MethodInfo genericMethod = method.MakeGenericMethod(type);

        //    IDocument r = null;
        //    try
        //    {
        //        if(id.HasValue && id != Guid.Empty)
        //            r = await (genericMethod.Invoke(null, new object[] { id.Value }) as Task<IDocument>);
        //    }
        //    catch (Exception s)
        //    {
        //        Console.WriteLine(s.Message);
        //    }

        //    return r;
        //}


        //public static async Task<T> GetByIdAsync<T>(Guid id) where T:IDocument
        //{
        //    try
        //    {
        //        var db = DS.db;
        //        var r = await db.GetAllAsync<T>(a => a.Id == id);
        //        return r.FirstOrDefault();
        //    }
        //    catch 
        //    {
        //        return null;    
        //    }
        //}

        public static int GetDays(DateTime? doc)
        {
            try
            {
                return (int)(DateTime.Today - doc)?.TotalDays;
            }
            catch
            {
                return -1;
            }
        }

        public static int GetDays(DateTime? debut, DateTime? fin)
        {
            try
            {
                return (int)(fin - debut)?.TotalDays;
            }
            catch
            {
                return -1;
            }
        }

        public static string GetPeriodeString(DateTime debut, DateTime end)
        {
            var days = new AgeHelper(debut, end);
            if (days.Years == 0)
            {
                if (days.Months == 0 && days.TotalDays > 0)
                {
                    return $"{days.Days} {_("jours")}";
                }
                else if (days.TotalDays == 0)
                {
                    return _($"Aujourd'hui");
                }
                else
                {
                    return $"{days.Months} {_("mois")},{days.Days} {_("jours")}";
                }
            }
            else
            {
                return $"{days.Years} {_("ans")} et {days.Months} {_("mois")}";
            }

            return "";
        }

        //public static async Task<IEnumerable<IDocument>> GetMongoData(string concrete)
        //{
        //    //  var type = Type.GetType($"Ovresko.Generix.Core.Modules.{concrete}");
        //    Type type;
        //    try
        //    {
        //        type = DataHelpers.GetTypesModule.Resolve(concrete];
        //    }
        //    catch
        //    {
        //        throw new ModuleClassNotFoundException(concrete);
        //    }

        //    var generic = GetGenericData(concrete);

        //    var methds = (generic.GetType() as Type).GetMethod("FindAllAsync");
        //    Task<IEnumerable<IDocument>> res = methds.Invoke(generic ,null);
        //    return await res;

        //  //  return await GetGenericData(type).FindAllAsync();
        //    // return await  FilterEngine.GetMongoData(concrete);
        //}


        //public static IEnumerable<IDocument> GetMongoData(string concrete, string field, object value, bool strict,int count)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(concrete))
        //            throw new Exception("Collection name is not valide");

        //        var generic = GetGenericData(concrete);
        //        //Type t = generic.GetExpressionForSearch();

        //        var methds = (generic.GetType() as Type).GetMethod("Find", new Type[] { typeof(string), typeof(object), typeof(bool), typeof(int) });
        //        IEnumerable<IDocument> res = methds.Invoke(generic, new object[] { field, value, strict,count });
        //        return res;
        //        //var result = generic.Find(field, value, strict);

        //        //return result;
        //    }
        //    catch (Exception s)
        //    {
        //        DataHelpers.ShowMessage(s.Message);
        //        return null;
        //    }
        //    //return FilterEngine.GetMongoData(concrete, field, value, strict);

        //}

        //// collect / Vache / Name
        //public static IEnumerable<IDocument> GetMongoData(string concrete, string field, object value, bool strict)
        //{
        //    //try
        //    //{
        //        if (string.IsNullOrWhiteSpace(concrete))
        //            throw new Exception("Collection name is not valide");

        //        var generic = GetGenericData(concrete);
        //        //Type t = generic.GetExpressionForSearch();

        //        var methds = (generic.GetType() as Type).GetMethod("Find",new Type[] { typeof(string), typeof(object), typeof(bool) });
        //        IEnumerable<IDocument> res = methds.Invoke(generic, new object[] { field, value, strict });
        //        return res;
        //        //var result = generic.Find(field, value, strict);
               
        //        //return result;
        //    //}
        //    //catch (Exception s)
        //    //{
        //    //     DataHelpers.ShowMessage( s.Message);
        //    //    return null;
        //    //}
        //    //return FilterEngine.GetMongoData(concrete, field, value, strict);

        //}

        ////    return collection.Find(a => true).ToEnumerable();
        ////}
        //public static IEnumerable<IDocument> GetMongoDataSync(string concrete)
        //{
        //    //var type = Type.GetType($"Ovresko.Generix.Core.Modules.{concrete}");
        //    Type type;

        //    try
        //    {
        //        type = DataHelpers.GetTypesModule.Resolve(concrete];
        //    }
        //    catch
        //    {
        //        throw new ModuleClassNotFoundException(concrete);
        //    }
        //    var generic = GetGenericData(type);

        //    var metGetCount = (generic.GetType() as Type).GetMethod("FindAll");
        //    return metGetCount.Invoke(generic, null);
        //   // return GetGenericData(type).FindAll();
        //}

        public static StyletIoC.IContainer container;
        public static Process mongod;
        public static Process restheart;
        private static MethodInfo metGetByIdNullable; 

        public static List<string> GetSelectData(string option)
        {
            switch (option)
            {
                case "Sexe":
                    return new List<string>()
                    { 
                       _( "Homme"),
                       _( "Femme")
                    };

                    break;
                case "MessageStatus":
                    return new List<string>()
                    { 
                       _( "En attente"),
                       _( "Vue / terminée")
                    };

                    break;

                case "StatusProjet":
                    return new List<string>()
                    {"",
                       _( "En cours"),
                       _( "Terminée"),
                       _( "Annulée")
                    };

                    break;
                 case "StatusFacture":
                    return new List<string>()
                    {"",
                        _("Brouillon"),
                       _( "À Délivrée"),
                       _( "À Payée"),
                       _( "Terminée"),
                       _( "Annulée")
                    };

                case "StatusCommande":
                    return new List<string>()
                    {"",
                       _( "Brouillon"),
                       _( "À Délivrée"),
                       _( "À Facturée"),
                       _( "À Payée"),
                       _( "Terminée"),
                       _( "Annulée")
                    };

                case "TypeEcritureCompte":
                    return new List<string>()
                    {
                       _( "Recevoir"),
                       _( "Payer"),
                       _("Réconciliation"),
                        _("Transfert interne")
                    };

                case "ModePaiement":
                    return new List<string>()
                    {
                       _( "Espèces"),
                       _( "Chèque"),
                       _( "Virement bancaire")
                    };


                case "TypeEcritureJournal":
                    return new List<string>()
                    {
                       _( "Facture de vente"),
                       _( "Facture d'achat"),
                       _( "Entrée Paiement"),
                       _( "Sortie Paiement"),
                       _( "TVA Collectée"),
                       _( "TVA Remboursée"),
                       _( "Retour vente"),
                       _( "Retour d'achat")
                    };
                case "TypeEcritureStock":
                    return new List<string>()
                    {"",
                      _(  "Sortie de Matériel"),
                       _( "Réception Matériel"),
                       _( "Transfert de Matériel")
                      // _( "Transfert de Matériel pour la Fabrication"),
                      //_(  "Fabrication"),
                      //_(  "Ré-emballer"),
                      // _( "Sous-traiter")
                    };
                case "TypeColumn":
                    return new List<string>()
                    {"",
                        "Text",
        "Date",
        "Devise",
        "Numero",
        "Select",
        "Check",
        "TextLarge",
        "Lien",
        "LienField",
        "LienButton",
        "LienFetch",
        "ReadOnly",
        "Separation",
        "FiltreText",
        "FiltreDate",
        "Image",
        "ImageSide",
        "Table",
        "WeakTable",
        "Button",
        "OpsButton",
        "BaseButton"
                    };

                case "UniteMesure":
                    return new List<string>()
                    {"",
                      _(  "Unité"),
                      _(  "Kg"),
                      _(  "Paires"),
                      _(  "CM"),
                      _(  "m"),
                      _(  "m²"),
                      _(  "Boite"),
                       _( "Carton")
                    };

               

                case "CatégorieArticle":
                    return new List<string>()
                    {
                        _("N/A"),
                       _( "Produit"),
                       _( "Service"),
                      _(  "Abonnement")
                    };
                case "MethodeAppro":
                    return new List<string>()
                    {_("N/A"),
                      _(  "Achat"),
                      _(  "Stock"),
                       _( "Produire")
                    };

                case "TypeVentilation":
                    return new List<string>()
                    {
                      _(  "Les anciens d'abord"),
                       _( "Les nouveaux d'abord")
                    };
                case "EtatPaiement":
                    return new List<string>()
                    {
                      _(  "Montant non payé"),
                       _( "Payé partiellement"),
                      _(  "Montant payé")
                    };

                case "TypeClient":
                    return new List<string>()
                    {"",
                       _( "Personne"),
                      _(  "Société")
                    };

                default:
                    return null;
                    break;
            }
        }

        //public static IDocument GetById(string typename, Guid Id)
        //{
        //    var generic = GetGenericData(typename); 

        //    var GetByIdNullable = (generic.GetType() as Type).GetMethod("GetByIdNullable");
        //    return GetByIdNullable.Invoke(generic, new object[] { Id }); 
        //}


        //public static IDocument GetById(Type type, Guid Id)
        //{
        //    var generic = GetGenericData(type);

        //    metGetByIdNullable = (generic.GetType() as Type).GetMethod("GetByIdNullable");
        //    IDocument res = metGetByIdNullable.Invoke(generic, new object[] { Id });
        //    return res;
        //    //var attre = (generic.GetType() as Type).GetProperties();
        //    //var result = generic.GetByIdNullable((dynamic) Id);
        //   // return result;
        //}


        public static bool fun(IDocument t)
        {
            return true;
        }

        //public Task<List<T>> GetMongoDataAll<T>() where T : IDocument
        //{
        //    return Task.Run(() => { return DS.db.GetAll<T>().ToList(); });
        //}

        //public async Task<IEnumerable<dynamic>> GetMongoDataRange(string type,int pageNumber)
        //{
        //    var generic = GetGenericData(type); 

        //    var GetRange = (generic.GetType() as Type).GetMethod("GetRange");
        //    Task<IEnumerable<dynamic>> res = GetRange.Invoke(generic, new object[] { pageNumber });

        //    return ( (await res) as IEnumerable<dynamic>);

        //}

        //public async Task<List<T>> GetMongoDataPaged<T>(int pageNumber, int selectedCOunt) where T : IDocument
        //{
        //    var result = await DS.db.GetPaginatedAsync<T,Guid>(a => true, ((pageNumber - 1) * selectedCOunt), selectedCOunt);
        //    return result.OrderByDescending(a => a.AddedAtUtc).ToList();
        //    //return await DS.db.GetPaginatedAsyncA<T>(a => true, ((pageNumber - 1) * selectedCOunt), selectedCOunt);

        //}

        //public async Task<List<T>> GetMongoDataFilterPaged<T>(string lowerName, int pageNumber, int selectedCOunt) where T : IDocument
        //{

        //    return await DS.db.GetPaginatedAsyncA<T>(a => a.NameSearch.ToLower().Contains(lowerName), ((pageNumber - 1) * selectedCOunt), selectedCOunt);

        //}

        //public async Task<long> GetMongoDataCount<T>(Expression<Func<T, bool>> filter) where T : IDocument
        //{
        //    return await DS.db.CountAsync<T>(filter);

        //}

        //public async Task<long> GetMongoDataCount(string type,bool IsTypeFullName = false)
        //{

        //    //Type _type;
        //    //if (IsTypeFullName)
        //    //{
        //    //    _type = DataHelpers.GetTypes[type];// Type.GetType(type);
        //    //}
        //    //else
        //    //{
        //    //    _type = DataHelpers.GetTypes[type]; ;// Type.GetType($"Ovresko.Generix.Core.Modules.{type}");
        //    //}

        //    var generic = GetGenericData(type);

        //    var metGetCount = (generic.GetType() as Type).GetMethod("GetCount");
        //    Task<long> res = metGetCount.Invoke(generic, null);

        //    return await res;

        //}

        //public async Task<List<T>> GetData<T>(Expression<Func<T, bool>> filter) where T : IDocument
        //{
        //    return await DS.db.GetAllAsync<T>(filter);

        //}


        /// <summary>
        /// /////
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>

        //public static async Task<List<dynamic>> GetDataStatic(Type type, Expression<Func<dynamic, bool>> filter)
        //{

        //    MethodInfo method = typeof(DataHelpers).GetMethod("GetDataStaticGeneric");
        //    MethodInfo genericMethod = method.MakeGenericMethod(type);

        //    var s = (genericMethod.Invoke(null, new object[] { filter }));

        //    return await (s as Task<List<dynamic>>);

        //}

        //public static List<T> GetDataStaticGeneric<T>(Expression<Func<T, bool>> filter) where T : IDocument
        //{
        //    var s = DS.db.GetAllAsync<T>(filter).Result;
        //    return s;
        //}

        //public List<T> GetDataSync<T>(Expression<Func<T, bool>> filter) where T : IDocument
        //{
        //    return DS.db.GetAll<T>(filter);

        //}
        //public async Task<T> GetOne<T>(Expression<Func<T, bool>> filter) where T : IDocument
        //{
        //    return await DS.db.GetOneAsync<T>(filter); 
        //}


        //public Task<long> GetMongoDataCount<T>() where T : IDocument
        //{
        //    return DS.db.CountAsync<T>(a => true);

        //}

        //public Task<List<T>> GetMongoDataSearch<T>(string nameSearch) where T : IDocument
        //{
        //    return DS.db.GetAllAsync<T>(a => a.Name.ToLower().Contains(nameSearch.ToLower()));
        //}

        //public static void MapPropertiesRef(ref IDocument selected, dynamic original,string fetchValue)
        //{ 
        //    selected = MapProperties(selected, original,true, fetchValue);
        //}

        public static IDocument MapProperties(IDocument toDocument, dynamic fromDocument, bool OnlyIfNull = false, string selectedProeprty = null)
        {
            try
            {

                var notAlloewd = DataHelpers.NotAllowedProperties;
                notAlloewd.Add("Series");

                List<PropertyInfo> propsSelected = new List<PropertyInfo>(); 
                if (!string.IsNullOrWhiteSpace(selectedProeprty))
                {
                    propsSelected.Add(toDocument.GetType().GetProperty(selectedProeprty));
                }
                else
                {
                    propsSelected = toDocument.GetType().GetProperties().Where(z => z.GetCustomAttribute(typeof(DisplayNameAttribute)) != null).ToList();

                }

                var propsModel = (fromDocument.GetType().GetProperties() as PropertyInfo[]).Select(z => z.Name);

                var commun = propsSelected.Where(a => propsModel.Contains(a.Name)
                && !notAlloewd.Contains(a.Name)
                && a.CanWrite);

                if (commun != null)
                {

                    foreach (var prop in commun)
                    {
                        try
                        {
                            var propModel = (fromDocument as IDocument).GetType().GetProperty(prop.Name);
                            Type typeModel = propModel?.PropertyType;
                            Type typeSelected = prop.PropertyType;
                            if (typeSelected.Equals(typeModel) == true)
                            {
                                var value = propModel.GetValue(fromDocument);
                                dynamic meValue = prop.GetValue(toDocument);
                                if (OnlyIfNull)
                                {
                                    var type = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                                    if((type != null && type.FieldType == ModelFieldType.ReadOnly) )
                                        prop.SetValue(toDocument, value);
                                }
                                else
                                {
                                    prop.SetValue(toDocument, value);
                                }
                              
                            }
                        }
                        catch (Exception s)
                        {
                            DataHelpers.Logger.LogError(s);
                             DataHelpers.ShowMessage( s.Message);
                            continue;
                        }
                    }
                }

                //selected = original.MapRefsTo(selected);

                return toDocument;
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
                 DataHelpers.ShowMessage( s.Message);
                return null;
            }

            
        }




        #region DATA METHODS



        #endregion


        public static IEnumerable<KeyValuePair<string,string>> GetEmptyStrings(Dictionary<string,string> strings)
        { 
           return strings.Where(a => string.IsNullOrEmpty(a.Value));
           
        }
        public static bool IsEmty(string value)
        {
            return string.IsNullOrEmpty(value);
        }



    }

    //public class RawPrinterHelper
    //{
    //    // Structure and API declarions:
    //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    //    public class DOCINFOA
    //    {
    //        [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
    //        [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
    //        [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
    //    }
    //    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    //    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool ClosePrinter(IntPtr hPrinter);

    //    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    //    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool EndDocPrinter(IntPtr hPrinter);

    //    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool StartPagePrinter(IntPtr hPrinter);

    //    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool EndPagePrinter(IntPtr hPrinter);

    //    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

    //    // SendBytesToPrinter()
    //    // When the function is given a printer name and an unmanaged array
    //    // of bytes, the function sends those bytes to the print queue.
    //    // Returns true on success, false on failure.
    //    public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
    //    {
    //        Int32 dwError = 0, dwWritten = 0;
    //        IntPtr hPrinter = new IntPtr(0);
    //        DOCINFOA di = new DOCINFOA();
    //        bool bSuccess = false; // Assume failure unless you specifically succeed.

    //        di.pDocName = "My C#.NET RAW Document";
    //        di.pDataType = "RAW";

    //        // Open the printer.
    //        if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
    //        {
    //            // Start a document.
    //            if (StartDocPrinter(hPrinter, 1, di))
    //            {
    //                // Start a page.
    //                if (StartPagePrinter(hPrinter))
    //                {
    //                    // Write your bytes.
    //                    bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
    //                    EndPagePrinter(hPrinter);
    //                }
    //                EndDocPrinter(hPrinter);
    //            }
    //            ClosePrinter(hPrinter);
    //        }
    //        // If you did not succeed, GetLastError may give more information
    //        // about why not.
    //        if (bSuccess == false)
    //        {
    //            dwError = Marshal.GetLastWin32Error();
    //        }
    //        return bSuccess;
    //    }

    //    public static bool SendFileToPrinter(string szPrinterName, string szFileName)
    //    {
    //        // Open the file.
    //        FileStream fs = new FileStream(szFileName, FileMode.Open);
    //        // Create a BinaryReader on the file.
    //        BinaryReader br = new BinaryReader(fs);
    //        // Dim an array of bytes big enough to hold the file's contents.
    //        Byte[] bytes = new Byte[fs.Length];
    //        bool bSuccess = false;
    //        // Your unmanaged pointer.
    //        IntPtr pUnmanagedBytes = new IntPtr(0);
    //        int nLength;

    //        nLength = Convert.ToInt32(fs.Length);
    //        // Read the contents of the file into the array.
    //        bytes = br.ReadBytes(nLength);
    //        // Allocate some unmanaged memory for those bytes.
    //        pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
    //        // Copy the managed byte array into the unmanaged array.
    //        Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
    //        // Send the unmanaged bytes to the printer.
    //        bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
    //        // Free the unmanaged memory that you allocated earlier.
    //        Marshal.FreeCoTaskMem(pUnmanagedBytes);
    //        return bSuccess;
    //    }
    //    public static bool SendStringToPrinter(string szPrinterName, string szString)
    //    {
    //        IntPtr pBytes;
    //        Int32 dwCount;
    //        // How many characters are in the string?
    //        dwCount = szString.Length;
    //        // Assume that the printer is expecting ANSI text, and then convert
    //        // the string to ANSI text.
    //        pBytes = Marshal.StringToCoTaskMemAnsi(szString);
    //        // Send the converted ANSI string to the printer.
    //        SendBytesToPrinter(szPrinterName, pBytes, dwCount);
    //        Marshal.FreeCoTaskMem(pBytes);
    //        return true;
    //    }
    //}
}