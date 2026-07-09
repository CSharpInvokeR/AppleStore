using System;

namespace AppleStore.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int UserID { get; set; }
        public int ProdID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Price * Quantity;
    }
}