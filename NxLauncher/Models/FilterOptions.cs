using System.Collections.Generic;

namespace NxLauncher.Models;

public class FilterOptions
{
    public PriceFilterItem? PriceFilterItem { get; set; }
    public required List<Genre>  Genres { get; set; } 
    public string? SearchQuery { get; set; }
    public List<AgeRating> AgeRatings { get; set; }
}