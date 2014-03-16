using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BurningICE.WebBrowserEx;

namespace TopMusic
{
    public partial class FormScriptError : Form
    {
        private Form owner;
        private List<ScriptError> scriptErrors;
        private int currentIndex;

        public FormScriptError()
        {
            InitializeComponent();
        }

        public FormScriptError(Form owner)
        {
            InitializeComponent();
            this.owner = owner;
        }

        internal void ShowScriptErrors(List<ScriptError> scriptErrors) {
            this.scriptErrors = scriptErrors;
            this.ShowScriptError(0);
            this.ShowDialog(this.owner);
        }

        private void ShowScriptError(int index) {
            if (index < 0) {
                index = 0;
            }

            if (index >= this.scriptErrors.Count) {
                index = this.scriptErrors.Count - 1;
            }

            ScriptError scripError = scriptErrors[index];
            this.lblMessage.Text = "ÐÐ:" + scripError.Line + "\r\nURL:" + scripError.Url + "\r\n´íÎó:" + scripError.Description;

            this.btnPrev.Enabled = (index > 0);
            this.btnNext.Enabled = (index + 1 < this.scriptErrors.Count);
            this.currentIndex = index;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            this.ShowScriptError(currentIndex - 1);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.ShowScriptError(currentIndex + 1);
        }
    }
}