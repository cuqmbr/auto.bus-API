using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class TicketGroupDto : CreateTicketGroupDto
{
    public int Id { get; set; }
}

public class CreateTicketGroupDto
{
    [Required]
    public string UserId { get; set; } = null!;
}

public class UpdateTicketGroupDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = null!;
}

public class TicketGroupWithTicketsDto
{
    public int Id { get; set; }
    public IList<InTicketGroupTicketDto> Tickets { get; set; } = null!;
}

public class CreateTicketGroupWithTicketsDto : CreateTicketGroupDto
{
    [Required]
    public IList<CreateInTicketGroupTicketDto> Tickets { get; set; } = null!;
}