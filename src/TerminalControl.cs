using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AeroTerminal
{
    public class TerminalControl : UserControl
    {
        private RichTextBox txtOutput;
        private TextBox txtInput;
        private Process process;
        private StreamWriter processInput;
        private bool isPowerShell;

        public TerminalControl(string shellPath, string workingDir, Color bgColor, Color fgColor, bool powerShell)
        {
            this.isPowerShell = powerShell;

            txtOutput = new RichTextBox();
            txtOutput.Dock = DockStyle.Fill;
            txtOutput.BackColor = bgColor;
            txtOutput.ForeColor = fgColor;
            txtOutput.BorderStyle = BorderStyle.None;
            txtOutput.Font = new Font("Consolas", 10F, FontStyle.Regular);
            txtOutput.ReadOnly = true;
            this.Controls.Add(txtOutput);

            txtInput = new TextBox();
            txtInput.Dock = DockStyle.Bottom;
            txtInput.BackColor = bgColor;
            txtInput.ForeColor = fgColor;
            txtInput.BorderStyle = BorderStyle.FixedSingle;
            txtInput.Font = new Font("Consolas", 10F, FontStyle.Regular);
            txtInput.KeyDown += TxtInput_KeyDown;
            this.Controls.Add(txtInput);

            StartProcess(shellPath, workingDir);
        }

        private void StartProcess(string shellPath, string workingDir)
        {
            try
            {
                process = new Process();
                process.StartInfo.FileName = shellPath;
                process.StartInfo.WorkingDirectory = workingDir;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        AppendOutput(e.Data + Environment.NewLine, false);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        bool isErrorRed = isPowerShell;
                        AppendOutput(e.Data + Environment.NewLine, isErrorRed);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                processInput = process.StandardInput;
            }
            catch (Exception ex)
            {
                AppendOutput("Failed to start process: " + ex.Message + Environment.NewLine, true);
            }
        }

        private void AppendOutput(string text, bool isError)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, bool>(AppendOutput), text, isError);
                return;
            }

            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.SelectionLength = 0;
            txtOutput.SelectionColor = isError ? Color.Red : txtOutput.ForeColor;
            txtOutput.AppendText(text);
            txtOutput.SelectionColor = txtOutput.ForeColor;
            txtOutput.ScrollToCaret();
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string cmd = txtInput.Text;
                
                txtInput.Clear();

                if (processInput != null)
                {
                    processInput.WriteLine(cmd);
                    processInput.Flush();
                }
            }
        }
    }
}