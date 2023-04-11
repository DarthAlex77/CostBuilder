using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using DevExpress.Mvvm;
using ExcelDataReader;

namespace CostBuilder.Model
{
    public class Meal : BindableBase, IEquatable<Meal>
    {
        #region Constructor

        public Meal()
        {
            Unit         = string.Empty;
            MealGroup    = string.Empty;
            Category1    = string.Empty;
            Category2    = string.Empty;
            Category3    = string.Empty;
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

        public static void RemoveEmptyColumns(ref DataTable table, int columnStartIndex = 0)
        {
            for (int i = table.Columns.Count - 1; i >= columnStartIndex; i--)
            {
                DataColumn col = table.Columns[i];
                if (table.AsEnumerable().All(r => r.IsNull(col) || string.IsNullOrWhiteSpace(r[col].ToString())))
                {
                    table.Columns.RemoveAt(i);
                }
            }
            table.AcceptChanges();
        }

        public static List<Meal> GetMealsListFromExcel(string filePath)
        {
            List<Meal>        meals = new List<Meal>(25000);
            List<Modificator> mods  = new List<Modificator>(25000);
            DataTable         table;
            //Clear unused row and headers
            try
            {
                using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                {
                    using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                    {
                        table = reader.AsDataSet().Tables[0];
                        RemoveEmptyColumns(ref table);
                        table.Rows.RemoveAt(table.Rows.Count);
                        foreach (DataRow row in table.Rows)
                        {
                            if (row[0].ToString().Contains("всего") || row[2].ToString().Contains("всего") ||
                                row[3].ToString().Contains("всего") || !double.TryParse(row[6].ToString(), out _))
                            {
                                row.Delete();
                            }
                        }
                        table.AcceptChanges();
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Файл открыт в другой программе. Закройте его", "Закройте файл", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Meal>(0);
            }
            //end Clear unused row and headers
            bool       isModificator = false;
            string     hotelName     = string.Empty;
            string     modName       = string.Empty;
            string     mealName      = string.Empty;
            DateTime   lasDateTime   = default;
            List<Meal> dict          = null;
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
            Modificator modificator;
            Meal        meal = null;
            foreach (DataRow row in table.Rows)
            {
                if (row[1].ToString().Contains("всего"))
                {
                    isModificator = false;
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(row[0].ToString()))
                {
                    hotelName = row[0].ToString();
                }
                if (!string.IsNullOrWhiteSpace(row[1].ToString()))
                {
                    mealName      = row[1].ToString();
                    isModificator = true;
                }
                if (!string.IsNullOrWhiteSpace(row[2].ToString()))
                {
                    if (isModificator)
                    {
                        modName = row[2].ToString();
                    }
                    else
                    {
                        mealName = row[2].ToString();
                    }
                }
                if (DateTime.TryParse(row[3].ToString(), out DateTime t))
                {
                    lasDateTime = t;
                }
                if (isModificator)
                {
                    modificator = new Modificator
                    {
                        MealName      = mealName,
                        Name          = modName,
                        Hotel         = hotelName,
                        DayOfSale     = lasDateTime,
                        Count         = double.Parse(row[6].ToString()),
                        Sum           = decimal.Parse(row[7].ToString()),
                        SumWithoutVat = decimal.Parse(row[8].ToString()),
                        Cost          = decimal.Parse(row[9].ToString()),
                        CostPerUnit   = decimal.Parse(row[10].ToString())
                    };
                    mods.Add(modificator);
                }
                else
                {
                    meal = new Meal
                    {
                        Hotel         = hotelName,
                        Name          = mealName,
                        DayOfSale     = lasDateTime,
                        Count         = double.Parse(row[6].ToString()),
                        Sum           = decimal.Parse(row[7].ToString()),
                        SumWithoutVat = decimal.Parse(row[8].ToString()),
                        Cost          = decimal.Parse(row[9].ToString()),
                        CostPerUnit   = decimal.Parse(row[10].ToString()),
                        DoNotShow     = false
                    };

                    if (isDictExist)
                    {
                        Meal tmp = dict.Find(x => string.Equals(x.Hotel, meal.Hotel) && string.Equals(x.Name, meal.Name));
                        if (tmp != null)
                        {
                            meal.DoNotShow = tmp.DoNotShow;
                            meal.Unit      = tmp.Unit;
                            meal.MealGroup = tmp.MealGroup;
                            meal.Category1 = tmp.Category1;
                            meal.Category2 = tmp.Category2;
                            meal.Category3 = tmp.Category3;
                        }
                        else
                        {
                            meal.Unit      = row[4].ToString();
                            meal.MealGroup = row[5].ToString();
                            dict.Add(meal);
                        }
                    }
                    else
                    {
                        meal.Unit      = row[4].ToString();
                        meal.MealGroup = row[5].ToString();
                    }
                    if (!meal.DoNotShow)
                    {
                        meals.Add(meal);
                    }
                }
            }
            if (!isDictExist)
            {
                using FileStream fileStream = File.Open("dict", FileMode.Create);
                {
                    JsonSerializer.Serialize(fileStream, meals.Distinct());
                }
            }
            foreach (Meal meal1 in meals)
            {
                List<Modificator> m =mods.FindAll(x => x.Hotel == meal1.Hotel && x.MealName == meal1.Name && x.DayOfSale == meal1.DayOfSale);
                if (m.Any())
                {
                    meal1.Modificators = new BindingList<Modificator>(m);
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
        public decimal CalculatedCost
        {
            get
            {
                if (Modificators!=null&&Modificators.Any())
                {
                    return Cost + Modificators.Sum(x => x.Cost);
                }
                return Cost;
            }
        }

        public decimal CostPerUnit
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }
        public bool DoNotShow
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        public string Unit
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string MealGroup
        {
            get => GetValue<string>();
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
        public BindingList<Modificator> Modificators
        {
            get => GetValue<BindingList<Modificator>>();
            set => SetValue(value);
        }

        #endregion
    }
}