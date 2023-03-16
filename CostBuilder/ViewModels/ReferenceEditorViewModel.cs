using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CostBuilder.Model;
using DevExpress.Mvvm;
using Microsoft.Win32;

namespace CostBuilder.ViewModels
{
    internal class ReferenceEditorViewModel : ViewModelBase
    {
        #region Constructor

        public ReferenceEditorViewModel()
        {
            CloseTrigger = false;
            Meals        = new ObservableCollection<Meal>();
            SaveCommand  = new DelegateCommand(Save);
            LoadCommand  = new DelegateCommand(Load);
            NewCommand   = new DelegateCommand(New);
        }

        #endregion

        #region Properties

        public bool CloseTrigger
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public ObservableCollection<Meal> Meals
        {
            get => GetValue<ObservableCollection<Meal>>();
            set => SetValue(value);
        }

        #endregion

        #region Commands

        #region LoadCommad

        public DelegateCommand LoadCommand { get; set; }

        private void Load()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\dict";
            dialog.Filter           = "CostBuilder dict file (*.dict)|*.dict";
            if (dialog.ShowDialog() == true)
            {
                using FileStream                            st = File.Open(dialog.FileName, FileMode.Open);
                Dictionary<string, Dictionary<int, string>> dict;
                {
                    dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, string>>>(st);
                }
                foreach (KeyValuePair<string, Dictionary<int, string>> pair in dict)
                {
                    Meals.Add(new Meal(pair.Key, pair.Value[1], pair.Value[2], pair.Value[3], pair.Value[4]));
                }
            }
        }

        #endregion

        #region SaveCommand

        public DelegateCommand SaveCommand { get; set; }

        private void Save()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt       = ".dict";
            dialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\dict";
            dialog.Filter           = "CostBuilder dict file (*.dict)|*.dict";
            if (dialog.ShowDialog() == true)
            {
                Dictionary<string, Dictionary<int, string>> dict = new Dictionary<string, Dictionary<int, string>>();
                foreach (Meal meal in Meals)
                {
                    if (!dict.ContainsKey(meal.Name))
                    {
                        dict.Add(meal.Name,
                                 new Dictionary<int, string>
                                 {
                                     {1, meal.Category1},
                                     {2, meal.Category2},
                                     {3, meal.Category3},
                                     {4, meal.Category4}
                                 });
                    }
                    using FileStream createStream = File.Create(dialog.FileName);
                    {
                        JsonSerializer.Serialize(createStream,
                                                 dict,
                                                 new JsonSerializerOptions
                                                 {
                                                     WriteIndented = true,
                                                     Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
                                                 });
                    }
                    CloseTrigger = true;
                }
            }
        }

        #endregion

        #region NewCommad

        public DelegateCommand NewCommand { get; set; }

        public void New()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel Worksheets|*.xls;*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                Meals = new ObservableCollection<Meal>(Meal.GetMealsListFromExcel(dialog.FileName, false).Distinct());
            }
        }

        #endregion

        #endregion
    }
}