public class Device
{
    public int Id { get; set; }
    public int Connection { get; set; }
    public string? LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
    public DateTime LastUpdated { get; set; }
}