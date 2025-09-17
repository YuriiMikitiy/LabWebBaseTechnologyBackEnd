namespace LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

public class PaymentEntity
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // e.g., "Pending", "Completed"
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public BookingEntity? Booking { get; set; }
}