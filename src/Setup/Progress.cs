using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;


namespace Setup
{
    public partial class Progress : Form
    {
        public Progress()
        {
            InitializeComponent();
        }

        private Thread installThread = null;
        private bool errors = false;

        private void Progress_Load(object sender, EventArgs e)
        {
            this.Activate();
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

            installThread = new Thread(Install);
            installThread.Start();
        }

        private void Install()
        {
            string regpkg = VSIP + @"VisualStudioIntegration\Tools\Bin\regpkg.exe";
            string package = TARGETDIR + @"dlls\Ruby.NET.VSPackage.dll";
            string devenv = VSDIR + @"devenv.exe";

            try
            {
                this.pictureBox2.Image = global::Setup.Properties.Resources.progress;
                Execute(regpkg, @"/root:Software\Microsoft\VisualStudio\8.0Exp /codebase " + "\"" + package + "\"");
                this.pictureBox2.Image = global::Setup.Properties.Resources.tick;
            }
            catch (Exception e)
            {
                this.pictureBox2.Image = global::Setup.Properties.Resources.cross;
                this.errors = true;
                this.ErrorBox.AppendText(e.ToString() + "\n");
            }

            try
            {
                this.pictureBox3.Image = global::Setup.Properties.Resources.progress;
                Execute(devenv, @"/rootsuffix Exp /setup");
                this.pictureBox3.Image = global::Setup.Properties.Resources.tick;
            }
            catch (Exception e)
            {
                this.pictureBox3.Image = global::Setup.Properties.Resources.cross;
                this.errors = true;
                this.ErrorBox.AppendText(e.ToString() + "\n");
            }

            try
            {
                this.pictureBox4.Image = global::Setup.Properties.Resources.progress;
                Execute(devenv, @"/rootsuffix Exp /installvstemplates");
                this.pictureBox4.Image = global::Setup.Properties.Resources.tick;
            }
            catch (Exception e)
            {
                this.pictureBox4.Image = global::Setup.Properties.Resources.cross;
                this.errors = true;
                this.ErrorBox.AppendText(e.ToString() + "\n");
            }

            try
            {
                this.pictureBox5.Image = global::Setup.Properties.Resources.progress;
                CreateShortcut(StartMenuFolder + @"\Ruby.NET\Visual Studio (for Ruby).lnk", devenv, "/rootsuffix Exp", devenv);
                this.pictureBox5.Image = global::Setup.Properties.Resources.tick;
            }
            catch (Exception e)
            {
                this.pictureBox5.Image = global::Setup.Properties.Resources.cross;
                this.errors = true;
                this.ErrorBox.AppendText(e.ToString() + "\n");
            }

            if (!errors) Close();

            this.Cursor = Cursors.Arrow;
            this.Cancel.Enabled = false;
            this.Restart.Enabled = true;
            this.CloseButton.Enabled = true;
            this.Ignore.Enabled = true;
        }

        private void Execute(string file, string args)
        {
            ErrorBox.AppendText(file + " " + args + "\n");

            Process process = new Process();

            process.StartInfo.FileName = file;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();

            StreamReader sOut = process.StandardOutput;
            string result = sOut.ReadToEnd();
            ErrorBox.AppendText(result);

            StreamReader eOut = process.StandardError;
            string error = eOut.ReadToEnd();
            ErrorBox.AppendText(error);
        }

        private void CreateShortcut(string shortcutFile, string targetFile, string arguments, string IconFile)
        {
            ErrorBox.AppendText("Create Shortcut " + shortcutFile + " -> " + targetFile + "\n");

            WshShellClass shell = new WshShellClass();
            WshShortcut link = (WshShortcut)shell.CreateShortcut(shortcutFile);
            link.Arguments = arguments;
            link.TargetPath = targetFile;
            link.IconLocation = IconFile;
            link.Save();
        }


        private void Cancel_Click(object sender, EventArgs e)
        {
            if (installThread != null)
                installThread.Abort();

            this.Cursor = Cursors.Arrow;
            Cancel.Enabled = false;
            Restart.Enabled = true;
            CloseButton.Enabled = true;
            Ignore.Enabled = true;
        }


        private void Restart_Click(object sender, EventArgs e)
        {
            this.errors = false;
            this.Restart.Enabled = false;
            this.CloseButton.Enabled = false;
            this.Cancel.Enabled = true;
            this.Ignore.Enabled = false;
            this.ErrorBox.Text = "";
            this.Cursor = Cursors.WaitCursor;

            installThread = new Thread(Install);
            installThread.Start();
        }

        private void CloseButton_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private string VSIP, VSDIR, TARGETDIR, StartMenuFolder;

        public static int Main(string[] args)
        {
            args = System.Environment.CommandLine.Split(new char[] {'|'});
            Progress form = new Progress();    

            form.VSIP = args[1];
            form.VSDIR = args[2];
            form.TARGETDIR = args[3];
            form.StartMenuFolder = args[4];

            Application.Run(form);

            if (form.errors)
                return -1;
            else
                return 0;
        }

        private void Ignore_Click(object sender, EventArgs e)
        {
            errors = false;
            Close();
        }
    }
}