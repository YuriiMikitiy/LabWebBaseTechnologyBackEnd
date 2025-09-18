using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AirportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ILogger<AnalyticsController> _logger;
        private static readonly List<(DateTime timestamp, int count)> _logCache = new();

        public AnalyticsController(ILogger<AnalyticsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            lock (_logCache)
            {
                if (!_logCache.Any())
                {
                    var rnd = new Random();
                    for (int i = 0; i < 7; i++)
                    {
                        _logCache.Add((DateTime.UtcNow.Date.AddDays(-i), rnd.Next(5, 50)));
                    }
                }

                var result = _logCache
                    .OrderBy(l => l.timestamp)
                    .Select(l => new { timestamp = l.timestamp, count = l.count })
                    .ToList();

                return Ok(result);
            }
        }
    }
}
