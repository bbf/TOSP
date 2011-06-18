using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using TOSP;
using System.Diagnostics;

namespace TOSP
{
    public partial class TOSLauncher : Form
    {
        private bool enableConfigSave;
        private Process process;

        public TOSLauncher()
        {
            InitializeComponent();
        }

        private void TOSLauncher_Load(object sender, EventArgs e)
        {

            TOSLConfig.PropertyChanged += new PropertyChangedEventHandler(configPropertyChanged);
            TOSLConfig.GetConfig();
            enableConfigSave = true;

        }

        private void configPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TOSLConfig senderConfig = (TOSLConfig)sender;
            pathBox.Text = senderConfig.TerrariaPath;
            if (enableConfigSave)
            {
                senderConfig.Save();
            }
        }




        private void pathBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = pathDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TOSLConfig.GetConfig().TerrariaPath = pathDialog.FileName;
            }
        }

        private void launchBtn_Click(object sender, EventArgs e)
        {
            if (process != null && process.HasExited == false)
            {
                Program.ShowWindow(process.MainWindowHandle, 9);
                Program.SetForegroundWindow(process.MainWindowHandle);
                return;
            }

            TOSLConfig config = TOSLConfig.GetConfig();

            if (config.TerrariaPath == null || config.TerrariaPatchedServer == null)
            {
                MessageBox.Show(this, "Please find the path to your TerrariaServer.exe before continuing.", "Cannot Start");
                return;
            }

            Injector.Inject(config.TerrariaPath, config.TerrariaPatchedServer);
            process = Process.Start(config.TerrariaPatchedServer);
            process.Exited += new EventHandler(processExited);

        }

        private void updateStatus()
        {
            launchBtn.Enabled = process == null;

        }

        private void processExited(object sender, EventArgs e)
        {
            process = null;
        }

        private void TOSLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (process != null && process.HasExited == false)
            {
                DialogResult result = MessageBox.Show(this, "Are you sure you want to quit? The server is still running and will be terminated.", "Warning", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                process.Kill();
                process.WaitForExit();
            }

            File.Delete(TOSLConfig.GetConfig().TerrariaPatchedServer);
        }
    }
}
