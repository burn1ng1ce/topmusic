using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using mshtml;

namespace TopMusic
{
    public partial class FormConsole : Form
    {
        private TopMusic parentForm;
        public FormConsole(TopMusic parentForm)
        {
            InitializeComponent();
            this.parentForm = parentForm;
        }

        private void txtCmdLine_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void txtCmdLine_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                // Enter to execute script
                string code = this.txtCmdLine.Text;
                Console.WriteLine("exec " + code);
                if (code != null)
                {
                    if (code.StartsWith("//@frame="))
                    {
                        mshtml.HTMLDocument doc = (mshtml.HTMLDocument)this.parentForm.WebBrowser.ActiveWindow.Document.DomDocument;
                        string frameName = code.Substring(9, code.IndexOfAny(new char[] { '\r', '\n' }) - 9);
                        frameName = frameName.Trim();
                        int framesCount = doc.frames.length;
                        for (int i = 0; i < framesCount; ++i)
                        {
                            object indexRef = i;
                            mshtml.IHTMLWindow2 frameWindow = (mshtml.IHTMLWindow2)doc.frames.item(ref indexRef);
                            if (frameWindow.name == frameName)
                            {
                                try
                                {
                                    frameWindow.execScript(code, "javascript");
                                }
                                catch
                                {

                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("exec " + code);
                        this.parentForm.WebBrowser.ExecuteScript(code);
                    }
                }

                this.txtCmdLine.Text = "";
            }
        }

        private void FormConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.parentForm.IsDisposed)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        public string LogConsole(string text)
        {
            this.txtConsole.AppendText(text);
            this.txtConsole.AppendText("\r\n");
            return text;
        }
    }
}
