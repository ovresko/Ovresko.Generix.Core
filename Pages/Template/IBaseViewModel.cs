using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.Events;
using Ovresko.Generix.Datasource.Models;
using Stylet;

namespace Ovresko.Generix.Core.Pages.Template
{
    public interface IBaseViewModel 
    {
        string ActionsLbl { get; }
        bool actionsVisible { get; set; }
        string btnFermerText { get; }
        string btnNouveauHint { get; }
        string btnNouveauText { get; }
        int CellPadding { get; set; }
        string ChercherHint { get; }
        string DataGridTag { get; set; }
        DataHelpers datahelper { get; set; }
        string displayName { get; }
        string ElementsCount { get; }
        WrapPanel Filtres { get; set; }
        bool FiltreVisible { get; set; }
        int fontSize { get; set; }
        bool ForSelectOnly { get; set; }
        FlowDirection GetFlowDirection { get; }
        bool IsRunning { get; set; }
         
        string LblAddBureau { get; }
        string LblEditionMasse { get; }
        string LblExporter { get; }
        string LblExportertemplate { get; }
        string LblImporter { get; }
        string LblModifierModel { get; }
        string LblOutils { get; }
        string LblSupprimer { get; }
        List<MenuItem> MenuItems { get; set; }
        SnackbarMessageQueue MessageQueue { get; set; }
        string NameSearch { get; set; }
        List<MenuItem> opeartionButtons { get; set; }
        
        int PageCount { get; set; }
        int PageNumber { get; set; }
        int PagesNumber { get; }
        int RowHeight { get; set; }
        bool SaveVisible { get; }

        IDocument selected { get; set; }
        IEnumerable<IDocument> selectedList { get; set; }
        IShell shell { get; set; }
        List<int> ShowCounts { get; }
        string StatusLabel { get; set; }
        long TotalCount { get; set; }
        Type type { get; }

        Task Actualiser();
        void Add();
        void AjouterAuBureau();
        void BigFont();
        void CloseWindows();
        void DeleteAll();
        void Dispose();
        void doFiltrer();
        void DoSearchKey();
        void ExporterPDF();
        void ExportTemplate();
        Task FilterThread();
        void GridKeyUp(object sender, KeyEventArgs e);
        void Handle(ModelChangeEvent message);
        void ImportData();
        void InitContextMenu();
        void MassEdit();
        void ModifierModule();
        void nextPage();
        Task NextPage();
        void ouvrirItem();
        void prevPage();
        void SearchKeyUp(object sender, KeyEventArgs e);
        void SelectAll();
        void SetupOperationButtons();
        void SmallFont();
        void UserControl_KeyDown(object sender, KeyEventArgs args);
        void ValidateAll();
    }
}