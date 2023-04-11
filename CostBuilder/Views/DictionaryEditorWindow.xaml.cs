using System;
using System.ComponentModel;
using DevExpress.Xpf.Grid;

namespace CostBuilder.Views
{
    public partial class DictionaryEditorWindow
    {
        private bool _collectChanging;

        public DictionaryEditorWindow()
        {
            InitializeComponent();
        }

        private void GridViewBase_OnCellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            e.Source.PostEditor();
        }

        private void GridViewBase_OnCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (_collectChanging)
            {
                return;
            }

            _collectChanging = true;
            TableView tableView = (TableView) sender;
            tableView.Grid.BeginDataUpdate();
            foreach (int rowHandle in tableView.Grid.GetSelectedRowHandles())
            {
                tableView.Grid.SetCellValue(rowHandle, e.Column, e.Cell.Value);
            }
            tableView.Grid.EndDataUpdate();
            _collectChanging = false;
        }
    }
}