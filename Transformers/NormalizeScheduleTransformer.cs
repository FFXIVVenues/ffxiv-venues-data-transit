using FFXIVVenues.VenueModels;

namespace FFXIVVenues.DataTransit.Transformers;

public class NormalizeScheduleTransformer : ITransformer
{
    
    public Task<Venue[]> TransformAsync(Venue[] venues)
    {
        Console.WriteLine($"Transforming venues; normalizing schedule.");
        foreach (var venue in venues)
        {
            if (venue.Openings == null || !venue.Openings.Any())
            {
                Console.WriteLine($"Skipping venue '{venue.Id}'; no openings.");
                continue;
            }

            foreach (var opening in venue.Openings)
            {
                if (opening.Start.NextDay)
                {
                    Console.WriteLine($"Transforming venue '{venue.Id}'; shifting {opening.Day} opening to {opening.Day.Next()}.");
                    opening.Day = opening.Day.Next();
                    opening.Start.NextDay = false;
                    if (opening.End != null)
                        opening.End.NextDay = false;
                }

                var newTimeZone = opening.Start.TimeZone switch 
                {
                    "Eastern Standard Time" => "America/New_York",
                    "Central Standard Time" => "America/Chicago",
                    "Mountain Standard Time" => "America/Denver",
                    "Pacific Standard Time" => "America/Los_Angeles",
                    "GMT Standard Time" => "Europe/London",
                    "Central Europe Standard Time" => "Europe/Budapest",
                    "E. Europe Standard Time" => "Europe/Chisinau",
                    _ => opening.Start.TimeZone
                };

                if (opening.Start.TimeZone == newTimeZone)
                {
                    Console.WriteLine($"Venue '{venue.Id}'; shifting {opening.Day} opening to {opening.Day.Next()}.");
                    continue;
                }
                
                Console.WriteLine($"Transforming venue '{venue.Id}'; changing TimeZone '{opening.Start.TimeZone}' to '{newTimeZone}'.");
                opening.Start.TimeZone = newTimeZone;
                if (opening.End != null) 
                    opening.End.TimeZone = newTimeZone;
            }
        }
        
        Console.WriteLine($"Transformed venues; normalized schedules.");
        return Task.FromResult(venues);
    }
}

