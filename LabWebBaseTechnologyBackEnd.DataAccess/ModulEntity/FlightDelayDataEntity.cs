namespace LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

public class FlightDelayDataEntity
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public string Weather { get; set; } = string.Empty; // e.g., "Rain", "Clear"
    public double DelayProbability { get; set; } // e.g., 0.75 (75% chance)

    public FlightEntity? Flight { get; set; }
}