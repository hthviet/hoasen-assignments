package com.laptopstore.ptmobile.activities;

import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.models.AuthResponse;
import com.laptopstore.ptmobile.models.RegisterRequest;
import com.laptopstore.ptmobile.network.ApiClient;
import com.laptopstore.ptmobile.utils.SessionManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RegisterActivity extends AppCompatActivity {

    private TextInputEditText etFullName, etEmail, etPhone, etPassword;
    private TextView tvError;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        etFullName = findViewById(R.id.et_full_name);
        etEmail = findViewById(R.id.et_email);
        etPhone = findViewById(R.id.et_phone);
        etPassword = findViewById(R.id.et_password);
        tvError = findViewById(R.id.tv_error);
        MaterialButton btnRegister = findViewById(R.id.btn_register);
        TextView tvLogin = findViewById(R.id.tv_login);

        btnRegister.setOnClickListener(v -> doRegister());
        tvLogin.setOnClickListener(v -> finish());
    }

    private void doRegister() {
        String fullName = etFullName.getText() != null ? etFullName.getText().toString().trim() : "";
        String email = etEmail.getText() != null ? etEmail.getText().toString().trim() : "";
        String phone = etPhone.getText() != null ? etPhone.getText().toString().trim() : "";
        String password = etPassword.getText() != null ? etPassword.getText().toString() : "";

        if (fullName.isEmpty() || email.isEmpty() || password.isEmpty()) {
            tvError.setText("Please fill in all required fields.");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        ApiClient.getService().register(new RegisterRequest(fullName, email, password, phone))
                .enqueue(new Callback<AuthResponse>() {
                    @Override
                    public void onResponse(Call<AuthResponse> call, Response<AuthResponse> response) {
                        if (response.isSuccessful() && response.body() != null) {
                            AuthResponse auth = response.body();
                            new SessionManager(RegisterActivity.this)
                                    .saveSession(auth.getToken(), auth.getUserId(), auth.getFullName(), auth.getEmail());
                            finish();
                        } else {
                            tvError.setText("Registration failed. Email may already be in use.");
                            tvError.setVisibility(View.VISIBLE);
                        }
                    }

                    @Override
                    public void onFailure(Call<AuthResponse> call, Throwable t) {
                        tvError.setText(getString(R.string.error_network));
                        tvError.setVisibility(View.VISIBLE);
                    }
                });
    }
}
