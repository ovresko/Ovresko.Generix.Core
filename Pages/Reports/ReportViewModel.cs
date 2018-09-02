using Ovresko.Generix.Core.Modules;
using LiveCharts;
using LiveCharts.Wpf;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Base;
using MaterialDesignThemes.Wpf;

namespace Ovresko.Generix.Core.Pages.Reports
{
    public class AChartValue
    {
        public string xValue { get; set; }
        public decimal yValue { get; set; }
        public Brush Color { get; set; }
    }

    public abstract class IOvReport
    {
        public abstract List<OvTreeItem> GetReport();
        public abstract string[] GetHeaders();
        public abstract string ReportName { get; set; }
        public virtual string BG { get; set; } = "White";
        public abstract List<AChartValue> GetChartValues();
    }


    public class OvTreeItem
    {
        public List<OvTreeItem> Children { get; set; } = new List<OvTreeItem>();

        public string CL1 { get; set; }
        public string CL2 { get; set; }
        public string CL3 { get; set; }
        public string CL4 { get; set; }
        public string CL5 { get; set; }

        public FontWeight CL1FontWeight { get; set; } = FontWeights.Normal;
        public FontWeight CL2FontWeight { get; set; } = FontWeights.Normal;
        
    }

    class ReportViewModel : Screen, IDisposable
    {

        public IEnumerable<IReportViewModel> Reports { get; set; }
        public WrapPanel RepportsContent { get; set; }
        public ReportViewModel(List<IReportViewModel> reports)
        {
            RepportsContent = new WrapPanel();
            Reports = reports;
            Setup();
        }

        private void Setup()
        {
            foreach (var item in Reports)
            {
                Card card = new Card();
                card.UniformCornerRadius = 4;
                var view = DataHelpers.container.Get<ViewManager>().CreateAndBindViewForModelIfNecessary(item);
                card.Content = view;
                card.Width = 600;
                card.Height = 350;
                RepportsContent.Children.Add(card);
            }

            NotifyOfPropertyChange("RepportsContent");
        }

        public ReportViewModel()
        {

        }

        #region CHART 

        public SeriesCollection ReportSeries { get; set; }
       // public IEnumerable<AChartValue> aChartValues { get; private set; }
        public string TitleX { get; set; } = ""; 
        public string[] Labels { get; set; }
        public Func<decimal,string> Formatter { get; set; }

        public void LoadChart()
        {

            //var values = Report.GetChartValues();
            //ReportSeries = new SeriesCollection()
            //{
            //    new ColumnSeries
            //    {
            //        Title = TitleX,
            //        Values = new ChartValues<decimal>(values.Select(z => z.yValue)),
                    
            //    }
            //};

            //Labels = values.Select(z => z.xValue).ToArray();
            //Formatter = v => v.ToString("N");

            NotifyOfPropertyChange("ReportSeries");
        }

        #endregion


        private IEnumerable<OvTreeItem> lines;
        
        public ReportViewModel(IOvReport report)
        {

            ReportSeries = new SeriesCollection();
            Report = report;

           // LoadChart();
        }


        public string CL1 { get { return Report.GetHeaders()[0]; } }
        public string CL2 { get { return Report.GetHeaders()[1]; } }
        public string CL3 { get { return Report.GetHeaders()[2]; } }
        public string CL4 { get { return Report.GetHeaders()[3]; } }
        public string CL5 { get { return Report.GetHeaders()[4]; } }
        public string BG { get { return Report.BG; } }

        public List<OvTreeItem> Items
        {
            get
            {
                var result =  Report.GetReport();
                Execute.OnUIThread(LoadChart);
                return result;
            }
        }
        
        public IOvReport Report { get; set; }
        
        public void refresh()
        {

            NotifyOfPropertyChange("Items");
            //Items.Add(new OvTreeItem()
            //{
            //    CL1 = "Hello",
            //    CL2 = "Red",
            //    CL3 = "cl3",
            //    Children = new List<OvTreeItem>()
            //{
            //    new OvTreeItem(){CL1 = "Hello",CL2 ="Red" },
            //    new OvTreeItem(){CL1 = "Hello",CL3 ="Red" }

            // }
            //});
            NotifyOfPropertyChange("Items");
        }

        public void PrintReport()
        {

            var posPrinter = ElvaSettings.getInstance().ImprimanteDefault;
            if (string.IsNullOrEmpty(posPrinter))
            {
                 DataHelpers.ShowMessage( "Imprimante introuvable, vérifier les paramétres");
                return;
            }

            File.WriteAllText("report.txt", $"{this.Report.ReportName}\n");
            
            using (var file = new StreamWriter("report.txt", true))
            {
                var typeRepor =  DataHelpers.ShowMessage( "Inclure les quantités vendues", "Type rapport", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(typeRepor == MessageBoxResult.Yes)
                {
                     lines = Report.GetReport().SelectMany(z => z.Children);
                }
                else
                {
                    lines = Report.GetReport().Where(z => !z.CL1.Contains("Détails")).SelectMany(z => z.Children);
                }
              
                foreach (var item in lines)
                {
                    file.WriteLine($"{item.CL1}\n{item.CL2} {item.CL3}\n___________________");

                }
                file.Close();
                Process p = null;
                try
                {
                    
                    p = new Process();
                    p.StartInfo.FileName = Path.GetFullPath("report.txt");

                    var verbs = p.StartInfo.Verbs;
                    foreach (var v in verbs)
                    {
                        Console.WriteLine(v);
                    }
                    p.StartInfo.Verb = "Print";
                    p.StartInfo.Arguments = "\"" + posPrinter + "\"";
                    p.StartInfo.Verb = "Printto";
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                    p.WaitForExit();
                }
                catch (Exception e)
                {
                     DataHelpers.ShowMessage( e.Message);
                }

            }


        }
        
        public void Dispose()
        {

        }
    }
}
