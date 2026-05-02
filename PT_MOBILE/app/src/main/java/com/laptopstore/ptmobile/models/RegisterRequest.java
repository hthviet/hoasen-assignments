package com.laptopstore.ptmobile.models;

public class RegisterRequest {
    private String fullName;
    private String email;
    private String password;
    private String phone;

    public RegisterRequest(String fullName, String email, String password, String phone) {
        this.fullName = fullName;
        this.email = email;
        this.password = password;
        this.phone = phone;
    }
}
