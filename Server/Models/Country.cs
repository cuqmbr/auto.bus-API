using System.ComponentModel.DataAnnotations;
using SharedModels.DataTransferObjects;

namespace Server.Models;

public class Country
{
    [Key]
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public virtual IList<State> States { get; set; } = null!;
}