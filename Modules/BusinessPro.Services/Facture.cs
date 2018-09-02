using MongoDB.Bson;
using Ovresko.Generix.Core.Modules.Base;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ovresko.Generix.Core.Modules
{

    interface IFacture
    {
        List<EcritureJournal> MakeEcritureJournal(CreateModes mode);
        EcritureStock MakeEcritureStock(CreateModes mode);
        EcriturePaiment MakePaiement(CreateModes mode);

    }

    public partial class Facture
    {
        //!Validation of properties
        #region VALIDATION

          

        public void ValidateClient()
        {
            /* TODO : Valider client en cas du sole maximum existe ou client bloqué
             *        ou client n'est pas client mais fournisseur aumoin
               */

            var tier = Tier.GetObject<Tier>();
            if(tier.IsClient == false)
            {
                var r = MessageBox.Show( "Le tier selectionner n'est pas un Client! Voulez-vous continuer?", "Type de tier", MessageBoxButton.YesNo);
                if (r == MessageBoxResult.No)
                    throw new Exception("Opération annulée");
            }

               
        }

        public void ValidateQte()
        {
            foreach (var item in ArticleFacture)
            {
                // Qte de vente maximal
                var article = item?.lArticle.GetObject<Article>();

                if (article == null)
                    throw new Exception($"Aucun article source selectionner pour l'article {item.Name}");

                if (article.QtsVenteMaximum < item.Qts && article.QtsVenteMaximum > 0)
                {
                    throw new Exception($"L'article {item.Name} à dépassé la quantité autorisée de vente {article.QtsVenteMaximum}");
                }
                // Qte négatif
                if (item.Qts < 0 && IsReturn == false)
                    throw new Exception($"La quantité de l'article <{item.Name}> est négatif : {item.Qts}! Vérifier les qts ou marquer la facture comme retour");

                // Qte est zéro
                if (item.Qts == 0)
                    throw new Exception($"La qantité de l'article {item.Name} est égale à zéro!");

                if (article.CanVendre == false && article.CanAchat == true)
                    throw new Exception($"L'article {article.Name} n'est pas un article Vendable, modifier les informations d'article!");

            }
        }

        public void ValidateCalcule()
        {
            // TODO: recalculer les sommes (Taxe, TTC, HT...)
        }



        #endregion
         
        //!Public actions & methods
        #region API


        public Facture CreateFactureRetour(CreateModes mode)
        {
            if (!EnsureIsSavedSubmit())
                return null;

            if (!IsReturn)
            {
                Facture fac = new Facture();
                fac = DataHelpers.MapProperties(fac, this) as Facture;
                fac.ArticleFacture.ForEach(a => a.Qts = (a.Qts * -1));
                fac.IsReturn = true;
                fac.RefOriginalFacture = this.Id;
                fac = this.MapRefsTo(fac) as Facture;

               fac = CreateModesHandler.Handle(fac, mode) as Facture;
                return fac;
            }
            else
            {
                MessageBox.Show( "Vous pouvez pas retourner une facture de retour");
                return null;
            }
        }

        public void UpdatSoldeTier()
        {
            SoldeClientEncien = Tier.GetObject<Tier>()?.TauxSolde(true);
            NotifyOfPropertyChange("SoldeClientEncien");
        }

     
        /// <summary>
        /// Create delivery note from invoice       
        /// </summary>
        /// <param name="mode">Creating mode</param>
        /// <returns>Created Delivery Note</returns>
        public BonLivraison CreateBonLivraison(CreateModes mode)
        {
            if (EnsureIsSavedSubmit() && IsReturn == false)
            { 
                if (this.RefBonLivraison.IsValideAndSubmited<BonLivraison>())
                {
                    var res = MessageBox.Show( $"Facture déja fait référencer a un Bon de livraison #{this.RefBonLivraison.GetObject("BonLivraison")?.Name}! Annuler ce document avant de continuer!", "Confirmation", MessageBoxButton.OK);
                    return null;
                }

                BonLivraison f = new BonLivraison();
                f = DataHelpers.MapProperties(f, this) as BonLivraison;
                f.DateBL = DateTime.Now;
                f.RefFacture = this.Id;
                foreach (var item in this.ArticleFacture)
                {
                    var line = new LigneEcritureStock();
                    line = DataHelpers.MapProperties(line, item) as LigneEcritureStock;
                    f.Articles.Add(line);
                }
                 
                f = this.MapRefsTo(f) as BonLivraison;
                f = CreateModesHandler.Handle(f, mode) as BonLivraison;
                return f;
            }
            return null;
        }
        public BonReception CreateBonReceptionRetour(CreateModes mode)
        {
            if (EnsureIsSavedSubmit() && IsReturn == true)
            {
                //if (this.RefBonLivraison.IsValideAndSubmited<BonLivraison>())
                //{
                //    var res = MessageBox.Show( $"Facture déja fait référencer a un Bon de livraison #{this.RefBonLivraison.GetObject("BonLivraison")?.Name}! Annuler ce document avant de continuer!", "Confirmation", MessageBoxButton.OK);
                //    return null;
                //}

                BonReception f = new BonReception();
                f = DataHelpers.MapProperties(f, this) as BonReception;
                f.DateBL = DateTime.Now;
                 
                foreach (var item in this.ArticleFacture)
                {
                    item.Qts = Math.Abs(item.Qts);
                    var line = new LigneEcritureStock();
                    line = DataHelpers.MapProperties(line, item) as LigneEcritureStock;
                    f.Articles.Add(line);
                }

                f = this.MapRefsTo(f) as BonReception;
                f = CreateModesHandler.Handle(f, mode) as BonReception;
                return f;
            }
            return null;
        }


        /// <summary>
        /// Create paiement Entry for the invoice   
        /// </summary>
        /// <param name="mode">Creating Mode</param>
        /// <returns></returns>
        public EcriturePaiment CreatePaiement(CreateModes mode, decimal MontantPaye)
        {
            if (this.RefEcriturePaiment.IsValide())
            {
                var res = MessageBox.Show( $"Facture déja fait référencer a un paiement #{this.RefEcriturePaiment.GetObject<EcriturePaiment>()?.Name}! voulez-vous continuer?", "Confirmation", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return null;
            }

            if (EnsureIsSavedSubmit())
            {
                // validate mode de paiment par default
                var modePaiementParDefault = CompteSettings.getInstance().DefaultModelPaiement;
                if (!modePaiementParDefault.HasValue || modePaiementParDefault == ObjectId.Empty)
                {
                    MessageBox.Show( "Mode de paiement par default introuvable, éditer les paramétres comptabilité");
                    return null;
                }

                var ecritureapiemenrt = new EcriturePaiment();
                if (this.IsReturn == false)
                {

                    ecritureapiemenrt = DataHelpers.MapProperties(ecritureapiemenrt, this) as EcriturePaiment;
                    ecritureapiemenrt.Tier = this.Tier;
                    ecritureapiemenrt.DateEcriture = this.DateEcheance;
                    ecritureapiemenrt.ModeDePiement = modePaiementParDefault;
                    ecritureapiemenrt.MontantPaye = MontantPaye;
                    ecritureapiemenrt.ObjetEcriture = "Recevoir";
                    ecritureapiemenrt.Factures = new List<Facture>() { this };
                    ecritureapiemenrt.ComtpeDebit = modePaiementParDefault.GetObject<ModePaiement>()?.CompteCredit;
                    ecritureapiemenrt.ComtpeCredit = CompteSettings.getInstance().CompteTier;
                    ecritureapiemenrt.Remarques = "Enregistrer depuis facture " + this.Name;

                    // return ecritureapiemenrt;
                }
                else
                {

                    ecritureapiemenrt = DataHelpers.MapProperties(ecritureapiemenrt, this) as EcriturePaiment;
                    ecritureapiemenrt.Tier = this.Tier;
                    ecritureapiemenrt.DateEcriture = this.DateEcheance;
                    ecritureapiemenrt.ModeDePiement = modePaiementParDefault;
                    ecritureapiemenrt.MontantPaye = Math.Abs(this.MontantReste);
                    ecritureapiemenrt.ComtpeCredit = modePaiementParDefault.GetObject<ModePaiement>()?.CompteCredit;
                    ecritureapiemenrt.ComtpeDebit = CompteSettings.getInstance().CompteTier;
                    ecritureapiemenrt.ObjetEcriture = "Payer";
                    ecritureapiemenrt.Factures = new List<Facture>() { this };
                    ecritureapiemenrt.Remarques = "Enregistrer depuis facture retour " + this.Name; 

                    // return ecritureapiemenrt;
                }

                ecritureapiemenrt = this.MapRefsTo(ecritureapiemenrt) as EcriturePaiment;
                ecritureapiemenrt = CreateModesHandler.Handle(ecritureapiemenrt, mode) as EcriturePaiment;
                return ecritureapiemenrt;

            }
            return null;
        }


        /// <summary>
        /// Create Journal Entries For the Invoice
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private List<EcritureJournal> CreateEcritureJournal(CreateModes mode)
        {
            var ej_result = new List<EcritureJournal>();

            if (this.EnsureIsSavedSubmit())
            {

                if (this.IsReturn == false)
                {

                    EcritureJournal credit = new EcritureJournal();
                    credit.DateEcriture = this.DateCreation;
                    credit.CompteJournal = this.CompteCredit.IsValide() ? this.CompteCredit : CompteSettings.getInstance().CompteVente;
                    credit.MontantCredit = this.MontantTotalHT;
                    credit.MontantDebit = 0;
                    credit.ObjetEcriture = "Facture de vente";
                    credit.RefDate = this.DateCreation;
                    credit.RefNumber = this.Name;
                    credit.RefFacture = this.Id;
                    credit.CompteContre = this.NomTier;
                    credit.Tier = this.Tier;
                    credit.Utilisateur = DataHelpers.ConnectedUser?.Id; 

                    EcritureJournal debit = new EcritureJournal();
                    debit.CompteJournal = this.CompteDebit.IsValide() ? this.CompteDebit : CompteSettings.getInstance().CompteTier;
                    debit.CompteContre = this.NomTier;
                    debit.DateEcriture = this.DateCreation;
                    debit.MontantCredit = 0;
                    debit.MontantDebit = this.MontantGlobalTTC;
                    debit.ObjetEcriture = "Facture de vente";
                    debit.RefDate = this.DateCreation;
                    debit.RefFacture = this.Id;
                    debit.RefNumber = this.Name;
                    debit.Tier = this.Tier;
                    debit.Utilisateur = DataHelpers.ConnectedUser?.Id; 

                    ej_result.Add(debit);
                    ej_result.Add(credit);
                    // Enregsitrement TVA

                    var taxes = this.TaxeLigne;
                    if (taxes.Any())
                    {
                        foreach (var taxe in taxes)
                        {
                            EcritureJournal credit_tva = new EcritureJournal();
                            credit_tva.CompteJournal = CompteSettings.getInstance().CompteTVA;
                            credit_tva.DateEcriture = this.DateCreation;
                            credit_tva.MontantCredit = taxe.MontantTaxe;
                            credit_tva.MontantDebit = 0;
                            credit_tva.ObjetEcriture = "TVA Collectée";
                            credit_tva.RefDate = this.DateCreation;
                            credit_tva.RefNumber = this.Name;
                            credit_tva.RefFacture = this.Id;
                            credit_tva.CompteContre = this.NomTier;
                            credit_tva.Tier = this.Tier;
                            credit_tva.Utilisateur = DataHelpers.ConnectedUser?.Id; 
                            ej_result.Add(credit_tva);
                        }
                    }

                }
                else
                {
                    // Enregistrement this

                    // Normalisé les qts positif

                    this.ArticleFacture.ForEach(a => a.Qts = System.Math.Abs(a.Qts));

                    EcritureJournal credit = new EcritureJournal();
                    credit.CompteJournal = this.CompteCredit.IsValide() ? this.CompteCredit : CompteSettings.getInstance().CompteVente;
                    //CompteSettings.getInstance().CompteVente;
                    credit.DateEcriture = this.DateCreation;
                    credit.MontantDebit = this.MontantTotalHT;
                    credit.MontantCredit = 0;
                    credit.ObjetEcriture = "Retour vente";
                    credit.RefDate = this.DateCreation;
                    credit.RefNumber = this.Name;
                    credit.RefFacture = this.Id;
                    credit.CompteContre = this.NomTier;
                    credit.Tier = this.Tier;
                    credit.Utilisateur = DataHelpers.ConnectedUser?.Id; 

                    EcritureJournal debit = new EcritureJournal();
                    debit.CompteJournal = this.CompteDebit.IsValide() ? this.CompteDebit : CompteSettings.getInstance().CompteTier;
                    //CompteSettings.getInstance().CompteTier;
                    debit.DateEcriture = this.DateCreation;
                    debit.CompteContre = this.NomTier;
                    debit.MontantDebit = 0;
                    debit.MontantCredit = this.MontantGlobalTTC;
                    debit.ObjetEcriture = "Retour vente";
                    debit.RefDate = this.DateCreation;
                    debit.RefFacture = this.Id;
                    debit.RefNumber = this.Name;
                    debit.Tier = this.Tier;
                    debit.Utilisateur = DataHelpers.ConnectedUser?.Id; 

                    ej_result.Add(debit);
                    ej_result.Add(credit);
                    // Enregsitrement TVA

                    var taxes = this.TaxeLigne;
                    if (taxes.Any())
                    {
                        foreach (var taxe in taxes)
                        {
                            EcritureJournal credit_tva = new EcritureJournal();
                            credit_tva.CompteJournal = CompteSettings.getInstance().CompteTVA;
                            credit_tva.MontantDebit = taxe.MontantTaxe;
                            credit_tva.DateEcriture = this.DateCreation;
                            credit_tva.MontantCredit = 0;
                            credit_tva.ObjetEcriture = "TVA Remboursée";
                            credit_tva.RefDate = this.DateCreation;
                            credit_tva.RefNumber = this.Name;
                            credit_tva.RefFacture = this.Id;
                            credit_tva.CompteContre = this.NomTier;
                            credit_tva.Tier = this.Tier;
                            credit_tva.Utilisateur = DataHelpers.ConnectedUser?.Id; 
                            ej_result.Add(credit_tva);
                        }
                    }

                }
            }

            ej_result.ForEach(a => a = CreateModesHandler.Handle(a, mode) as EcritureJournal);
            return ej_result;
        }

        #endregion
    }
}
