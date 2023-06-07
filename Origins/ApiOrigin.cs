using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Origins;

public class ApiOrigin : IOrigin, IDisposable
{

    private readonly HttpClient _httpClient = new();
    
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
    
    public async Task<Venue[]> FetchAllAsync()
    {
        Console.WriteLine("Fetching venues from origin API.");
        var response = await this._httpClient.GetAsync("/venue");
        if (!response.IsSuccessStatusCode)
            throw new WebException($"Receive unsuccessful status code '{response.StatusCode}' in fetch from origin.");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        if (result == null)
            throw new WebException($"Invalid response received in fetch from origin.");
        Console.WriteLine("Fetched venues from origin API.");
        return result;
    }
    
    public void Dispose() =>
        this._httpClient.Dispose();
    
}