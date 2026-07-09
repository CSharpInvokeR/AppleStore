namespace AppleStore.Models
{
    public class OrderDetail
    {
        public int OrdDetID { get; set; }
        public int OrdID { get; set; }
        public int ProdID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; }
    }
}