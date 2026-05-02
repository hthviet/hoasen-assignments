package com.laptopstore.ptmobile.activities;

import android.os.Bundle;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.models.Product;
import com.laptopstore.ptmobile.network.ApiClient;
import com.laptopstore.ptmobile.utils.CartManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProductDetailActivity extends AppCompatActivity {

    public static final String EXTRA_PRODUCT_ID = "product_id";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_product_detail);

        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        }

        int productId = getIntent().getIntExtra(EXTRA_PRODUCT_ID, -1);
        if (productId == -1) {
            finish();
            return;
        }

        loadProduct(productId);
    }

    private void loadProduct(int productId) {
        ApiClient.getService().getProductById(productId).enqueue(new Callback<Product>() {
            @Override
            public void onResponse(Call<Product> call, Response<Product> response) {
                if (response.isSuccessful() && response.body() != null) {
                    bindProduct(response.body());
                }
            }

            @Override
            public void onFailure(Call<Product> call, Throwable t) {
                Toast.makeText(ProductDetailActivity.this, getString(R.string.error_network), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void bindProduct(Product product) {
        setTitle(product.getName());

        ImageView ivProduct = findViewById(R.id.iv_product);
        TextView tvName = findViewById(R.id.tv_name);
        TextView tvBrand = findViewById(R.id.tv_brand);
        TextView tvPrice = findViewById(R.id.tv_price);
        TextView tvStock = findViewById(R.id.tv_stock);
        TextView tvDescription = findViewById(R.id.tv_description);
        MaterialButton btnAddToCart = findViewById(R.id.btn_add_to_cart);

        tvName.setText(product.getName());
        tvBrand.setText(product.getBrand());
        tvPrice.setText(String.format("$%.0f", product.getPrice()));
        tvStock.setText(getString(R.string.in_stock, product.getStock()));
        tvDescription.setText(product.getDescription());

        Glide.with(this).load(product.getImageUrl()).into(ivProduct);

        btnAddToCart.setOnClickListener(v -> {
            CartManager.getInstance().addProduct(product);
            Toast.makeText(this, product.getName() + " added to cart!", Toast.LENGTH_SHORT).show();
        });
    }

    @Override
    public boolean onSupportNavigateUp() {
        finish();
        return true;
    }
}
