using System.ComponentModel.DataAnnotations;

namespace TravelAgency.Models.DTO;

public class CreateClientDTO
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100)]
    public string FirstName { get; set; }
        
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100)]
    public string LastName { get; set; }
        
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100)]
    public string Email { get; set; }
        
    [Required(ErrorMessage = "Telephone is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string Telephone { get; set; }
        
    [Required(ErrorMessage = "PESEL is required")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be exactly 11 digits")]
    public string Pesel { get; set; }
}