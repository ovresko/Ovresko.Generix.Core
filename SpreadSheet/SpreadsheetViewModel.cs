 using Microsoft.Win32;
using MongoDB.Bson;
using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Stylet;
using System;
using System.Collections.Generic;
 using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using unvell.ReoGrid;
using unvell.ReoGrid.CellTypes;
using unvell.ReoGrid.Chart;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.SpreadSheet
{
  public  class SpreadsheetViewModel : Screen
    {

        private readonly string REPORTS = "/Reports";
        public Worksheet sheet { get; set; }

        public ReoGridControl ReoGrid { get; set; }
        public string ReportName { get; set; }
        public string FermerLabel
        {
            get
            {
                return _("Fermer");
            }
        }

        public void Close()
        {
            try
            {
                this.RequestClose(true);
            }
            catch
            {}
        }
        public void Undo()
        {
            ReoGrid.Undo();
        }

        public void Redo()
        {
            ReoGrid.Redo();
        }
        
        public void Excel()
        {
            var file = new SaveFileDialog();
          var res=  file.ShowDialog();
            if (res == true)
            {
                var output =  file.FileName;
                if(!string.IsNullOrWhiteSpace(output))
                {
                    ReoGrid.CurrentWorksheet.Workbook.Save(output+".xls", unvell.ReoGrid.IO.FileFormat.Excel2007);
                }
            }
        }

        public SpreadsheetViewModel()
        {
            //DisplayName = "Rapport";
            //var doc = DataHelpers.Shell.OpenScreenFind(typeof(Tier), $"{_("Selectioner tier")}...").Result;
            //if(doc == null)
            //{
            //     DataHelpers.ShowMessage("Vous devez choisir le Tier a affiche");
            //    return;
            //}
            //var tier = doc.FirstOrDefault() as Tier;

            
            //ReoGrid = new ReoGridControl();
            ////  ReoGrid.Load($"{REPORTS}/{_reportname}",unvell.ReoGrid.IO.FileFormat._Auto);
            //using (var ms = new System.IO.MemoryStream(Properties.Resources.SituationTier))
            //{
            //    ReoGrid.Load(ms,unvell.ReoGrid.IO.FileFormat.ReoGridFormat);

            //     sheet = ReoGrid.CurrentWorksheet;

            //    sheet["E4"] = DateTime.Now.ToLongDateString();

            //    var soldePaiement = DS.db.GetAll<EcriturePaiment>(a => a.DocStatus == 1 && a.Tier == tier.Id && a.ObjetEcriture == "Recevoir");
            //    var solde = soldePaiement?.Sum(a => a.MontantPaye);
            //    sheet["H17"] = solde;

            //    var factures = DS.db.GetAll<CommandeVente>(a =>a.DocStatus == 1 && a.Tier == tier.Id);
            //    int countFac = (int) factures?.Count();
               

            //    var factureWithouutCommand = DS.db.GetAll<Facture>(a => a.DocStatus == 1 && a.Tier == tier.Id && (a.RefCommandeVente == null || a.RefCommandeVente == Guid.Empty));
            //    int fwc = (int)factureWithouutCommand?.Count();
            //     var count = Math.Max(countFac, fwc);



            //    sheet.SetRows(Math.Max(200, count));
            //    sheet["tier"] = new object[,] { { tier.NomComplet, "" } };
                
            //    var data = new object[count,6];


            //    for (int i = 0; i < count; i++)
            //    {
            //        if(i >= countFac)
            //        {
            //            var index = i - (countFac);

            //            var fac = factureWithouutCommand[index];
            //            data[i, 0] = "";
            //            data[i, 1] = fac?.DateCreation?.ToString("dd/MM/yyyy");
            //            data[i, 2] = fac?.MontantGlobalTTC;
            //            data[i, 3] = fac?.Name;
            //            data[i, 4] = fac?.MontantGlobalTTC;
            //            data[i, 5] = fac?.MontantPaye; 
            //        }
            //        else
            //        {
            //            var fac = factures[i].RefFacture.GetObject<Facture>();
            //            data[i, 0] = factures[i].Name;
            //            data[i, 1] = factures[i].DateCommande?.ToString("dd/MM/yyyy");
            //            data[i, 2] = factures[i].MontantGlobalTTC;
            //            data[i, 3] = fac?.Name;
            //            data[i, 4] = fac?.MontantGlobalTTC;
            //            data[i, 5] = fac?.MontantPaye; 
            //        }
                   

            //    }

            //    sheet["factures"] = data; 

            //}

        }
        public string FunctionBar { get; set; }

        public SpreadsheetViewModel(ReoGridControl reoGrid, string display)
        {
            ReoGrid = reoGrid;
            ReoGrid.CurrentWorksheet.SelectionRangeChanged += CurrentWorksheet_SelectionRangeChanged;
            this.DisplayName = display;
            NotifyOfPropertyChange("ReoGrid");
        }

        private void CurrentWorksheet_SelectionRangeChanged(object sender, unvell.ReoGrid.Events.RangeEventArgs e)
        {
            FunctionBar = (sender as Worksheet).GetCellData(e.Range.StartPos)?.ToString();
            NotifyOfPropertyChange("FunctionBar");
        }

        private void Open_Click(object sender, EventArgs e)
        {
            //var idFac = (sender as ButtonCell);
            //if(idFac != null)
            //{
            //    var id = idFac.Cell.Data?.ToString();
            //    if (!string.IsNullOrWhiteSpace(id))
            //    {
            //        Guid oid = Guid.Parse(id);
            //        var facture = DS.db.GetOne<Facture>(a => a.Id == oid);

            //        DataHelpers.Shell.OpenScreenDetach(facture, facture.Name);
            //    }
            //}
        }
    }
}
