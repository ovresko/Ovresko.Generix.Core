using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules
{
    class SoldeTier_report : ModelBase<SoldeTier_report>
    {

        #region SETTINGS
       
        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = _("Comptes");
        public override string CollectionName { get; } = _("Situation Tier");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "ChartBar";
        public override bool ShowInDesktop { get; set; } = true;
        // public override string NameField => "Article"; 
        public override string Name { get; set; }

        #endregion

        public Tier SelectedTier { get; set; }



        [ExDisplayName("Mois")]
        [Column(ModelFieldType.ReadOnly,"")]
        public string Mois { get; set; }

        [ShowInTable]
        [ExDisplayName("Débit")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string SoldeDebit { get; set; }

        [ShowInTable]
        [ExDisplayName("Crédit")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string SoldeCredit { get; set; }

        [ShowInTable]
        [ExDisplayName("Montant créance")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string Creance { get; set; }

        [ShowInTable]
        [ExDisplayName("Ventes (factures / Taux)")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string FacturesVente { get; set; }

        [ShowInTable]
        [ExDisplayName("Achat (factures / Taux)")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string FacturesAchat { get; set; }


        

        public  override IEnumerable<ExtendedDocument> GetList()
        {
            var doc =  DataHelpers.Shell.OpenScreenFind(typeof(Tier), $"{_("Selectioner tier")}...").Result;

            List<SoldeTier_report> result = new List<SoldeTier_report>();
            if (doc != null)
            {
                if (doc.Count() > 1)
                {
                    MessageBox.Show("Selectionner un seul Tier");
                    return null;
                }
                SelectedTier = doc.FirstOrDefault() as Tier;

               

                var facture =  DS.db.GetAll<Facture>(a => a.Tier == SelectedTier.Id && a.DocStatus == 1);
                var facturesAchat = DS.db.GetAll<FactureAchat>(a => a.Tier == SelectedTier.Id && a.DocStatus == 1);

                var firstdate = facture.Min(a => a.DateCreation);
                var firstdateAchat = facturesAchat.Min(a => a.DateCreation);
                DateTime? oldest = null;
                DateTime? oldestPurchase = null;
                
                if (firstdate != null && firstdateAchat!=null)
                {
                    oldest = firstdate <= firstdateAchat ? firstdate : firstdateAchat;
                    oldestPurchase = oldest > DateTime.Now ? DateTime.Now : oldest;
                }
                else
                {
                    if (firstdate != null)
                        oldestPurchase = firstdate;
                }
                    

                var dates = DataHelpers.GetDateRangeByMonth(oldestPurchase.GetValueOrDefault(new DateTime(DateTime.Today.Year, 01, 01)), DateTime.Now);

                var compteTier = CompteSettings.getInstance().CompteTier;
                var compteFroun = CompteSettings.getInstance().CompteFournisseur;

                var ej = DS.db.GetAll<EcritureJournal>(a => a.DocStatus == 1 &&
                 (a.CompteJournal == compteFroun || a.CompteJournal == compteTier) && a.Tier == this.SelectedTier.Id);
                var MontantCreditT = ej?.Sum(d => d.MontantCredit);
                var MontantDebitT = ej?.Sum(a => a.MontantDebit);
                var soldeDefT = (MontantCreditT - MontantDebitT);

                result.Add(new SoldeTier_report()
                {
                    Name = "Nom Tier",
                    SoldeDebit = $"{SelectedTier.NameSearch}",
                });

                result.Add(new SoldeTier_report()
                {
                    Name = $"Type",
                    SoldeDebit = SelectedTier.IsClient ? "Client" : (SelectedTier.IsFournisseur ? "Fournisseur":"")
                });

                result.Add(new SoldeTier_report() { Name = "    "});

                foreach (var item in dates)
                {

                    var ejperiod = ej.Where(a => a.DateEcriture >= item && a.DateEcriture < item.AddMonths(1));
                    var MontantCredit = ejperiod?.Sum(d => d.MontantCredit);
                    var MontantDebit = ejperiod?.Sum(a => a.MontantDebit);

                    var soldeDef = (MontantCredit - MontantDebit);

                    var factures = facture.Where(a => a.DateCreation >= item && a.DateCreation < item.AddMonths(1));
                    var nombreFactureVente = factures?.Count();
                    var soldeFactureVente = factures?.Sum(a => a.MontantGlobalTTC);

                    var achat = facturesAchat.Where(a => a.DateCreation >= item && a.DateCreation < item.AddMonths(1));
                    var nombreFactureAchat = achat?.Count();
                    var soldeFactureAchat = achat?.Sum(a => a.MontantGlobalTTC);

                    var cmdVente = DS.db.GetAll<CommandeVente>(a => a.DocStatus == 1 && a.Tier == this.SelectedTier.Id);
                    var cmdVenteNonPaye = cmdVente?.Where(a => a.DateCommande >= item && a.DateCommande < item.AddMonths(1) && a.EstFacturer() ==false);
                    var countCmdNonFacture = cmdVenteNonPaye?.Count();
                    var soldeCmdNonFacture = cmdVenteNonPaye?.Sum(a => a.MontantGlobalTTC);


                    var cmdAchat= DS.db.GetAll<CommandeAchat>(a => a.DocStatus == 1 && a.Tier == this.SelectedTier.Id);
                    var cmdAchatNonPaye = cmdAchat?.Where(a => a.DateCommande >= item && a.DateCommande < item.AddMonths(1) && a.EstFacturer() == false);
                    var countCmdNonFactureAchat = cmdAchatNonPaye?.Count();
                    var soldeCmdNonFactureAchat = cmdAchatNonPaye?.Sum(a => a.MontantGlobalTTC);


                    result.Add(new SoldeTier_report()
                    {
                        Name = item.ToString("MMMM yyyy"),
                        Mois = item.ToString("MMMM yyyy"),
                        SoldeDebit = MontantDebit?.ToString("C"),
                        SoldeCredit = MontantCredit?.ToString("C"),
                        Creance = soldeDef?.ToString("C"),
                        FacturesVente = $@"{nombreFactureVente} Factures / {soldeFactureVente?.ToString("C")}
{countCmdNonFacture} Non facturée(s) / {soldeCmdNonFacture?.ToString("C")}",
                        FacturesAchat = $@"{nombreFactureAchat} Factures / {soldeFactureAchat?.ToString("C")}
{countCmdNonFactureAchat} Non facturée(s) / {soldeCmdNonFactureAchat?.ToString("C")}"
                    });
                }

                result.Add(new SoldeTier_report()
                {
                    Name = "                  ",
                    Mois = "                  ",
                    SoldeDebit = "                  ",
                    SoldeCredit = "                  ",
                    Creance = "                  ",
                    FacturesVente = "                  ",
                    FacturesAchat = "                  ",
                });

                result.Add(new SoldeTier_report()
                {
                    Name ="TOTAL",
                    Mois = "TOTAL",
                    SoldeDebit = MontantDebitT?.ToString("C"),
                    SoldeCredit = MontantCreditT?.ToString("C"),
                    Creance = soldeDefT?.ToString("C"),
                    
                });
                result.Add(new SoldeTier_report()
                {
                    Name = "                  ",
                });
                if(soldeDefT < 0)
                {
                    var notpaied = facture.Where(a => a.EstPaye() == false);

                    string names = "";
                    foreach (var item in notpaied)
                    {
                        names += $"\n{item.Name}";
                    }

                    result.Add(new SoldeTier_report()
                    {
                        Name = "RETARD DE PAIEMENT",
                        SoldeDebit = "----------------",
                        SoldeCredit = "----------------",
                        Creance = Math.Abs(soldeDefT.GetValueOrDefault(0)).ToString("C"),
                        FacturesVente = $@"{notpaied.Count()} Factures non payés {names }"
                    });
                }
                else
                {
                    result.Add(new SoldeTier_report()
                    {
                        Name = "AVANCE DE PAIEMENT",
                        SoldeDebit = "----------------",
                        SoldeCredit = "----------------",
                        Creance = Math.Abs(soldeDefT.GetValueOrDefault(0)).ToString("C"),
                       
                    });
                }
               

            }

            return result;
        }
      
    }
}
