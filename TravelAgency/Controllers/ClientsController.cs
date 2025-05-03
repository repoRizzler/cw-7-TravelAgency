using Microsoft.AspNetCore.Mvc;
using TravelAgency.Models.DTO;
using TravelAgency.Services;

namespace TravelAgency.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ITripService _tripService;

        public ClientsController(IClientService clientService, ITripService tripService)
        {
            _clientService = clientService;
            _tripService = tripService;
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClientDTO>> CreateClient([FromBody] CreateClientDTO clientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var clientId = await _clientService.CreateClientAsync(clientDto);

                return CreatedAtAction(
                    nameof(GetClientTrips),
                    new { id = clientId },
                    new ClientDTO
                    {
                        Id = clientId,
                        FirstName = clientDto.FirstName,
                        LastName = clientDto.LastName,
                        Email = clientDto.Email,
                        Telephone = clientDto.Telephone,
                        Pesel = clientDto.Pesel
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all trips for a specific client
        /// </summary>
        [HttpGet("{id}/trips")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ClientTripDTO>>> GetClientTrips(int id)
        {
            var clientTrips = await _tripService.GetClientTripsAsync(id);

            if (!clientTrips.Any())
            {
                // We can't differentiate between "client not found" and "client has no trips" from service response
                // You might want to enhance the service to provide this distinction
                return NotFound($"Client with ID {id} not found or has no trips");
            }

            return Ok(clientTrips);
        }

        /// <summary>
        /// Registers a client for a specific trip
        /// </summary>
        [HttpPut("{id}/trips/{tripId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegisterClientForTripResponseDTO>> RegisterClientForTrip(int id, int tripId)
        {
            try
            {
                var result = await _clientService.RegisterClientForTripAsync(id, tripId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unregisters a client from a specific trip
        /// </summary>
        [HttpDelete("{id}/trips/{tripId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UnregisterClientFromTrip(int id, int tripId)
        {
            try
            {
                await _clientService.UnregisterClientFromTripAsync(id, tripId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}