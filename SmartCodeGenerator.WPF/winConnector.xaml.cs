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
using System.Windows.Shapes;
using SmartCodeGenerator.Uility;
using System.Data;
using AvalonDock;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for wConnector.xaml
    /// </summary>
    public partial class winConnector : Window
    {
        /// <summary>
        /// 
        /// </summary>
        protected bool isConnected = false;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SqlDBHelper dbHelper = new SqlDBHelper();
        public string ServerName = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        public winConnector()
        {            
            InitializeComponent();                     
        }

        /// <summary>
        /// Get Server List By SQLDMO Com
        /// </summary>
        private void InitServerList()
        {
            this.cboxServerList.Items.Clear();
            SQLDMO.Application sqlApp = new SQLDMO.Application();
            SQLDMO.NameList sqlServers = sqlApp.ListAvailableSQLServers();
            for (int i = 0; i < sqlServers.Count; i++)
            {
                object srv = sqlServers.Item(i + 1);
                if (srv != null)
                {
                    this.cboxServerList.Items.Add(srv);
                }
            }
            if (this.cboxServerList.Items.Count > 0)
                this.cboxServerList.SelectedIndex = 0;
            else
            {
                this.cboxServerList.Text = "<No available SQL Servers>";
            }
        }
        /// <summary>
        /// Get Database Instance List
        /// </summary>
        private void InitDatabases()
        {
            if (this.cboxServerList.Items.Count > 0 || !string.IsNullOrEmpty(this.cboxServerList.Text))
            {
                try
                {
                    bool trusted = false;
                    if (cboxAuthentication.Text == "Windows Authentication")
                    {
                        trusted = true;
                    }
                    dbHelper.SetConnection(this.cboxServerList.Text, string.Empty, this.txtLoginName.Text, this.txtPassword.SecurePassword.ToString(), trusted);
                    if (dbHelper.Open())
                    {
                        this.ServerName = this.cboxServerList.Text;
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Cannot connect to " + this.cboxServerList.Text + " sever.");
                    }
                    this.isConnected = true; 
                }
                catch
                {
                    this.isConnected = false;
                    MessageBox.Show("You cannot access current sever.");
                }
            }
            else
            {
                this.isConnected = false;
                MessageBox.Show("No available SQL Servers.");
            }
        }

        /// <summary>
        /// Window Closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.mainWin.connector = null;
        }
        /// <summary>
        /// Click Connect Button to Init Database Instance list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            InitDatabases();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxAutoGetServerList_Click(object sender, RoutedEventArgs e)
        {
            InitServerList();   
        }
 
    }
}
