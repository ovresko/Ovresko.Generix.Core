using Ovresko.Generix.Core.Modules.Core.Module;
using MahApps.Metro.Controls;
using Ovresko.Generix.Utils;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using System.ComponentModel;
using System.Collections;
using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core.Data;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Ovresko.Generix.Core.Exceptions;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Pages.Export
{
    class ExportManagerViewModel : Screen, IDisposable
    {
        public ExportManagerViewModel(Type type, object item,string TYPE, bool UseDefault = false,bool UseHeader = true)
        {
            this.UseHeader = UseHeader;
            this.DefaultTemplate = UseDefault;
            this.type = type;
            this.Item = item;
            this.TYPE = TYPE;
            GetModels();
            NotifyOfPropertyChange("Model");
            NotifyOfPropertyChange("DefaultTemplate");


            if (!string.IsNullOrWhiteSpace((Item).MyModule()?.DefaultTemplateName))
            {
                Model = (Item).MyModule()?.DefaultTemplateName;
                DefaultTemplate = true;
                NotifyOfPropertyChange("Model");
                NotifyOfPropertyChange("DefaultTemplate");
                CurrentText = Model;
            }

            NotifyOfPropertyChange("Model");
            NotifyOfPropertyChange("DefaultTemplate");
        }

       





        public bool DefaultTemplate { get; set; } = true;
        public string TYPE { get; set; }
        public dynamic Item { get; set; }
        public Type type { get; set; }
        public HashSet<string> Models { get; set; } = new HashSet<string>();
        public string Model { get; set; }
        public PdfModelExport ovExport { get; set; }
        public string CurrentText { get; set; }
        public bool UseHeader { get; set; } = true;

        public void Print()
        {
            var file = DoCreate(false, "WORD");
            var verb = "Printto";
            if (string.IsNullOrEmpty(file))
                return;


            var defaultPrinter = ElvaSettings.getInstance().ImprimanteDefault;

            if (string.IsNullOrEmpty(defaultPrinter))
            {
                 DataHelpers.ShowMessage( _("Aucune imprimante par default, modifier les paramétres"));
                verb = "Print";
            }

            try
            {
                ProcessStartInfo info = new ProcessStartInfo(file);
                info.Arguments = "\"" + defaultPrinter + "\"";
                info.Verb = verb;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(info);
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage( s.Message);
            }

            try { this.RequestClose(); } catch { }
        }


        bool open = true;
        string aType = "";

        private string DoCreate(bool _open = true, string _aType = "")
        {
            open = _open;
            aType = _aType;
            return Create();
        }
        public string Create()
        {
            if(string.IsNullOrWhiteSpace(Model))
            {
                 DataHelpers.ShowMessage( _("Choisir un modéle"));
                return "";
            }

            if (DefaultTemplate)
            {
                var module = (Item).MyModule();
                module.DefaultTemplateName = Model;
                if (Item.DefaultTemplate != module.DefaultTemplateName)
                {
                    //Item.DefaultTemplate = module.DefaultTemplateName;
                    //Item.Save();
                }

                module.Save();
            }


            if (!string.IsNullOrWhiteSpace(aType))
            {
                TYPE = aType;
            }

            ovExport = new PdfModelExport(type,Model,Item, DataHelpers.GetTypesModule);
            ovExport.UseHeader = this.UseHeader;
            string file = "";
            if(TYPE == "PDF")
                file= ovExport.GeneratePdf();

            if (TYPE == "WORD")
                file= ovExport.GenerateOffice();

            
            if (!string.IsNullOrWhiteSpace(file) && open)
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    if (File.Exists(file))
                    {
                        try
                        {
                            Process.Start(file);
                        }
                        catch (Exception s)
                        {
                             DataHelpers.ShowMessage(s.Message);
                            throw;
                        }

                        break;
                    }
                   
                }
               
                try { this.RequestClose(); } catch { }
            }

            return file;

        }

        private void GetModels()
        {
            var temp = (Item as IDocument).DefaultTemplate;
            var ModelName = type.Name;
            if (DefaultTemplate )
            {
                // use default
                Model = ModelName;
                Create();
                 
            }


           
            if (string.IsNullOrWhiteSpace(ModelName))
                return;

            var folder = $"templates/{ModelName }";
            if (!Directory.Exists(folder))
            {
                 DataHelpers.ShowMessage( _("Modéle introuvable!, Créer nouveau"));
                PdfModelExport ov = new PdfModelExport(type, DataHelpers.GetTypesModule);
                ov.EditTemplate(ModelName);
                return;
            }

            var models = Directory.EnumerateFiles(folder);
            foreach (var item in models)
            {
                var file = Path.GetFileNameWithoutExtension(item);
                Models.Add(file);
            }
          //  Models = new HashSet<string>(models.Select();
            NotifyOfPropertyChange("Models");
        }


        public void DeleteModel()
        {
            var folder = $"templates/{type.Name }";
            var path = Path.GetFullPath($"{folder}/{Model}.docx");
            File.Delete(path);
            GetModels();
        }

        public void OpenModel()
        {

            if (string.IsNullOrWhiteSpace(Model))
            {
                DataHelpers.ShowMessage(_("Selectionner template!"));
                return;
            }

            ovExport = new PdfModelExport(type, Model, Item, DataHelpers.GetTypesModule);

            // Find related properties
            var ed = (Item as IDocument);
            var props = ed.GetType().GetProperties();

            var liens = new List<ArrayList>();

            foreach (var item in props)
            {
                var lienAttrib = item.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                if (lienAttrib != null && lienAttrib.FieldType == ModelFieldType.Lien)
                {
                    var ChildClass = lienAttrib.Options;

                    try
                    {
                        Type type = DataHelpers.GetTypesModule.Resolve(ChildClass);
                        if (type == null)
                            continue;

                        // Type.GetType($"Ovresko.Generix.Core.Modules.{ChildClass}");
                        var typeProps = type.GetProperties();
                        foreach (var p in typeProps)
                        {
                            var hasDisplay = p.GetAttribute<DisplayNameAttribute>(false);
                            var used = p.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                            if (used != null && used.FieldType != ModelFieldType.Separation
                                && used.FieldType != ModelFieldType.Separation
                                  && used.FieldType != ModelFieldType.Button
                                    && used.FieldType != ModelFieldType.OpsButton
                                    && used.FieldType != ModelFieldType.OpsButton && used.FieldType != ModelFieldType.Table
                                    && used.FieldType != ModelFieldType.WeakTable
                                      && used.FieldType != ModelFieldType.BaseButton

                                && hasDisplay != null && !string.IsNullOrEmpty(hasDisplay.DisplayName))
                                liens.Add(new ArrayList { ChildClass, p, item }); // Tier / atier / Tel / 
                        }
                    }
                    catch (Exception s)
                    {
                        DataHelpers.ShowMessageError(s);
                    }
                }
            }

            ovExport.EditTemplate(liens);
        }

        public void CreateModel()
        {
            if (string.IsNullOrWhiteSpace(CurrentText)  )
            {
                 DataHelpers.ShowMessage( _("Saisir nom template"));
                return;
            }


            ovExport = new PdfModelExport(type, CurrentText, Item, DataHelpers.GetTypesModule);
            ovExport.EditTemplate();

        }


        public void Dispose()
        {
           

        }
    }
}
