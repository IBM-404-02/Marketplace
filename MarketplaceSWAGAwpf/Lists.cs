using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketplaceSWAGAwpf
{
    internal class Lists
    {
        public static List<Categories> Categories = Core.Context.Categories.ToList();
        public static List<PickupPoints> PickupPoints = Core.Context.PickupPoints.ToList();
        public static List<DeliveryMethods> DeliveryMethods = Core.Context.DeliveryMethods.ToList();
        public static List<OrderStatuses> OrderStatuses = Core.Context.OrderStatuses.ToList();

        public static void UpdateLists()
        {
            Categories = Core.Context.Categories.ToList();
            PickupPoints = Core.Context.PickupPoints.ToList();
            DeliveryMethods = Core.Context.DeliveryMethods.ToList();
            OrderStatuses = Core.Context.OrderStatuses.ToList();
        }
    }
}
