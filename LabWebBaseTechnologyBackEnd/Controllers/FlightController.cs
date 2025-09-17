using LabWebBaseTechnologyBackEnd.DataAccess;
using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using LabWebBaseTechnologyBackEnd.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabWebBaseTechnologyBackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;
        private readonly LabWebBaseTechnologyDBContext _context;

        public FlightController(IFlightRepository flightRepository, LabWebBaseTechnologyDBContext dBContext)
        {
            _flightRepository = flightRepository;
            _context = dBContext;
        }

        //[HttpGet]
        //public async Task<ActionResult<List<FlightEntity>>> GetBook()
        //{
        //    var book = await _flightRepository.GetAll();

        //    return Ok(book);
        //}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightEntity>>> GetFlights()
        {
            return await _context.Flights.ToListAsync();
        }

        [HttpGet("data")]
        public async Task<ActionResult<object>> GetTrainingData()
        {
            var flights = await _context.Flights
                .Include(f => f.DelayData)
                .ToListAsync();

            var data = flights.Select(f => new
            {
                Weather = f.DelayData.FirstOrDefault()?.Weather ?? "Clear",
                DelayProbability = f.DelayData.FirstOrDefault()?.DelayProbability ?? 0.0,
                Status = f.Status
            }).ToList();

            return Ok(data);
        }

    }
}
