using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using LabWebBaseTechnologyBackEnd.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LabWebBaseTechnologyBackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;

        public FlightController(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<FlightEntity>>> GetBook()
        {
            var book = await _flightRepository.GetAll();

            return Ok(book);
        }

    }
}
