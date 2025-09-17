namespace LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

public class BookingEntity
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public FlightEntity? Flight { get; set; }
    public UserEntity? User { get; set; }
    public PaymentEntity? Payment { get; set; }
}