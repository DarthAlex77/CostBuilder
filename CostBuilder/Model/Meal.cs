using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using ExcelDataReader;
using Microsoft.Win32;

namespace CostBuilder.Model
{
    public class Meal : BindableBase, IEquatable<Meal>
    {
        #region Constructors

        public Meal()
        {
            Category1 = string.Empty;
            Category2 = string.Empty;
            Category3 = string.Empty;
            Category4 = string.Empty;
        }

        public Meal(string name, string category1, string category2, string category3, string category4)
        {
            Name      = name;
            Category1 = category1;
            Category2 = category2;
            Category3 = category3;
            Category4 = category4;
        }

        #endregion

        #region Methods

        #region IEquatable

        public bool Equals(Meal other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name, StringComparison.InvariantCulture) && string.Equals(Category1, other.Category1);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == GetType() && Equals((Meal) obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);
        }

        #endregion

        public static List<Meal> GetMealsListFromExcel(string filePath, bool askForDict)
        {
            bool                                        isDictOpened = false;
            List<Meal>                                  meals        = new List<Meal>(500);
            Dictionary<string, Dictionary<int, string>> dictionary   = null;
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataTable table = reader.AsDataSet().Tables[0];
                    foreach (DataRow row in table.Rows)
                    {
                        if (row[0].ToString().Contains("всего") || row[0].ToString().Contains("Итого") || !double.TryParse(row[3].ToString(), out _) || row[1].ToString().Contains("всего"))
                        {
                            row.Delete();
                        }
                    }
                    table.AcceptChanges();
                    string   mealName    = string.Empty;
                    DateTime lasDateTime = default;
                    if (askForDict)
                    {
                        if (ThemedMessageBox.Show("Открыть словарь?", "Вы хотите открыть файл словаря для этого отчета", MessageBoxButton.YesNo, MessageBoxResult.OK) == MessageBoxResult.Yes)
                        {
                            OpenFileDialog dialog = new OpenFileDialog();
                            dialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\dict";
                            dialog.Filter           = "CostBuilder dict file (*.dict)|*.dict";
                            if (dialog.ShowDialog() == true)
                            {
                                using FileStream st = File.Open(dialog.FileName, FileMode.Open);
                                {
                                    dictionary = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, string>>>(st);
                                }
                                isDictOpened = true;
                            }
                        }
                    }
                    foreach (DataRow row in table.Rows)
                    {
                        if (!string.IsNullOrWhiteSpace(row[0].ToString()))
                        {
                            mealName = row[0].ToString();
                        }
                        if (DateTime.TryParse(row[1].ToString(), out DateTime t))
                        {
                            lasDateTime = t;
                        }
                        try
                        {
                            Meal meal = new Meal
                            {
                                Name          = mealName,
                                DayOfSale     = lasDateTime,
                                Count         = double.Parse(row[3].ToString()),
                                Sum           = decimal.Parse(row[4].ToString()),
                                SumWithoutVat = decimal.Parse(row[6].ToString()),
                                Cost          = decimal.Parse(row[7].ToString()),
                                CostPerUnit   = decimal.Parse(row[8].ToString())
                            };
                            if (isDictOpened)
                            {
                                meal.Category1 = dictionary[mealName][1];
                                meal.Category2 = dictionary[mealName][2];
                                meal.Category3 = dictionary[mealName][3];
                                meal.Category4 = dictionary[mealName][4];
                            }
                            else
                            {
                                meal.Category1 = row[2].ToString();
                            }

                            meals.Add(meal);
                        }
                        catch (FormatException)
                        {
                            // ignored
                        }
                    }
                }
            }
            return meals;
        }

        #endregion

        #region Properties

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public DateTime DayOfSale
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }

        public double Count
        {
            get => GetValue<double>();
            set => SetValue(value);
        }

        public decimal Sum
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }

        public decimal SumWithoutVat
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }

        public decimal Cost
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }

        public decimal CostPerUnit
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }
        public string Category1
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Category2
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Category3
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Category4
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        #endregion
    }
}