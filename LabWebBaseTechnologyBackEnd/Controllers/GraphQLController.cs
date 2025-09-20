using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace AirportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphQLController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GraphQLController> _logger;

        public GraphQLController(IHttpClientFactory httpClientFactory, ILogger<GraphQLController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<string>> QueryGraphQL([FromBody] GraphQLRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
                return BadRequest("GraphQL query is required");

            _logger.LogInformation("GraphQL query received: {Query} at {Time}", request.Query, DateTime.UtcNow);

            var url = "https://api.github.com/graphql";
            var requestBody = new { query = request.Query };
            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed GraphQL request: {Error} at {Time}", error, DateTime.UtcNow);
                    return StatusCode((int)response.StatusCode, error);
                }

                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GraphQL response sent successfully at {Time}", DateTime.UtcNow);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing GraphQL query: {Error} at {Time}", ex.Message, DateTime.UtcNow);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class GraphQLRequest
    {
        public string Query { get; set; } = string.Empty;
    }
}