using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Destinations;

public class ApiDestination : IDestination, IDisposable
{

    private readonly HttpClient _httpClient = new();
    private readonly HttpClient _bannerHttpClient = new();

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

    public bool SendModel { get; set; } = true;
    public bool SendBanners { get; set; } = true;
    public bool SendApprovals { get; set; } = true;
    public bool SendAdded { get; set; } = true;

    public async Task SendAsync(Venue[] venues)
    {
        Console.WriteLine($"Sending venues to destination API.");
        foreach (var venue in venues)
        {
            Console.WriteLine($"Sending venue {venue.Id}");
            if (this.SendModel)
            {
                var response = await this._httpClient.PutAsJsonAsync($"/venue/{venue.Id}", venue);
                var json = response.RequestMessage?.Content?.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    _ = Console.Error.WriteLineAsync($"Failed to send venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
                    continue;
                }
            }
            if (this.SendBanners && venue.BannerUri != null)
            {
                var response = await _bannerHttpClient.GetAsync(venue.BannerUri);
                if (!response.IsSuccessStatusCode)
                    _ = Console.Error.WriteLineAsync($"Failed to get banner for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
                var bannerStream = await response.Content.ReadAsStreamAsync();
                var streamContent = new StreamContent(bannerStream);
                streamContent.Headers.ContentType = response.Content.Headers.ContentType;
                response = await _httpClient.PutAsync($"/venue/{venue.Id}/media", streamContent);
                if (!response.IsSuccessStatusCode)
                    _ = Console.Error.WriteLineAsync($"Failed to send banner for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
            }
            if (this.SendAdded)
            {
                var response = await this._httpClient.PutAsJsonAsync($"/venue/{venue.Id}/added", venue.Added);
                if (!response.IsSuccessStatusCode)
                    _ = Console.Error.WriteLineAsync($"Failed to set added date for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
            }
            if (this.SendApprovals)
            {
                var response = await this._httpClient.PutAsJsonAsync($"/venue/{venue.Id}/approved", venue.Approved);
                if (!response.IsSuccessStatusCode)
                    _ = Console.Error.WriteLineAsync($"Failed to set added date for venue {venue.Id}, API responded with status code '{response.StatusCode}'.");
            }
            Console.WriteLine($"Sent venue {venue.Id}");
        }
        Console.WriteLine($"Sent all venues to destination API.");
    }

    public void Dispose() =>
        this._httpClient.Dispose();

}