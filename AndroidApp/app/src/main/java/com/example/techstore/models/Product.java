package com.example.techstore.models;

public class Product {
    private int id;
    private String name;
    private int categoryId;
    private double price;
    private int stock;
    private String photo;
    private int warranty;

    public Product(int id, String name, int categoryId, double price, int stock, String photo, int warranty) {
        this.id = id;
        this.name = name;
        this.categoryId = categoryId;
        this.price = price;
        this.stock = stock;
        this.photo = photo;
        this.warranty = warranty;
    }

    public int getId() { return id; }
    public String getName() { return name; }
    public int getCategoryId() { return categoryId; }
    public double getPrice() { return price; }
    public int getStock() { return stock; }
    public String getPhoto() { return photo; }
    public int getWarranty() { return warranty; }
}