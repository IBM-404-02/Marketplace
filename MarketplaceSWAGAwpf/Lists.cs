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
        public static List<Products> Products = Core.Context.Products.ToList();
        public static List<Orders> Orders = Core.Context.Orders.ToList();
        public static List<OrderItems> OrderItems = Core.Context.OrderItems.ToList();
        public static List<Favorites> Favorites = Core.Context.Favorites.ToList();

        public static void UpdateLists()
        {
            Categories = Core.Context.Categories.ToList();
            PickupPoints = Core.Context.PickupPoints.ToList();
            DeliveryMethods = Core.Context.DeliveryMethods.ToList();
            OrderStatuses = Core.Context.OrderStatuses.ToList();
        }
    }
}
