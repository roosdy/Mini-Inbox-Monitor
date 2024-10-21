// Form Designer Partial Class (Form1.Designer.cs)
using InboxMonitoringApp;
using System.Data.SQLite;
using System.Text;
using Google.Apis.Gmail.v1.Data;
using Label = System.Windows.Forms.Label;

namespace InboxMonitoringApp
{
    partial class Form1
    {
        internal Button btnSignIn;
        internal Button btnSignOut;
        internal Button btnFetchEmails;
        internal Label lblStatus;
        internal ListView listViewEmails;
        internal Button btnFlagUrgent;
        internal Button btnFlagAttention;
        internal Button btnFlagGood;
        internal TextBox systemPromptTextBox;
        internal TextBox chatInputTextBox;
        internal RichTextBox chatHistoryTextBox;
        internal ComboBox chatSessionsComboBox;
        internal ChatSession currentChatSession;
        internal Button btnExportToExcel;
        private const int MaxResults = 10; // Fix for email listing issue
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            notifyIcon1 = new NotifyIcon(components);
            btnSignIn = new Button();
            btnSignOut = new Button();
            btnFetchEmails = new Button();
            lblStatus = new Label();
            listViewEmails = new ListView();
            btnFlagUrgent = new Button();
            btnFlagAttention = new Button();
            btnFlagGood = new Button();
            systemPromptTextBox = new TextBox();
            chatInputTextBox = new TextBox();
            chatHistoryTextBox = new RichTextBox();
            chatSessionsComboBox = new ComboBox();
            newChatButton = new Button();
            sendButton = new Button();
            btnExportToExcel = new Button();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.DoubleClick += NotifyIcon1_DoubleClick;
            // 
            // btnSignIn
            // 
            btnSignIn.Location = new Point(12, 47);
            btnSignIn.Name = "btnSignIn";
            btnSignIn.Size = new Size(88, 38);
            btnSignIn.TabIndex = 0;
            btnSignIn.Text = "Sign in with Google";
            btnSignIn.UseVisualStyleBackColor = true;
            btnSignIn.Click += BtnSignIn_Click;
            // 
            // btnSignOut
            // 
            btnSignOut.Location = new Point(12, 94);
            btnSignOut.Name = "btnSignOut";
            btnSignOut.Size = new Size(88, 38);
            btnSignOut.TabIndex = 1;
            btnSignOut.Text = "Sign Out";
            btnSignOut.UseVisualStyleBackColor = true;
            btnSignOut.Click += BtnSignOut_Click;
            // 
            // btnFetchEmails
            // 
            btnFetchEmails.Location = new Point(12, 141);
            btnFetchEmails.Name = "btnFetchEmails";
            btnFetchEmails.Size = new Size(88, 38);
            btnFetchEmails.TabIndex = 2;
            btnFetchEmails.Text = "Fetch Latest Emails";
            btnFetchEmails.UseVisualStyleBackColor = true;
            btnFetchEmails.Click += BtnFetchEmails_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 197);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(88, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Not Connected";
            // 
            // listViewEmails
            // 
            listViewEmails.Location = new Point(207, 300);
            listViewEmails.Name = "listViewEmails";
            listViewEmails.Size = new Size(708, 188);
            listViewEmails.TabIndex = 4;
            listViewEmails.UseCompatibleStateImageBehavior = false;
            listViewEmails.View = View.Details;
            listViewEmails.DoubleClick += ListViewEmails_DoubleClick;
            // 
            // btnFlagUrgent
            // 
            btnFlagUrgent.Location = new Point(12, 303);
            btnFlagUrgent.Name = "btnFlagUrgent";
            btnFlagUrgent.Size = new Size(68, 38);
            btnFlagUrgent.TabIndex = 7;
            btnFlagUrgent.Text = "Flag Urgent";
            btnFlagUrgent.UseVisualStyleBackColor = true;
            btnFlagUrgent.Click += BtnFlagUrgent_Click;
            // 
            // btnFlagAttention
            // 
            btnFlagAttention.Location = new Point(12, 350);
            btnFlagAttention.Name = "btnFlagAttention";
            btnFlagAttention.Size = new Size(68, 38);
            btnFlagAttention.TabIndex = 8;
            btnFlagAttention.Text = "Flag Attention";
            btnFlagAttention.UseVisualStyleBackColor = true;
            btnFlagAttention.Click += BtnFlagAttention_Click;
            // 
            // btnFlagGood
            // 
            btnFlagGood.Location = new Point(12, 397);
            btnFlagGood.Name = "btnFlagGood";
            btnFlagGood.Size = new Size(68, 38);
            btnFlagGood.TabIndex = 9;
            btnFlagGood.Text = "Flag Good";
            btnFlagGood.UseVisualStyleBackColor = true;
            btnFlagGood.Click += BtnFlagGood_Click;
            // 
            // systemPromptTextBox
            // 
            systemPromptTextBox.ForeColor = SystemColors.InactiveCaption;
            systemPromptTextBox.Location = new Point(207, 13);
            systemPromptTextBox.Multiline = true;
            systemPromptTextBox.Name = "systemPromptTextBox";
            systemPromptTextBox.Size = new Size(438, 23);
            systemPromptTextBox.TabIndex = 0;
            systemPromptTextBox.Text = "You are a helpful assistant with access to the user's latest 10 emails. Use this information to assist the user.";
            systemPromptTextBox.TextChanged += SystemPromptTextBox_TextChanged;
            // 
            // chatInputTextBox
            // 
            chatInputTextBox.Location = new Point(207, 213);
            chatInputTextBox.Multiline = true;
            chatInputTextBox.Name = "chatInputTextBox";
            chatInputTextBox.Size = new Size(627, 62);
            chatInputTextBox.TabIndex = 1;
            chatInputTextBox.TextChanged += chatInputTextBox_TextChanged;
            // 
            // chatHistoryTextBox
            // 
            chatHistoryTextBox.Location = new Point(207, 42);
            chatHistoryTextBox.Name = "chatHistoryTextBox";
            chatHistoryTextBox.ScrollBars = RichTextBoxScrollBars.None;
            chatHistoryTextBox.Size = new Size(708, 165);
            chatHistoryTextBox.TabIndex = 2;
            chatHistoryTextBox.Text = "";
            // 
            // chatSessionsComboBox
            // 
            chatSessionsComboBox.Location = new Point(764, 13);
            chatSessionsComboBox.Name = "chatSessionsComboBox";
            chatSessionsComboBox.Size = new Size(151, 23);
            chatSessionsComboBox.TabIndex = 3;
            chatSessionsComboBox.SelectedIndexChanged += chatSessionsComboBox_SelectedIndexChanged;
            // 
            // newChatButton
            // 
            newChatButton.Location = new Point(651, 13);
            newChatButton.Name = "newChatButton";
            newChatButton.Size = new Size(111, 23);
            newChatButton.TabIndex = 4;
            newChatButton.Text = "New Chat";
            newChatButton.Click += NewChatButton_Click;
            // 
            // sendButton
            // 
            sendButton.Location = new Point(840, 252);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(75, 23);
            sendButton.TabIndex = 5;
            sendButton.Text = "Send";
            sendButton.Click += SendButton_Click;
            // 
            // btnExportToExcel
            // 
            btnExportToExcel.Location = new Point(12, 250);
            btnExportToExcel.Name = "btnExportToExcel";
            btnExportToExcel.Size = new Size(88, 38);
            btnExportToExcel.TabIndex = 10;
            btnExportToExcel.Text = "Export to Excel";
            btnExportToExcel.UseVisualStyleBackColor = true;
            btnExportToExcel.Click += BtnExportToExcel_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(927, 504);
            Controls.Add(systemPromptTextBox);
            Controls.Add(chatInputTextBox);
            Controls.Add(chatHistoryTextBox);
            Controls.Add(chatSessionsComboBox);
            Controls.Add(newChatButton);
            Controls.Add(sendButton);
            Controls.Add(listViewEmails);
            Controls.Add(lblStatus);
            Controls.Add(btnFetchEmails);
            Controls.Add(btnSignOut);
            Controls.Add(btnSignIn);
            Controls.Add(btnFlagUrgent);
            Controls.Add(btnFlagAttention);
            Controls.Add(btnFlagGood);
            Controls.Add(btnExportToExcel);
            Name = "Form1";
            Text = "Mercury Inbox Monitoring App";
            ResumeLayout(false);
            PerformLayout();
        }

        private void chatInputTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Example Logic: Enable Send Button only if there's text
                if (string.IsNullOrWhiteSpace(chatInputTextBox.Text))
                {
                    sendButton.Enabled = false;
                }
                else
                {
                    sendButton.Enabled = true;
                }

                // Additional Logic Can Be Placed Here
                // For example, updating a character count label
                // lblCharCount.Text = $"{chatInputTextBox.Text.Length} characters";
            }
            catch (Exception ex)
            {
                // Handle the exception gracefully
                MessageBox.Show($"An error occurred while processing your input: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Optional: Log the exception details for debugging purposes
                // For example, using a logging framework like NLog or log4net
                // Logger.Error(ex, "Error in chatInputTextBox_TextChanged");
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Button newChatButton;
        private Button sendButton;
        private System.ComponentModel.IContainer components;
    }
}
