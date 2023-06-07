using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Destinations;

public interface IDestination
{
    Task SendAsync(Venue[] venues);
}