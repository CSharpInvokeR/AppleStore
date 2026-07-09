package com.example.techstore.activities;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.example.techstore.R;
import com.example.techstore.fragments.CatalogFragment;
import com.example.techstore.fragments.CartFragment;
import com.example.techstore.fragments.OrdersFragment;
import com.example.techstore.fragments.ProfileFragment;
import com.example.techstore.utils.SessionManager;

public class MainActivity extends AppCompatActivity {

    private BottomNavigationView bottomNavigation;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        sessionManager = new SessionManager(this);

        // Проверка роли пользователя
        String userRole = sessionManager.getRole();
        if (!"Клиент".equals(userRole) && !"Customer".equals(userRole)) {
            Toast.makeText(this, "Доступ только для клиентов", Toast.LENGTH_LONG).show();
            sessionManager.logout();
            startActivity(new Intent(this, LoginActivity.class));
            finish();
            return;
        }

        bottomNavigation = findViewById(R.id.bottomNavigation);
        bottomNavigation.setOnItemSelectedListener(item -> {
            Fragment selectedFragment = null;
            int itemId = item.getItemId();

            if (itemId == R.id.nav_catalog) {
                selectedFragment = new CatalogFragment();
            } else if (itemId == R.id.nav_cart) {
                selectedFragment = new CartFragment();
            } else if (itemId == R.id.nav_orders) {
                selectedFragment = new OrdersFragment();
            } else if (itemId == R.id.nav_profile) {
                selectedFragment = new ProfileFragment();
            }

            if (selectedFragment != null) {
                getSupportFragmentManager().beginTransaction()
                        .replace(R.id.fragmentContainer, selectedFragment)
                        .commit();
            }
            return true;
        });

        bottomNavigation.setSelectedItemId(R.id.nav_catalog);
    }
}