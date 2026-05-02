package com.laptopstore.ptmobile.activities;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.models.AuthResponse;
import com.laptopstore.ptmobile.models.LoginRequest;
import com.laptopstore.ptmobile.network.ApiClient;
import com.laptopstore.ptmobile.utils.SessionManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    private TextInputEditText etEmail, etPassword;
    private TextView tvError;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        etEmail = findViewById(R.id.et_email);
        etPassword = findViewById(R.id.et_password);
        tvError = findViewById(R.id.tv_error);
        MaterialButton btnLogin = findViewById(R.id.btn_login);
        TextView tvRegister = findViewById(R.id.tv_register);

        btnLogin.setOnClickListener(v -> doLogin());

        tvRegister.setOnClickListener(v -> {
            startActivity(new Intent(this, RegisterActivity.class));
        });
    }

    private void doLogin() {
        String email = etEmail.getText() != null ? etEmail.getText().toString().trim() : "";
        String password = etPassword.getText() != null ? etPassword.getText().toString() : "";

        if (email.isEmpty() || password.isEmpty()) {
            tvError.setText("Please enter email and password.");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        ApiClient.getService().login(new LoginRequest(email, password))
                .enqueue(new Callback<AuthResponse>() {
                    @Override
                    public void onResponse(Call<AuthResponse> call, Response<AuthResponse> response) {
                        if (response.isSuccessful() && response.body() != null) {
                            AuthResponse auth = response.body();
                            new SessionManager(LoginActivity.this)
                                    .saveSession(auth.getToken(), auth.getUserId(), auth.getFullName(), auth.getEmail());
                            finish();
                        } else {
                            tvError.setText(getString(R.string.error_login_failed));
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
