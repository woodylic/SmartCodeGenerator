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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvalonDock;
using System.ComponentModel;
using SmartCodeGenerator.Uility;
using System.Text.RegularExpressions;
using System.IO; 
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for ucCodeDocument.xaml
    /// </summary>
    public partial class ucBaseDocument : DocumentContent
    { 
        /// <summary>
        /// 
        /// </summary>
        public ucBaseDocument()
        {
            InitializeComponent();            
        }
        /// <summary>
        /// 
        /// </summary>
        public PropertyGrid DocPropertyGrid = new PropertyGrid();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CustomProperty GetProperty(string name)
        {
            foreach (CustomProperty cp in collection)
            {
                if (name.ToLower() == cp.Name.Trim().ToLower())
                {
                    return cp;
                }
            }
            return null;
        }
        /// <summary>
        /// /
        /// </summary>
        /// <returns></returns>
        public List<CustomProperty> GetParamters()
        {
            List<CustomProperty> cpList = new List<CustomProperty>(); 
            foreach (CustomProperty cp in collection)
            {
                if ("parameters" == cp.Category.Trim().ToLower())
                {
                    cpList.Add( cp);
                }
            }
            return cpList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpList"></param>
        public void SetParameterValues(List<CustomProperty> cpList)
        {
            if (cpList != null)
            {
                foreach (CustomProperty cp in cpList)
                {
                    foreach (CustomProperty ccp in collection)
                    {
                        if (cp.Name.Trim().ToLower() == ccp.Name.Trim().ToLower())
                        {
                            ccp.Value = cp.Value;
                            break;
                        }
                    }
                }
                
                DocPropertyGrid.SelectedObject = collection;
                DocPropertyGrid.Refresh();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cc"></param>
        public void LoadProperties(ContentControl cc)
        {
            collection.Clear();
            if (File.Exists(this.DocPath))
            {
                //SetOutputExtension(this.Content.ToString());
                TextEditor te = this.Content as TextEditor;
                System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(te.Text, @"<#@ parameter name="".*?"" ");
                this.PropertyCollection.Clear();
                List<string> pnList = new List<string>();
                foreach (Match item in ma)
                {
                    if (item.Success)
                    {
                        pnList.Add(item.Value.Substring(item.Value.IndexOf(@"""")).Replace(@"""", ""));
                    }
                }
                if (pnList.Count > 0)
                {
                    for (int i = 0; i < pnList.Count; i++)
                    {
                        CustomProperty cp = new CustomProperty();
                        cp.Category = "Parameters";
                        cp.Value = string.Empty;
                        cp.Name = pnList[i];
                        cp.IsBrowsable = true;
                        cp.ValueType = typeof(string);
                        collection.Add(cp);
                    }
                }
            }

            CustomProperty cpDocExt = new CustomProperty();
            cpDocExt.Category = "Document Properties";
            cpDocExt.Name = "DocExt";
            cpDocExt.DefaultValue = this.docExt;
            cpDocExt.Value =  this.docExt;
            cpDocExt.IsReadOnly = true;
            cpDocExt.IsBrowsable = true;
            cpDocExt.ValueType = typeof(string);

            CustomProperty cpDocType = new CustomProperty();
            cpDocType.Category = "Document Properties";
            cpDocType.Name = "DocType";
            cpDocType.DefaultValue = this.docType;
            cpDocType.Value = this.docType;
            //cpDocType.IsReadOnly = true;
            cpDocType.IsBrowsable = true;
            cpDocType.ValueType = typeof(DocumentType);

            //CustomProperty cpDocChanged = new CustomProperty();
            //cpDocChanged.Category = "Document Properties";
            //cpDocChanged.Name = "DocChanged";
            //cpDocChanged.DefaultValue = this.docChanged;
            //cpDocChanged.Value = this.docChanged;
            //cpDocChanged.IsReadOnly = true;
            //cpDocChanged.IsBrowsable = true;

            CustomProperty cpDocPath = new CustomProperty();
            cpDocPath.Category = "Document Properties";
            cpDocPath.Name = "DocPath";
            cpDocPath.DefaultValue = this.docPath;
            cpDocPath.Value = this.docPath;
            cpDocPath.IsReadOnly = true;
            cpDocPath.IsBrowsable = true;
            cpDocPath.ValueType = typeof(string);           
            
            collection.Add(cpDocType);
            collection.Add(cpDocPath);
            //collection.Add(cpDocChanged);
            collection.Add(cpDocExt);

            //var pg = new System.Windows.Forms.PropertyGrid();
            DocPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(DocPropertyGrid_PropertyValueChanged);
            DocPropertyGrid.SelectedObject = collection;            
            WindowsFormsHost wfh = new WindowsFormsHost();
            wfh.Child = DocPropertyGrid;
            cc.Content = wfh;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public void DocPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
             
                if ((e.ChangedItem).Label.ToLower() == "doctype")
                {
                    this.DocType= (DocumentType)(e.ChangedItem).Value;  
                }
             
        }
                    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputTemplate"></param>
        private void SetOutputExtension(string inputTemplate)
        {
            System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(inputTemplate, @"<#@ output extension="".*?"" ");
            foreach (Match item in ma)
            {
                if (item.Success)
                {
                    docExt = item.Value.Substring(item.Value.IndexOf(@"""")).Replace(@"""", "");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string docExt;

        [CategoryAttribute("Document Properties"),
        ReadOnlyAttribute(true)]
        public string DocExt
        {
            get
            {
                return docExt;
            }
            set
            {
                docExt = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private DocumentType docType;

        [CategoryAttribute("Document Properties"),   
        ReadOnlyAttribute(true) ] 
        public DocumentType DocType
        {
            get
            {
                return docType;
            }
            set
            {
                docType = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private string docPath;
        [CategoryAttribute("Document Properties"),
        ReadOnlyAttribute(true)] 
        public string DocPath
        {
            get
            {
                return docPath;
            }
            set
            {
                docPath = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private bool docChanged;
        [CategoryAttribute("Document Properties"),
        ReadOnlyAttribute(true)] 
        public bool DocChanged
        {
            get
            {
                return docChanged;
            }
            set
            {
                docChanged = value;
            }
        }
 
        /// <summary>
        /// 
        /// </summary>
        private CustomPropertyCollection collection = new CustomPropertyCollection();
        public  CustomPropertyCollection PropertyCollection
        {
            get { return collection; }
            set { collection = value; }
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public enum DocumentType
    {
        CodeDocument = 1,
        TemplateDocument = 2
    }
     
}
