using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class PaymentDto
{
    [Required]
    public double Amount { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    [StringLength(maximumLength: 255, ErrorMessage = "Order Id is too long")]
    public string OrderId { get; set; }
}
