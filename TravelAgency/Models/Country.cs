namespace TravelAgency.Models;
public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<CountryTrip> CountryTrips { get; set; } = new List<CountryTrip>();
}