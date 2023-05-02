using System.ComponentModel.DataAnnotations;
using Utils;

namespace SharedModels.DataTransferObjects;

public class UserDto
{
    public string Id { get; set; } = null!;
    
    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public string Patronymic { get; set; } = null!;
    
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
    
    public Identity.Gender? Gender { get; set; }
    
    public Identity.Document? Document { get; set; }
    
    public string? DocumentDetails { get; set; }
    
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; } = false;
    
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; } = false;

    public virtual IList<string> Roles { get; set; } = null!;
    
    public virtual IList<TicketGroupDto>? TicketGroups { get; set; } = null!;
    public virtual IList<ReviewDto>? Reviews { get; set; } = null!;
}

public class CreateUserDto
{
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;
    
    [Required]
    public string Patronymic { get; set; } = null!;
    
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
    
    public Identity.Gender? Gender { get; set; }
    
    public Identity.Document? Document { get; set; }
    
    public string? DocumentDetails { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; } = false;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
    public bool? PhoneNumberConfirmed { get; set; } = false;

    public virtual IList<string> Roles { get; set; } = null!;
}

public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = null!;
    
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;
    
    [Required]
    public string Patronymic { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
    
    [Required]
    public Identity.Gender? Gender { get; set; }
    
    [Required]
    public Identity.Document? Document { get; set; }
    
    [Required]
    public string? DocumentDetails { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; } = false;
    
    [DataType(DataType.Password)]
    public string? Password { get; set; } = null!;
    
    [Required]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
    [Required]
    public bool PhoneNumberConfirmed { get; set; } = false;

    public virtual IList<string> Roles { get; set; } = null!;
}

public class StrippedUserDto
{
    public string Id { get; set; } = null!;
    
    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public string Patronymic { get; set; } = null!;
    
    public Identity.Gender? Gender { get; set; }
}