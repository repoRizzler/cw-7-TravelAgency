using Microsoft.Data.SqlClient;
using TravelAgency.Models.DTO;


namespace TravelAgency.Services;

public interface ITripService
{
    Task<IEnumerable<TripDTO>> GetAllTripsAsync();
    Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId);
}
 public class TripService : ITripService
    {
        private readonly string _connectionString;

        public TripService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TravelAgencyDb") 
                ?? throw new InvalidOperationException("Connection string 'TravelAgencyDb' not found.");
        }

        public async Task<IEnumerable<TripDTO>> GetAllTripsAsync()
        {
            var trips = new List<TripDTO>();
            var tripCountriesDict = new Dictionary<int, List<string>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string tripSql = @"
                    SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
                    FROM Trip t
                    ORDER BY t.DateFrom";

                using (var command = new SqlCommand(tripSql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var trip = new TripDTO
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = Convert.ToDateTime(reader.GetValue(3)),
                            DateTo = Convert.ToDateTime(reader.GetValue(4)),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<string>()
                        };
                        trips.Add(trip);
                        tripCountriesDict[trip.Id] = trip.Countries;
                    }
                }
                
                if (trips.Any())
                {
                    string countrySql = @"
                        SELECT ct.IdTrip, c.Name
                        FROM Country_Trip ct
                        JOIN Country c ON ct.IdCountry = c.IdCountry
                        WHERE ct.IdTrip IN (SELECT IdTrip FROM Trip)
                        ORDER BY ct.IdTrip";

                    using (var command = new SqlCommand(countrySql, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int tripId = reader.GetInt32(0);
                            string countryName = reader.GetString(1);
                            if (tripCountriesDict.ContainsKey(tripId))
                            {
                                tripCountriesDict[tripId].Add(countryName);
                            }
                        }
                    }
                }
            }

            return trips;
        }

        public async Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId)
        {
            var clientTrips = new List<ClientTripDTO>();
            var tripCountriesDict = new Dictionary<int, List<string>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                string clientSql = "SELECT 1 FROM Client WHERE IdClient = @ClientId";
                using (var command = new SqlCommand(clientSql, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    var result = await command.ExecuteScalarAsync();
                    if (result == null)
                    {
                        return new List<ClientTripDTO>();
                    }
                }
                
                string tripSql = @"
                    SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, ct.RegisteredAt, ct.PaymentDate
                    FROM Client_Trip ct
                    JOIN Trip t ON ct.IdTrip = t.IdTrip
                    WHERE ct.IdClient = @ClientId
                    ORDER BY t.DateFrom";

                using (var command = new SqlCommand(tripSql, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var trip = new ClientTripDTO
                            {
                                TripId = reader.GetInt32(0),
                                TripName = reader.GetString(1),
                                Description = reader.GetString(2),
                                DateFrom = Convert.ToDateTime(reader.GetValue(3)),
                                DateTo = Convert.ToDateTime(reader.GetValue(4)),
                                RegisteredAt = reader.GetInt32(5),
                                PaymentDate = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                Countries = new List<string>()
                            };

                            clientTrips.Add(trip);
                            tripCountriesDict[trip.TripId] = trip.Countries;
                        }
                    }
                }
                
                if (clientTrips.Any())
                {
                    string countrySql = @"
                        SELECT ct.IdTrip, c.Name
                        FROM Country_Trip ct
                        JOIN Country c ON ct.IdCountry = c.IdCountry
                        WHERE ct.IdTrip IN (
                            SELECT IdTrip 
                            FROM Client_Trip 
                            WHERE IdClient = @ClientId
                        )
                        ORDER BY ct.IdTrip";

                    using (var command = new SqlCommand(countrySql, connection))
                    {
                        command.Parameters.AddWithValue("@ClientId", clientId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int tripId = reader.GetInt32(0);
                                string countryName = reader.GetString(1);
                                if (tripCountriesDict.ContainsKey(tripId))
                                {
                                    tripCountriesDict[tripId].Add(countryName);
                                }
                            }
                        }
                    }
                }
            }

            return clientTrips;
        }
    }
