using Microsoft.AspNetCore.Mvc;
using TravelAgency.Models.DTO;
using TravelAgency.Services;

namespace TravelAgency.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    /// <summary>
    /// Gets all available trips with their basic information and countries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TripDTO>>> GetTrips()
    {
        var trips = await _tripService.GetAllTripsAsync();
        return Ok(trips);
    }
}

