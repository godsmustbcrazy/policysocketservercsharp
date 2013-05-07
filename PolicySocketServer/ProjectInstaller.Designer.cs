namespace PolicySocketServer
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.policySocketServerInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.policySocketServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // policySocketServerInstaller
            // 
            this.policySocketServerInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.policySocketServerInstaller.Password = null;
            this.policySocketServerInstaller.Username = null;
            // 
            // policySocketServiceInstaller
            // 
            this.policySocketServiceInstaller.Description = "This service serves a socket policy file on a port as specified by the configurat" +
                "ion";
            this.policySocketServiceInstaller.ServiceName = "Policy Socket Service";
            this.policySocketServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.policySocketServerInstaller,
            this.policySocketServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller policySocketServerInstaller;
        private System.ServiceProcess.ServiceInstaller policySocketServiceInstaller;
    }
}