package com.example.techstore.fragments;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.adapters.ProductAdapter;
import com.example.techstore.models.CartItem;
import com.example.techstore.models.Product;
import com.example.techstore.network.ApiService;
import com.example.techstore.utils.CartManager;
import com.example.techstore.utils.SessionManager;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CatalogFragment extends Fragment {

    private RecyclerView rvProducts;
    private ProgressBar progressBar;
    private EditText etSearch;
    private Spinner spCategory, spSort;
    private ProductAdapter adapter;
    private List<Product> productList = new ArrayList<>();
    private List<Product> filteredList = new ArrayList<>();
    private List<String> categories = new ArrayList<>();
    private Map<Integer, String> categoryMap = new HashMap<>();
    private ApiService apiService;
    private SessionManager sessionManager;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_catalog, container, false);

        rvProducts = view.findViewById(R.id.rvProducts);
        progressBar = view.findViewById(R.id.progressBar);
        etSearch = view.findViewById(R.id.etSearch);
        spCategory = view.findViewById(R.id.spCategory);
        spSort = view.findViewById(R.id.spSort);

        rvProducts.setLayoutManager(new GridLayoutManager(getContext(), 2));

        apiService = RetrofitClient.getApiService(getContext());
        sessionManager = new SessionManager(requireContext());

        setupSpinners();
        setupSearchListener();
        loadProducts();

        return view;
    }

    private void setupSpinners() {
        categories.add("Все категории");

        ArrayAdapter<String> categoryAdapter = new ArrayAdapter<>(getContext(), android.R.layout.simple_spinner_item, categories);
        categoryAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spCategory.setAdapter(categoryAdapter);

        spCategory.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                applyFilterAndSort();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {}
        });

        String[] sortOptions = {"По умолчанию", "Цена ↑", "Цена ↓", "Название А-Я", "Название Я-А", "По наличию"};
        ArrayAdapter<String> sortAdapter = new ArrayAdapter<>(getContext(), android.R.layout.simple_spinner_item, sortOptions);
        sortAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spSort.setAdapter(sortAdapter);

        spSort.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                applyFilterAndSort();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void setupSearchListener() {
        etSearch.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                applyFilterAndSort();
            }

            @Override
            public void afterTextChanged(Editable s) {}
        });
    }

    private void loadProducts() {
        if (!isAdded()) return;
        progressBar.setVisibility(View.VISIBLE);

        apiService.getProducts().enqueue(new Callback<List<Product>>() {
            @Override
            public void onResponse(Call<List<Product>> call, Response<List<Product>> response) {
                if (!isAdded()) return;
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    productList = response.body();
                    filteredList = new ArrayList<>(productList);
                    initCategoryMap();
                    loadCategories();
                    applyFilterAndSort();
                } else {
                    if (getContext() != null) {
                        Toast.makeText(getContext(), "Ошибка загрузки товаров", Toast.LENGTH_SHORT).show();
                    }
                }
            }

            @Override
            public void onFailure(Call<List<Product>> call, Throwable t) {
                if (!isAdded()) return;
                progressBar.setVisibility(View.GONE);
                if (getContext() != null) {
                    Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    private void initCategoryMap() {
        categoryMap.clear();
        categoryMap.put(1, "iPhone");
        categoryMap.put(2, "MacBook");
        categoryMap.put(3, "iPad");
        categoryMap.put(4, "Apple Watch");
        categoryMap.put(5, "Аксессуары");
    }

    private String getCategoryName(int categoryId) {
        String name = categoryMap.get(categoryId);
        return name != null ? name : "Другое";
    }

    private void loadCategories() {
        categories.clear();
        categories.add("Все категории");

        for (Product product : productList) {
            String categoryName = getCategoryName(product.getCategoryId());
            if (!categories.contains(categoryName)) {
                categories.add(categoryName);
            }
        }

        ArrayAdapter<String> adapter = (ArrayAdapter<String>) spCategory.getAdapter();
        if (adapter != null) {
            adapter.notifyDataSetChanged();
        }
    }

    private void applyFilterAndSort() {
        String searchText = etSearch.getText().toString().toLowerCase().trim();
        String selectedCategory = spCategory.getSelectedItem().toString();
        int sortPosition = spSort.getSelectedItemPosition();

        filteredList.clear();

        for (Product product : productList) {
            String categoryName = getCategoryName(product.getCategoryId());

            boolean matchesSearch = searchText.isEmpty() ||
                    product.getName().toLowerCase().contains(searchText) ||
                    categoryName.toLowerCase().contains(searchText);

            boolean matchesCategory = selectedCategory.equals("Все категории") ||
                    categoryName.equals(selectedCategory);

            if (matchesSearch && matchesCategory) {
                filteredList.add(product);
            }
        }

        applySorting(sortPosition);

        adapter = new ProductAdapter(filteredList, product -> {
            addToCart(product);
        });
        rvProducts.setAdapter(adapter);
    }

    private void applySorting(int sortPosition) {
        switch (sortPosition) {
            case 1:
                Collections.sort(filteredList, new Comparator<Product>() {
                    @Override
                    public int compare(Product p1, Product p2) {
                        return Double.compare(p1.getPrice(), p2.getPrice());
                    }
                });
                break;
            case 2:
                Collections.sort(filteredList, new Comparator<Product>() {
                    @Override
                    public int compare(Product p1, Product p2) {
                        return Double.compare(p2.getPrice(), p1.getPrice());
                    }
                });
                break;
            case 3:
                Collections.sort(filteredList, new Comparator<Product>() {
                    @Override
                    public int compare(Product p1, Product p2) {
                        return p1.getName().compareToIgnoreCase(p2.getName());
                    }
                });
                break;
            case 4:
                Collections.sort(filteredList, new Comparator<Product>() {
                    @Override
                    public int compare(Product p1, Product p2) {
                        return p2.getName().compareToIgnoreCase(p1.getName());
                    }
                });
                break;
            case 5:
                Collections.sort(filteredList, new Comparator<Product>() {
                    @Override
                    public int compare(Product p1, Product p2) {
                        return Integer.compare(p2.getStock(), p1.getStock());
                    }
                });
                break;
            default:
                break;
        }
    }

    private void addToCart(Product product) {
        if (getContext() == null || !isAdded()) return;

        int currentStock = product.getStock();

        List<CartItem> currentCart = CartManager.getInstance().getCartItems();
        int currentQuantityInCart = 0;
        for (CartItem item : currentCart) {
            if (item.getProductId() == product.getId()) {
                currentQuantityInCart = item.getQuantity();
                break;
            }
        }

        int newTotalQuantity = currentQuantityInCart + 1;

        if (newTotalQuantity > currentStock) {
            Toast.makeText(getContext(),
                    "Нельзя добавить больше " + product.getName() + "\n" +
                            "Доступно на складе: " + currentStock + " шт.\n" +
                            "В корзине уже: " + currentQuantityInCart + " шт.",
                    Toast.LENGTH_LONG).show();
            return;
        }

        ApiService.AddToCartRequest request = new ApiService.AddToCartRequest();
        request.userId = sessionManager.getUserId();
        request.productId = product.getId();
        request.quantity = 1;

        apiService.addToCart(request).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (getContext() == null || !isAdded()) return;

                if (response.isSuccessful()) {
                    CartItem cartItem = new CartItem(
                            product.getId(),
                            product.getName(),
                            product.getPrice(),
                            1,
                            product.getStock(),
                            product.getPhoto(),
                            product.getWarranty()
                    );
                    CartManager.getInstance().addToCart(cartItem);
                    Toast.makeText(getContext(), product.getName() + " добавлен в корзину", Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(getContext(), "Ошибка добавления в корзину", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                if (getContext() != null && isAdded()) {
                    Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
                }
            }
        });
    }
}