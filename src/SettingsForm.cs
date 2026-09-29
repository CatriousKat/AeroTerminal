using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AeroTerminal
{
    public class SettingsForm : Form
    {
        private ComboBox cmbStartAs;
        private CheckBox chkExplorerIntegration;
        private Button btnSave;

        public SettingsForm()
        {
            this.Text = "Terminal Settings";
            this.Size = new Size(360, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblStart = new Label();
            lblStart.Text = "Start as:";
            lblStart.Location = new Point(20, 25);
            lblStart.AutoSize = true;
            this.Controls.Add(lblStart);

            cmbStartAs = new ComboBox();
            cmbStartAs.Location = new Point(160, 22);
            cmbStartAs.Size = new Size(160, 22);
            cmbStartAs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStartAs.Items.Add("Command Prompt");
            cmbStartAs.Items.Add("Windows PowerShell");
            this.Controls.Add(cmbStartAs);

            chkExplorerIntegration = new CheckBox();
            chkExplorerIntegration.Text = "Add \"Open in Terminal\" in Explorer";
            chkExplorerIntegration.Location = new Point(20, 70);
            chkExplorerIntegration.Size = new Size(300, 24);
            this.Controls.Add(chkExplorerIntegration);

            btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.Size = new Size(80, 28);
            btnSave.Location = new Point(240, 130);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\AeroTerminal"))
                {
                    if (key != null)
                    {
                        string startAs = key.GetValue("StartAs") as string;
                        cmbStartAs.SelectedIndex = (startAs == "powershell.exe") ? 1 : 0;

                        object expInt = key.GetValue("ExplorerIntegration");
                        if (expInt != null)
                        {
                            chkExplorerIntegration.Checked = ((int)expInt == 1);
                        }
                        else
                        {
                            chkExplorerIntegration.Checked = true;
                        }
                    }
                    else
                    {
                        cmbStartAs.SelectedIndex = 0;
                        chkExplorerIntegration.Checked = true;
                    }
                }
            }
            catch
            {
                cmbStartAs.SelectedIndex = 0;
                chkExplorerIntegration.Checked = true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string shellVal = (cmbStartAs.SelectedIndex == 1) ? "powershell.exe" : "cmd.exe";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\AeroTerminal"))
                {
                    if (key != null)
                    {
                        key.SetValue("StartAs", shellVal);
                        key.SetValue("ExplorerIntegration", chkExplorerIntegration.Checked ? 1 : 0);
                    }
                }

                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string bgShellPath = @"Software\Classes\Directory\Background\shell\AeroTerminal";
                string dirShellPath = @"Software\Classes\Directory\shell\AeroTerminal";

                if (chkExplorerIntegration.Checked)
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(bgShellPath))
                    {
                        key.SetValue("", "Open in Terminal");
                        key.SetValue("Icon", exePath + ",0");
                    }
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(bgShellPath + @"\command"))
                    {
                        key.SetValue("", "\"" + exePath + "\" \"%V\"");
                    }

                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(dirShellPath))
                    {
                        key.SetValue("", "Open in Terminal");
                        key.SetValue("Icon", exePath + ",0");
                    }
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(dirShellPath + @"\command"))
                    {
                        key.SetValue("", "\"" + exePath + "\" \"%V\"");
                    }
                }
                else
                {
                    try { Registry.CurrentUser.DeleteSubKeyTree(bgShellPath, false); } catch { }
                    try { Registry.CurrentUser.DeleteSubKeyTree(dirShellPath, false); } catch { }
                }

                MessageBox.Show("Settings saved successfully!", "Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}