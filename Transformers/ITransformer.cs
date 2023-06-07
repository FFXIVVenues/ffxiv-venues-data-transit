using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Transformers;

public interface ITransformer
{
    Task<Venue[]> TransformAsync(Venue[] venues);
}