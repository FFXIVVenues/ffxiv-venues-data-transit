namespace FFXIVVenues.DataTransit.PostActions;

public class HeartbeatPostAction : IPostAction
{

    public string? HeartbeatUrl { get; init; }
    private HttpClient _client = new();

    public Task ExecuteAsync()
    {
        Console.WriteLine("Sending heartbeat.");
        return this._client.GetAsync(this.HeartbeatUrl);
    }
    
}