package com.example.techstore.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.bumptech.glide.Glide;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.models.Product;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ProductAdapter extends RecyclerView.Adapter<ProductAdapter.ViewHolder> {

    private final List<Product> products;
    private final OnAddToCartListener listener;
    private Map<Integer, String> categoryMap;

    public interface OnAddToCartListener {
        void onAddToCart(Product product);
    }

    public ProductAdapter(List<Product> products, OnAddToCartListener listener) {
        this.products = products;
        this.listener = listener;
        initCategoryMap();
    }

    private void initCategoryMap() {
        categoryMap = new HashMap<>();
        categoryMap.put(1, "Смартфоны");
        categoryMap.put(2, "Ноутбуки");
        categoryMap.put(3, "Планшеты");
        categoryMap.put(4, "Умные часы");
        categoryMap.put(5, "Наушники");
        categoryMap.put(6, "Аксессуары");
    }

    private String getCategoryName(int categoryId) {
        String name = categoryMap.get(categoryId);
        return name != null ? name : "Другое";
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_product, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        Product product = products.get(position);
        holder.tvName.setText(product.getName());
        holder.tvCategory.setText(getCategoryName(product.getCategoryId()));
        holder.tvPrice.setText(String.format("%.2f ₽", product.getPrice()));
        holder.tvStock.setText("Остаток: " + product.getStock() + " шт.");

        // Отображение гарантии
        if (product.getWarranty() > 0) {
            holder.tvWarranty.setText("Гарантия: " + product.getWarranty() + " мес.");
            holder.tvWarranty.setVisibility(View.VISIBLE);
        } else {
            holder.tvWarranty.setVisibility(View.GONE);
        }

        Glide.with(holder.itemView.getContext())
                .load(RetrofitClient.BASE_URL + "Resources/" + product.getPhoto())
                .placeholder(R.drawable.ic_store)
                .into(holder.ivPhoto);

        holder.btnAddToCart.setOnClickListener(v -> listener.onAddToCart(product));
    }

    @Override
    public int getItemCount() {
        return products.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView ivPhoto;
        TextView tvName, tvCategory, tvPrice, tvStock, tvWarranty;
        Button btnAddToCart;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            ivPhoto = itemView.findViewById(R.id.ivPhoto);
            tvName = itemView.findViewById(R.id.tvName);
            tvCategory = itemView.findViewById(R.id.tvCategory);
            tvPrice = itemView.findViewById(R.id.tvPrice);
            tvStock = itemView.findViewById(R.id.tvStock);
            tvWarranty = itemView.findViewById(R.id.tvWarranty);
            btnAddToCart = itemView.findViewById(R.id.btnAddToCart);
        }
    }
}