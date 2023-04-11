using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using CostBuilder.Model;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using Microsoft.Win32;

namespace CostBuilder.ViewModels
{
    internal class DictionaryEditorViewModel : ViewModelBase
    {
        #region Fields

        private bool _changesOccurred;

        #endregion

        #region Constructor

        public DictionaryEditorViewModel()
        {
            _changesOccurred     = false;
            Meals                = new BindingList<Meal>();
            SaveCommand          = new DelegateCommand(Save);
            ExportToExcelCommand = new DelegateCommand<TableView>(ExportToExcel);
            CheckChangesCommand  = new DelegateCommand<CancelEventArgs>(CheckChanges);
            try
            {
                using FileStream fileStream = File.Open("dict", FileMode.Open);
                {
                    Meals = JsonSerializer.Deserialize<BindingList<Meal>>(fileStream);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show("Файл справочника не найден.\nОткройте файл отчета в главном окне для генерации справочника", "Файл не найден", MessageBoxButton.OK, MessageBoxImage.Error);
                Meals = new BindingList<Meal>();
            }
            Meals.ListChanged += Meals_ListChanged;
        }



        #endregion

        #region Properties

        public BindingList<Meal> Meals
        {
            get => GetValue<BindingList<Meal>>();
            set => SetValue(value);
        }

        #endregion

        #region Methods

        private void Meals_ListChanged(object sender, ListChangedEventArgs e)
        {
            _changesOccurred = true;
        }

        #endregion

        #region Commands

        #region CheckChangesCommand

        public DelegateCommand<CancelEventArgs> CheckChangesCommand { get; set; }

        private void CheckChanges(CancelEventArgs obj)
        {
            if (_changesOccurred)
            {
                MessageBoxResult result = MessageBox.Show("Произошли изменения в словаре.\n Вы хотите сохранить их перед закрытием ?",
                                                          "Сохранить",
                                                          MessageBoxButton.YesNoCancel,
                                                          MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.Cancel:
                        obj.Cancel = true;
                        break;
                    case MessageBoxResult.None:
                        break;
                    case MessageBoxResult.OK:
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        #region SaveCommand

        public DelegateCommand SaveCommand { get; set; }

        private void Save()
        {
            using FileStream createStream = File.Create("dict");
            {
                JsonSerializer.Serialize(createStream,
                                         Meals,
                                         new JsonSerializerOptions
                                         {
                                             WriteIndented = true,
                                             Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
                                         });
            }
            _changesOccurred = false;
        }

        #endregion

        #region ExportToExcelCommand

        public DelegateCommand<TableView> ExportToExcelCommand { get; set; }

        public void ExportToExcel(TableView tableView)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".xlsx";
            dialog.Filter     = "Excel Worksheets|*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                tableView.ExportToXlsx(dialog.FileName);
            }
        }

        #endregion

        #endregion
    }
}