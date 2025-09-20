using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabWebBaseTechnologyBackEnd.DataAccess;
using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace AirportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly LabWebBaseTechnologyDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherController> _logger;
        private readonly IConfiguration _configuration;

        public WeatherController(LabWebBaseTechnologyDBContext context, IHttpClientFactory httpClientFactory, ILogger<WeatherController> logger, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("current/{city}")]
        public async Task<ActionResult> GetCurrentWeather(string city)
        {
            _logger.LogInformation("Fetching weather for city: {City} at {Time}", city, DateTime.UtcNow);
            var apiKey = _configuration["OpenWeatherApiKey"];
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=uk";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var weatherData = JsonSerializer.Deserialize<JsonElement>(response);
                var temperature = weatherData.GetProperty("main").GetProperty("temp").GetDouble();
                var description = weatherData.GetProperty("weather")[0].GetProperty("description").GetString();
                var humidity = weatherData.GetProperty("main").GetProperty("humidity").GetInt32();

                // Оновлення статусу рейсів на основі погоди
                var flights = await _context.Flights
                    .Where(f => f.From.ToLower() == city.ToLower())
                    .ToListAsync();
                foreach (var flight in flights)
                {
                    if (temperature < 0 || description?.Contains("rain") == true || description?.Contains("snow") == true)
                    {
                        flight.Status = "Delayed";
                        _logger.LogInformation("Flight {FlightId} delayed due to weather (Temp: {Temp}, Desc: {Desc}) at {Time}", flight.Id, temperature, description, DateTime.UtcNow);
                    }
                    else
                    {
                        flight.Status = "On Time";
                        _logger.LogInformation("Flight {FlightId} status updated to On Time at {Time}", flight.Id, DateTime.UtcNow);
                    }
                }
                await _context.SaveChangesAsync();

                return Ok(new { city, temperature, description, humidity, updatedFlights = flights.Select(f => new { f.Id, f.Status }) });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching weather for {City}: {Error} at {Time}", city, ex.Message, DateTime.UtcNow);
                return StatusCode(500, $"Error fetching weather: {ex.Message}");
            }
        }
    }
}