namespace TravelAgency.Models;

public class CountryTrip
{
    public int CountryId { get; set; }
    public Country Country { get; set; }
    public int TripId { get; set; }
    public Trip Trip { get; set; }
}