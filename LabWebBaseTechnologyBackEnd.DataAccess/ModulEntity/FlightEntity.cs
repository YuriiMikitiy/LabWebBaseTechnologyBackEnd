namespace LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

public class FlightEntity
{
    public Guid Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public ICollection<BookingEntity> Bookings { get; set; } = new List<BookingEntity>();
    public ICollection<FlightDelayDataEntity> DelayData { get; set; } = new List<FlightDelayDataEntity>();
}

