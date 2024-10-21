//ChatSession.cs
public class ChatSession
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<ChatMessage> Messages { get; set; }
    public DateTime CreatedAt { get; set; }

    public ChatSession()
    {
        Id = Guid.NewGuid().ToString();
        Name = ""; // Assign default value
        Messages = new List<ChatMessage>();
        CreatedAt = DateTime.Now;
    }
}

public class ChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public ChatMessage()
    {
        Role = "";
        Content = "";
        Timestamp = DateTime.Now;
    }
}
