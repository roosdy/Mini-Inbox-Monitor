// DatabaseHelper.cs
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using static InboxMonitoringApp.Form1;

namespace InboxMonitoringApp
{
    public static class DatabaseHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // Define the path to the SQLite database file
        public static string DbPath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InboxMonitoringApp.db");

        /// <summary>
        /// Initializes the database by creating it and necessary tables if they don't exist.
        /// </summary>
        public static void InitializeDatabase()
        {
            try
            {
                bool dbExists = File.Exists(DbPath);

                if (!dbExists)
                {
                    // Create the database file
                    SQLiteConnection.CreateFile(DbPath);
                }

                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    // Create ChatSessions table if it doesn't exist
                    string createChatSessionsTable = @"
                        CREATE TABLE IF NOT EXISTS ChatSessions (
                            Id TEXT PRIMARY KEY,
                            Name TEXT NOT NULL,
                            CreatedAt DATETIME NOT NULL
                        );";

                    using (var cmd = new SQLiteCommand(createChatSessionsTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create ChatMessages table if it doesn't exist
                    string createChatMessagesTable = @"
                        CREATE TABLE IF NOT EXISTS ChatMessages (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            SessionId TEXT NOT NULL,
                            Role TEXT NOT NULL,
                            Content TEXT NOT NULL,
                            Timestamp DATETIME NOT NULL,
                            FOREIGN KEY(SessionId) REFERENCES ChatSessions(Id)
                        );";

                    using (var cmd = new SQLiteCommand(createChatMessagesTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create Emails table if it doesn't exist
                    string createEmailsTable = @"
                        CREATE TABLE IF NOT EXISTS Emails (
                            Id TEXT PRIMARY KEY,
                            FromText TEXT NOT NULL,
                            Subject TEXT NOT NULL,
                            Date TEXT NOT NULL,
                            Snippet TEXT NOT NULL,
                            Flag TEXT NOT NULL
                        );";

                    using (var cmd = new SQLiteCommand(createEmailsTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create Attachments table if it doesn't exist
                    string createAttachmentsTable = @"
                        CREATE TABLE IF NOT EXISTS Attachments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            EmailId TEXT NOT NULL,
                            Filename TEXT NOT NULL,
                            Data TEXT NOT NULL,
                            FOREIGN KEY(EmailId) REFERENCES Emails(Id)
                        );";

                    using (var cmd = new SQLiteCommand(createAttachmentsTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Logger.Info("Database initialized successfully.");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle accordingly
                Logger.Error(ex, "Error initializing database.");
                throw new Exception($"Error initializing database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all chat sessions from the database.
        /// </summary>
        /// <returns>List of ChatSession objects.</returns>
        public static List<ChatSession> GetAllChatSessions()
        {
            var chatSessions = new List<ChatSession>();

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string query = "SELECT Id, Name, CreatedAt FROM ChatSessions ORDER BY CreatedAt DESC;";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var session = new ChatSession
                                {
                                    Id = reader["Id"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                                    Messages = GetChatMessagesBySessionId(reader["Id"].ToString())
                                };

                                chatSessions.Add(session);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle accordingly
                throw new Exception($"Error retrieving chat sessions: {ex.Message}", ex);
            }

            return chatSessions;
        }
        public static void SaveChatSession(ChatSession session)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        // Check if the session already exists
                        string checkSessionQuery = "SELECT COUNT(1) FROM ChatSessions WHERE Id = @Id;";
                        using (var cmd = new SQLiteCommand(checkSessionQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@Id", session.Id);
                            long count = (long)cmd.ExecuteScalar();
                            if (count == 0)
                            {
                                // Insert new ChatSession
                                string insertSessionQuery = @"
                            INSERT INTO ChatSessions (Id, Name, CreatedAt)
                            VALUES (@Id, @Name, @CreatedAt);";

                                using (var insertCmd = new SQLiteCommand(insertSessionQuery, connection))
                                {
                                    insertCmd.Parameters.AddWithValue("@Id", session.Id);
                                    insertCmd.Parameters.AddWithValue("@Name", session.Name);
                                    insertCmd.Parameters.AddWithValue("@CreatedAt", session.CreatedAt);

                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Insert ChatMessages
                        string insertMessageQuery = @"
                    INSERT INTO ChatMessages (SessionId, Role, Content, Timestamp)
                    VALUES (@SessionId, @Role, @Content, @Timestamp);";

                        foreach (var message in session.Messages)
                        {
                            using (var cmd = new SQLiteCommand(insertMessageQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@SessionId", session.Id);
                                cmd.Parameters.AddWithValue("@Role", message.Role);
                                cmd.Parameters.AddWithValue("@Content", message.Content);
                                cmd.Parameters.AddWithValue("@Timestamp", message.Timestamp);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }

                    connection.Close();
                }
            }
            catch (SQLiteException sqlex)
            {
                // Handle specific SQLite exceptions
                MessageBox.Show($"Database error: {sqlex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                MessageBox.Show($"An error occurred while saving the chat session: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Retrieves all chat messages associated with a specific chat session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <returns>List of ChatMessage objects.</returns>
        private static List<ChatMessage> GetChatMessagesBySessionId(string sessionId)
        {
            var messages = new List<ChatMessage>();

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string query = "SELECT Role, Content, Timestamp FROM ChatMessages WHERE SessionId = @SessionId ORDER BY Timestamp ASC;";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var message = new ChatMessage
                                {
                                    Role = reader["Role"].ToString(),
                                    Content = reader["Content"].ToString(),
                                    Timestamp = DateTime.Parse(reader["Timestamp"].ToString())
                                };

                                messages.Add(message);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle accordingly
                throw new Exception($"Error retrieving chat messages for session {sessionId}: {ex.Message}", ex);
            }

            return messages;
        }
        public static string GetEmailFlag(string emailId)
        {
            string flag = "NONE";
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string query = "SELECT Flag FROM Emails WHERE Id = @EmailId;";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@EmailId", emailId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            flag = result.ToString();
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving email flag: {ex.Message}", ex);
            }
            return flag;
        }
        public static List<EmailModel> GetLatestEmails(int count)
        {
            var emails = new List<EmailModel>();
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string query = $"SELECT Id, FromText, Subject, Date, Snippet FROM Emails ORDER BY Date DESC LIMIT {count};";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var email = new EmailModel
                                {
                                    Id = reader["Id"].ToString(),
                                    From = reader["FromText"].ToString(),
                                    Subject = reader["Subject"].ToString(),
                                    Date = reader["Date"].ToString(),
                                    Snippet = reader["Snippet"].ToString()
                                };
                                emails.Add(email);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving latest emails: {ex.Message}", ex);
            }
            return emails;
        }
        public static List<EmailModel> GetAllEmails()
        {
            var emails = new List<EmailModel>();
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string query = "SELECT Id, FromText, Subject, Date, Snippet, Flag FROM Emails;";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var email = new EmailModel
                                {
                                    Id = reader["Id"].ToString(),
                                    From = reader["FromText"].ToString(),
                                    Subject = reader["Subject"].ToString(),
                                    Date = reader["Date"].ToString(),
                                    Snippet = reader["Snippet"].ToString(),
                                    Flag = reader["Flag"].ToString()
                                };
                                emails.Add(email);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving emails: {ex.Message}", ex);
            }
            return emails;
        }

        /// <summary>
        /// Inserts a new chat session into the database.
        /// </summary>
        /// <param name="session">The ChatSession object to insert.</param>
        public static void InsertChatSession(ChatSession session)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO ChatSessions (Id, Name, CreatedAt) VALUES (@Id, @Name, @CreatedAt);";

                    using (var cmd = new SQLiteCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", session.Id);
                        cmd.Parameters.AddWithValue("@Name", session.Name);
                        cmd.Parameters.AddWithValue("@CreatedAt", session.CreatedAt);

                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting chat session: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new chat message into the database.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <param name="message">The ChatMessage object to insert.</param>
        public static void InsertChatMessage(string sessionId, ChatMessage message)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO ChatMessages (SessionId, Role, Content, Timestamp) VALUES (@SessionId, @Role, @Content, @Timestamp);";

                    using (var cmd = new SQLiteCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        cmd.Parameters.AddWithValue("@Role", message.Role);
                        cmd.Parameters.AddWithValue("@Content", message.Content);
                        cmd.Parameters.AddWithValue("@Timestamp", message.Timestamp);

                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting chat message: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts an email into the Emails table.
        /// </summary>
        /// <param name="email">The EmailModel object to insert.</param>
        public static void InsertEmail(EmailModel email)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string insertEmailQuery = @"
                        INSERT OR IGNORE INTO Emails (Id, FromText, Subject, Date, Snippet, Flag)
                        VALUES (@Id, @FromText, @Subject, @Date, @Snippet, @Flag);";

                    using (var cmd = new SQLiteCommand(insertEmailQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", email.Id);
                        cmd.Parameters.AddWithValue("@FromText", email.From);
                        cmd.Parameters.AddWithValue("@Subject", email.Subject);
                        cmd.Parameters.AddWithValue("@Date", email.Date);
                        cmd.Parameters.AddWithValue("@Snippet", email.Snippet);
                        cmd.Parameters.AddWithValue("@Flag", "NONE"); // Default flag

                        cmd.ExecuteNonQuery();
                    }

                    // Insert attachments
                    foreach (var attachment in email.Attachments)
                    {
                        string insertAttachmentQuery = @"
                            INSERT INTO Attachments (EmailId, Filename, Data)
                            VALUES (@EmailId, @Filename, @Data);";

                        using (var cmd = new SQLiteCommand(insertAttachmentQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@EmailId", email.Id);
                            cmd.Parameters.AddWithValue("@Filename", attachment.Filename);
                            cmd.Parameters.AddWithValue("@Data", attachment.Data);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting email: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates the flag of an email.
        /// </summary>
        /// <param name="emailId">The ID of the email to update.</param>
        /// <param name="flag">The new flag value.</param>
        public static void UpdateEmailFlag(string emailId, string flag)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;"))
                {
                    connection.Open();

                    string updateQuery = "UPDATE Emails SET Flag = @Flag WHERE Id = @EmailId;";

                    using (var cmd = new SQLiteCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Flag", flag);
                        cmd.Parameters.AddWithValue("@EmailId", emailId);

                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating email flag: {ex.Message}", ex);
            }
        }
    }
}
