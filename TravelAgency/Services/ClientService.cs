using Microsoft.Data.SqlClient;
using TravelAgency.Models.DTO;

namespace TravelAgency.Services;

public interface IClientService
{
    Task<int> CreateClientAsync(CreateClientDTO clientDto);
    Task<RegisterClientForTripResponseDTO> RegisterClientForTripAsync(int clientId, int tripId);
    Task UnregisterClientFromTripAsync(int clientId, int tripId);
}

public class ClientService : IClientService
    {
        private readonly string _connectionString;

        public ClientService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TravelAgencyDb")
                ?? throw new InvalidOperationException("Connection string 'TravelAgencyDb' not found.");
        }

        public async Task<int> CreateClientAsync(CreateClientDTO clientDto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkSql = "SELECT COUNT(*) FROM Client WHERE Pesel = @Pesel";
                using (var command = new SqlCommand(checkSql, connection))
                {
                    command.Parameters.AddWithValue("@Pesel", clientDto.Pesel);
                    int count = (int)await command.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        throw new InvalidOperationException("Client with this PESEL already exists");
                    }
                }

                string insertSql = @"
                    INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                    VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
                    command.Parameters.AddWithValue("@LastName", clientDto.LastName);
                    command.Parameters.AddWithValue("@Email", clientDto.Email);
                    command.Parameters.AddWithValue("@Telephone", clientDto.Telephone);
                    command.Parameters.AddWithValue("@Pesel", clientDto.Pesel);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<RegisterClientForTripResponseDTO> RegisterClientForTripAsync(int clientId, int tripId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string checkClientSql = "SELECT 1 FROM Client WHERE IdClient = @ClientId";
                        using (var command = new SqlCommand(checkClientSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@ClientId", clientId);
                            var clientResult = await command.ExecuteScalarAsync();
                            if (clientResult == null)
                            {
                                throw new KeyNotFoundException($"Client with ID {clientId} not found");
                            }
                        }

                        string checkTripSql = "SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId";
                        int maxPeople;
                        using (var command = new SqlCommand(checkTripSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TripId", tripId);
                            var tripResult = await command.ExecuteScalarAsync();
                            if (tripResult == null)
                            {
                                throw new KeyNotFoundException($"Trip with ID {tripId} not found");
                            }
                            maxPeople = Convert.ToInt32(tripResult);
                        }

                        string checkRegistrationSql = "SELECT 1 FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId";
                        using (var command = new SqlCommand(checkRegistrationSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@ClientId", clientId);
                            command.Parameters.AddWithValue("@TripId", tripId);
                            var existingRegistration = await command.ExecuteScalarAsync();
                            if (existingRegistration != null)
                            {
                                throw new InvalidOperationException("Client is already registered for this trip");
                            }
                        }

                        string currentParticipantsSql = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId";
                        using (var command = new SqlCommand(currentParticipantsSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TripId", tripId);
                            int currentParticipants = (int)await command.ExecuteScalarAsync();
                            if (currentParticipants >= maxPeople)
                            {
                                throw new InvalidOperationException("This trip has reached its maximum number of participants");
                            }
                        }

                        var registeredAt = DateTime.Now;
                        string registerSql = @"
                            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                            VALUES (@ClientId, @TripId, @RegisteredAt)";

                        using (var command = new SqlCommand(registerSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@ClientId", clientId);
                            command.Parameters.AddWithValue("@TripId", tripId);
                            // Use SqlDbType to ensure correct type conversion
                            var paramRegDate = command.Parameters.AddWithValue("@RegisteredAt", registeredAt);
                            paramRegDate.SqlDbType = System.Data.SqlDbType.DateTime;
                            await command.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();

                        return new RegisterClientForTripResponseDTO
                        {
                            ClientId = clientId,
                            TripId = tripId,
                            RegisteredAt = registeredAt
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task UnregisterClientFromTripAsync(int clientId, int tripId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkSql = "SELECT 1 FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId";
                using (var command = new SqlCommand(checkSql, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    command.Parameters.AddWithValue("@TripId", tripId);
                    var result = await command.ExecuteScalarAsync();
                    if (result == null)
                    {
                        throw new KeyNotFoundException("Registration not found");
                    }
                }

                string deleteSql = "DELETE FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId";
                using (var command = new SqlCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    command.Parameters.AddWithValue("@TripId", tripId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    } 