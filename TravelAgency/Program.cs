// Program.cs
using TravelAgency.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register services
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IClientService, ClientService>();

// Configure connection string
// Make sure to add this to appsettings.json:
// "ConnectionStrings": {
//   "TravelAgencyDb": "Server=your_server;Database=travel_agency;User Id=your_user;Password=your_password;"
// }

var app = builder.Build();

// Configure the HTTP request pipeline


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();