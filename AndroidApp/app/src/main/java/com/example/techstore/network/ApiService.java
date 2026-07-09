package com.example.techstore.network;

import com.example.techstore.models.CartItem;
import com.example.techstore.models.Order;
import com.example.techstore.models.Product;
import com.example.techstore.models.User;
import com.google.gson.annotations.SerializedName;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface ApiService {

    @POST("api/auth/login")
    Call<User> login(@Body LoginRequest request);

    @POST("api/auth/register")
    Call<Void> register(@Body RegisterRequest request);

    @GET("api/products")
    Call<List<Product>> getProducts();

    @GET("api/cart")
    Call<List<CartItem>> getCart(@Query("userId") int userId);

    @POST("api/cart")
    Call<Void> addToCart(@Body AddToCartRequest request);

    @PUT("api/cart")
    Call<Void> updateCart(@Body UpdateCartRequest request);

    @DELETE("api/cart")
    Call<Void> removeFromCart(@Query("userId") int userId, @Query("productId") int productId);

    @POST("api/orders")
    Call<Order> createOrder(@Body CreateOrderRequest request);

    @GET("api/orders")
    Call<List<Order>> getOrders(@Query("userId") int userId);

    @PUT("api/users/profile")
    Call<Void> updateProfile(@Body UpdateProfileRequest request);

    class LoginRequest {
        public String username;
        public String password;
        public LoginRequest(String username, String password) {
            this.username = username;
            this.password = password;
        }
    }

    class RegisterRequest {
        public String username;
        public String password;
        public String email;
        public String firstName;
        public String patronymic;
        public String lastName;
        public String phoneNumber;
    }

    class AddToCartRequest {
        public int userId;
        public int productId;
        public int quantity;
    }

    class UpdateCartRequest {
        public int userId;
        public int productId;
        public int quantity;
    }

    class CreateOrderRequest {
        public int userId;
        public String email;
        public List<OrderDetailRequest> items;
        public String deliveryAddress;
        public String paymentMethod;

        public CreateOrderRequest() {
            items = new java.util.ArrayList<>();
        }
    }

    class OrderDetailRequest {
        @SerializedName("ProdID")
        public int productId;

        @SerializedName("Quantity")
        public int quantity;

        @SerializedName("UnitPrice")
        public double price;

        public String productName;
        public int warranty;
    }

    class UpdateProfileRequest {
        public int userId;
        public String firstName;
        public String patronymic;
        public String lastName;
        public String phoneNumber;
        public String email;
    }
}