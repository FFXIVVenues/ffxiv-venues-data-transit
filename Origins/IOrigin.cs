using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Origins;

public interface IOrigin
{
    Task<Venue[]> FetchAllAsync();
}