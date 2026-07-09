package com.example.techstore.models;

public class CartItem {
    private int productId;
    private String productName;
    private double price;
    private int quantity;
    private int stock;
    private String photo;
    private int warranty;

    public CartItem(int productId, String productName, double price, int quantity, int stock, String photo, int warranty) {
        this.productId = productId;
        this.productName = productName;
        this.price = price;
        this.quantity = quantity;
        this.stock = stock;
        this.photo = photo;
        this.warranty = warranty;
    }

    public int getProductId() { return productId; }
    public String getProductName() { return productName; }
    public double getPrice() { return price; }
    public int getQuantity() { return quantity; }
    public void setQuantity(int quantity) { this.quantity = quantity; }
    public int getStock() { return stock; }
    public String getPhoto() { return photo; }
    public int getWarranty() { return warranty; }
    public double getTotal() { return price * quantity; }
}