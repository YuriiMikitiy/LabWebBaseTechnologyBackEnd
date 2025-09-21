using LabWebBaseTechnologyBackEnd.DataAccess;
using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace AirportApi.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly LabWebBaseTechnologyDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FlightController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public FlightController(LabWebBaseTechnologyDBContext context, IHttpClientFactory httpClientFactory, ILogger<FlightController> logger, IMemoryCache cache, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightEntity>>> GetFlights()
        {
            const string cacheKey = "FlightsList";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<FlightEntity> cachedFlights))
            {
                _logger.LogInformation("Returning cached flights at {Time}", DateTime.UtcNow);
                return Ok(cachedFlights);
            }

            _logger.LogInformation("Fetching flights from DB at {Time}", DateTime.UtcNow);
            var flights = await _context.Flights
                .Include(f => f.DelayData)
                .Include(f => f.Bookings)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            _cache.Set(cacheKey, flights, cacheEntryOptions);

            return Ok(flights);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightEntity>> GetFlight(Guid id)
        {
            _logger.LogInformation("Fetching flight with ID: {FlightId} at {Time}", id, DateTime.UtcNow);
            var flight = await _context.Flights
                .Include(f => f.DelayData)
                .Include(f => f.Bookings)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                _logger.LogWarning("Flight with ID {FlightId} not found at {Time}", id, DateTime.UtcNow);
                return NotFound();
            }

            return flight;
        }

        [HttpGet("data")]
        public async Task<ActionResult<object>> GetTrainingData()
        {
            _logger.LogInformation("Fetching training data at {Time}", DateTime.UtcNow);
            var flights = await _context.Flights
                .Include(f => f.DelayData)
                .ToListAsync();

            var data = flights.Select(f => new
            {
                Weather = f.DelayData.FirstOrDefault()?.Weather ?? "Clear",
                DelayProbability = f.DelayData.FirstOrDefault()?.DelayProbability ?? 0.0,
                Status = f.Status
            }).ToList();

            _logger.LogInformation("Fetched {Count} training records", data.Count);

            return Ok(data);
        }

        [HttpGet("weather/{city}")]
        public async Task<ActionResult> GetWeather(string city)
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

                var flights = await _context.Flights
                    .Where(f => f.From.ToLower() == city.ToLower())
                    .ToListAsync();
                foreach (var flight in flights)
                {
                    if (temperature < 0 || description?.Contains("rain") == true)
                    {
                        flight.Status = "Delayed";
                        _logger.LogInformation("Flight {FlightId} delayed due to weather at {Time}", flight.Id, DateTime.UtcNow);
                    }
                }
                await _context.SaveChangesAsync();

                return Ok(new { temperature, description });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching weather for {City}: {Error} at {Time}", city, ex.Message, DateTime.UtcNow);
                return StatusCode(500, $"Error fetching weather: {ex.Message}");
            }
        }
    }
}


//using LabWebBaseTechnologyBackEnd.DataAccess;
//using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
//using LabWebBaseTechnologyBackEnd.DataAccess.Repositories;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//namespace LabWebBaseTechnologyBackEnd.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class FlightController : ControllerBase
//    {
//        private readonly IFlightRepository _flightRepository;
//        private readonly LabWebBaseTechnologyDBContext _context;
//        private readonly ILogger<FlightController> _logger;
//        private readonly IConfiguration _configuration;
//        private readonly HttpClient _httpClient;

//        public FlightController(IFlightRepository flightRepository, LabWebBaseTechnologyDBContext dBContext, ILogger<FlightController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
//        {
//            _flightRepository = flightRepository;
//            _context = dBContext;
//            _logger = logger;
//            _httpClient = httpClientFactory.CreateClient();
//            _configuration = configuration;
//        }

//        //[HttpGet]
//        //public async Task<ActionResult<List<FlightEntity>>> GetBook()
//        //{
//        //    var book = await _flightRepository.GetAll();

//        //    return Ok(book);
//        //}
//        [HttpGet("weather/{city}")]
//        public async Task<ActionResult> GetWeather(string city)
//        {
//            _logger.LogInformation("Fetching weather for city: {City} at {Time}", city, DateTime.UtcNow);
//            var apiKey = _configuration["OpenWeatherApiKey"];
//            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=uk";
//            try
//            {
//                var response = await _httpClient.GetStringAsync(url);
//                var weatherData = JsonSerializer.Deserialize<JsonElement>(response);
//                var temperature = weatherData.GetProperty("main").GetProperty("temp").GetDouble();
//                var description = weatherData.GetProperty("weather")[0].GetProperty("description").GetString();

//                // Оновлення статусу рейсів на основі погоди (приклад)
//                var flights = await _context.Flights
//                    .Where(f => f.From.ToLower() == city.ToLower())
//                    .ToListAsync();
//                foreach (var flight in flights)
//                {
//                    if (temperature < 0 || description?.Contains("rain") == true)
//                    {
//                        flight.Status = "Delayed";
//                        _logger.LogInformation("Flight {FlightId} delayed due to weather at {Time}", flight.Id, DateTime.UtcNow);
//                    }
//                }
//                await _context.SaveChangesAsync();

//                return Ok(new { temperature, description });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("Error fetching weather for {City}: {Error} at {Time}", city, ex.Message, DateTime.UtcNow);
//                return StatusCode(500, $"Error fetching weather: {ex.Message}");
//            }
//        }


//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<FlightEntity>>> GetFlights()
//        {
//            _logger.LogInformation("Fetching all flights at {Time}", DateTime.UtcNow);
//            return await _context.Flights.ToListAsync();
//        }

//        [HttpGet("data")]
//        public async Task<ActionResult<object>> GetTrainingData()
//        {
//            _logger.LogInformation("Fetching training data at {Time}", DateTime.UtcNow);
//            var flights = await _context.Flights
//                .Include(f => f.DelayData)
//                .ToListAsync();

//            var data = flights.Select(f => new
//            {
//                Weather = f.DelayData.FirstOrDefault()?.Weather ?? "Clear",
//                DelayProbability = f.DelayData.FirstOrDefault()?.DelayProbability ?? 0.0,
//                Status = f.Status
//            }).ToList();

//            _logger.LogInformation("Fetched {Count} training records", data.Count);

//            return Ok(data);
//        }

//    }
//}




