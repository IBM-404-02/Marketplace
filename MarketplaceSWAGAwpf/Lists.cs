using MarketplaceSWAGAwpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketplaceSWAGAwpf
{
    public static class Lists
    {
        //справочники и загрузка данных
        public static List<CategoryDto> Categories { get; private set; } = new();
        public static List<PickupPointDto> PickupPoints { get; private set; } = new();
        public static List<DeliveryMethodDto> DeliveryMethods { get; private set; } = new();
        public static List<StatusDto> Statuses { get; private set; } = new();

        public static async Task LoadAllAsync()
        {
            Categories = await Core.GetCategoriesAsync();
            PickupPoints = await Core.GetPickupPointsAsync();
            DeliveryMethods = await Core.GetDeliveryMethodsAsync();
            Statuses = await Core.GetStatusesAsync();
        }

        public static async Task UpdateAsync()
        {
            await LoadAllAsync();
        }
    }
}
