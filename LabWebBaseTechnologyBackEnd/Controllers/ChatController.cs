using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using LabWebBaseTechnologyBackEnd.DataAccess;

namespace AirportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly LabWebBaseTechnologyDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;

        public ChatController(LabWebBaseTechnologyDBContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ChatController> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostChat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message))
            {
                _logger.LogWarning("Chat request failed: empty message.");
                return BadRequest("Message is required");
            }

            _logger.LogInformation("Received chat request: {Message}", request.Message);

            // Отримання актуальних даних із БД (обмеження до 10 рейсів)
            var flights = await _context.Flights
                .Include(f => f.DelayData)
                .Take(10)
                .ToListAsync();

            _logger.LogInformation("Loaded {Count} flights for chat processing", flights.Count);

            var flightData = flights.Select(f => new
            {
                Id = f.Id,
                FlightNumber = f.FlightNumber,
                From = f.From,
                To = f.To,
                Time = f.Time,
                Status = f.Status,
                DelayProbability = f.DelayData.FirstOrDefault()?.DelayProbability ?? 0 // Коректний доступ
            }).ToList();

            var jsonData = JsonSerializer.Serialize(flightData);
            var prompt = $"На основі цих даних про рейси: {jsonData}. Відповідай природно українською мовою на запит користувача: {request.Message}. Будь корисним асистентом аеропорту. Якщо запит стосується конкретного рейсу, вкажи деталі з даних.";

            // Виклик Gemini API
            var apiKey = _configuration["GeminiApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";

            var content = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new { temperature = 0.7, maxOutputTokens = 200 }
            };

            var json = JsonSerializer.Serialize(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("Sending request to Gemini API...");
                var response = await _httpClient.PostAsync(url, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {Error}", error);
                    return StatusCode((int)response.StatusCode, $"Error calling Gemini API: {error}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
                var aiText = geminiResponse.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString()
                    ?? "Вибачте, не вдалося обробити запит.";

                // Додатковий аналіз для конкретних рейсів
                var flightNumberMatch = System.Text.RegularExpressions.Regex.Match(request.Message, @"[A-Za-z]{2}\d{3}");
                if (flightNumberMatch.Success)
                {
                    var flightNumber = flightNumberMatch.Value;
                    var flight = await _context.Flights
                        .Include(f => f.DelayData)
                        .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
                    if (flight != null)
                    {
                        aiText += $"\nДодаткова інформація: Рейс {flightNumber} — З {flight.From} до {flight.To}, Статус: {flight.Status}, Ймовірність затримки: {flight.DelayData.FirstOrDefault()?.DelayProbability ?? 0}";
                    }
                }

                _logger.LogInformation("Gemini response received successfully.");

                return Ok(new { response = aiText });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing Gemini response.");
                return StatusCode(500, $"Error parsing Gemini response: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error in ChatController.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}



//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Text;
//using System.Text.Json;
//using LabWebBaseTechnologyBackEnd.DataAccess;

//namespace AirportApi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ChatController : ControllerBase
//    {
//        private readonly LabWebBaseTechnologyDBContext _context;
//        private readonly HttpClient _httpClient;
//        private readonly IConfiguration _configuration;

//        public ChatController(LabWebBaseTechnologyDBContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
//        {
//            _context = context;
//            _configuration = configuration;
//            _httpClient = httpClientFactory.CreateClient();
//        }

//        [HttpPost]
//        public async Task<ActionResult<object>> PostChat([FromBody] ChatRequest request)
//        {
//            // Get relevant data from DB
//            var flights = await _context.Flights
//                .Include(f => f.DelayData)
//                .ToListAsync();

//            var flightData = flights.Select(f => new
//            {
//                Id = f.Id,
//                From = f.From,
//                To = f.To,
//                Time = f.Time,
//                Status = f.Status,
//                DelayProbability = f.DelayData.FirstOrDefault()?.DelayProbability ?? 0
//            }).ToList();

//            var jsonData = JsonSerializer.Serialize(flightData);
//            var prompt = $"На основі цих даних про рейси: {jsonData}. Відповідай природно українською мовою на запит користувача: {request.Message}. Будь корисним асистентом аеропорту.";

//            // Call Gemini API
//            var apiKey = _configuration["GeminiApiKey"]; // Add to appsettings.json: "GeminiApiKey": "your_api_key_here"
//            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";

//            var content = new
//            {
//                contents = new[]
//                {
//                    new
//                    {
//                        parts = new[]
//                        {
//                            new { text = prompt }
//                        }
//                    }
//                }
//            };

//            var json = JsonSerializer.Serialize(content);
//            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PostAsync(url, httpContent);
//            if (!response.IsSuccessStatusCode)
//            {
//                return BadRequest("Error calling Gemini API");
//            }

//            var responseJson = await response.Content.ReadAsStringAsync();
//            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
//            var aiText = geminiResponse.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

//            return Ok(new { response = aiText });
//        }
//    }

//    public class ChatRequest
//    {
//        public string Message { get; set; } = string.Empty;
//    }
//}