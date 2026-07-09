package com.example.techstore.fragments;

import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.GridView;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.example.techstore.R;
import com.example.techstore.RetrofitClient;
import com.example.techstore.activities.LoginActivity;
import com.example.techstore.network.ApiService;
import com.example.techstore.utils.SessionManager;
import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import android.text.TextUtils;
import java.util.regex.Pattern;

public class ProfileFragment extends Fragment {

    private de.hdodenhof.circleimageview.CircleImageView ivAvatar;
    private TextView tvName, tvEmail, tvPhone, tvChangeAvatar;
    private Button btnEdit, btnLogout;
    private SessionManager sessionManager;
    private ApiService apiService;
    private String currentAvatar = "";
    private SharedPreferences avatarPrefs;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_profile, container, false);

        ivAvatar = view.findViewById(R.id.ivAvatar);
        tvName = view.findViewById(R.id.tvName);
        tvEmail = view.findViewById(R.id.tvEmail);
        tvPhone = view.findViewById(R.id.tvPhone);
        tvChangeAvatar = view.findViewById(R.id.tvChangeAvatar);
        btnEdit = view.findViewById(R.id.btnEdit);
        btnLogout = view.findViewById(R.id.btnLogout);

        sessionManager = new SessionManager(getContext());
        apiService = RetrofitClient.getApiService(getContext());

        avatarPrefs = getContext().getSharedPreferences("avatar_prefs", Context.MODE_PRIVATE);
        loadAvatar();
        loadUserProfile();

        ivAvatar.setOnClickListener(v -> showAvatarDialog());
        tvChangeAvatar.setOnClickListener(v -> showAvatarDialog());
        btnEdit.setOnClickListener(v -> showEditDialog());
        btnLogout.setOnClickListener(v -> logout());

        return view;
    }

    private void loadAvatar() {
        if (!isAdded()) return;
        currentAvatar = avatarPrefs.getString("avatar_" + sessionManager.getUserId(), "");
        if (!currentAvatar.isEmpty() && getContext() != null) {
            Glide.with(this)
                    .load(RetrofitClient.BASE_URL + "Resources/" + currentAvatar)
                    .placeholder(R.drawable.ic_store)
                    .diskCacheStrategy(DiskCacheStrategy.NONE)
                    .skipMemoryCache(true)
                    .into(ivAvatar);
        }
    }

    private void saveAvatarLocally(String avatar) {
        SharedPreferences.Editor editor = avatarPrefs.edit();
        editor.putString("avatar_" + sessionManager.getUserId(), avatar);
        editor.apply();
        currentAvatar = avatar;
    }

    private void loadUserProfile() {
        if (!isAdded()) return;
        String patronymic = sessionManager.getPatronymic();
        String fullName = sessionManager.getFirstName() + " " +
                sessionManager.getLastName() + " " +
                (patronymic.isEmpty() ? "" : patronymic);
        tvName.setText(fullName.trim().isEmpty() ? sessionManager.getUsername() : fullName);
        tvEmail.setText(sessionManager.getEmail());
        tvPhone.setText(sessionManager.getPhone().isEmpty() ? "Не указан" : sessionManager.getPhone());
    }

    private void showAvatarDialog() {
        if (!isAdded() || getContext() == null) return;

        AlertDialog.Builder builder = new AlertDialog.Builder(getContext());
        View dialogView = LayoutInflater.from(getContext()).inflate(R.layout.dialog_avatar, null);
        builder.setView(dialogView);

        GridView gridAvatars = dialogView.findViewById(R.id.gridAvatars);

        String[] avatarNames = getResources().getStringArray(R.array.avatar_list);
        List<String> avatarList = new ArrayList<>(Arrays.asList(avatarNames));

        AvatarAdapter adapter = new AvatarAdapter(getContext(), avatarList);
        gridAvatars.setAdapter(adapter);

        AlertDialog dialog = builder.create();

        gridAvatars.setOnItemClickListener((parent, view, position, id) -> {
            String selectedAvatar = avatarList.get(position);
            if (getContext() != null && isAdded()) {
                Glide.with(ProfileFragment.this)
                        .load(RetrofitClient.BASE_URL + "Resources/" + selectedAvatar)
                        .diskCacheStrategy(DiskCacheStrategy.NONE)
                        .skipMemoryCache(true)
                        .into(ivAvatar);
                saveAvatarLocally(selectedAvatar);
                Toast.makeText(getContext(), "Аватар сохранен", Toast.LENGTH_SHORT).show();
                dialog.dismiss();
            }
        });

        dialog.show();
    }

    private void showEditDialog() {
        if (!isAdded() || getContext() == null) return;

        View dialogView = LayoutInflater.from(getContext()).inflate(R.layout.dialog_edit_profile, null);
        EditText etFirstName = dialogView.findViewById(R.id.etFirstName);
        EditText etPatronymic = dialogView.findViewById(R.id.etPatronymic);
        EditText etLastName = dialogView.findViewById(R.id.etLastName);
        EditText etPhone = dialogView.findViewById(R.id.etPhone);
        EditText etEmail = dialogView.findViewById(R.id.etEmail);

        etFirstName.setText(sessionManager.getFirstName());
        etPatronymic.setText(sessionManager.getPatronymic());
        etLastName.setText(sessionManager.getLastName());
        etPhone.setText(sessionManager.getPhone());
        etEmail.setText(sessionManager.getEmail());

        new AlertDialog.Builder(getContext())
                .setTitle("Редактировать профиль")
                .setView(dialogView)
                .setPositiveButton("Сохранить", (dialog, which) -> {
                    updateProfile(
                            etFirstName.getText().toString().trim(),
                            etPatronymic.getText().toString().trim(),
                            etLastName.getText().toString().trim(),
                            etPhone.getText().toString().trim(),
                            etEmail.getText().toString().trim()
                    );
                })
                .setNegativeButton("Отмена", null)
                .show();
    }

    private void updateProfile(String firstName, String patronymic, String lastName, String phone, String email) {
        if (!isAdded() || getContext() == null) return;

        // Email
        if (TextUtils.isEmpty(email)) {
            Toast.makeText(getContext(), "Введите email", Toast.LENGTH_SHORT).show();
            return;
        }
        if (!Pattern.matches("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", email)) {
            Toast.makeText(getContext(), "Введите корректный email", Toast.LENGTH_SHORT).show();
            return;
        }

        // Имя (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(firstName) && !Pattern.matches("^[a-zA-Zа-яА-ЯёЁ]+$", firstName)) {
            Toast.makeText(getContext(), "Имя должно содержать только буквы", Toast.LENGTH_SHORT).show();
            return;
        }

        // Отчество (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(patronymic) && !Pattern.matches("^[a-zA-Zа-яА-ЯёЁ]+$", patronymic)) {
            Toast.makeText(getContext(), "Отчество должно содержать только буквы", Toast.LENGTH_SHORT).show();
            return;
        }

        // Фамилия (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(lastName) && !Pattern.matches("^[a-zA-Zа-яА-ЯёЁ]+$", lastName)) {
            Toast.makeText(getContext(), "Фамилия должна содержать только буквы", Toast.LENGTH_SHORT).show();
            return;
        }

        // Телефон (необязательно, но если заполнено - проверяем)
        if (!TextUtils.isEmpty(phone)) {
            String cleaned = phone.replaceAll("[^0-9]", "");
            if (cleaned.length() < 10 || cleaned.length() > 11) {
                Toast.makeText(getContext(), "Введите корректный номер телефона", Toast.LENGTH_SHORT).show();
                return;
            }
        }

        ApiService.UpdateProfileRequest request = new ApiService.UpdateProfileRequest();
        request.userId = sessionManager.getUserId();
        request.firstName = firstName;
        request.patronymic = patronymic;
        request.lastName = lastName;
        request.phoneNumber = phone;
        request.email = email;

        apiService.updateProfile(request).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (!isAdded() || getContext() == null) return;
                if (response.isSuccessful()) {
                    sessionManager.saveUser(
                            sessionManager.getUserId(),
                            sessionManager.getUsername(),
                            firstName,
                            patronymic,
                            lastName,
                            email,
                            phone
                    );
                    loadUserProfile();
                    Toast.makeText(getContext(), "Профиль обновлен", Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(getContext(), "Ошибка обновления", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                if (!isAdded() || getContext() == null) return;
                Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void logout() {
        if (!isAdded() || getContext() == null) return;
        sessionManager.logout();
        Intent intent = new Intent(getContext(), LoginActivity.class);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(intent);
        if (getActivity() != null) {
            getActivity().finish();
        }
    }

    class AvatarAdapter extends ArrayAdapter<String> {
        private Context context;
        private List<String> avatarList;

        public AvatarAdapter(Context context, List<String> avatarList) {
            super(context, R.layout.item_avatar, avatarList);
            this.context = context;
            this.avatarList = avatarList;
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View view = inflater.inflate(R.layout.item_avatar, parent, false);

            ImageView ivAvatarItem = view.findViewById(R.id.ivAvatarItem);

            String avatarName = avatarList.get(position);

            Glide.with(context)
                    .load(RetrofitClient.BASE_URL + "Resources/" + avatarName)
                    .placeholder(R.drawable.ic_store)
                    .into(ivAvatarItem);

            return view;
        }
    }
}