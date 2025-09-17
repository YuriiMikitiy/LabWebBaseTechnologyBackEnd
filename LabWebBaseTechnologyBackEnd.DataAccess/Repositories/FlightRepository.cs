using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWebBaseTechnologyBackEnd.DataAccess.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly LabWebBaseTechnologyDBContext _context;

        public FlightRepository(LabWebBaseTechnologyDBContext context)
        {
            _context = context;
        }

        public async Task<List<FlightEntity>> GetAll()
        {
            var flight = await _context.Flights
                .AsNoTracking()
                .ToListAsync();

            return flight;
        }


    }
}
