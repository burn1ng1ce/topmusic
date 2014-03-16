using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DotRas;

namespace TopMusic
{
    public partial class FormConfig : Form
    {
        private Config config;
        private RasDialer dialer = null;

        public FormConfig(Config config)
        {
            InitializeComponent();
            this.config = config;
        }

        private void FormRasDial_Load(object sender, EventArgs e)
        {
            ((TopMusic)this.Owner).Pause();
            this.cmbActiveConnections.Items.Clear();
            this.cmbActiveConnections.Items.Add(new ComboBoxItem("请选择一个连接...", null));

            int selectedIndex = 0;
            try
            {
                foreach (RasConnection connection in RasConnection.GetActiveConnections())
                {
                    ComboBoxItem item = new ComboBoxItem(connection.EntryName, connection.EntryId);
                    this.cmbActiveConnections.Items.Add(item);
                    if (this.config != null && connection.EntryName == this.config.RasEntryName)
                    {
                        selectedIndex = this.cmbActiveConnections.Items.Count - 1;
                    }
                }
            }
            catch (Exception)
            { 
                
            }

            this.cmbActiveConnections.SelectedIndex = selectedIndex;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (this.cmbActiveConnections.SelectedIndex <= 0)
            {
                MessageBox.Show("请选择一个拨号连接");
                return;
            }

            string rasEntryName = (this.cmbActiveConnections.SelectedIndex <= 0 ? null : this.cmbActiveConnections.Text);
            string formTitle = this.Text;
            string activePhoneBookPath = null;
            RasConnection connection = this.GetActiveConnection(rasEntryName);
            if (connection != null)
            {
                // reconnect
                activePhoneBookPath = connection.PhoneBookPath;
                this.Text = "正在断开连接 " + rasEntryName + "...";
                connection.HangUp();

                if (this.dialer == null)
                {
                    dialer = new RasDialer();
                    dialer.EntryName = rasEntryName;
                    dialer.AllowUseStoredCredentials = true;

                    dialer.PhoneBookPath = activePhoneBookPath;
                    dialer.Timeout = 10000;
                }

                // dial
                if (!dialer.IsBusy)
                {
                    this.Text = ("正在连接到 " + rasEntryName + "...");
                    dialer.Dial();
                    this.Text = ("已成功连接到" + rasEntryName);
                }
            }
        }

        private RasConnection GetActiveConnection(string rasEntryName)
        {
            try
            {
                foreach (RasConnection connection in RasConnection.GetActiveConnections())
                {
                    if (connection.EntryName == rasEntryName)
                    {
                        return connection;
                    }
                }
            }
            catch
            { 
            
            }
            return null;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.config != null)
            {
                this.config.RasEntryName = (this.cmbActiveConnections.SelectedIndex <= 0 ? null : this.cmbActiveConnections.Text);
                this.config.Save();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormConfig_FormClosed(object sender, FormClosedEventArgs e)
        {
            ((TopMusic)this.Owner).Resume();
        }
    }
}
