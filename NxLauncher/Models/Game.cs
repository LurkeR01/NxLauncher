using System;
using System.Runtime.InteropServices.JavaScript;

namespace NxLauncher.Models;

public class Game
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int DeveloperId { get; set; }
    public decimal Rating { get; set; }
    public required string AgeRating { get; set; }
    
    public string AgeRatingDescription
    {
        get
        {
            if (string.IsNullOrEmpty(AgeRating)) return string.Empty;

            return AgeRating switch
            {
                "3+" => "Подходит для всех возрастных групп",
                "7+" => "Может содержать некоторые сцены или звуки, которые пугают детей. Умеренное насилие (нереалистичное) разрешено",
                "12+" => "Может содержать насилие с участием фантастических персонажей",
                "16+" => "Реалистичные сцены насилия, сексуальной активности, ненормативная лексика",
                "18+" => "Может содержать интенсивное насилие или откровенный сексуальный контент",
                _ => AgeRating
            };
        }
    }

    public required string ImageUrl { get; set; }
    public required string Description { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }
    public DateTimeOffset? PurchaseDate { get; set; }
}