using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CostBuilder.Model;
using DevExpress.Mvvm;

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
            using FileStream fileStream = File.Open("dict", FileMode.Open);
            {
                Meals = JsonSerializer.Deserialize<ObservableCollection<Meal>>(fileStream);
            }
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
            CloseTrigger = true;
        }

        #endregion
    }
}