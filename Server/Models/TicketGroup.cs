using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class TicketGroup
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    public virtual IList<Ticket> Tickets { get; set; }
    
    public double GetCost()
    {
        double cost = 0;

        foreach (var ticket in Tickets)
        {
            cost += ticket.GetCost();
        }

        return cost;
    }
}