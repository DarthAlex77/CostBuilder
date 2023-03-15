using System.Collections.ObjectModel;
using CostBuilder.Model;
using CostBuilder.Views;
using DevExpress.Export;
using DevExpress.Mvvm;
using DevExpress.Utils;
using DevExpress.Xpf.Grid;
using DevExpress.XtraPrinting;
using Microsoft.Win32;

namespace CostBuilder.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        #region Constructor

        public MainViewModel()
        {
            OpenFileCommand            = new DelegateCommand(OpenFile);
            OpenReferenceEditorCommand = new DelegateCommand(OpenReferenceEditor);
            ExportToExcelCommand       = new DelegateCommand<TableView>(ExportToExcel);
        }

        #endregion

        #region Properties

        public ObservableCollection<Meal> Meals
        {
            get => GetValue<ObservableCollection<Meal>>();
            set => SetValue(value);
        }

        #endregion

        #region Commads

        #region OpenReferenceEditorCommand

        public DelegateCommand OpenReferenceEditorCommand { get; set; }

        private static void OpenReferenceEditor()
        {
            new DictionaryEditorWindow().ShowDialog();
        }

        #endregion

        #region OpenFileCommand

        public DelegateCommand OpenFileCommand { get; set; }

        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel Worksheets|*.xls;*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                Meals = new ObservableCollection<Meal>(Meal.GetMealsListFromExcel(dialog.FileName, true));
            }
        }

        #endregion

        #region ExportToExcelCommand

        public DelegateCommand<TableView> ExportToExcelCommand { get; set; }

        public void ExportToExcel(TableView tableView)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".xlsx";
            dialog.Filter     = "Excel Worksheets|*.xls;*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                tableView.ExportToXlsx(dialog.FileName);
            }
        }

        #endregion

        #endregion
    }
}