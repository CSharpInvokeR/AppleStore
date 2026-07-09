package com.example.techstore.adapters;

import android.graphics.Color;
import android.graphics.drawable.GradientDrawable;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.techstore.R;
import com.example.techstore.models.Order;
import java.util.List;

public class OrdersAdapter extends RecyclerView.Adapter<OrdersAdapter.ViewHolder> {

    private List<Order> orders;
    private OnOrderClickListener listener;

    public interface OnOrderClickListener {
        void onOrderClick(Order order);
    }

    public OrdersAdapter(List<Order> orders, OnOrderClickListener listener) {
        this.orders = orders;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_order, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        Order order = orders.get(position);
        holder.tvId.setText("Заказ №" + order.getId());
        holder.tvTotal.setText(String.format("%.2f ₽", order.getTotal()));
        holder.tvStatus.setText(order.getStatus());
        holder.tvDate.setText(order.getDate());

        setStatusColor(holder.tvStatus, order.getStatus());

        holder.itemView.setOnClickListener(v -> listener.onOrderClick(order));
    }

    private void setStatusColor(TextView tvStatus, String status) {
        GradientDrawable drawable = new GradientDrawable();
        drawable.setCornerRadius(20);

        switch (status) {
            case "Оплачен":
                drawable.setColor(Color.parseColor("#D4EDDA"));
                tvStatus.setTextColor(Color.parseColor("#155724"));
                break;
            case "Отправлен":
                drawable.setColor(Color.parseColor("#CCE5FF"));
                tvStatus.setTextColor(Color.parseColor("#004085"));
                break;
            case "Доставлен":
                drawable.setColor(Color.parseColor("#D1ECF1"));
                tvStatus.setTextColor(Color.parseColor("#0C5460"));
                break;
            case "Отменен":
                drawable.setColor(Color.parseColor("#F8D7DA"));
                tvStatus.setTextColor(Color.parseColor("#721C24"));
                break;
            default:
                drawable.setColor(Color.parseColor("#F8F9FA"));
                tvStatus.setTextColor(Color.parseColor("#000000"));
                break;
        }

        tvStatus.setBackground(drawable);
        tvStatus.setPadding(20, 8, 20, 8);
    }

    @Override
    public int getItemCount() {
        return orders.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvId, tvTotal, tvStatus, tvDate;

        ViewHolder(View itemView) {
            super(itemView);
            tvId = itemView.findViewById(R.id.tvId);
            tvTotal = itemView.findViewById(R.id.tvTotal);
            tvStatus = itemView.findViewById(R.id.tvStatus);
            tvDate = itemView.findViewById(R.id.tvDate);
        }
    }
}