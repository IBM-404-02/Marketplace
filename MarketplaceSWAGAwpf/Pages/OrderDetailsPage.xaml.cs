using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Models;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class OrderDetailsPage : Page
    {
        private int _orderId;

        public OrderDetailsPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadOrder();
        }

        private async void LoadOrder()
        {
            var order = await Core.GetOrderAsync(_orderId);
            if (order == null)
            {
                MessageBox.Show("Заказ не найден!");
                return;
            }

            TBTitle.Text = $"Заказ №{order.Id}";
            TBDate.Text = $"Дата оформления: {order.OrderDate:dd.MM.yyyy HH:mm}";
            TBStatus.Text = $"Статус: {order.Status}";

            if (order.DeliveryMethod == "Самовывоз")
            {
                TBDelivery.Text = $"Способ получения: Самовывоз из ПВЗ ({order.PickupPoint})";
            }
            else
            {
                TBDelivery.Text = $"Способ получения: Доставка по адресу ({order.DeliveryAddress})";
            }

            var itemsToShow = new List<OrderItemView>();
            decimal total = 0;

            foreach (var oi in order.Items)
            {
                decimal sum = oi.Quantity * oi.PriceAtPurchase;
                total += sum;

                itemsToShow.Add(new OrderItemView
                {
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    Sum = sum
                });
            }

            LBItems.ItemsSource = itemsToShow;
            TBTotal.Text = $"Итого: {total:N2} ₽";
        }
    }

    public class OrderItemView
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal Sum { get; set; }
    }
}