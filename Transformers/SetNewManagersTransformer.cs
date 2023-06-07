using System.Configuration;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Transformers;

public class SetNewManagersTransformer : ITransformer
{
    public List<string> NewManagerIds { get; init; } = new();
    public SelectionBounds Select { get; init; } = new(1, 3);
    
    public Task<Venue[]> TransformAsync(Venue[] venues)
    {
        if (this.NewManagerIds == null || !this.NewManagerIds.Any())
            throw new ConfigurationErrorsException("NewManagerIds not set for SetNewManagersTransformer.");
        
        var min = int.Min(this.Select.Min, 1);
        var max = int.Max(this.Select.Max, this.NewManagerIds.Count);

        Console.WriteLine($"Transforming venues; setting new managers.");
        var random = new Random();
        foreach (var venue in venues)
        {
            var selectionCount = random.Next(min, max);
            var selection = NewManagerIds.OrderBy(v => random.Next(0, NewManagerIds.Count)).Take(selectionCount).ToList();
            Console.WriteLine($"Transforming venue '{venue.Id}'; setting managers to {string.Join(", ", selection)}.");
            venue.Managers = selection;
        }
        Console.WriteLine($"Transformed venues; set new managers.");
        return Task.FromResult(venues);
    }
}

public record SelectionBounds(int Min, int Max);