package com.laptopstore.ptmobile.activities;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.models.CartItem;
import com.laptopstore.ptmobile.models.CheckoutRequest;
import com.laptopstore.ptmobile.models.Order;
import com.laptopstore.ptmobile.network.ApiClient;
import com.laptopstore.ptmobile.utils.CartManager;
import com.laptopstore.ptmobile.utils.SessionManager;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CheckoutActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            setTitle("Checkout");
        }

        TextInputEditText etPhone = findViewById(R.id.et_phone);
        TextInputEditText etAddress = findViewById(R.id.et_address);
        TextView tvTotal = findViewById(R.id.tv_total);
        TextView tvOrderSummary = findViewById(R.id.tv_order_summary);
        MaterialButton btnPlaceOrder = findViewById(R.id.btn_place_order);
        ProgressBar progress = findViewById(R.id.progress);

        CartManager cart = CartManager.getInstance();
        tvTotal.setText(String.format("Total: $%.0f", cart.getTotal()));

        // Build summary
        StringBuilder sb = new StringBuilder();
        for (CartItem item : cart.getItems()) {
            sb.append(item.getProduct().getName())
                    .append(" x").append(item.getQuantity())
                    .append(" = $").append(String.format("%.0f", item.getSubtotal()))
                    .append("\n");
        }
        tvOrderSummary.setText(sb.toString().trim());

        btnPlaceOrder.setOnClickListener(v -> {
            String phone = etPhone.getText() != null ? etPhone.getText().toString().trim() : "";
            String address = etAddress.getText() != null ? etAddress.getText().toString().trim() : "";

            if (phone.isEmpty()) {
                etPhone.setError("Please enter phone number");
                return;
            }

            if (address.isEmpty()) {
                etAddress.setError("Please enter shipping address");
                return;
            }

            SessionManager session = new SessionManager(this);
            if (!session.isLoggedIn()) {
                Toast.makeText(this, "Please login to place an order.", Toast.LENGTH_SHORT).show();
                startActivity(new Intent(this, LoginActivity.class));
                return;
            }

            // Build items list
            List<CheckoutRequest.CheckoutItem> checkoutItems = new ArrayList<>();
            for (CartItem item : cart.getItems()) {
                checkoutItems.add(new CheckoutRequest.CheckoutItem(
                        item.getProduct().getProductId(), item.getQuantity()));
            }

            progress.setVisibility(View.VISIBLE);
            btnPlaceOrder.setEnabled(false);

            ApiClient.getService(session.getToken())
                    .checkout(new CheckoutRequest(address, phone, checkoutItems))
                    .enqueue(new Callback<Order>() {
                        @Override
                        public void onResponse(Call<Order> call, Response<Order> response) {
                            progress.setVisibility(View.GONE);
                            btnPlaceOrder.setEnabled(true);
                            if (response.isSuccessful()) {
                                cart.clear();
                                Toast.makeText(CheckoutActivity.this,
                                        getString(R.string.success_order), Toast.LENGTH_LONG).show();
                                finish();
                            } else {
                                Toast.makeText(CheckoutActivity.this,
                                        "Order failed. Please try again.", Toast.LENGTH_SHORT).show();
                            }
                        }

                        @Override
                        public void onFailure(Call<Order> call, Throwable t) {
                            progress.setVisibility(View.GONE);
                            btnPlaceOrder.setEnabled(true);
                            Toast.makeText(CheckoutActivity.this,
                                    getString(R.string.error_network), Toast.LENGTH_SHORT).show();
                        }
                    });
        });
    }

    @Override
    public boolean onSupportNavigateUp() {
        finish();
        return true;
    }
}
