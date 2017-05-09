using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SmartCodeGenerator.Uility.ExcelToScript;
using System.IO;
using System.Data;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for winExcelToScript.xaml
    /// </summary>
    public partial class winExcelToScript : Window
    {
        private Workbook spreadsheet;
        private winMain winMainForm;

        public winExcelToScript(winMain mainform)
        {
            InitializeComponent();
            winMainForm = mainform;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Worksheets|*.xls;*.xlsx";
            if ( openFileDialog.ShowDialog(this) == true)
            {
                spreadsheet = new Workbook(openFileDialog.FileName);
                var fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                Title = string.Format("Script Generator - {0}", fileName);
                txtTableName.Text = fileName;
                this.tvSheets .ItemsSource = spreadsheet.Tables.Keys;
            }
        }

        private void LoadDataTable(DataTable datasource)
        {
            this.lvSheetData.DataContext = datasource;
            var gridView = this.lvSheetData.View as GridView;
            gridView.Columns.Clear();

            foreach (DataColumn col in datasource.Columns)
            {
                GridViewColumn gvc = new GridViewColumn();
                gvc.DisplayMemberBinding = new Binding(col.ColumnName);
                gvc.CellTemplate = new DataTemplate();
                gvc.Header = col.ColumnName;
                gridView.Columns.Add(gvc);
            }
            lvSheetData.SetBinding(ListView.ItemsSourceProperty, new Binding());
        }

        private void btnGenerateSql_Click(object sender, RoutedEventArgs e)
        {
            if (null == tvSheets.SelectedItem)
            {
                MessageBox.Show("Please select a sheet first.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var sheetName = this.tvSheets.SelectedItem.ToString();
            var tableValue = spreadsheet.Tables[sheetName];
            var tableName = txtTableName.Text;
            if (string.IsNullOrEmpty(tableName)) MessageBox.Show("Please specify table name first.", "", MessageBoxButton.OK, MessageBoxImage.Error);

            if (true == rbUpdate.IsChecked && 0 == listPrimaryKey.SelectedItems.Count)
            {
                MessageBox.Show("Please specify primary key first.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
             
            var workbookName = Path.GetFileNameWithoutExtension(spreadsheet.WorkbookPath);
            var fileNameSuffix = (true == rbUpdate.IsChecked ? "update" : "insert");
            StringBuilder writer = new StringBuilder(); 
            if (rbInsert.IsChecked == true)
            {
                SqlGenerator.GenerateInsertStatements(tableName, tableValue, ref writer);
            }
            else if (rbUpdate.IsChecked == true)
            {
                var pks = listPrimaryKey.SelectedItems.Cast<string>().ToArray();
                SqlGenerator.GenerateUpdateStatements(tableName, pks, tableValue,ref writer);
            }
            ucBaseDocument doc = new ucBaseDocument()
            {
                Title = string.Format("{0}_{1}", this.txtTableName.Text, fileNameSuffix),
                Content = new TextEditor()
                {
                    IsReadOnly = false,
                    Text = writer.ToString(),
                    SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".sql")
                }
            };
            doc.DocExt = "sql";
            doc.DocType = DocumentType.CodeDocument;          
            winMainForm.CreateDoc(doc);
 
        }

        private void tvSheets_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (spreadsheet == null) return;
            var currentDatatable = spreadsheet.Tables[e.NewValue.ToString()];
            LoadDataTable(currentDatatable);
            listPrimaryKey.ItemsSource = from col in currentDatatable.Columns.Cast<DataColumn>()
                                         select col.ColumnName;
        }
    }
}
