using LabWebBaseTechnologyBackEnd.DataAccess;
using LabWebBaseTechnologyBackEnd.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LabWebBaseTechnologyDBContext>(
    options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));     //For MSSQL
    });


// Enable CORS for React frontend (adjust origin if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite default port for React
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
});
});

// Add HttpClient for Gemini API
builder.Services.AddHttpClient();

builder.Services.AddScoped<IFlightRepository, FlightRepository>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine("Connection: " + builder.Configuration.GetConnectionString("DefaultConnection"));


app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
