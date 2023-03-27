using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using CostBuilder.Helpers;
using DevExpress.Mvvm;
using ExcelDataReader;

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
            return string.Equals(Name, other.Name, StringComparison.InvariantCulture) && string.Equals(Hotel, other.Hotel, StringComparison.InvariantCultureIgnoreCase);
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

        public static List<Meal> GetMealsListFromExcel(string filePath)
        {
            List<Meal> meals = new List<Meal>(5000);
            DataTable  table;
            try
            {
                using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                {
                    using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                    {
                        table = reader.AsDataSet().Tables[0];
                        foreach (DataRow row in table.Rows)
                        {
                            if (row[0].ToString().Contains("всего") || row[0].ToString().Contains("Итого") || !double.TryParse(row[4].ToString(), out _) ||
                                row[1].ToString().Contains("всего") || row[2].ToString().Contains("всего"))
                            {
                                row.Delete();
                            }
                        }
                        table.AcceptChanges();
                        table.RemoveEmptyColumns();
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Файл открыт в другой программе. Закройте его", "Закройте файл", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            string     hotelName   = string.Empty;
            string     mealName    = string.Empty;
            DateTime   lasDateTime = default;
            List<Meal> dict        = null;
            bool       isDictExist;

            try
            {
                using FileStream fileStream = File.Open("dict", FileMode.Open);
                {
                    dict = JsonSerializer.Deserialize<List<Meal>>(fileStream);
                }
                isDictExist = true;
            }
            catch (IOException e)
            {
                isDictExist = false;
            }
            foreach (DataRow row in table.Rows)
            {
                if (!string.IsNullOrWhiteSpace(row[0].ToString()))
                {
                    hotelName = row[0].ToString();
                }
                if (!string.IsNullOrWhiteSpace(row[1].ToString()))
                {
                    mealName = row[1].ToString();
                }
                if (DateTime.TryParse(row[2].ToString(), out DateTime t))
                {
                    lasDateTime = t;
                }
                Meal meal = new Meal
                {
                    Hotel         = hotelName,
                    Name          = mealName,
                    DayOfSale     = lasDateTime,
                    Count         = double.Parse(row[4].ToString()),
                    Sum           = decimal.Parse(row[5].ToString()),
                    SumWithoutVat = decimal.Parse(row[6].ToString()),
                    Cost          = decimal.Parse(row[7].ToString()),
                    CostPerUnit   = decimal.Parse(row[8].ToString())
                };
                if (isDictExist)
                {
                    Meal tmp = dict.Find(x => string.Equals(x.Hotel, meal.Hotel) && string.Equals(x.Name, meal.Name));
                    if (tmp != null)
                    {
                        meal.Category1 = tmp.Category1;
                        meal.Category2 = tmp.Category2;
                        meal.Category3 = tmp.Category3;
                        meal.Category4 = tmp.Category4;
                    }
                    else
                    {
                        meal.Category1 = row[3].ToString();
                        dict.Add(meal);
                    }
                }
                else
                {
                    meal.Category1 = row[3].ToString();
                }
                meals.Add(meal);
            }
            if (!isDictExist)
            {
                using FileStream fileStream = File.Open("dict", FileMode.Create);
                {
                    JsonSerializer.Serialize(fileStream, meals.Distinct());
                }
            }
            return meals;
        }

        #endregion

        #region Properties

        public string Hotel
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

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