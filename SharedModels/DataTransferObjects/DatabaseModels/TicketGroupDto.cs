using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class TicketGroupDto : CreateTicketGroupDto
{
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PurchaseDateTime { get; set; }
    
    public bool IsReturned { get; set; }
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
    
    [DataType(DataType.DateTime)]
    public DateTime PurchaseDateTime { get; set; }
    
    public bool IsReturned { get; set; } = false;
}

public class TicketGroupWithTicketsDto
{
    public int Id { get; set; }
    public IList<TicketDto> Tickets { get; set; } = null!;
}

public class CreateTicketGroupWithTicketsDto : CreateTicketGroupDto
{
    [Required]
    public IList<CreateInTicketGroupTicketDto> Tickets { get; set; } = null!;
}