using System;
using DevExpress.Mvvm;

namespace CostBuilder.Model
{
    public class Modificator : ViewModelBase
    {
        public string Hotel
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string MealName
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

        public decimal CostPerUnit
        {
            get => Cost / (decimal) Quantity;
        }
    }
}