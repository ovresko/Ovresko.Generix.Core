using MongoDB.Bson.Serialization.Attributes;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Datasource.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using System.Threading.Tasks;
using Ovresko.Generix.Core.Pages.Export;
using System.Reflection;
using Ovresko.Generix.Datasource.Services.Queries;

namespace Ovresko.Generix.Core.Modules.Core.Module
{


    [BsonIgnoreExtraElements(true, Inherited = true)]
    public abstract class Document : PropertyChangedBase, IDocument
    {

        [BsonIgnore]
        [LiteDB.BsonIgnore]
        public override Action<Action> PropertyChangedDispatcher { get => base.PropertyChangedDispatcher; set => base.PropertyChangedDispatcher = value; }


        public Document()
        {
            Id = Guid.NewGuid();// Guid.NewObjectId();
            AddedAtUtc = DateTime.UtcNow;
        }

        [LiteDB.BsonId]
        [BsonId]
        public Guid Id { get; set; }

        //public virtual DateTime AddedAtUtc { get; set; }
        public int Version { get; set; }

        //[LiteDB.BsonIgnore]
        //[BsonIgnore]
        //public virtual string Name { get; set; }
        public virtual string NameSearch { get; set; }

        public virtual void InstallWithBootstrap()
        {

        }

        public virtual void RunWithBootstrap()
        {

        }

        public virtual ModuleErp MyModuleBase()
        {
            return DataHelpers.GetModule(this);
        }

        public virtual IDocument MyModule()
        {
            return DataHelpers.GetModule(this);

        }

        public virtual EventHandler ReloadDetail { get; set; }

        public string GenericName { get; set; }
        // public override string NameSearch { get; set; }

        public virtual int MenuIndex { get; } = 10;

        public abstract string DefaultTemplate { get; }

        public virtual List<string> GroupeProperties { get; }

        //public bool IsReport
        //{
        //    get
        //    {
        //        return this.GetType().Name.Contains("_report");
        //    }
        //}
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public string QualifiedNameSpace
        {
            get
            {

                return this.GetType().FullName;
            }
        }

        [ExDisplayName("Réf")]
        [DontShowInDetail()]
        [ShowInTable(true)]
        public virtual string Name { get; set; }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public abstract string CollectionName { get; }




        [ShowInTable]
        [ExDisplayName("Crée par")]
        [Column(ModelFieldType.Lien, "User")]
        [DontShowInDetail]
        [myType(typeof(User))]
        public Guid CreatedBy { get; set; } = Guid.Empty;

        //[BsonIgnore]
        //[ExDisplayName("Crée par")]
        //public string CreatedByName { get
        //    {
        //        return CreatedBy?.GetObject("User")?.Name;
        //    }
        //}

        public virtual string EmailMessage { get; }
        public virtual string EmailRecepiant { get; }
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual bool IsInstance { get; set; } = false;
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual OpenMode DocOpenMod { get; set; } = OpenMode.Attach;

        public Guid _etag { get; set; }
        /// <summary>
        /// 0 not handled - 1 Handled
        /// </summary>
        public int isHandled { get; set; } // 0 no / 1 handled

        /// <summary>
        /// TRUE NEWDOW - FALSE SAVED
        /// </summary>
        public bool isLocal { get; set; } = true;
        /// <summary>
        /// 0: Draft. 1: Submit 2: cancel
        /// </summary>  
        public int DocStatus { get; set; }
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual bool Submitable { get; set; } = false;

        [ExDisplayName("Crée le")]
        [DontShowInDetail()]
        [ShowInTable(true)]
        public DateTime AddedAtUtc { get; set; }

        [ExDisplayName("Éditer le")]
        [DontShowInDetail()]
        public DateTime? EditedAtUtc { get; set; }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        [ExDisplayName("Status")]
        [DontShowInDetail()]
        [ShowInTable(true)]
        public virtual string Status
        {
            get
            {
                if (isLocal)
                    return _("Nouveau");//, "#3F51B5"); new Tuple<string, string>( , "#FFC400") new Tuple<string, string>(, "#00E676"), "#00E676")
                if (DocStatus == 2)
                    return _("Annulé");

                if (Submitable)
                {
                    return DocStatus == 0 ? _("Brouillon") : _("Validé");
                }
                return _("Enregistré");
            }
        }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        [DontShowInDetail()]
        public virtual string StatusColor
        {
            get
            {
                if (isLocal)
                    return "#3F51B5";//, "#3F51B5"); new Tuple<string, string>( , "#FFC400") new Tuple<string, string>(, "#00E676"), "#00E676")
                if (DocStatus == 2)
                    return "Red";

                if (Submitable)
                {
                    return DocStatus == 0 ? "Orange" : "#2FCC71";// "#AEEA00";// "#2FCC71";
                }
                return "#3F51B5";
            }
        }

        // Method
        public virtual void DoFunction(string methodName)
        {
            try
            {
                var method = this.GetType().GetMethod(methodName);
                if (method != null)
                {
                    method.Invoke(this, null);
                }
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);// Logger.LogError($"Method {methodName} not found in {this.CollectionName}\n{s.Message}");
            }
        }
        public virtual object DoFunction(string methodName, object[] parameters = null)
        {
            try
            {
                var method = this.GetType().GetMethod(methodName);
                if (method != null)
                {
                    return method.Invoke(this, parameters);
                }
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError($"Method {methodName} not found in {this.CollectionName}\n{s.Message}");
            }

            return null;
        }

        public event EventHandler CloseEvent;

        public void CloseParent()
        {
            EventHandler handler = CloseEvent;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }



        public virtual void ExportPDF(Type t, bool useDefault = false, bool UseHeader = true)
        {
            var exportPdf = new ExportManagerViewModel(t, this, "PDF", useDefault, UseHeader);
            exportPdf.UseHeader = UseHeader;
            if (useDefault != true)
                DataHelpers.windowManager.ShowDialog(exportPdf);

        }


        public virtual string ExportPDF(Type t, string template, bool Useheader = true)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                ExportPDF(t, false, true);
            }
            Ovresko.Generix.Utils.PdfModelExport ov = new Ovresko.Generix.Utils.PdfModelExport(t, template, this, DataHelpers.GetTypesModule);
            ov.UseHeader = Useheader;
            return ov.GeneratePdf();
        }

        public virtual void ExportWORD(Type t, bool useDefault = false, bool UseHeader = true)
        {
            var exportPdf = new ExportManagerViewModel(t, this, "WORD", useDefault);
            exportPdf.UseHeader = UseHeader;
            if (useDefault != true)
                DataHelpers.windowManager.ShowDialog(exportPdf);
        }

        public virtual string ExportWORD(Type t, string template, bool Useheader = true)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                ExportWORD(t, false, true);
            }
            Ovresko.Generix.Utils.PdfModelExport ov = new Ovresko.Generix.Utils.PdfModelExport(t, template, this, DataHelpers.GetTypesModule);
            ov.UseHeader = Useheader;
            ov.GenerateProps = false;
            return ov.GenerateOffice();
        }
        //public virtual IEnumerable<IDocument> GetList()
        //{
        //    return DataHelpers.GetMongoData(this.CollectionName) as IEnumerable<IDocument>;
        //}

        //public virtual IEnumerable<dynamic> GetList(string link)
        //{
        //    try
        //    {  
        //        try
        //        { 
        //            var result = DataHelpers.GetMongoData(link, $"l{CollectionName}", Id.ToString(), true).OrderByDescending(a => a.AddedAtUtc).ToList<dynamic>();
        //            return result;
        //        }
        //        catch 
        //        {
        //            return new List<dynamic>();
        //        }
        //    }
        //    catch 
        //    {
        //        return new List<dynamic>();
        //    }
        //}
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        [ShowInTable(true)]
        [DontShowInDetail]
        public bool IsSelectedd { get; set; }

        public virtual IDocument Map(string mappedClass)
        {
            return this;
        }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual string ModuleName { get; set; }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual bool ShowInDesktop { get; set; }

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual string SubModule { get; set; }

        //[LiteDB.BsonIgnore]
        //[BsonIgnore]
        //public virtual DocCard DocCardOne { get; } = null;

        //[LiteDB.BsonIgnore]
        //[BsonIgnore]
        //public virtual DocCard DocCardTow { get; } = null;
        //[LiteDB.BsonIgnore]
        //[BsonIgnore]
        //public virtual DocCard DocCardThree { get; } = null;
        //[LiteDB.BsonIgnore]
        //[BsonIgnore]
        //public virtual DocCard DocCardFor { get; } = null;

        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public virtual string IconName { get; set; } = "InformationOutline";
        public bool Has(string value, Type t)
        {
            var attrib = t.GetProperties();

            foreach (var item in attrib)
            {
                if (item.GetValue(this)?.ToString().Contains(value) == true)
                {
                    return true;
                }
            }
            return false;
        }


        public bool EnsureIsSavedSubmit(bool IgnorMessage = false)
        {
            if (this.isLocal || (this.Submitable && this.DocStatus != 1))
            {
                if (IgnorMessage == false)
                    DataHelpers.ShowMessage(_("Document non enregsitré ou validé!")+$" {this.CollectionName}");
                return false;
            }
            return true;
        }

        public bool EnsureIsSaved(bool IgnorMessage = false)
        {
            if (this.isLocal)
            {
                if (!IgnorMessage)
                    DataHelpers.ShowMessage(_("Document non enregsitré!") + $" {this.CollectionName}");
                return false;
            }
            return true;
        }
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public bool ForceIgniorValidatUnique { get; set; } = false;

        public string GetValueDynamic(string propName, string link, string propery) // Tier / Adresse
        {

            var p = this.GetType().GetProperty(propName);
            var obj = p.GetValue(this); ;
            if (obj != null && (obj.GetType() == typeof(Guid) || obj.GetType() == typeof(Guid?)))
            {
                var concrete = ((Guid)obj).GetObject(link);
                if (concrete != null)
                {
                    var res = concrete.GetType().GetProperty(propery).GetValue(concrete);

                    return res?.ToString();


                }
            }
            else
            {
                return obj?.ToString();
            }
            return null;
        }
        public dynamic GetLiteralValue(PropertyInfo property, string propertyName = null)
        {
            var id = property.GetValue(this);
            dynamic value = null;
            if (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid))
            {
                var isLink = (property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute);
                if (isLink != null && isLink.FieldType == ModelFieldType.Lien)
                {
                    var lienClass = isLink.Options;

                    if (string.IsNullOrEmpty(propertyName))
                    {
                        value = id?.ToString();// DS.db.Generic(Type t).
                        var name = DS.Generic(lienClass)?.GetById(GuidParser.Convert(id))?.NameSearch;
                        value = name;
                    }
                    else
                    {
                        var obj = DS.Generic(lienClass)?.GetById(GuidParser.Convert(id));
                        var name = obj?.GetType().GetProperty(propertyName)?.GetValue(obj);
                        value = name;
                    }

                }
                if (isLink != null && isLink.FieldType == ModelFieldType.LienField)
                {
                    if (isLink.Options.Contains('>'))
                    {
                        try
                        {
                            var lienClass = isLink.Options.Split('>');
                            var type_ = (property.GetCustomAttribute(typeof(myTypeAttribute)) as myTypeAttribute);

                            var className = lienClass[0].ToString(); // CLient
                            var field = lienClass[1].ToString(); // Adresses list ids

                            //var inner = $"Ovresko.Generix.Core.Modules.{className}";
                            Type t = DataHelpers.GetTypesModule.Resolve(className);


                            var pro = (t.GetProperty(field).GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute);
                            if (pro.FieldType == ModelFieldType.Table)
                            {
                                var classLien = pro.Options; // Adresse
                                var concrete = DS.Generic(classLien)?.GetById((Guid)id)?.NameSearch; // Tier object
                                value = concrete;
                            }
                        }
                        catch (Exception s)
                        {
                            DataHelpers.ShowMessageError(s);
                        }
                    }

                }

            }

            return value;
        }



        public virtual void AfterOpen()
        {

        }

        public virtual void AfterRefresh()
        {

        }





        public virtual void BeforRefresh()
        {

        }

        //public void HidePropety(string propertyName)
        //{
        //    try
        //    {
        //        this.PropertiesToHide.Clear();
        //        this.PropertiesToHide.Add(propertyName);
        //    }
        //    catch (Exception s)
        //    {
        //        Console.WriteLine(s.Message);
        //    }

        //}
        [LiteDB.BsonIgnore]
        [BsonIgnore]
        public HashSet<string> PropertiesToHide { get; set; } = new HashSet<string>();
       
        public virtual Dictionary<string, string> InfoCards { get;   } = new Dictionary<string, string>();

        public virtual void BeforeClose()
        {

        }


        public IDocument MapRefsTo(IDocument toThisDoc, bool Nested = false)
        {


            var myRefs = GetType().GetProperties();
            Dictionary<string, Guid> modelToUpdate = new Dictionary<string, Guid>();
            string toThisName = toThisDoc.GetType().Name;
            foreach (var item in myRefs)
            {
                var refAtt = item.GetCustomAttribute(typeof(IsRefAttribute)) as IsRefAttribute;
                if (refAtt != null)
                {
                    var className = refAtt.TypeName;

                    var value = GuidParser.Convert(item.GetValue(this));// as Guid;
                    var destProp = toThisDoc.GetType().GetProperty(item.Name);
                    if (destProp == null)
                        continue;

                    if (refAtt.TypeName != toThisName)
                        modelToUpdate.Add(className, value);

                    var destination = GuidParser.Convert(destProp.GetValue(toThisDoc));// as Guid;                   

                    if (item.CanWrite && destination.IsValide())
                    {
                        item.SetValue(this, destination);
                    }
                    else if (value.IsValide() && destProp.CanWrite)
                    {
                        destProp.SetValue(toThisDoc, value);
                    }
                }
            }

            if (Nested == false)
                Task.Factory.StartNew(() => ApplyMapRefsToAll(modelToUpdate));
            return toThisDoc;
        }

        public void ApplyMapRefsToAll(Dictionary<string, Guid> Models) // Models == Tier, Devis...etc
        {
            foreach (var item in Models.Keys)
            {
                var id = Models[item];
                if (!id.IsValide())
                    continue;
                var doc = DS.Generic(item)?.GetById(id);
                if (doc != null)
                {
                    doc = this.MapRefsTo(doc, Nested: true);
                    (doc as IModel).Save();
                }
            }
        }


    }

}
