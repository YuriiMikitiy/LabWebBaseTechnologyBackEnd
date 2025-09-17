namespace LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";

    public ICollection<BookingEntity> Bookings { get; set; } = new List<BookingEntity>();
}