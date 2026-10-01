using System;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AeroTerminal
{
    public class MainForm : Form
    {
        private TabControl tabControl;
        private Button btnPlus;
        private Button btnSettings;
        private ContextMenuStrip plusMenu;
        private string startDirectory;

        public MainForm(string initialDir)
        {
            startDirectory = initialDir ?? Environment.CurrentDirectory;
            CheckDefaultExplorerIntegration();
            InitializeComponent();
        }

        private void CheckDefaultExplorerIntegration()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\AeroTerminal", true))
                {
                    if (key != null)
                    {
                        if (key.GetValue("ExplorerIntegration") == null)
                        {
                            key.SetValue("ExplorerIntegration", 1);
                            ApplyExplorerIntegration(true);
                        }
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"Software\AeroTerminal"))
                        {
                            newKey.SetValue("ExplorerIntegration", 1);
                            newKey.SetValue("StartAs", "cmd.exe");
                        }
                        ApplyExplorerIntegration(true);
                    }
                }
            }
            catch { }
        }

        private void ApplyExplorerIntegration(bool enable)
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string bgShellPath = @"Software\Classes\Directory\Background\shell\AeroTerminal";
                string dirShellPath = @"Software\Classes\Directory\shell\AeroTerminal";

                if (enable)
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(bgShellPath))
                    {
                        key.SetValue("", "Open in Terminal");
                        key.SetValue("Icon", exePath + ",0");
                    }
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(bgShellPath + @"\command"))
                    {
                        key.SetValue("", "\"" + exePath + "\" \"%1\"");
                    }

                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(dirShellPath))
                    {
                        key.SetValue("", "Open in Terminal");
                        key.SetValue("Icon", exePath + ",0");
                    }
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(dirShellPath + @"\command"))
                    {
                        key.SetValue("", "\"" + exePath + "\" \"%1\"");
                    }
                }
                else
                {
                    try { Registry.CurrentUser.DeleteSubKeyTree(bgShellPath, false); } catch { }
                    try { Registry.CurrentUser.DeleteSubKeyTree(dirShellPath, false); } catch { }
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {
            this.Text = "Terminal";
            this.Size = new Size(900, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            btnSettings = new Button();
            btnSettings.Text = "⚙";
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
            btnSettings.ForeColor = Color.White;
            btnSettings.Size = new Size(35, 28);
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.Location = new Point(this.ClientSize.Width - 75, 4);
            btnSettings.Click += BtnSettings_Click;
            this.Controls.Add(btnSettings);

            btnPlus = new Button();
            btnPlus.Text = "+";
            btnPlus.FlatStyle = FlatStyle.Flat;
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            btnPlus.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
            btnPlus.ForeColor = Color.White;
            btnPlus.Size = new Size(35, 28);
            btnPlus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPlus.Location = new Point(this.ClientSize.Width - 38, 4);
            btnPlus.Click += BtnPlus_Click;
            this.Controls.Add(btnPlus);

            tabControl = new TabControl();
            tabControl.Location = new Point(0, 0);
            tabControl.Size = new Size(this.ClientSize.Width - 78, this.ClientSize.Height);
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Padding = new Point(22, 8);
            
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;
            tabControl.MouseDown += TabControl_MouseDown;
            this.Controls.Add(tabControl);

            this.Resize += (s, e) => {
                btnSettings.Location = new Point(this.ClientSize.Width - 75, 4);
                btnPlus.Location = new Point(this.ClientSize.Width - 38, 4);
            };

            plusMenu = new ContextMenuStrip();
            plusMenu.Items.Add("Command Prompt", null, (s, e) => AddNewTab("cmd.exe", "Command Prompt"));
            plusMenu.Items.Add("Windows PowerShell", null, (s, e) => AddNewTab("powershell.exe", "Windows PowerShell"));

            string defaultShell = "cmd.exe";
            string defaultTitle = "Command Prompt";
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\AeroTerminal"))
                {
                    if (key != null)
                    {
                        string saved = key.GetValue("StartAs") as string;
                        if (saved == "powershell.exe")
                        {
                            defaultShell = "powershell.exe";
                            defaultTitle = "Windows PowerShell";
                        }
                    }
                }
            }
            catch { }

            AddNewTab(defaultShell, defaultTitle, startDirectory);
        }

        private void AddNewTab(string shellPath, string title, string workingDir = null)
        {
            if (tabControl.TabCount >= 5)
            {
                MessageBox.Show("Maximum 5 tabs reached.", "Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string currentDir = workingDir ?? startDirectory;
            TabPage tabPage = new TabPage(title);
            tabPage.Padding = new Padding(0);

            bool isPowerShell = shellPath.Contains("powershell");
            Color terminalBg = isPowerShell ? Color.FromArgb(1, 36, 86) : Color.FromArgb(30, 30, 30);

            TerminalControl terminal = new TerminalControl(shellPath, currentDir, terminalBg, Color.White, isPowerShell);
            terminal.Dock = DockStyle.Fill;

            tabPage.Controls.Add(terminal);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
        }

        private void BtnPlus_Click(object sender, EventArgs e)
        {
            plusMenu.Show(btnPlus, new Point(0, btnPlus.Height));
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm settingsForm = new SettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
        }

        private void TabControl_DrawItem(Object sender, DrawItemEventArgs e)
        {
            try
            {
                TabPage tabPage = tabControl.TabPages[e.Index];
                Rectangle tabRect = tabControl.GetTabRect(e.Index);
                
                bool isSelected = (tabControl.SelectedTab == tabPage);
                Color backColor = isSelected ? Color.FromArgb(45, 45, 45) : Color.FromArgb(30, 30, 30);
                Color textColor = isSelected ? Color.White : Color.LightGray;

                using (SolidBrush bgBrush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(bgBrush, tabRect);
                }

                Rectangle textRect = new Rectangle(tabRect.X + 10, tabRect.Y + 6, tabRect.Width - 30, tabRect.Height - 6);
                TextRenderer.DrawText(e.Graphics, tabPage.Text, e.Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                using (Pen pen = new Pen(textColor, 1.5f))
                {
                    int xPos = tabRect.Right - 16;
                    int yPos = tabRect.Top + (tabRect.Height / 2) - 4;
                    e.Graphics.DrawLine(pen, xPos, yPos, xPos + 6, yPos + 6);
                    e.Graphics.DrawLine(pen, xPos, yPos + 6, xPos + 6, yPos);
                }
            }
            catch { }
        }

        private void TabControl_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                Rectangle tabRect = tabControl.GetTabRect(i);
                Rectangle closeRect = new Rectangle(tabRect.Right - 20, tabRect.Top + 4, 16, 16);

                if (closeRect.Contains(e.Location))
                {
                    if (tabControl.TabPages.Count == 1)
                    {
                        this.Close();
                    }
                    else
                    {
                        tabControl.TabPages.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
}