using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DeepEqual.Syntax;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.PostActions;

public class ValidateTransit : IPostAction
{

    public ValidateTransitApi Origin { get; set; } = new();
    public ValidateTransitApi Destination { get; set; } = new();
    
    public async Task ExecuteAsync()
    {
        Console.WriteLine("Validating transit.");
        Console.WriteLine("Fetching venues from origin API.");
        var response = await this.Origin.Client.GetAsync("/venue");
        if (!response.IsSuccessStatusCode)
            throw new WebException($"Receive unsuccessful status code '{response.StatusCode}' in fetch from origin.");
        var originVenues = await response.Content.ReadFromJsonAsync<Venue[]>();
        if (originVenues == null)
            throw new WebException($"Invalid response received in fetch from origin.");
        Console.WriteLine("Fetched venues from origin API.");
        
        Console.WriteLine("Fetching venues from destination API.");
        response = await this.Destination.Client.GetAsync("/venue");
        if (!response.IsSuccessStatusCode)
            throw new WebException($"Receive unsuccessful status code '{response.StatusCode}' in fetch from destination.");
        var destinationVenues = await response.Content.ReadFromJsonAsync<Venue[]>();
        if (destinationVenues == null)
            throw new WebException($"Invalid response received in fetch from destination.");
        Console.WriteLine("Fetched venues from destination API.");

        Console.WriteLine("Validating venues.");

        foreach (var originVenue in originVenues)
        {
            Console.WriteLine($"Validating venue {originVenue.Id}.");
            var venue = destinationVenues.SingleOrDefault(v => v.Id == originVenue.Id);
            if (venue == null)
                throw new ValidationException($"Venue {originVenue.Id} could not be found at destination.");

            if (venue.BannerUri == null && originVenue.BannerUri != null)
                throw new ValidationException($"Venue {originVenue.Id} does not have a banner.");

            venue.WithDeepEqual(originVenue)
                  .IgnoreProperty<Venue>(x => x.BannerUri)
                  .Assert();
        }
    }
}

public class ValidateTransitApi
{
    public readonly HttpClient Client = new();

    public string? BaseUrl
    {
        get => this.Client.BaseAddress?.ToString();
        init => this.Client.BaseAddress = value != null ? new Uri(value) : null;
    }
    public string? AuthorizationKey
    {
        get => this.Client.DefaultRequestHeaders.Authorization?.Parameter;
        init => this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", value);
    }
}