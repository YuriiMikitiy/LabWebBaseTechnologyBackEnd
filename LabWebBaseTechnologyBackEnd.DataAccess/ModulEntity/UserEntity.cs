using System.ComponentModel.DataAnnotations;

namespace LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;

public class UserEntity
{
    public Guid Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";

    public ICollection<BookingEntity> Bookings { get; set; } = new List<BookingEntity>();
}