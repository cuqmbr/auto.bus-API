using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class UserDto : UpdateUserDto
{
    public virtual CompanyDto Company { get; set; } = null!;
    
    public virtual IList<TicketDto> Tickets { get; set; } = null!;
    public virtual IList<ReviewDto> Ratings { get; set; } = null!;
}

public class CreateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; } = null!;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; } = false;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
    
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; } = false;

    public virtual IList<string> RoleIds { get; set; } = new List<string> { "User" };
}

public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = null!;
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string UserName { get; set; } = null!;
    
    [EmailAddress]
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
    
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    
    public virtual IList<string> RoleIds { get; set; } = new List<string> { "User" };
}