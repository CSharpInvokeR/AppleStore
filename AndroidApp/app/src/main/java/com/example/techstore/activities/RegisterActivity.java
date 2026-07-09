package com.example.techstore.activities;

import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.models.User;
import com.example.techstore.network.ApiService;
import com.example.techstore.utils.SessionManager;
import java.util.regex.Pattern;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RegisterActivity extends AppCompatActivity {

    private EditText etUsername, etPassword, etConfirmPassword, etEmail, etFirstName, etPatronymic, etLastName, etPhone;
    private Button btnRegister, btnBack;
    private ProgressBar progressBar;
    private ApiService apiService;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        apiService = RetrofitClient.getApiService(this);
        sessionManager = new SessionManager(this);

        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);
        etConfirmPassword = findViewById(R.id.etConfirmPassword);
        etEmail = findViewById(R.id.etEmail);
        etFirstName = findViewById(R.id.etFirstName);
        etPatronymic = findViewById(R.id.etPatronymic);
        etLastName = findViewById(R.id.etLastName);
        etPhone = findViewById(R.id.etPhone);
        btnRegister = findViewById(R.id.btnRegister);
        btnBack = findViewById(R.id.btnBack);
        progressBar = findViewById(R.id.progressBar);

        btnRegister.setOnClickListener(v -> register());
        btnBack.setOnClickListener(v -> startActivity(new Intent(RegisterActivity.this, LoginActivity.class)));
    }

    private boolean isValidName(String name) {
        if (TextUtils.isEmpty(name)) return true;
        return Pattern.matches("^[a-zA-Zа-яА-ЯёЁ]+$", name);
    }

    private boolean isValidPhone(String phone) {
        if (TextUtils.isEmpty(phone)) return true;
        String cleaned = phone.replaceAll("[^0-9]", "");
        return cleaned.length() >= 10 && cleaned.length() <= 11;
    }

    private boolean isValidEmail(String email) {
        return !TextUtils.isEmpty(email) && Pattern.matches("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", email);
    }

    private void register() {
        String username = etUsername.getText().toString().trim();
        String password = etPassword.getText().toString().trim();
        String confirmPassword = etConfirmPassword.getText().toString().trim();
        String email = etEmail.getText().toString().trim();
        String firstName = etFirstName.getText().toString().trim();
        String patronymic = etPatronymic.getText().toString().trim();
        String lastName = etLastName.getText().toString().trim();
        String phone = etPhone.getText().toString().trim();

        // Логин
        if (TextUtils.isEmpty(username)) {
            etUsername.setError("Введите логин");
            etUsername.requestFocus();
            return;
        }
        if (username.length() < 3) {
            etUsername.setError("Логин должен содержать минимум 3 символа");
            etUsername.requestFocus();
            return;
        }

        // Пароль
        if (TextUtils.isEmpty(password)) {
            etPassword.setError("Введите пароль");
            etPassword.requestFocus();
            return;
        }
        if (password.length() < 4) {
            etPassword.setError("Пароль должен содержать минимум 4 символа");
            etPassword.requestFocus();
            return;
        }

        // Подтверждение пароля
        if (TextUtils.isEmpty(confirmPassword)) {
            etConfirmPassword.setError("Подтвердите пароль");
            etConfirmPassword.requestFocus();
            return;
        }
        if (!password.equals(confirmPassword)) {
            etConfirmPassword.setError("Пароли не совпадают");
            etConfirmPassword.requestFocus();
            return;
        }

        // Email
        if (TextUtils.isEmpty(email)) {
            etEmail.setError("Введите email");
            etEmail.requestFocus();
            return;
        }
        if (!isValidEmail(email)) {
            etEmail.setError("Введите корректный email");
            etEmail.requestFocus();
            return;
        }

        // Имя (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(firstName) && !isValidName(firstName)) {
            etFirstName.setError("Имя должно содержать только буквы");
            etFirstName.requestFocus();
            return;
        }

        // Отчество (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(patronymic) && !isValidName(patronymic)) {
            etPatronymic.setError("Отчество должно содержать только буквы");
            etPatronymic.requestFocus();
            return;
        }

        // Фамилия (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(lastName) && !isValidName(lastName)) {
            etLastName.setError("Фамилия должна содержать только буквы");
            etLastName.requestFocus();
            return;
        }

        // Телефон (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(phone) && !isValidPhone(phone)) {
            etPhone.setError("Введите корректный номер телефона");
            etPhone.requestFocus();
            return;
        }

        progressBar.setVisibility(View.VISIBLE);
        btnRegister.setEnabled(false);

        ApiService.RegisterRequest request = new ApiService.RegisterRequest();
        request.username = username;
        request.password = password;
        request.email = email;
        request.firstName = firstName;
        request.patronymic = patronymic;
        request.lastName = lastName;
        request.phoneNumber = phone;

        apiService.register(request).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                progressBar.setVisibility(View.GONE);
                btnRegister.setEnabled(true);

                if (response.isSuccessful()) {
                    showLoginDialog(username, password);
                } else {
                    String errorMsg = "Ошибка регистрации";
                    try {
                        if (response.errorBody() != null) {
                            errorMsg = response.errorBody().string();
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    Toast.makeText(RegisterActivity.this, errorMsg, Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnRegister.setEnabled(true);
                Toast.makeText(RegisterActivity.this, "Ошибка: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    private void showLoginDialog(String username, String password) {
        new AlertDialog.Builder(this)
                .setTitle("Регистрация успешна!")
                .setMessage("Вы успешно зарегистрировались!\n\nВойти в систему сейчас?")
                .setPositiveButton("Да", (dialog, which) -> {
                    autoLogin(username, password);
                })
                .setNegativeButton("Нет", (dialog, which) -> {
                    startActivity(new Intent(RegisterActivity.this, LoginActivity.class));
                    finish();
                })
                .setCancelable(false)
                .show();
    }

    private void autoLogin(String username, String password) {
        progressBar.setVisibility(View.VISIBLE);
        btnRegister.setEnabled(false);

        ApiService.LoginRequest loginRequest = new ApiService.LoginRequest(username, password);

        apiService.login(loginRequest).enqueue(new Callback<User>() {
            @Override
            public void onResponse(Call<User> call, Response<User> response) {
                progressBar.setVisibility(View.GONE);
                btnRegister.setEnabled(true);

                if (response.isSuccessful() && response.body() != null && response.body().getId() > 0) {
                    User user = response.body();
                    sessionManager.saveUser(
                            user.getId(),
                            user.getUsername(),
                            user.getFirstName(),
                            user.getPatronymic(),
                            user.getLastName(),
                            user.getEmail(),
                            user.getPhone()
                    );
                    startActivity(new Intent(RegisterActivity.this, MainActivity.class));
                    finish();
                } else {
                    Toast.makeText(RegisterActivity.this, "Ошибка автоматического входа. Войдите вручную.", Toast.LENGTH_LONG).show();
                    startActivity(new Intent(RegisterActivity.this, LoginActivity.class));
                    finish();
                }
            }

            @Override
            public void onFailure(Call<User> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnRegister.setEnabled(true);
                Toast.makeText(RegisterActivity.this, "Ошибка подключения: " + t.getMessage(), Toast.LENGTH_LONG).show();
                startActivity(new Intent(RegisterActivity.this, LoginActivity.class));
                finish();
            }
        });
    }
}