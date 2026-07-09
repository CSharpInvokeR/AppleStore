package com.example.techstore.utils;

import android.content.Context;
import android.content.SharedPreferences;

public class SessionManager {
    private static final String PREF_NAME = "TechStoreSession";
    private static final String KEY_USER_ID = "userId";
    private static final String KEY_USERNAME = "username";
    private static final String KEY_ROLE = "role";
    private static final String KEY_FIRST_NAME = "firstName";
    private static final String KEY_PATRONYMIC = "patronymic";
    private static final String KEY_LAST_NAME = "lastName";
    private static final String KEY_EMAIL = "email";
    private static final String KEY_PHONE = "phone";
    private static final String KEY_IS_LOGGED_IN = "isLoggedIn";

    private SharedPreferences pref;
    private SharedPreferences.Editor editor;

    public SessionManager(Context context) {
        pref = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        editor = pref.edit();
    }

    public void saveUser(int userId, String username, String firstName, String patronymic, String lastName, String email, String phone) {
        editor.putInt(KEY_USER_ID, userId);
        editor.putString(KEY_USERNAME, username);
        editor.putString(KEY_ROLE, "Клиент");
        editor.putString(KEY_FIRST_NAME, firstName);
        editor.putString(KEY_PATRONYMIC, patronymic);
        editor.putString(KEY_LAST_NAME, lastName);
        editor.putString(KEY_EMAIL, email);
        editor.putString(KEY_PHONE, phone);
        editor.putBoolean(KEY_IS_LOGGED_IN, true);
        editor.apply();
    }

    public int getUserId() { return pref.getInt(KEY_USER_ID, -1); }
    public String getUsername() { return pref.getString(KEY_USERNAME, ""); }
    public String getRole() { return pref.getString(KEY_ROLE, ""); }
    public String getFirstName() { return pref.getString(KEY_FIRST_NAME, ""); }
    public String getPatronymic() { return pref.getString(KEY_PATRONYMIC, ""); }
    public String getLastName() { return pref.getString(KEY_LAST_NAME, ""); }
    public String getEmail() { return pref.getString(KEY_EMAIL, ""); }
    public String getPhone() { return pref.getString(KEY_PHONE, ""); }
    public boolean isLoggedIn() { return pref.getBoolean(KEY_IS_LOGGED_IN, false); }

    public void logout() {
        editor.clear();
        editor.apply();
    }
}