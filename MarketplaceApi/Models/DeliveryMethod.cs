using System;
using System.Collections.Generic;

namespace MarketplaceApi.Models;

public partial class DeliveryMethod
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
