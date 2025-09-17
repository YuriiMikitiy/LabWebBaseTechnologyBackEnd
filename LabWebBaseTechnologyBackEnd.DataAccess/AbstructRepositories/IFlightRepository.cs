using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

namespace LabWebBaseTechnologyBackEnd.DataAccess.Repositories
{
    public interface IFlightRepository
    {
        Task<List<FlightEntity>> GetAll();
    }
}