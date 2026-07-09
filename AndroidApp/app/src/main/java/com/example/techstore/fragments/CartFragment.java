package com.example.techstore.fragments;

import android.app.AlertDialog;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.adapters.CartAdapter;
import com.example.techstore.models.CartItem;
import com.example.techstore.models.Order;
import com.example.techstore.network.ApiService;
import com.example.techstore.utils.CartManager;
import com.example.techstore.utils.SessionManager;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CartFragment extends Fragment implements CartAdapter.OnCartActionListener {

    private RecyclerView rvCart;
    private TextView tvTotal, tvEmpty;
    private Button btnCheckout;
    private CartAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;
    private boolean isChecking = false;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_cart, container, false);

        rvCart = view.findViewById(R.id.rvCart);
        tvTotal = view.findViewById(R.id.tvTotal);
        tvEmpty = view.findViewById(R.id.tvEmpty);
        btnCheckout = view.findViewById(R.id.btnCheckout);

        rvCart.setLayoutManager(new LinearLayoutManager(getContext()));

        sessionManager = new SessionManager(requireContext());
        apiService = RetrofitClient.getApiService(getContext());

        loadCartFromServer();

        btnCheckout.setOnClickListener(v -> {
            if (checkStockBeforeCheckout()) {
                showCheckoutDialog();
            }
        });

        return view;
    }

    private boolean checkStockBeforeCheckout() {
        List<CartItem> cartItems = CartManager.getInstance().getCartItems();
        if (cartItems.isEmpty()) {
            Toast.makeText(getContext(), "Корзина пуста", Toast.LENGTH_SHORT).show();
            return false;
        }

        for (CartItem item : cartItems) {
            int currentStock = item.getStock();
            if (currentStock == 0) {
                Toast.makeText(getContext(), "Товар '" + item.getProductName() + "' закончился на складе", Toast.LENGTH_LONG).show();
                CartManager.getInstance().removeFromCart(item.getProductId());
                displayCart();
                return false;
            }
            if (item.getQuantity() > currentStock) {
                Toast.makeText(getContext(), "Количество '" + item.getProductName() + "' уменьшено до " + currentStock + " шт.", Toast.LENGTH_LONG).show();
                CartManager.getInstance().updateQuantity(item.getProductId(), currentStock);
                displayCart();
                return false;
            }
        }
        return true;
    }

    private void showCheckoutDialog() {
        List<CartItem> cartItems = CartManager.getInstance().getCartItems();
        if (cartItems.isEmpty()) {
            Toast.makeText(getContext(), "Корзина пуста", Toast.LENGTH_SHORT).show();
            return;
        }

        AlertDialog.Builder builder = new AlertDialog.Builder(getContext());
        View dialogView = LayoutInflater.from(getContext()).inflate(R.layout.dialog_checkout, null);
        builder.setView(dialogView);

        TextView tvAddress = dialogView.findViewById(R.id.tvAddress);
        EditText etEmail = dialogView.findViewById(R.id.etEmail);
        EditText etCardNumber = dialogView.findViewById(R.id.etCardNumber);
        EditText etCardExpiry = dialogView.findViewById(R.id.etCardExpiry);
        EditText etCardCvv = dialogView.findViewById(R.id.etCardCvv);

        tvAddress.setText("г. Волгоград, ул. проспект Металлургов, 14");

        String userEmail = sessionManager.getEmail();
        if (userEmail != null && !userEmail.isEmpty()) {
            etEmail.setText(userEmail);
        }

        builder.setTitle("Оформление заказа");
        builder.setPositiveButton("Оплатить", (dialog, which) -> {
            String address = tvAddress.getText().toString().trim();
            String email = etEmail.getText().toString().trim();
            String cardNumber = etCardNumber.getText().toString().trim();
            String cardExpiry = etCardExpiry.getText().toString().trim();
            String cardCvv = etCardCvv.getText().toString().trim();

            if (email.isEmpty() || !android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
                Toast.makeText(getContext(), "Введите корректный email", Toast.LENGTH_SHORT).show();
                return;
            }
            if (cardNumber.isEmpty() || cardNumber.replace(" ", "").length() < 16) {
                Toast.makeText(getContext(), "Введите номер карты (16 цифр)", Toast.LENGTH_SHORT).show();
                return;
            }
            if (cardExpiry.isEmpty()) {
                Toast.makeText(getContext(), "Введите срок действия карты", Toast.LENGTH_SHORT).show();
                return;
            }
            if (cardCvv.isEmpty() || cardCvv.length() < 3) {
                Toast.makeText(getContext(), "Введите CVV код (3 цифры)", Toast.LENGTH_SHORT).show();
                return;
            }

            createOrderOnServer(address, email);
        });
        builder.setNegativeButton("Отмена", null);

        AlertDialog dialog = builder.create();
        dialog.show();
        Button positiveButton = dialog.getButton(AlertDialog.BUTTON_POSITIVE);
        if (positiveButton != null) {
            positiveButton.setBackgroundColor(android.graphics.Color.BLACK);
            positiveButton.setTextColor(android.graphics.Color.WHITE);
            positiveButton.setTextSize(16);
            positiveButton.setAllCaps(true);
            positiveButton.setPadding(40, 16, 40, 16);
        }

        Button negativeButton = dialog.getButton(AlertDialog.BUTTON_NEGATIVE);
        if (negativeButton != null) {
            negativeButton.setTextColor(android.graphics.Color.GRAY);
            negativeButton.setBackground(null);
            negativeButton.setTextSize(14);
        }
    }

    private void createOrderOnServer(String address, String email) {
        List<CartItem> cartItems = CartManager.getInstance().getCartItems();
        if (cartItems.isEmpty()) {
            Toast.makeText(getContext(), "Корзина пуста", Toast.LENGTH_SHORT).show();
            return;
        }

        for (CartItem item : cartItems) {
            if (item.getStock() == 0) {
                Toast.makeText(getContext(), "Товар '" + item.getProductName() + "' закончился на складе", Toast.LENGTH_SHORT).show();
                CartManager.getInstance().removeFromCart(item.getProductId());
                displayCart();
                return;
            }
            if (item.getQuantity() > item.getStock()) {
                Toast.makeText(getContext(), "Количество '" + item.getProductName() + "' превышает остаток", Toast.LENGTH_SHORT).show();
                return;
            }
        }

        ApiService.CreateOrderRequest request = new ApiService.CreateOrderRequest();
        request.userId = sessionManager.getUserId();
        request.email = email;
        request.deliveryAddress = address;
        request.paymentMethod = "Card";

        for (CartItem item : cartItems) {
            ApiService.OrderDetailRequest detail = new ApiService.OrderDetailRequest();
            detail.productId = item.getProductId();
            detail.quantity = item.getQuantity();
            detail.price = item.getPrice();
            detail.productName = item.getProductName();
            detail.warranty = item.getWarranty();
            request.items.add(detail);
        }

        apiService.createOrder(request).enqueue(new Callback<Order>() {
            @Override
            public void onResponse(Call<Order> call, Response<Order> response) {
                if (!isAdded()) return;

                if (response.isSuccessful() && response.body() != null) {
                    Order order = response.body();
                    double total = CartManager.getInstance().getTotal();

                    String successMessage = "✅ Оплата прошла успешно!\n\n" +
                            "Заказ №" + order.getId() + "\n" +
                            "Сумма: " + String.format("%.2f ₽", total) + "\n" +
                            "Адрес доставки: " + address + "\n" +
                            "Чек отправлен на: " + email + "\n\n" +
                            "Спасибо за покупку!";

                    new AlertDialog.Builder(getContext())
                            .setTitle("Заказ оформлен")
                            .setMessage(successMessage)
                            .setPositiveButton("OK", (d, w) -> {
                                clearCartCompletely();
                            })
                            .show();
                } else {
                    String errorMsg = "Ошибка при оформлении заказа";
                    try {
                        if (response.errorBody() != null) {
                            errorMsg = response.errorBody().string();
                        }
                    } catch (Exception ex) {
                        ex.printStackTrace();
                    }
                    Toast.makeText(getContext(), errorMsg, Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<Order> call, Throwable t) {
                if (!isAdded()) return;
                Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    private void clearCartCompletely() {
        List<CartItem> items = CartManager.getInstance().getCartItems();
        for (CartItem item : items) {
            int userId = sessionManager.getUserId();
            apiService.removeFromCart(userId, item.getProductId()).enqueue(new Callback<Void>() {
                @Override
                public void onResponse(Call<Void> call, Response<Void> response) {}
                @Override
                public void onFailure(Call<Void> call, Throwable t) {}
            });
        }
        CartManager.getInstance().clearCart();
        loadCartFromServer();
        Toast.makeText(getContext(), "Корзина очищена", Toast.LENGTH_SHORT).show();
    }

    private void loadCartFromServer() {
        int userId = sessionManager.getUserId();
        apiService.getCart(userId).enqueue(new Callback<List<CartItem>>() {
            @Override
            public void onResponse(Call<List<CartItem>> call, Response<List<CartItem>> response) {
                if (isAdded() && response.isSuccessful() && response.body() != null) {
                    List<CartItem> items = response.body();
                    boolean needUpdate = false;
                    for (CartItem item : items) {
                        int currentStock = item.getStock();
                        if (currentStock == 0) {
                            CartManager.getInstance().removeFromCart(item.getProductId());
                            needUpdate = true;
                        } else if (item.getQuantity() > currentStock) {
                            CartManager.getInstance().updateQuantity(item.getProductId(), currentStock);
                            needUpdate = true;
                        }
                    }

                    if (needUpdate) {
                        displayCart();
                        return;
                    }

                    CartManager.getInstance().setCartItems(items);
                    displayCart();
                } else if (isAdded()) {
                    displayCart();
                }
            }

            @Override
            public void onFailure(Call<List<CartItem>> call, Throwable t) {
                if (isAdded() && getContext() != null) {
                    Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
                }
                displayCart();
            }
        });
    }

    private void displayCart() {
        if (!isAdded()) return;
        List<CartItem> cartItems = CartManager.getInstance().getCartItems();
        if (cartItems.isEmpty()) {
            rvCart.setVisibility(View.GONE);
            tvEmpty.setVisibility(View.VISIBLE);
            tvTotal.setVisibility(View.GONE);
            btnCheckout.setVisibility(View.GONE);
        } else {
            rvCart.setVisibility(View.VISIBLE);
            tvEmpty.setVisibility(View.GONE);
            tvTotal.setVisibility(View.VISIBLE);
            btnCheckout.setVisibility(View.VISIBLE);
            adapter = new CartAdapter(cartItems, this);
            rvCart.setAdapter(adapter);
            updateTotal();
        }
    }

    private void updateTotal() {
        if (!isAdded()) return;
        double total = CartManager.getInstance().getTotal();
        tvTotal.setText("Итого: " + String.format("%.2f ₽", total));
    }

    private void syncCartToServer() {
        List<CartItem> items = CartManager.getInstance().getCartItems();
        for (CartItem item : items) {
            ApiService.UpdateCartRequest request = new ApiService.UpdateCartRequest();
            request.userId = sessionManager.getUserId();
            request.productId = item.getProductId();
            request.quantity = item.getQuantity();
            apiService.updateCart(request).enqueue(new Callback<Void>() {
                @Override
                public void onResponse(Call<Void> call, Response<Void> response) {}
                @Override
                public void onFailure(Call<Void> call, Throwable t) {}
            });
        }
    }

    private void deleteFromServer(int productId) {
        int userId = sessionManager.getUserId();
        apiService.removeFromCart(userId, productId).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {}
            @Override
            public void onFailure(Call<Void> call, Throwable t) {}
        });
    }

    @Override
    public void onIncreaseQuantity(int productId) {
        List<CartItem> items = CartManager.getInstance().getCartItems();
        for (CartItem item : items) {
            if (item.getProductId() == productId) {
                int currentStock = item.getStock();
                int newQuantity = item.getQuantity() + 1;
                if (newQuantity <= currentStock) {
                    CartManager.getInstance().updateQuantity(productId, newQuantity);
                    if (adapter != null) adapter.notifyDataSetChanged();
                    updateTotal();
                    syncCartToServer();
                } else {
                    Toast.makeText(getContext(),
                            "Нельзя увеличить количество. Доступно на складе: " + currentStock + " шт.",
                            Toast.LENGTH_SHORT).show();
                }
                break;
            }
        }
    }

    @Override
    public void onDecreaseQuantity(int productId) {
        List<CartItem> items = CartManager.getInstance().getCartItems();
        for (CartItem item : items) {
            if (item.getProductId() == productId && item.getQuantity() > 1) {
                CartManager.getInstance().updateQuantity(productId, item.getQuantity() - 1);
                break;
            }
        }
        if (adapter != null) adapter.notifyDataSetChanged();
        updateTotal();
        syncCartToServer();
    }

    @Override
    public void onRemoveItem(int productId) {
        deleteFromServer(productId);
        CartManager.getInstance().removeFromCart(productId);
        displayCart();
    }
}