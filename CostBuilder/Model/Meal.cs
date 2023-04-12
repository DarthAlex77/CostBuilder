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

        private static void RemoveEmptyColumns(ref DataTable table, int columnStartIndex = 0)
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

        private static void ClearUnusedRow(string filePath, ref DataTable table)
        {
            try
            {
                FileStream       stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                table = reader.AsDataSet().Tables[0];
                reader.Dispose();
                stream.Dispose();
            }
            catch (IOException)
            {
                MessageBox.Show("Файл открыт в другой программе. Закройте его", "Закройте файл", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            RemoveEmptyColumns(ref table);
            table.Rows.RemoveAt(table.Rows.Count - 1);
            foreach (DataRow row in table.Rows)
            {
                if (double.TryParse(row[6].ToString(), out _))
                {
                    break;
                }
                row.Delete();
            }
            table.AcceptChanges();
            foreach (DataRow row in table.Rows)
            {
                if (row[0].ToString().Contains("всего") || row[1].ToString().Equals(" всего") || row[2].ToString().Contains("всего") || row[3].ToString().Contains("всего") ||
                    row[4].ToString().Contains("всего") || row[5].ToString().Contains("всего"))
                {
                    row.Delete();
                }
            }
            table.AcceptChanges();
        }

        public static List<Meal> GetMealsListFromExcel(string filePath)
        {
            List<Meal>        meals = new List<Meal>(25000);
            List<Modificator> mods  = new List<Modificator>(25000);
            DataTable         table = null;
            ClearUnusedRow(filePath, ref table);
            if (table==null)
            {
                return new List<Meal>();
            }

            bool       isModificator = false;
            string     hotelName     = string.Empty;
            string     modName       = string.Empty;
            string     mealName      = string.Empty;
            string     unit          = string.Empty;
            Meal       meal;
            DateTime   lasDateTime = default;
            List<Meal> dict        = null!;
            bool       isDictExist;
            Meal       tmp;

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
                if (row[1].ToString() == mealName + " всего")
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
                if (!string.IsNullOrWhiteSpace(row[4].ToString()))
                {
                    unit = row[4].ToString();
                }
                if (isModificator)
                {
                    Modificator modificator = new Modificator
                    {
                        MealName      = mealName,
                        Name          = modName,
                        Hotel         = hotelName,
                        DayOfSale     = lasDateTime,
                        Quantity      = double.Parse(row[6].ToString()),
                        Sum           = decimal.Parse(row[7].ToString()),
                        SumWithoutVat = decimal.Parse(row[8].ToString()),
                        Cost          = decimal.Parse(row[9].ToString())
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
                        Unit          = unit,
                        MealGroup     = row[5].ToString(),
                        Quantity      = double.Parse(row[6].ToString()),
                        Sum           = decimal.Parse(row[7].ToString()),
                        SumWithoutVat = decimal.Parse(row[8].ToString()),
                        Cost          = decimal.Parse(row[9].ToString()),
                        DoNotShow     = false
                    };

                    if (isDictExist)
                    {
                        tmp = dict.Find(x => string.Equals(x.Hotel, meal.Hotel) && string.Equals(x.Name, meal.Name));
                        if (tmp != null)
                        {
                            meal.DoNotShow = tmp.DoNotShow;
                            meal.Category1 = tmp.Category1;
                            meal.Category2 = tmp.Category2;
                            meal.Category3 = tmp.Category3;
                        }
                        else
                        {
                            dict.Add(meal);
                        }
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
            tmp              =   meals.Find(meal1 => meal1.Hotel == mods[0].Hotel && meal1.Name == mods[0].MealName && meal1.DayOfSale == mods[0].DayOfSale);
            tmp.Modificators ??= new BindingList<Modificator>();
            foreach (Modificator modificator in mods)
            {
                if (modificator.Hotel == tmp.Hotel && modificator.MealName == tmp.Name && modificator.DayOfSale == tmp.DayOfSale)
                {
                    tmp.Modificators.Add(modificator);
                }
                else
                {
                    tmp              =   meals.Find(meal1 => meal1.Hotel == modificator.Hotel && meal1.Name == modificator.MealName && meal1.DayOfSale == modificator.DayOfSale);
                    tmp.Modificators ??= new BindingList<Modificator>();
                    tmp.Modificators.Add(modificator);
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

        public double Quantity
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
                if (Modificators != null && Modificators.Any())
                {
                    return Cost + Modificators.Sum(x => x.Cost);
                }
                return Cost;
            }
        }

        public decimal CostPerUnit
        {
            get
            {
                if (CalculatedCost != 0 || Quantity != 0)
                {
                    return CalculatedCost / (decimal) Quantity;
                }
                return 0;
            }
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