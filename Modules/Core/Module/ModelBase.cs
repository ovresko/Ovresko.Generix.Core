using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Pages.Export;
using Ovresko.Generix.Core.Pages.Helpers;
using Ovresko.Generix.Core.Pages.Reports;
using MaterialDesignThemes.Wpf;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes; 
using Stylet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; 
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Ovresko.Generix.Core.Exceptions;
using Ovresko.Generix.Datasource.Models;
using Ovresko.Generix.Datasource.Services.Queries;

namespace Ovresko.Generix.Core.Modules.Core.Module
{
    [BsonIgnoreExtraElements(true, Inherited = true)]
    public abstract class ModelBase<T> : Document, IModel where T : Document, new()
    {
       // public override string Name { get; set; } 

        private Type type; 

        public   virtual  T getInstance()
        {
            return new T();
        }

        public ModelBase()
        {
            Name = _("Nouveau"); 
            type = typeof(T);
            this.Version++;
            
        }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public override Action<Action> PropertyChangedDispatcher { get => base.PropertyChangedDispatcher; set => base.PropertyChangedDispatcher = value; }

        public void Open(OpenMode mode = OpenMode.Attach)
        {
            switch (mode)
            {
                case OpenMode.Attach:
                    DataHelpers.Shell.OpenScreenAttach(this, this.Name);
                    break;
                case OpenMode.Detach:
                    DataHelpers.Shell.OpenScreenDetach(this, this.Name).Wait();
                    break;
                default:
                    DataHelpers.Shell.OpenScreenAttach(this, this.Name);
                    break;
            }
        }
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public bool DoRefresh { get; set; }
        public async virtual Task  NotifyUpdates()
        {
           await Execute.PostToUIThreadAsync(() =>
            {

            var props = GetType().GetProperties(); 
            this.PropertyChangedDispatcher = Execute.OnUIThread;
            foreach (var item in props.Reverse())
            {
                try
                { 
                    var display = item.GetAttribute<DisplayNameAttribute>(false) as DisplayNameAttribute;                  
                    if (display != null || item.Name.ContainsIgniorCase("card"))
                    {
                        this.NotifyOfPropertyChange(item.Name);
                    }
                }
                catch
                {
                }
            }

                Execute.PostToUIThread(() => { OnNotifyUpdates(); });
                //AddFetch(this);


            });
        }

        public virtual void NotifyUpdates(string source)
        { 
                try
                {
                   // this.NotifyOfPropertyChange(source);
                   Execute.PostToUIThread(() => { this.OnNotifyUpdates(source); });
                }
                catch
                {
                }
            

            OnNotifyUpdates();
            //AddFetch(this);
        }

        public virtual void OnNotifyUpdates()
        { }
        public virtual void OnNotifyUpdates(string propertyName)
        { }

        public long Index { get; set; }



        

         

        public override string DefaultTemplate { get { return (MyModule() as ModuleErp)?.DefaultTemplateName; } }


        [LiteDB.BsonIgnore]
        [BsonIgnore]
        private string _NameField;
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual string NameField
        {
            get {
                var v = (MyModule() as ModuleErp)?.NameFieldEntity;
                return (string.IsNullOrWhiteSpace(v)) ? "Id" : (MyModule() as ModuleErp)?.NameFieldEntity;
            }
            set { _NameField = value; }
        }


        //public async static Task<T> GetByIdBase(Guid id)
        //{
        //    return await DS.db.GetOneAsync<T>(a => a.Id == id); 
        //}

       
        public void SetSeries()
        {
            var index = (MyModule() as ModuleErp);
            bool named = false;
            var propert = GetType().GetProperty(this.NameField);
            var name = propert?.GetValue(this);
            GenericName = name?.ToString(); // ZEMZEM

            if (index != null)
            {
                var usedSeries = GuidParser.Convert( this.GetType().GetProperty("Series")?.GetValue(this)  );
                if (usedSeries != null && usedSeries != Guid.Empty)
                {
                    var serie = index.SeriesNames.FirstOrDefault(a => a.Id == usedSeries);
                    if (serie != null)
                    {
                        string finaluffix = serie?.Sufix;
                        int? trailingZeroz = 5;
                        if (finaluffix.Contains("#") == true)
                        {
                            trailingZeroz = finaluffix.Count(a => a == '#');
                            finaluffix= finaluffix.Replace("#", "");
                        }
                        if (finaluffix.Contains("MM") == true)
                        {
                            finaluffix = finaluffix.Replace("MM", this.AddedAtUtc.ToString("MM"));
                        }
                        if (finaluffix.Contains("AAAA") == true)
                        {
                            finaluffix = finaluffix.Replace("AAAA", this.AddedAtUtc.ToString("yyyy"));
                        }
                        if (finaluffix.Contains("AA") == true)
                        {
                            finaluffix = finaluffix.Replace("AA", this.AddedAtUtc.ToString("yy"));
                        }
                        if (finaluffix.Contains("JJ") == true)
                        {
                            finaluffix = finaluffix.Replace("JJ", this.AddedAtUtc.ToString("dd"));
                        }
                       
                        if (isLocal)
                        {
                           
                            this.Index = serie.Indexe++;                           
                            this.Name = $"{finaluffix}{this.Index.ToString($"D{trailingZeroz}")}";
                            (index as IModel).Save();
                            named = true;
                        }
                        else
                        {
                            // index==0 means first update 
                            if (this.Index > 0)
                            {
                                
                                this.Name = $"{finaluffix}{this.Index.ToString($"D{trailingZeroz}")}";
                                (index as IModel).Save();
                                named = true;
                            }
                            else
                            {
                                this.Index = serie.Indexe++;
                                this.Name = $"{finaluffix}{this.Index.ToString($"D{trailingZeroz}")}";
                                (index as IModel).Save();
                                named = true;
                            }
                        }
                      
                    }
                }
            }
            if (!named)
            {               
                this.Name = GenericName;
                NameSearch = $"{Name}";
            }else if(NameField == "Id")
            {
                NameSearch = Name;
            }
            else
            {
                NameSearch = $"{Name} - {GenericName}";
               // GenericName = this.Name; //TR-0000055
            }
        }
        public virtual bool Save()
        {

            if (this.isLocal)
            {
                if (FrameworkManager.CanAdd(type))
                {
                    try
                    {
                        BeforeSave();
                        SetSeries();
                      
                        this.isLocal = false;
                        DS.db.AddOne<T>(this as T);
                        AfterSave();
                        return true;
                    }
                    catch (Exception s)
                    {
                        DataHelpers.Logger.LogError(s);
                        DataHelpers.ShowMessage( s.Message);
                        return false;
                    }
                }
                DataHelpers.ShowMessage("Vous n'avez pas l'autorisation pour créer!", "Action non autorisée");
                 return false;
            }
            else
            {
                if (FrameworkManager.CanSave(type))
                {
                    try {

                        BeforeEdit();
                        if(this.DocStatus != 1)
                            SetSeries();
                   
                    if(DocStatus == 2)
                        this.DocStatus = 0;
                    var res = DS.db.UpdateOne<T>(this as T);
                    AfterEdit();
                    return res;
                    }
                    catch (Exception s)
                    {
                        DataHelpers.Logger.LogError(s);
                         DataHelpers.ShowMessage( s.Message);
                        return false;
                    }
                }
                 DataHelpers.ShowMessage( "Vous n'avez pas l'autorisation pour modifier!", "Action non autorisée");
                return false;
            }
        }
        
        public virtual bool Delete(bool ConfirmFromUser = true)
        {
            if (FrameworkManager.CanDelete(type))
            {
                try
                {
                    if (BeforeDelete(ConfirmFromUser))
                    {                   
                    DS.db.DeleteOne<T>(this as T);                  
                    AfterDelete();
                    return true;
                    }
                   
                }
                catch (Exception s)
                {
                    DataHelpers.Logger.LogError(s);
                     DataHelpers.ShowMessage( s.Message);
                    return false;
                }
                return false;
            }
             
             DataHelpers.ShowMessage( _("Vous n'avez pas l'autorisation de supprimer!"), _("Action non autorisée"));
            return false;
        }

        public virtual bool Cancel()
        {
            if (FrameworkManager.CanCancel(type))
            {
                try
                {
                    BeforeCancel();
                    this.DocStatus = 2;
                    var res = DS.db.UpdateOne<T>(this as T);
                    AfterCancel();
                    return res;
                }
                catch (Exception s)
                {
                    DataHelpers.Logger.LogError(s);
                     DataHelpers.ShowMessage( s.Message);
                    return false;
                }
            }
            else
            {
                 DataHelpers.ShowMessage( _("Vous n'avez pas l'autorisation pour annuler!"), _("Action non autorisée"));
                return false;
            }
        }

        public virtual bool Submit()
        {
            if (FrameworkManager.CanValidate(type))
            {
                try
                {
                    BeforeEdit();
                    this.DocStatus = 1;
                    var res = DS.db.UpdateOne<T>(this as T);
                    AfterEdit();
                    return res;
                }
                catch (Exception s)
                {
                    DataHelpers.Logger.LogError(s);
                     DataHelpers.ShowMessage( s.Message);
                    return false;
                }
            }
             DataHelpers.ShowMessage( _("Vous n'avez pas l'autorisation pour valider!"), _("Action non autorisée"));
            return false;
        }

        public virtual void AfterSave()
        {

        }

        public virtual void BeforeSave()
        {
            //this.Version++;
            if(DataHelpers.ConnectedUser!=null)
                this.CreatedBy = DataHelpers.ConnectedUser.Id;
            Validate();
        }

        public virtual void AfterDelete()
        {

        }

        public virtual bool BeforeDelete(bool Confirm = true)
        {
           
            if (Confirm)
            {
                var result = DataHelpers.ShowMessage(_("Voulez-vous supprimer le document?"), _("Confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                CheckReferences();
                return (result == MessageBoxResult.Yes); 
            }
            CheckReferences();
            return true;
        }

        public virtual void AfterEdit()
        {

        }

        public virtual void BeforeEdit()
        {
            Validate();
            //this.Version++;
            this.EditedAtUtc = DateTime.Now;
        }

        public virtual void BeforeCancel()
        {
            this.Version++;
            CheckReferences();
        }

        public virtual void AfterCancel()
        {

        }


        public void CheckReferences()
        {
            var refs = GetType().GetProperties();
            foreach (var item in refs)
            {
                var isref = item.GetCustomAttribute(typeof(IsRefAttribute)) as IsRefAttribute;
                if(isref != null && isref?.Ignore == false)
                {
                    var doc = isref.TypeName;
                    var id = item.GetValue(this);
                    if (id == null)
                        continue;
                    if( id.GetType() == typeof(IList))
                    {
                        var listIds = id as IList;
                        foreach (var oneId in listIds)
                        {
                            try
                            {

                                var o = GuidParser.Convert(oneId).GetObject(doc) as IDocument;
                                if (o != null && (o).DocStatus == 1)
                                    throw new Exception($"{this.CollectionName} {this.Name} => {_("ce document fait référencer à un(e)")} {o.CollectionName} {o.Name}  {_("validé")}.\n {_("annuler les références avant d'annuler ce document")}");


                            }
                            catch (ModuleClassNotFoundException s)
                            {
                                
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var o = GuidParser.Convert(id ).GetObject(doc) as IDocument;
                            if (o != null && (o).DocStatus == 1)
                                throw new Exception($"{this.CollectionName} {this.Name} => {_("ce document fait référencer à un(e)")} {o.CollectionName} {o.Name}  {_("validé")}.\n {_("annuler les références avant d'annuler ce document")}");

                        }
                        catch (ModuleClassNotFoundException)
                        {
                        }
                    }

                }
            }
        }
        //[BsonIgnore]


        //public virtual string Name { get
        //    {
        //        return CollectionName + " " + Id.ToString();
        //    }
        //}
        //public Guid Id { get ; set ; }
        // public int Version { get ; set ; }

        //public T GetById(Guid id)
        //{
        //    return DS.db.GetById<T>(id);
        //}


        //public virtual IEnumerable<Tuple<string, string, string>> FetchList { get;  } = new List<Tuple<string, string, string>>();

        //public void AddFetch(ExtendedDocument Me)
        //{
        //    if (this.DocStatus == 1)
        //        return;

        //    foreach (var fetch in FetchList) // Tier, Addresse
        //    {
        //        var p = GetType().GetProperty(fetch.Item1);
        //        if (p != null)
        //        {
        //            var link = p.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
        //            var value = p.GetValue(this);
        //            if(link != null && value.GetType().Name.Contains("Guid"))
        //            {
        //                var id = (value as Guid);
        //                if (id.IsValide())
        //                {
        //                    var obj = id.GetObject(link.Options);
        //                    if(obj != null)
        //                    {
        //                        DataHelpers.MapPropertiesRef(ref Me, obj,fetch.Item2);
        //                    }
        //                }
        //            }
        //        }
        //        NotifyOfPropertyChange(fetch.Item2);
        //    }

        //}

        public virtual void ValidateUnique()
        {
            if (ForceIgniorValidatUnique)
                return;

            var names = DS.db.Count<T>(a => a.Name == this.Name);
            if (names > 0 && isLocal)
            {
                var confirmation =  DataHelpers.ShowMessage( $"{_("Un document avec le méme identifiant existe déja")} '{this.Name} / {this.CollectionName}'\n {_("Voullez-vous continuer")}", _("Identifiant existe!"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.No)
                {

                   throw new Exception(_("Opération annuler"));
                  
                }
            }
            else if (names > 1 && !isLocal)
            {
                var confirmation = DataHelpers.ShowMessage($"{_("Un document avec le méme identifiant existe déja")} '{this.Name} / {this.CollectionName}'\n {_("Voullez-vous continuer")}", _("Identifiant existe!"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.No)
                {

                    throw new Exception(_("Opération annuler"));

                }

            }
        }

        /// <summary>
        /// Default validation method for the model
        /// </summary>
        public virtual void Validate()
        {
            var props = this.GetType().GetProperties();
            var required = props.Where(a => (a.GetCustomAttribute(typeof(RequiredAttribute)) as RequiredAttribute) != null);

            if (required != null)
            {
                foreach (var item in required)
                {
                    var display = item.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
                    var value = item.GetValue(this);
                    if (display != null)
                    {
                        try
                        {
                            Guid d = (Guid) value;
                            if(d == Guid.Empty)
                                throw new Exception($"{_("Le champ")} <{display?.DisplayName}> {_("est requis")}!");
                        }
                        catch { }
                        if (value == null
                                       || (value as IList)?.Count < 1)
                        {
                            throw new Exception($"{_("Le champ")} <{display?.DisplayName}> {_("est requis")}!");
                        }

                    }
                }
            }

           

        }
        /// <summary>
        /// Generate one instance of the class T with parameters (ID,Docstatus)
        /// </summary>
        /// <param name="args">1st param ID, 2nd docstatus (0 draft and 1 submit)</param>
        /// <returns></returns>
        public static T GetOneStatic(params object[] args)
        {
            return (T)Activator.CreateInstance(typeof(T), args);
        }

        /// <summary>
        /// Generate one instance of the class T
        /// </summary>
        /// <returns></returns>
        public static T GetOneStatic()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

       


       
    }
}
