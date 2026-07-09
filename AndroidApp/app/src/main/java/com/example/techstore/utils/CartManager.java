package com.example.techstore.utils;

import android.content.Context;
import com.example.techstore.models.CartItem;
import java.util.ArrayList;
import java.util.List;

public class CartManager {
    private static CartManager instance;
    private List<CartItem> cartItems;
    private Context context;

    private CartManager() {
        cartItems = new ArrayList<>();
    }

    public static CartManager getInstance() {
        if (instance == null) {
            instance = new CartManager();
        }
        return instance;
    }

    public void initContext(Context context) {
        this.context = context;
    }

    public List<CartItem> getCartItems() {
        return cartItems;
    }

    public void setCartItems(List<CartItem> items) {
        this.cartItems = items;
    }

    public void addToCart(CartItem item) {
        for (CartItem existing : cartItems) {
            if (existing.getProductId() == item.getProductId()) {
                int newQuantity = existing.getQuantity() + item.getQuantity();
                if (newQuantity <= existing.getStock()) {
                    existing.setQuantity(newQuantity);
                } else {
                    existing.setQuantity(existing.getStock());
                }
                return;
            }
        }
        cartItems.add(item);
    }

    public void updateQuantity(int productId, int quantity) {
        for (CartItem item : cartItems) {
            if (item.getProductId() == productId) {
                if (quantity <= item.getStock()) {
                    item.setQuantity(quantity);
                } else {
                    item.setQuantity(item.getStock());
                }
                break;
            }
        }
    }

    public void removeFromCart(int productId) {
        cartItems.removeIf(item -> item.getProductId() == productId);
    }

    public void clearCart() {
        cartItems.clear();
    }

    public double getTotal() {
        double total = 0;
        for (CartItem item : cartItems) {
            total += item.getTotal();
        }
        return total;
    }

    public int getItemCount() {
        int count = 0;
        for (CartItem item : cartItems) {
            count += item.getQuantity();
        }
        return count;
    }
}