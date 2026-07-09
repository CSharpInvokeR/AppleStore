package com.example.techstore.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.adapters.OrdersAdapter;
import com.example.techstore.models.Order;
import com.example.techstore.models.OrderItem;
import com.example.techstore.network.ApiService;
import com.example.techstore.utils.SessionManager;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class OrdersFragment extends Fragment implements OrdersAdapter.OnOrderClickListener {

    private RecyclerView rvOrders;
    private ProgressBar progressBar;
    private TextView tvEmpty;
    private OrdersAdapter adapter;
    private List<Order> orderList = new ArrayList<>();
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_orders, container, false);

        rvOrders = view.findViewById(R.id.rvOrders);
        progressBar = view.findViewById(R.id.progressBar);
        tvEmpty = view.findViewById(R.id.tvEmpty);

        rvOrders.setLayoutManager(new LinearLayoutManager(getContext()));

        sessionManager = new SessionManager(getContext());
        apiService = RetrofitClient.getApiService(getContext());

        return view;
    }

    @Override
    public void onResume() {
        super.onResume();
        loadOrders();
    }

    private void loadOrders() {
        if (!isAdded()) return;

        int userId = sessionManager.getUserId();

        progressBar.setVisibility(View.VISIBLE);
        tvEmpty.setVisibility(View.GONE);

        apiService.getOrders(userId).enqueue(new Callback<List<Order>>() {
            @Override
            public void onResponse(Call<List<Order>> call, Response<List<Order>> response) {
                if (!isAdded()) return;
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null && !response.body().isEmpty()) {
                    orderList = response.body();
                    adapter = new OrdersAdapter(orderList, OrdersFragment.this);
                    rvOrders.setAdapter(adapter);
                    tvEmpty.setVisibility(View.GONE);
                } else {
                    orderList.clear();
                    tvEmpty.setVisibility(View.VISIBLE);
                }
            }

            @Override
            public void onFailure(Call<List<Order>> call, Throwable t) {
                if (!isAdded()) return;
                progressBar.setVisibility(View.GONE);
                tvEmpty.setVisibility(View.VISIBLE);
                if (getContext() != null) {
                    Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    @Override
    public void onOrderClick(Order order) {
        if (!isAdded() || getContext() == null) return;

        StringBuilder details = new StringBuilder();
        details.append("Заказ №").append(order.getId()).append("\n");
        details.append("Дата: ").append(order.getDate()).append("\n");
        details.append("Статус: ").append(order.getStatus()).append("\n");
        details.append("Сумма: ").append(String.format("%.2f ₽", order.getTotal())).append("\n\n");
        details.append("Товары:\n");

        if (order.getItems() != null && !order.getItems().isEmpty()) {
            for (OrderItem item : order.getItems()) {
                details.append("• ").append(item.getProductName())
                        .append(" x").append(item.getQuantity())
                        .append(" = ").append(String.format("%.2f ₽", item.getTotal())).append("\n");
            }
        } else {
            details.append("Нет информации о товарах\n");
        }

        AlertDialog dialog = new AlertDialog.Builder(getContext())
                .setTitle("Детали заказа")
                .setMessage(details.toString())
                .setPositiveButton("OK", null)
                .create();

        dialog.show();

        Button positiveButton = dialog.getButton(AlertDialog.BUTTON_POSITIVE);
        if (positiveButton != null) {
            positiveButton.setBackgroundColor(android.graphics.Color.BLACK);
            positiveButton.setTextColor(android.graphics.Color.WHITE);
            positiveButton.setTextSize(14);
            positiveButton.setAllCaps(true);
            positiveButton.setPadding(40, 12, 40, 12);
        }
    }
}