package com.example.techstore.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.appcompat.app.AlertDialog;
import androidx.recyclerview.widget.RecyclerView;
import com.bumptech.glide.Glide;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.models.CartItem;
import java.util.List;

public class CartAdapter extends RecyclerView.Adapter<CartAdapter.ViewHolder> {

    private List<CartItem> cartItems;
    private OnCartActionListener listener;

    public interface OnCartActionListener {
        void onIncreaseQuantity(int productId);
        void onDecreaseQuantity(int productId);
        void onRemoveItem(int productId);
    }

    public CartAdapter(List<CartItem> cartItems, OnCartActionListener listener) {
        this.cartItems = cartItems;
        this.listener = listener;
    }

    @androidx.annotation.NonNull
    @Override
    public ViewHolder onCreateViewHolder(@androidx.annotation.NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_cart, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@androidx.annotation.NonNull ViewHolder holder, int position) {
        CartItem item = cartItems.get(position);
        holder.tvName.setText(item.getProductName());
        holder.tvPrice.setText(String.format("%.2f ₽", item.getPrice()));
        holder.tvQuantity.setText(String.valueOf(item.getQuantity()));
        holder.tvQuantity.setTextSize(20);
        holder.tvTotal.setText(String.format("%.2f ₽", item.getTotal()));

        Glide.with(holder.itemView.getContext())
                .load(RetrofitClient.BASE_URL + "Resources/" + item.getPhoto())
                .placeholder(R.drawable.ic_store)
                .into(holder.ivPhoto);

        holder.btnDecrease.setBackgroundColor(android.graphics.Color.TRANSPARENT);
        holder.btnDecrease.setTextColor(android.graphics.Color.BLACK);
        holder.btnDecrease.setTextSize(24);

        holder.btnIncrease.setBackgroundColor(android.graphics.Color.TRANSPARENT);
        holder.btnIncrease.setTextColor(android.graphics.Color.BLACK);
        holder.btnIncrease.setTextSize(24);

        holder.btnDecrease.setOnClickListener(v -> {
            if (listener != null) {
                listener.onDecreaseQuantity(item.getProductId());
            }
        });
        holder.btnIncrease.setOnClickListener(v -> {
            if (listener != null) {
                listener.onIncreaseQuantity(item.getProductId());
            }
        });

        holder.btnRemove.setBackgroundColor(android.graphics.Color.TRANSPARENT);
        holder.btnRemove.setTextColor(android.graphics.Color.BLACK);
        holder.btnRemove.setAllCaps(false);
        holder.btnRemove.setTextSize(15);

        holder.btnRemove.setOnClickListener(v -> {
            new AlertDialog.Builder(v.getContext())
                    .setTitle("Удаление товара")
                    .setMessage("Вы уверены, что хотите удалить \"" + item.getProductName() + "\" из корзины?")
                    .setPositiveButton("Да", (dialog, which) -> {
                        if (listener != null) {
                            listener.onRemoveItem(item.getProductId());
                        }
                    })
                    .setNegativeButton("Нет", null)
                    .show();
        });
    }

    @Override
    public int getItemCount() {
        return cartItems.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView ivPhoto;
        TextView tvName, tvPrice, tvQuantity, tvTotal;
        TextView btnDecrease, btnIncrease;
        Button btnRemove;

        ViewHolder(View itemView) {
            super(itemView);
            ivPhoto = itemView.findViewById(R.id.ivPhoto);
            tvName = itemView.findViewById(R.id.tvName);
            tvPrice = itemView.findViewById(R.id.tvPrice);
            tvQuantity = itemView.findViewById(R.id.tvQuantity);
            tvTotal = itemView.findViewById(R.id.tvTotal);
            btnDecrease = itemView.findViewById(R.id.btnDecrease);
            btnIncrease = itemView.findViewById(R.id.btnIncrease);
            btnRemove = itemView.findViewById(R.id.btnRemove);
        }
    }
}