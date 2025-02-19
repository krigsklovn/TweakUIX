﻿using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TweakUIX
{
    public partial class PluginsPageView : UserControl
    {
        private string optionalPluginsDir = Helpers.Strings.Data.PluginsRootDir + "MajorGeeks Windows Tweaks";

        public PluginsPageView() => InitializeComponent();

        private void PluginsPageView_Load(object sender, EventArgs e) => InitializePlugins();

        private void InitializePlugins()
        {
            listPlugs.Items.Clear();

            try
            {
                DirectoryInfo dirs = new DirectoryInfo(Helpers.Strings.Data.PluginsRootDir);
                FileInfo[] listSettings = dirs.GetFiles("*.ps1");
                foreach (FileInfo fi in listSettings)
                {
                    listPlugs.Items.Add(Path.GetFileNameWithoutExtension(fi.Name));
                    listPlugs.Enabled = true;
                }
            }
            catch { MessageBox.Show("No plugins directory found."); btnApply.Visible = false; btnCancel.Visible = false; }
        }

        public async void DoPlugin()
        {
            if (listPlugs.CheckedItems.Count == 0)
            {
                MessageBox.Show("No plugin selected.");
                return;
            }

            if (MessageBox.Show("Do you want to apply selected plugins", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (int i = 0; i < listPlugs.Items.Count; i++)
                {
                    if (listPlugs.GetItemChecked(i))
                    {
                        listPlugs.SelectedIndex = i;
                        string plugsDir = Helpers.Strings.Data.PluginsRootDir + "\\" + listPlugs.SelectedItem.ToString() + ".ps1";
                        var ps1File = plugsDir;

                        var equals = new[] { "Requires -RunSilent" };

                        var str = richPluginInfo.Text;
                        btnCancel.Visible = true;
                        progress.Visible = true;
                        progress.Style = ProgressBarStyle.Marquee;
                        progress.MarqueeAnimationSpeed = 30;

                        btnApply.Enabled = false;
                        groupBoxPlugins.Text = "Processing " + listPlugs.Text;

                        if (equals.Any(str.Contains))                   // Silent
                        {
                            var startInfo = new ProcessStartInfo()
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-executionpolicy bypass -file \"{ps1File}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            };

                            await Task.Run(() => { Process.Start(startInfo).WaitForExit(); });
                        }
                        else                                            // Create ConsoleWindow
                        {
                            var startInfo = new ProcessStartInfo()
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-executionpolicy bypass -noexit -file \"{ps1File}\"",
                                UseShellExecute = false,
                            };

                            await Task.Run(() => { Process.Start(startInfo).WaitForExit(); });
                        }

                        btnApply.Enabled = true;
                        groupBoxPlugins.Text = "";
                    }
                }

                progress.Visible = false;
                btnCancel.Visible = false;

                MessageBox.Show("Plugins have been successfully applied.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void listPlugs_SelectedIndexChanged(object sender, EventArgs e)
        {
            string plugsDir = Helpers.Strings.Data.PluginsRootDir + "\\" + listPlugs.Text + ".ps1";

            try
            {
                using (StreamReader sr = new StreamReader(plugsDir, Encoding.Default))
                {
                    StringBuilder content = new StringBuilder();

                    while (!sr.EndOfStream)
                        content.AppendLine(sr.ReadLine());

                    richPluginInfo.Text = string.Join(Environment.NewLine, File.ReadAllLines(plugsDir).Where(s => s.StartsWith("###")).Select(s => s.Substring(3).Replace("###", "\n")));
                }
            }
            catch { }
        }

        private void btnApply_Click(object sender, EventArgs e) => DoPlugin();

        private void btnCancel_Click(object sender, EventArgs e)
        {
            String CurrentUser = Environment.UserName;
            Process[] allProcesses = Process.GetProcessesByName("powershell");
            if (null != allProcesses)
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C TASKKILL /F /FI \"USERNAME eq " + CurrentUser + "\" /IM powershell.exe";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }

            btnCancel.Visible = false;
        }

        private void AddPlusPack()
        {
            if (!Directory.Exists(optionalPluginsDir))
            {
                richHelp.Text = "The following packages are supported."
                                + "\n- https://www.majorgeeks.com/files/details/majorgeeks_registry_tweaks.html"
                                + "\n(After downloading, just extract it to \"app\\plugins\" directory.)";

                lblComponentCategory.Visible =
                lblTweakCategory.Visible =
                lblTweak.Visible =
                comboCategory.Visible =
                comboTweaks.Visible = false;

                return;
            }
            richHelp.Text = "How does it work?"
                       + "\n1. Select a category and an individual tweak from the list"
                       + "\n2. Double click to trigger the tweak"
                       + "\n3. Online help files are automatically searched and highlighted green"
                       + "\n\n(This package is powered by https://www.majorgeeks.com)";

            comboCategory.DataSource = Directory.GetDirectories(optionalPluginsDir)
                .Select(Path.GetFileName).ToList();
            comboCategory.SelectedIndexChanged += listPlusPackCategory;
            comboTweaks.SelectedIndexChanged += listPlusPackTweaks;
        }

        private void listPlusPackCategory(object sender, EventArgs e)
        {
            var parentDir = Path.Combine(optionalPluginsDir, comboCategory.SelectedItem.ToString());
            comboTweaks.DataSource = Directory.GetDirectories(parentDir)
                 .Select(Path.GetFileName).ToList();
        }

        private void listPlusPackTweaks(object sender, EventArgs e)
        {
            var parentDir = Path.Combine(optionalPluginsDir, comboCategory.SelectedItem.ToString(),
            comboTweaks.SelectedItem.ToString());
            dataGridView.DataSource = Directory.GetFiles(parentDir)
                .Select(f => new { FileName = Path.GetFileName(f) }).ToList();
        }

        private void dataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value.ToString().Contains(".html"))
                {
                    row.DefaultCellStyle.ForeColor = Color.SeaGreen;
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex != -1)
                    Process.Start(optionalPluginsDir + "\\" + comboCategory.Text + "\\" + comboTweaks.Text + "\\" + dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void tab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tab.SelectedTab == tab.TabPages[1])
                AddPlusPack();
            else if (tab.SelectedTab == tab.TabPages[2])
            {
                MessageBox.Show("In the pipeline...\nThis was also a feature of Microsoft at that time of the ubiquitous Tweak UI app.");
                tab.SelectedTab = tab.TabPages[0];
            }
            return;
        }

        private void richPluginInfo_LinkClicked(object sender, LinkClickedEventArgs e) => Helpers.Utils.LaunchUri(e.LinkText);

        private void richHelp_LinkClicked(object sender, LinkClickedEventArgs e) => Helpers.Utils.LaunchUri(e.LinkText);
    }
}