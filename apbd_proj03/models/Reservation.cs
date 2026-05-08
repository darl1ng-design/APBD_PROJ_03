namespace apbd_proj03.models;
using System.ComponentModel.DataAnnotations;


public class Reservation : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    [Required]
    [MinLength(1)]
    public string OrganizerName { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Topic { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [Required]
    [MinLength(1)]
    public string Status { get; set; } = "planned";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "EndTime must be later than StartTime.",
                new[] { nameof(StartTime), nameof(EndTime) }
            );
        }
    }
}