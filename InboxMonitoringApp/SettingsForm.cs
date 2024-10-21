// SettingsForm.cs
using System;
using System.IO;
using System.Windows.Forms;

namespace InboxMonitoringApp
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {

            // Load existing settings
            txtClientId.Text = Config.ClientId;
            txtClientSecret.Text = Config.ClientSecret;
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            string clientId = txtClientId.Text;
            string clientSecret = txtClientSecret.Text;

            // Save to .env file
            using (StreamWriter writer = new StreamWriter(".env"))
            {
                writer.WriteLine($"CLIENT_ID={clientId}");
                writer.WriteLine($"CLIENT_SECRET={clientSecret}");
            }

            // Reload the environment variables
            Config.Reload();

            MessageBox.Show("Settings saved successfully.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
// SettingsForm.Designer.cs
namespace InboxMonitoringApp
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtClientSecret;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.Label lblClientSecret;

        private void InitializeComponent()
        {
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.txtClientSecret = new System.Windows.Forms.TextBox();
            this.lblClientId = new System.Windows.Forms.Label();
            this.lblClientSecret = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // btnSaveSettings
            this.btnSaveSettings.Location = new System.Drawing.Point(100, 120);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(100, 23);
            this.btnSaveSettings.TabIndex = 0;
            this.btnSaveSettings.Text = "Save Settings";
            this.btnSaveSettings.Click += new System.EventHandler(this.BtnSaveSettings_Click);

            // txtClientId
            this.txtClientId.Location = new System.Drawing.Point(100, 30);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.Size = new System.Drawing.Size(200, 23);
            this.txtClientId.TabIndex = 1;

            // txtClientSecret
            this.txtClientSecret.Location = new System.Drawing.Point(100, 70);
            this.txtClientSecret.Name = "txtClientSecret";
            this.txtClientSecret.Size = new System.Drawing.Size(200, 23);
            this.txtClientSecret.TabIndex = 2;

            // lblClientId
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(20, 33);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(54, 15);
            this.lblClientId.TabIndex = 3;
            this.lblClientId.Text = "Client ID:";

            // lblClientSecret
            this.lblClientSecret.AutoSize = true;
            this.lblClientSecret.Location = new System.Drawing.Point(20, 73);
            this.lblClientSecret.Name = "lblClientSecret";
            this.lblClientSecret.Size = new System.Drawing.Size(77, 15);
            this.lblClientSecret.TabIndex = 4;
            this.lblClientSecret.Text = "Client Secret:";

            // SettingsForm
            this.ClientSize = new System.Drawing.Size(334, 161);
            this.Controls.Add(this.lblClientSecret);
            this.Controls.Add(this.lblClientId);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.txtClientId);
            this.Controls.Add(this.txtClientSecret);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}
