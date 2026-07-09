package com.example.techstore.activities;

import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.models.User;
import com.example.techstore.network.ApiService;
import com.example.techstore.utils.SessionManager;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    private EditText etUsername, etPassword;
    private Button btnLogin, btnRegister;
    private ProgressBar progressBar;
    private TextView tvError;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService(this);

        if (sessionManager.isLoggedIn()) {
            startActivity(new Intent(LoginActivity.this, MainActivity.class));
            finish();
        }

        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);
        btnLogin = findViewById(R.id.btnLogin);
        btnRegister = findViewById(R.id.btnRegister);
        progressBar = findViewById(R.id.progressBar);
        tvError = findViewById(R.id.tvError);

        btnLogin.setOnClickListener(v -> login());
        btnRegister.setOnClickListener(v -> startActivity(new Intent(LoginActivity.this, RegisterActivity.class)));
    }

    private void login() {
        String username = etUsername.getText().toString().trim();
        String password = etPassword.getText().toString().trim();

        if (TextUtils.isEmpty(username)) {
            tvError.setText("Введите логин");
            tvError.setVisibility(View.VISIBLE);
            etUsername.requestFocus();
            return;
        }

        if (TextUtils.isEmpty(password)) {
            tvError.setText("Введите пароль");
            tvError.setVisibility(View.VISIBLE);
            etPassword.requestFocus();
            return;
        }

        progressBar.setVisibility(View.VISIBLE);
        btnLogin.setEnabled(false);
        tvError.setVisibility(View.GONE);

        ApiService.LoginRequest request = new ApiService.LoginRequest(username, password);

        apiService.login(request).enqueue(new Callback<User>() {
            @Override
            public void onResponse(Call<User> call, Response<User> response) {
                progressBar.setVisibility(View.GONE);
                btnLogin.setEnabled(true);

                if (response.isSuccessful() && response.body() != null && response.body().getId() > 0) {
                    User user = response.body();
                    String role = user.getRole();
                    if (!"Клиент".equals(role) && !"Customer".equals(role)) {
                        tvError.setText("Доступ только для клиентов");
                        tvError.setVisibility(View.VISIBLE);
                        return;
                    }
                    sessionManager.saveUser(
                            user.getId(),
                            user.getUsername(),
                            user.getFirstName(),
                            user.getPatronymic(),
                            user.getLastName(),
                            user.getEmail(),
                            user.getPhone()
                    );
                    startActivity(new Intent(LoginActivity.this, MainActivity.class));
                    finish();
                } else {
                    String errorMessage = "Неверный логин или пароль";
                    try {
                        if (response.errorBody() != null) {
                            String errorBody = response.errorBody().string();
                            if (errorBody.contains("Invalid credentials")) {
                                errorMessage = "Неверный логин или пароль";
                            } else if (errorBody.contains("error")) {
                                errorMessage = "Ошибка авторизации";
                            }
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    tvError.setText(errorMessage);
                    tvError.setVisibility(View.VISIBLE);
                    etPassword.setText("");
                    etPassword.requestFocus();
                }
            }

            @Override
            public void onFailure(Call<User> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnLogin.setEnabled(true);
                tvError.setText("Ошибка подключения к серверу: " + t.getMessage());
                tvError.setVisibility(View.VISIBLE);
            }
        });
    }
}