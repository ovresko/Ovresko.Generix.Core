using MongoDB.Bson;
using MongoDbGenericRepository.Models;
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
    class SoldeStock_report : ModelBase<SoldeStock_report>
    {

        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = _("Stocks");
        public override string CollectionName { get; } = _("Inventaire");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "ChartBar";
        public override bool ShowInDesktop { get; set; } = true;
        // public override string NameField => "Article"; 
        public override string Name { get;set; }

        #endregion

        [ExDisplayName("Article")]
        [ColumnAttribute(ModelFieldType.ReadOnly,"")]
        public string Article { get; set; }

         

        public Article lArticle { get; set; }

        [ShowInTable]
        [ExDisplayName("Quantité Réel")]
        [Column(ModelFieldType.ReadOnly,"")]
        public string Qts { get; set; }

        [ShowInTable]
        [ExDisplayName("Réservée")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string QtsReserved { get; set; }


        [ShowInTable]
        [ExDisplayName("Disponible")]
        [Column(ModelFieldType.ReadOnly, "")]
        public decimal? QtsDisponible { get; set; }


        [ShowInTable]
        [ExDisplayName("Minimum")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string QtsMin { get; set; }

        [ShowInTable]
        [ExDisplayName("Valeur De Stock")]
        [Column(ModelFieldType.ReadOnly, "")]
        public string Valeur { get; set; }



        public override IEnumerable<ExtendedDocument> GetList()
        {
            var result = new List<SoldeStock_report>();
            var articles = DS.db.GetAll<Article>(a => true);

            var reservedd = DS.db.GetAll<CommandeVente>(a => a.DocStatus == 1);
            decimal? QtsTotal = 0;
            decimal? QtsTotalRes = 0;
            decimal? ValeurTotal = 0;
            foreach (var item in articles)
            {
                decimal? qts = 0;

                var reserved = reservedd?.Where(a => a.ArticleFacture.Select(e => e.lArticle).Contains(item.Id)
                && a.EstDelivrer() == false);
                qts= reserved?.SelectMany(a => a.ArticleFacture)?.Where(a => a.lArticle == item.Id)?.Sum(a => a.Qts);
                QtsTotalRes += qts;
                var taux = item.TauxStock();
                QtsTotal += taux;
                var val = (taux * item.PrixAchat).GetValueOrDefault(0);
                ValeurTotal += val;
                result.Add(new SoldeStock_report()
                {
                    Name = item.NameSearch,
                    lArticle = item,
                    QtsReserved = $"{qts}",
                    QtsDisponible = taux - qts,
                    QtsMin = $"{string.Format("{0:0.##}",item.QtsStockMinimum)}",
                    Article = item.NameSearch,
                    Qts = $"{taux}",
                    Valeur = $"{val.ToString("C")}"
                });
            }

            result.Add(new SoldeStock_report
            {
                Name = "----------------------",
                Qts = "--------------",
                QtsReserved = "--------------",
            });

            result.Add(new SoldeStock_report
            {
                Name = "Total",
                Qts = $"{QtsTotal}",
                QtsReserved = $"{QtsTotalRes}",
                Valeur = $"{ValeurTotal.GetValueOrDefault(0).ToString("C")}"
            });

            return result;

        }
         

        #region FILTRE

        [Column(ModelFieldType.BaseFilter, "EnRepture")]
        [ExDisplayName("En Rupture De Stock")]
        public string FilterEnRepture { get; set; }

        public IEnumerable<SoldeStock_report> EnRepture(IEnumerable<SoldeStock_report> articles)
        { 
            try
            {
                var result = articles.Where(a => a.lArticle?.TauxStock() <= a.lArticle?.QtsStockMinimum);
                return result;
            }
            catch (Exception s)
            {
                MessageBox.Show(s.Message);
                throw s;
            }
        }


        #endregion
    }
}
