namespace AppleStore.Models
{
    public class Product
    {
        public int ProdID { get; set; }
        public int CatID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Foto { get; set; }
        public string CategoryName { get; set; }
    }
}