using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FFXIVVenues.DataTransit.PostActions;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.PreActions;

public class ApiBackUpPreAction : IPreAction, IDisposable
{
    
    private readonly HttpClient _httpClient = new();

    public string SavePath { get; set; } = "venues-backup-{date}.json";
    public string? BaseUrl
    {
        get => this._httpClient.BaseAddress?.ToString();
        init => this._httpClient.BaseAddress = value != null ? new Uri(value) : null;
    }
    public string? AuthorizationKey
    {
        get => this._httpClient.DefaultRequestHeaders.Authorization?.Parameter;
        init => this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", value);
    }
    
    public async Task ExecuteAsync()
    {
        var response = await this._httpClient.GetAsync($"/venue");
        if (!response.IsSuccessStatusCode)
        {
            var message =
                $"Could not back up venues at destination; API responded with status code '{response.StatusCode}'.";
            _ = Console.Error.WriteLineAsync(message);
            throw new WebException(message);
        }
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        if (result == null)
            throw new WebException($"Invalid response received in fetch from origin.");
        var date = DateTime.UtcNow.ToFileTimeUtc();
        var path = this.SavePath.Replace("{date}", date.ToString());
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(result));
    }
    
    public void Dispose() =>
        this._httpClient.Dispose();
    
}