using System.Net.Http.Headers;
using System.Net.Http.Json;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Destinations;

public class ApiDestination : IDestination, IDisposable
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

    public async Task SendAsync(Venue[] venues)
    {
        Console.WriteLine($"Sending venues to destination API.");
        foreach (var venue in venues)
        {
            Console.WriteLine($"Sending venue {venue.Id}");
            var response = await this._httpClient.PutAsJsonAsync($"/venue/{venue.Id}", venue);
            if (!response.IsSuccessStatusCode)
            {
                _ = Console.Error.WriteLineAsync($"Failed to send venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
                continue;
            }
            if (venue.BannerUri != null)
            {
                response = await _httpClient.GetAsync(venue.BannerUri);
                if (!response.IsSuccessStatusCode)
                    _ = Console.Error.WriteLineAsync($"Failed to get banner for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
                var bannerStream = await response.Content.ReadAsStreamAsync();
                var streamContent = new StreamContent(bannerStream);
                streamContent.Headers.ContentType = response.Content.Headers.ContentType;
                await _httpClient.PutAsync($"/venue/{venue.Id}/media", streamContent);
                if (!response.IsSuccessStatusCode)
                    _ = Console.Error.WriteLineAsync($"Failed to send banner for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
            }
            response = await this._httpClient.PutAsJsonAsync($"/venue/{venue.Id}/added", venue.Added);
            if (!response.IsSuccessStatusCode)
                _ = Console.Error.WriteLineAsync($"Failed to set added date for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
            response = await this._httpClient.PutAsJsonAsync($"/venue/{venue.Id}/approved", venue.Approved);
            if (!response.IsSuccessStatusCode)
                _ = Console.Error.WriteLineAsync($"Failed to set added date for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
            Console.WriteLine($"Sent venue {venue.Id}");
        }
        Console.WriteLine($"Sent all venues to destination API.");
    }

    public void Dispose() =>
        this._httpClient.Dispose();

}