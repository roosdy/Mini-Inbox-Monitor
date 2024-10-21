# **Mini Inbox Monitoring App**

### **Description**

Mini Inbox Monitoring App is a Windows desktop application that integrates with Gmail to fetch, organize, and display your latest emails. The app allows you to categorize and flag emails for easy prioritization, export email data to Excel, and utilize an integrated chatbot to assist with summarizing or interacting with your email content.

### **Features**
- Fetch and display the latest 10 Gmail emails.
- Flag emails as **URGENT**, **ATTENTION**, or **GOOD**.
- Save emails and attachments locally.
- Export emails to Excel for offline review.
- Integrated chatbot powered by GPT-4o-mini to summarize and interact with your email context.
- Automatically adjusts layout and content visibility upon window resize.

### **Technologies Used**
- C#
- .NET
- Google Gmail API
- SQLite for local storage
- ClosedXML for exporting to Excel
- GPT-4o-mini for AI-based interactions

---

## **Installation**

### **Requirements**
- Windows 10 or later
- .NET 6.0 Runtime (can be bundled with the app)
- Gmail API access (OAuth2 credentials)

### **Steps to Run the App**
1. **Download the Application:**
   - Download the latest release from the [releases page](#) (Insert link if hosted).
   
2. **Run the Application:**
   - Open the `.exe` file in the download folder.
   - If required, allow the application access through any firewall prompts.

3. **Google Authentication:**
   - Upon launching the app, sign in with your Google account to grant access to Gmail.
   
4. **Fetching Emails:**
   - Click the **Fetch Latest Emails** button to retrieve the 10 most recent emails from your inbox.
   
5. **Flagging Emails:**
   - Use the buttons (**URGENT**, **ATTENTION**, **GOOD**) to categorize the emails and see the flagged status in the list view.

6. **Export to Excel:**
   - Click the **Export to Excel** button to export the fetched emails into an Excel file for easy offline access.

---

## **Development Setup**

### **Pre-Requisites**
- Visual Studio 2022 or later
- .NET 6.0 SDK
- Gmail API enabled on your Google Developer Console
  - Client ID and Client Secret configured in a `.env` file.

### **Setting Up the Project**
1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/Mercury-Inbox-Monitoring-App.git
   ```
   
2. **Install Dependencies:**
   - Ensure you have the necessary libraries by running:
     - `DotNetEnv` for environment variables.
     - `ClosedXML` for Excel export.
     - `Google.Apis.Gmail.v1` for Gmail integration.
     - `SQLite` for database management.

3. **Setup OAuth2 Credentials:**
   - Create a `.env` file in the root of the project with the following variables:
     ```
     CLIENT_ID=your_google_client_id
     CLIENT_SECRET=your_google_client_secret
     GPT4O_API_KEY=your_gpt4o_mini_api_key
     ```

4. **Build and Run:**
   - Open the solution in Visual Studio.
   - Set the configuration to `Release` mode.
   - Build and run the project from Visual Studio.

---

## **Contributing**
- Fork the repository and create a new feature branch for any changes.
- Submit a pull request with a detailed description of the changes.

---

## **Known Issues**
- Scrollbars may not appear until the window is maximized.
- Only supports Gmail accounts at the moment.
  
---

## **License**
This project is licensed under the MIT License. See the LICENSE file for more details.
