using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using DevExpress.Xpf.Grid;

namespace CostBuilder.Model
{
    public class IsDetailButtonVisibleBindingConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue)
            {
                return false;
            }
            int       count  = (int) values[0];
            RowData   row    = (RowData) values[1];
            TableView view   = (TableView) row.View;
            int       handle = row.RowHandle.Value;
            if (count == 0)
            {
                if (view.Grid.IsMasterRowExpanded(handle))
                {
                    view.Grid.CollapseMasterRow(handle);
                }
                return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}