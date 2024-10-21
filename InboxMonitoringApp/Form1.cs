//Form1.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using DotNetEnv;
using System.Drawing;
using Message = Google.Apis.Gmail.v1.Data.Message;
using System.Text.RegularExpressions;
using InboxMonitoringApp;
using System.Text;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace InboxMonitoringApp
{
    public partial class Form1 : Form
    {
        private UserCredential? credential;
        private GmailService? gmailService;
        private ChatSession? chatSession;
        public string systemPrompt = "You are a helpful assistant. The user will provide you with their latest emails. Use this information to assist with their queries."
;

        public Form1()
        {
            InitializeComponent();
            try
            {
                LoadEnvironmentVariables();
                InitializeUIElements();
                DatabaseHelper.InitializeDatabase();
                CreateNewChatSession();
                LoadChatSessions();

            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            systemPrompt = systemPromptTextBox.Text;
        }
        public class EmailModel
        {
            public string Id { get; set; }
            public string From { get; set; }
            public string Subject { get; set; }
            public string Date { get; set; }
            public string Snippet { get; set; }
            public string Flag { get; set; }
            public List<EmailAttachment> Attachments { get; set; }

            public EmailModel()
            {
                Id = "";
                From = "";
                Subject = "";
                Date = "";
                Snippet = "";
                Flag = "NONE";
                Attachments = new List<EmailAttachment>();
            }
        }

        public class EmailAttachment
        {
            public string Filename { get; set; }
            public string Data { get; set; }

            public EmailAttachment()
            {
                Filename = "";
                Data = "";
            }
        }

        private void SaveEmailToDatabase(Message message, string flag)
        {
            string from = string.Empty;
            string subject = string.Empty;
            string date = string.Empty;

            foreach (var header in message.Payload.Headers)
            {
                if (header.Name == "From")
                {
                    from = header.Value;
                }
                else if (header.Name == "Subject")
                {
                    subject = header.Value;
                }
                else if (header.Name == "Date")
                {
                    date = header.Value;
                }
            }

            // Create an EmailModel object to store all the details
            var email = new EmailModel
            {
                Id = message.Id,
                From = from,
                Subject = subject,
                Date = date,
                Snippet = message.Snippet,
                Flag = "NONE",
                Attachments = new List<EmailAttachment>()
            };

            // Add attachments, if any
            if (message.Payload.Parts != null)
            {
                foreach (var part in message.Payload.Parts)
                {
                    if (!string.IsNullOrEmpty(part.Filename))
                    {
                        var attachmentId = part.Body?.AttachmentId;
                        if (attachmentId != null)
                        {
                            var attachment = gmailService.Users.Messages.Attachments.Get("me", message.Id, attachmentId).Execute();
                            var attachmentData = Convert.FromBase64String(attachment.Data.Replace('-', '+').Replace('_', '/'));

                            email.Attachments.Add(new EmailAttachment
                            {
                                Filename = part.Filename,
                                Data = Convert.ToBase64String(attachmentData)
                            });
                        }
                    }
                }
            }

            // Insert the email into the database
            DatabaseHelper.InsertEmail(email);
        }

        private void SaveEmailToJsonFile(Message message, string flag)
        {
            string from = string.Empty;
            string subject = string.Empty;
            string date = string.Empty;

            foreach (var header in message.Payload.Headers)
            {
                if (header.Name == "From")
                {
                    from = header.Value;
                }
                else if (header.Name == "Subject")
                {
                    subject = header.Value;
                }
                else if (header.Name == "Date")
                {
                    date = header.Value;
                }
            }

            // Create an EmailModel object to store all the details
            var email = new EmailModel
            {
                Id = message.Id,
                From = from,
                Subject = subject,
                Date = date,
                Snippet = message.Snippet,
                Attachments = new List<EmailAttachment>()
            };

            // Add attachments, if any
            if (message.Payload.Parts != null)
            {
                foreach (var part in message.Payload.Parts)
                {
                    if (!string.IsNullOrEmpty(part.Filename))
                    {
                        var attachmentId = part.Body?.AttachmentId;
                        if (attachmentId != null)
                        {
                            var attachment = gmailService.Users.Messages.Attachments.Get("me", message.Id, attachmentId).Execute();
                            var attachmentData = Convert.FromBase64String(attachment.Data.Replace('-', '+').Replace('_', '/'));

                            email.Attachments.Add(new EmailAttachment
                            {
                                Filename = part.Filename,
                                Data = Convert.ToBase64String(attachmentData)
                            });
                        }
                    }
                }
            }

            // Set up the path for the JSON file
            string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emails");
            string jsonFilePath = Path.Combine(baseDirectory, "emails.json");

            // Create the base directory if it doesn't exist
            Directory.CreateDirectory(baseDirectory);

            // Read existing emails from JSON (if the file already exists)
            List<EmailModel> emails = new List<EmailModel>();
            if (File.Exists(jsonFilePath))
            {
                var existingData = File.ReadAllText(jsonFilePath);
                emails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EmailModel>>(existingData) ?? new List<EmailModel>();
            }

            // Add the new email to the list and save
            emails.Add(email);
            File.WriteAllText(jsonFilePath, Newtonsoft.Json.JsonConvert.SerializeObject(emails, Newtonsoft.Json.Formatting.Indented));
        }

        private static void LoadEnvironmentVariables()
        {
            Env.Load();
        }

        private void InitializeUIElements()
        {
            lblStatus.Text = "Not Connected";
            lblStatus.ForeColor = Color.Red;
            btnSignOut.Enabled = false;
            btnFetchEmails.Enabled = false;
            sendButton.Enabled = false;
            chatSessionsComboBox.SelectedIndexChanged += ChatSessionsComboBox_SelectedIndexChanged;
        }

        private void ChatSessionsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (chatSessionsComboBox.SelectedItem is ChatSession selectedSession)
                {
                    currentChatSession = selectedSession;
                    DisplayChatHistory(selectedSession.Messages);
                    sendButton.Enabled = true; // Enable send button when a session is selected
                }
                else
                {
                    currentChatSession = null;
                    chatHistoryTextBox.Clear();
                    sendButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting chat session: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void BtnSignIn_Click(object sender, EventArgs e)
        {
            await SignInWithGoogle();
        }

        private void BtnSignOut_Click(object sender, EventArgs e)
        {
            SignOut();
        }

        private async void BtnFetchEmails_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable the button and change the status to indicate progress
                btnFetchEmails.Enabled = false;
                btnFetchEmails.Text = "Fetching...";
                lblStatus.Text = "Fetching Emails...";
                lblStatus.ForeColor = Color.Orange;

                await FetchAndDisplayEmails();
                // Re-enable the button after fetching
                btnFetchEmails.Enabled = true;
                btnFetchEmails.Text = "Fetch Latest Emails";
                lblStatus.Text = "Emails Fetched";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching emails: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Re-enable button and update status in case of error
                btnFetchEmails.Enabled = true;
                btnFetchEmails.Text = "Fetch Latest Emails";
                lblStatus.Text = "Not Connected";
                lblStatus.ForeColor = Color.Red;
            }
        }

        // Modify SignInWithGoogle to include retry logic
        private async Task SignInWithGoogle(int retryCount = 3)
        {
            while (retryCount > 0)
            {
                try
                {
                    var clientSecrets = new ClientSecrets
                    {
                        ClientId = Config.ClientId,
                        ClientSecret = Config.ClientSecret
                    };

                    string[] scopes = { GmailService.Scope.GmailReadonly };
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        clientSecrets,
                        scopes,
                        "user",
                        CancellationToken.None,
                        new NullDataStore()
                    );

                    gmailService = new GmailService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Mercury Inbox Monitoring App",
                    });

                    lblStatus.Text = "Connected";
                    lblStatus.ForeColor = Color.Green;
                    btnSignIn.Enabled = false;
                    btnSignOut.Enabled = true;
                    btnFetchEmails.Enabled = true;

                    MessageBox.Show("Sign-in successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                catch (Exception ex)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        MessageBox.Show($"Error during sign-in: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Retrying sign-in... Remaining attempts: {retryCount}", "Retry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        private void OpenSettingsButton_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.Show();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000, "Mercury Inbox Monitoring App", "App minimized to tray.", ToolTipIcon.Info);
                Hide();
            }
        }
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
        private void SignOut()
        {
            // Reset credentials and Gmail service
            credential = null;
            gmailService = null;

            // Update UI elements
            lblStatus.Text = "Not Connected";
            lblStatus.ForeColor = Color.Red;
            btnSignIn.Enabled = true;
            btnSignOut.Enabled = false;
            btnFetchEmails.Enabled = false;

            MessageBox.Show("You have signed out.", "Sign Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task FetchAndDisplayEmails()
        {
            try
            {
                if (gmailService == null)
                {
                    MessageBox.Show("Please sign in first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                listViewEmails.Items.Clear();
                var request = gmailService.Users.Messages.List("me");
                request.MaxResults = 10;
                request.LabelIds = "INBOX";
                string flag = "NONE";

                var response = await request.ExecuteAsync();

                if (response.Messages != null && response.Messages.Count > 0)
                {
                    List<Message> fetchedEmails = new List<Message>();

                    foreach (var msg in response.Messages)
                    {
                        try
                        {
                            var emailInfoRequest = gmailService.Users.Messages.Get("me", msg.Id);
                            var email = await emailInfoRequest.ExecuteAsync();

                            try
                            {
                                SaveEmailToLocal(email);
                                SaveEmailToDatabase(email, flag); // Add this line
                            }
                            catch (Exception saveEx)
                            {
                                MessageBox.Show($"Error saving email {msg.Id}: {saveEx.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            fetchedEmails.Add(email);
                        }
                        catch (Exception msgEx)
                        {
                            MessageBox.Show($"Error fetching email {msg.Id}: {msgEx.Message}", "Fetch Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    if (fetchedEmails.Count > 0)
                    {
                        DisplayEmails(fetchedEmails);
                    }
                    else
                    {
                        MessageBox.Show("No emails were fetched successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("No emails found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching emails: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void DisplayEmails(List<Message> messages)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayEmails(messages)));
                return;
            }

            foreach (var message in messages)
            {
                string from = GetHeaderValue(message, "From");
                string subject = GetHeaderValue(message, "Subject");
                string date = GetHeaderValue(message, "Date");

                string flag = "NONE";
                try
                {
                    flag = DatabaseHelper.GetEmailFlag(message.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving flag for email {message.Id}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    flag = "NONE";
                }

                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    parsedDate = DateTime.Now;
                }
                string formattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm");

                ListViewItem item = new ListViewItem(new[] { from, subject, formattedDate, flag })
                {
                    Tag = message
                };

                switch (flag)
                {
                    case "URGENT":
                        item.BackColor = Color.Red;
                        item.ForeColor = Color.White;
                        break;
                    case "ATTENTION":
                        item.BackColor = Color.Yellow;
                        item.ForeColor = Color.Black;
                        break;
                    case "GOOD":
                        item.BackColor = Color.Green;
                        item.ForeColor = Color.White;
                        break;
                    default:
                        // Default appearance
                        break;
                }

                listViewEmails.Items.Add(item);
            }
        }


        /// <summary>
        /// Helper method to safely get the value of a header.
        /// </summary>
        /// <param name="message">The email message.</param>
        /// <param name="headerName">The name of the header.</param>
        /// <returns>The header value or an empty string if not found.</returns>
        private string GetHeaderValue(Message message, string headerName)
        {
            if (message.Payload?.Headers == null)
            {
                return string.Empty;
            }

            var header = message.Payload.Headers.FirstOrDefault(h => h.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase));
            return header?.Value ?? string.Empty;
        }


        private void ListViewEmails_DoubleClick(object sender, EventArgs e)
        {
            if (listViewEmails.SelectedItems.Count == 1)
            {
                var selectedItem = listViewEmails.SelectedItems[0];
                var message = (Message)selectedItem.Tag;
                DisplayEmailContent(message);
            }
        }

        //private void DisplayEmailContent(Message message)
        //{
        //    var content = message.Payload?.Body?.Data;
        //    var headers = message.Payload?.Headers;

        //    var emailViewForm = new Form
        //    {
        //        Text = "Email Content",
        //        Size = new Size(800, 600)
        //    };

        //    if (content != null)
        //    {
        //        // Decode the base64url encoded email body content
        //        var decodedContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content.Replace('-', '+').Replace('_', '/')));

        //        var textBox = new TextBox
        //        {
        //            Multiline = true,
        //            Dock = DockStyle.Fill,
        //            ScrollBars = ScrollBars.Both,
        //            Text = decodedContent
        //        };
        //        emailViewForm.Controls.Add(textBox);
        //    }

        //    if (message.Payload.Parts != null)
        //    {
        //        foreach (var part in message.Payload.Parts)
        //        {
        //            if (!string.IsNullOrEmpty(part.Filename))
        //            {
        //                var attachmentButton = new Button
        //                {
        //                    Text = $"Download {part.Filename}",
        //                    Dock = DockStyle.Top
        //                };
        //                attachmentButton.Click += (s, e) =>
        //                {
        //                    var attachment = gmailService.Users.Messages.Attachments.Get("me", message.Id, part.Body.AttachmentId).Execute();
        //                    var attachmentData = Convert.FromBase64String(attachment.Data.Replace('-', '+').Replace('_', '/'));
        //                    var saveDialog = new SaveFileDialog
        //                    {
        //                        FileName = part.Filename
        //                    };
        //                    if (saveDialog.ShowDialog() == DialogResult.OK)
        //                    {
        //                        File.WriteAllBytes(saveDialog.FileName, attachmentData);
        //                        MessageBox.Show($"Attachment {part.Filename} saved successfully.", "Attachment Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                    }
        //                };
        //                emailViewForm.Controls.Add(attachmentButton);
        //            }
        //        }
        //    }

        //    emailViewForm.ShowDialog();
        //}
        private void BtnFlagUrgent_Click(object sender, EventArgs e)
        {
            FlagSelectedEmail("URGENT");
        }

        private void BtnFlagAttention_Click(object sender, EventArgs e)
        {
            FlagSelectedEmail("ATTENTION");
        }

        private void BtnFlagGood_Click(object sender, EventArgs e)
        {
            FlagSelectedEmail("GOOD");
        }

        private void FlagSelectedEmail(string flag)
        {
            if (listViewEmails.SelectedItems.Count == 1)
            {
                var selectedItem = listViewEmails.SelectedItems[0];
                var message = (Message)selectedItem.Tag;
                DatabaseHelper.UpdateEmailFlag(message.Id, flag);

                // Update UI to reflect the flagging
                selectedItem.SubItems[3].Text = flag; // Update the 'Flag' column

                // Change the item's color based on the flag
                switch (flag)
                {
                    case "URGENT":
                        selectedItem.BackColor = Color.Red;
                        selectedItem.ForeColor = Color.White;
                        break;
                    case "ATTENTION":
                        selectedItem.BackColor = Color.Yellow;
                        selectedItem.ForeColor = Color.Black;
                        break;
                    case "GOOD":
                        selectedItem.BackColor = Color.Green;
                        selectedItem.ForeColor = Color.White;
                        break;
                    default:
                        selectedItem.BackColor = Color.White;
                        selectedItem.ForeColor = Color.Black;
                        break;
                }

                MessageBox.Show($"Email flagged as {flag}", "Flagging", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select an email to flag.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetEmailBody(Message message)
        {
            string body = string.Empty;
            if (message.Payload.Parts == null && message.Payload.Body != null)
            {
                // This is a simple email with no attachments and no multipart
                body = DecodeBase64String(message.Payload.Body.Data);
            }
            else
            {
                body = GetPlainTextFromMessageParts(message.Payload.Parts);
            }
            return body;
        }
        private void SaveEmailToLocal(Message message)
        {
            string from = string.Empty;
            string subject = string.Empty;
            string date = string.Empty;
            string flag = "NONE";

            foreach (var header in message.Payload.Headers)
            {
                if (header.Name == "From")
                {
                    from = SanitizeForFileName(header.Value);
                }
                else if (header.Name == "Subject")
                {
                    subject = SanitizeForFileName(header.Value);
                }
                else if (header.Name == "Date")
                {
                    // Parse Gmail's date format properly
                    try
                    {
                        // Remove timezone indicator (MDT) and parse
                        string dateStr = header.Value;
                        dateStr = Regex.Replace(dateStr, @"\([^\)]*\)", "").Trim();
                        date = DateTimeOffset.Parse(dateStr).ToString("yyyy-MM-dd");
                    }
                    catch
                    {
                        // Fallback if parsing fails - use current date
                        date = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                }
            }

            // Truncate fields to prevent path length issues
            from = TruncateString(from, 30);
            subject = TruncateString(subject, 30);

            // Create a more concise folder name
            string folderName = $"{date}_{from}";
            string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emails");
            string emailDirectory = Path.Combine(baseDirectory, folderName);

            try
            {
                Directory.CreateDirectory(baseDirectory);
                Directory.CreateDirectory(emailDirectory);

                // Get the full email body
                string emailBody = GetEmailBody(message);
                // Save email metadata and content
                string emailFilePath = Path.Combine(emailDirectory, "email.txt");
                StringBuilder emailContent = new StringBuilder();
                emailContent.AppendLine($"From: {from}");
                emailContent.AppendLine($"Subject: {subject}");
                emailContent.AppendLine($"Date: {date}");
                emailContent.AppendLine($"Flag: {flag}");
                emailContent.AppendLine("\nContent:");
                emailContent.AppendLine(emailBody);

                File.WriteAllText(emailFilePath, emailContent.ToString());

                // Save attachments if present
                if (message.Payload.Parts != null)
                {
                    foreach (var part in message.Payload.Parts)
                    {
                        if (!string.IsNullOrEmpty(part.Filename))
                        {
                            var attachmentId = part.Body?.AttachmentId;
                            if (attachmentId != null)
                            {
                                try
                                {
                                    var attachment = gmailService.Users.Messages.Attachments.Get("me", message.Id, attachmentId).Execute();
                                    var attachmentData = Convert.FromBase64String(attachment.Data.Replace('-', '+').Replace('_', '/'));
                                    string sanitizedFileName = SanitizeForFileName(part.Filename);
                                    string attachmentPath = Path.Combine(emailDirectory, sanitizedFileName);
                                    File.WriteAllBytes(attachmentPath, attachmentData);
                                }
                                catch (Exception ex)
                                {
                                    // Log attachment save error but continue processing
                                    File.AppendAllText(Path.Combine(emailDirectory, "errors.log"),
                                        $"Error saving attachment {part.Filename}: {ex.Message}\n");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors during save process
                string errorPath = Path.Combine(baseDirectory, "errors.log");
                File.AppendAllText(errorPath, $"Error saving email {message.Id}: {ex.Message}\n");
                throw; // Re-throw to handle in calling method
            }
        }

        private static string GetPlainTextFromMessageParts(IList<MessagePart> parts)
        {
            string emailBody = "";
            foreach (var part in parts)
            {
                if (part.MimeType == "text/plain")
                {
                    emailBody += DecodeBase64String(part.Body.Data);
                }
                else if (part.Parts != null)
                {
                    emailBody += GetPlainTextFromMessageParts(part.Parts);
                }
            }
            return emailBody;
        }

        private static string DecodeBase64String(string s)
        {
            var ts = s.Replace('-', '+').Replace('_', '/');
            var bytes = Convert.FromBase64String(ts);
            return Encoding.UTF8.GetString(bytes);
        }
        private void chatSessionsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (chatSessionsComboBox.SelectedItem is ChatSession selectedSession)
                {
                    currentChatSession = selectedSession;
                    DisplayChatHistory(selectedSession.Messages);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting chat session: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static string SanitizeForFileName(string input)
        {
            if (string.IsNullOrEmpty(input)) return "unnamed";

            // Replace invalid characters with underscores
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                input = input.Replace(c, '_');
            }

            // Remove multiple consecutive underscores
            input = Regex.Replace(input, @"_+", "_");

            // Remove leading/trailing underscores
            return input.Trim('_');
        }

        private static string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }
        /// <summary>
        /// Loads all chat sessions from the database and populates the ComboBox.
        /// </summary>
        internal void LoadChatSessions()
        {
            try
            {
                chatSessionsComboBox.Items.Clear();
                var sessions = DatabaseHelper.GetAllChatSessions();

                foreach (var session in sessions)
                {
                    chatSessionsComboBox.Items.Add(session);
                }

                chatSessionsComboBox.DisplayMember = "Name";

                if (sessions.Count > 0)
                {
                    chatSessionsComboBox.SelectedIndex = 0; // Select the first session by default
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chat sessions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DisplayChatHistory(List<ChatMessage> messages)
        {
            chatHistoryTextBox.Clear();
            foreach (var message in messages)
            {
                string role = message.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? "You" : "Assistant";
                chatHistoryTextBox.AppendText($"{role}: {message.Content}\n\n");
            }
        }


        private void CreateNewChatSession()
        {
            currentChatSession = new ChatSession
            {
                Name = $"Chat {DateTime.Now:yyyy-MM-dd HH:mm}"
            };

            try
            {
                DatabaseHelper.SaveChatSession(currentChatSession);
                LoadChatSessions();
                chatSessionsComboBox.SelectedItem = currentChatSession;
                chatHistoryTextBox.Clear();
                sendButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new chat session: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SystemPromptTextBox_TextChanged(object sender, EventArgs e)
        {
            // Update the system prompt variable with the current text
            systemPrompt = systemPromptTextBox.Text;
        }
        private void NewChatButton_Click(object sender, EventArgs e)
        {
            CreateNewChatSession();
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentChatSession == null)
                {
                    MessageBox.Show("No active chat session. Please create or select a chat session.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(chatInputTextBox.Text))
                {
                    MessageBox.Show("Please enter a message to send.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var userMessage = new ChatMessage
                {
                    Role = "user",
                    Content = chatInputTextBox.Text,
                    Timestamp = DateTime.Now
                };

                currentChatSession.Messages.Add(userMessage);
                DisplayChatHistory(currentChatSession.Messages);

                chatInputTextBox.Clear();

                // Prepare email context
                var emailContext = GetEmailContext();

                // Use the updated system prompt variable
                var response = await CallGPTApi(systemPrompt, emailContext, currentChatSession.Messages);

                var assistantMessage = new ChatMessage
                {
                    Role = "assistant",
                    Content = response,
                    Timestamp = DateTime.Now
                };
                currentChatSession.Messages.Add(assistantMessage);
                DisplayChatHistory(currentChatSession.Messages);

                DatabaseHelper.SaveChatSession(currentChatSession);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private static string GetEmailContext()
        {
            var emails = DatabaseHelper.GetLatestEmails(10);

            var context = new StringBuilder();
            foreach (var email in emails)
            {
                context.AppendLine("----- Email -----");
                context.AppendLine($"From: {email.From}");
                context.AppendLine($"Subject: {email.Subject}");
                context.AppendLine($"Date: {email.Date}");
                context.AppendLine($"Content: {email.Snippet}");
                context.AppendLine("----- End Email -----\n");
            }
            return context.ToString();
        }


        private static async Task<string> CallGPTApi(string systemPrompt, string emailContext, List<ChatMessage> messages)
        {
            try
            {
                var requestMessages = new List<object>
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = "Here are the latest emails:\n\n" + emailContext }
        };

                // Include the existing chat messages
                requestMessages.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = requestMessages,
                    temperature = 1,
                    max_tokens = 8000
                };

                // Send POST request to GPT-4o-mini API endpoint
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Environment.GetEnvironmentVariable("GPT4O_API_KEY"));
                    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                        new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    return responseJson.choices[0].message.content.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calling GPT API: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Error: Unable to fetch response from GPT-4";
            }
        }


        private void UpdateChatDisplay()
        {
            chatHistoryTextBox.Clear();
            foreach (var message in currentChatSession.Messages)
            {
                chatHistoryTextBox.AppendText($"{message.Role}: {message.Content}\n\n");
            }
            chatHistoryTextBox.ScrollToCaret();
        }

        // Fix for email display issue
        private static void DisplayEmailContent(Message message)
        {
            var emailViewForm = new Form
            {
                Text = "Email Content",
                Size = new Size(800, 600)
            };

            var contentTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };

            // Get the email body
            string emailBody = GetEmailBody(message);

            contentTextBox.Text = emailBody;

            emailViewForm.Controls.Add(contentTextBox);
            emailViewForm.Show();
        }
        private void BtnExportToExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Retrieve all emails from the database
                var emails = DatabaseHelper.GetAllEmails();

                // Use a library like EPPlus or ClosedXML to create an Excel file
                // For this example, we'll use ClosedXML

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Emails");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "From";
                    worksheet.Cell(1, 2).Value = "Subject";
                    worksheet.Cell(1, 3).Value = "Date";
                    worksheet.Cell(1, 4).Value = "Flag";
                    worksheet.Cell(1, 5).Value = "Snippet";

                    // Populate data
                    int row = 2;
                    foreach (var email in emails)
                    {
                        worksheet.Cell(row, 1).Value = email.From;
                        worksheet.Cell(row, 2).Value = email.Subject;
                        worksheet.Cell(row, 3).Value = email.Date;
                        worksheet.Cell(row, 4).Value = email.Flag;
                        worksheet.Cell(row, 5).Value = email.Snippet;
                        row++;
                    }

                    // Save the workbook
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                        FileName = "Emails.xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show("Emails exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting to Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSendChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(chatInputTextBox.Text)) return;

            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = chatInputTextBox.Text
            };
            currentChatSession.Messages.Add(userMessage);
            UpdateChatDisplay();

            chatInputTextBox.Clear();

            // Prepare email context
            var emailContext = GetEmailContext();

            // Call GPT-4 API with system prompt, email context, and chat history
            var response = await CallGPTApi(systemPromptTextBox.Text, emailContext, currentChatSession.Messages);

            var assistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = response
            };
            currentChatSession.Messages.Add(assistantMessage);
            UpdateChatDisplay();

            DatabaseHelper.SaveChatSession(currentChatSession);
        }

        private void OpenChatButton_Click(object sender, EventArgs e)
        {
            CreateNewChatSession();
        }


    }
}

