using System;

namespace AppleStore.Models
{
    public class Order
    {
        public int OrdID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
    }
}