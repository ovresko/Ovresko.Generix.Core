using De.TorstenMandelkow.MetroChart;
using MongoDB.Bson.Serialization.Attributes;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.PdfViewer;
using Ovresko.Generix.Datasource.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules
{
    public class ValueObject
    {
        public string Libelle { get; set; }
        public double Value { get; set; } 
    }

    class TestReport : ModelBase<TestReport>, IReportView
    {   
        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Systémes";
        public override string CollectionName { get; } = _("TEST");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "AccountMultiple";
        public override bool ShowInDesktop { get; set; } = false;
        public override bool IsInstance { get; set; } = true;
        #endregion

        [ExDisplayName("Switch")]
        [Column(ModelFieldType.Button,"SwitchC")]
        public int BtnSwitch { get; }

        public BindableCollection<ValueObject> ItemsSource { get; set; } = new BindableCollection<ValueObject>();
        public string SeriesTitle { get; set; } = "SeriesTitle";
        public string DisplayMember { get; set; } = "Libelle";
        public string ValueMember { get; set; } = "Value";
        
        int inc = 100;
        public void SwitchC()
        {
            ItemsSource.Add(new ValueObject()
            {
                Libelle = $"Libelé {inc}",
                Value = inc++,
            });

            NotifyOfPropertyChange("ItemsSource");
        }


        [ExDisplayName("Rapport")]
        [Column(ModelFieldType.Separation, "")]
        public int sep { get; }

        [BsonIgnore]
        [ExDisplayName("Rapport test")]
        [Column(ModelFieldType.ReportControl, "")]
        public PdfViewerView Pdfview { get; set; }

        [BsonIgnore]
        [ExDisplayName("Rapport 2")]
        [Column(ModelFieldType.ReportControl, "")]
        public PdfViewerView Pdfview2 { get; set; }

        public TestReport()
        {
            Execute.OnUIThread(() =>
            {
                Pdfview = new PdfViewerView();
                Pdfview2 = new PdfViewerView();
            });
        }


        public override TestReport getInstance()
        {
            TestReport test = new TestReport();
            return test;
        }
    }
}
