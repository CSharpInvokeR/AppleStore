package com.example.techstore.models;

import java.util.List;

public class Order {
    private int id;
    private double total;
    private String status;
    private String date;
    private List<OrderItem> items;

    public Order() {}

    public Order(int id, double total, String status, String date, List<OrderItem> items) {
        this.id = id;
        this.total = total;
        this.status = status;
        this.date = date;
        this.items = items;
    }

    public int getId() { return id; }
    public double getTotal() { return total; }
    public String getStatus() { return status; }
    public String getDate() { return date; }
    public List<OrderItem> getItems() { return items; }

    public void setId(int id) { this.id = id; }
    public void setTotal(double total) { this.total = total; }
    public void setStatus(String status) { this.status = status; }
    public void setDate(String date) { this.date = date; }
    public void setItems(List<OrderItem> items) { this.items = items; }
}