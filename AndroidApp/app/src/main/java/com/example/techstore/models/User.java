package com.example.techstore.models;

public class User {
    private int id;
    private String username;
    private String role;
    private String firstName;
    private String patronymic;
    private String lastName;
    private String email;
    private String phone;

    public User(int id, String username, String role, String firstName, String patronymic, String lastName, String email, String phone) {
        this.id = id;
        this.username = username;
        this.role = role;
        this.firstName = firstName;
        this.patronymic = patronymic;
        this.lastName = lastName;
        this.email = email;
        this.phone = phone;
    }

    public int getId() { return id; }
    public String getUsername() { return username; }
    public String getRole() { return role; }
    public String getFirstName() { return firstName; }
    public String getPatronymic() { return patronymic; }
    public String getLastName() { return lastName; }
    public String getEmail() { return email; }
    public String getPhone() { return phone; }
}