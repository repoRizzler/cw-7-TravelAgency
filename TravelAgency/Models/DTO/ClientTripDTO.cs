namespace TravelAgency.Models.DTO;

public class ClientTripDTO
{
    public int TripId { get; set; }
    public string TripName { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
    public List<string> Countries { get; set; } = new List<string>();
}