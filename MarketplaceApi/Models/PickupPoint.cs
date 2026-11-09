using System;
using System.Collections.Generic;

namespace MarketplaceApi.Models;

public partial class PickupPoint
{
    public int Id { get; set; }

    public string Address { get; set; } = null!;

    public string? WorkingHours { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
