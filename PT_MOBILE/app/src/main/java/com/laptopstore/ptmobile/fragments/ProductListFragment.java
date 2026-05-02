package com.laptopstore.ptmobile.fragments;

import android.content.Intent;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;
import com.google.android.material.textfield.TextInputEditText;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.activities.ProductDetailActivity;
import com.laptopstore.ptmobile.adapters.ProductAdapter;
import com.laptopstore.ptmobile.models.Category;
import com.laptopstore.ptmobile.models.Product;
import com.laptopstore.ptmobile.models.ProductResponse;
import com.laptopstore.ptmobile.network.ApiClient;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProductListFragment extends Fragment {

    private final List<Product> products = new ArrayList<>();
    private ProductAdapter adapter;
    private ProgressBar progress;
    private TextView tvPage;
    private int currentPage = 1;
    private int totalPages = 1;
    private Integer selectedCategoryId = null;
    private String searchQuery = null;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_product_list, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        RecyclerView rv = view.findViewById(R.id.rv_products);
        rv.setLayoutManager(new GridLayoutManager(getContext(), 2));

        adapter = new ProductAdapter(getContext(), products, product -> {
            Intent intent = new Intent(getContext(), ProductDetailActivity.class);
            intent.putExtra(ProductDetailActivity.EXTRA_PRODUCT_ID, product.getProductId());
            startActivity(intent);
        });
        rv.setAdapter(adapter);

        progress = view.findViewById(R.id.progress);
        tvPage = view.findViewById(R.id.tv_page);

        TextInputEditText etSearch = view.findViewById(R.id.et_search);
        etSearch.setOnEditorActionListener((v2, actionId, event) -> {
            searchQuery = etSearch.getText() != null ? etSearch.getText().toString().trim() : null;
            currentPage = 1;
            loadProducts();
            return true;
        });

        MaterialButton btnPrev = view.findViewById(R.id.btn_prev);
        MaterialButton btnNext = view.findViewById(R.id.btn_next);
        btnPrev.setOnClickListener(v2 -> {
            if (currentPage > 1) { currentPage--; loadProducts(); }
        });
        btnNext.setOnClickListener(v2 -> {
            if (currentPage < totalPages) { currentPage++; loadProducts(); }
        });

        loadCategories(view);
        loadProducts();
    }

    private void loadCategories(View view) {
        ChipGroup chipGroup = view.findViewById(R.id.chip_group_category);
        ApiClient.getService().getCategories().enqueue(new Callback<List<Category>>() {
            @Override
            public void onResponse(Call<List<Category>> call, Response<List<Category>> response) {
                if (!isAdded() || response.body() == null) return;

                // All chip
                Chip chipAll = new Chip(getContext());
                chipAll.setText(getString(R.string.all_categories));
                chipAll.setCheckable(true);
                chipAll.setChecked(true);
                chipAll.setOnClickListener(v -> {
                    selectedCategoryId = null;
                    currentPage = 1;
                    loadProducts();
                });
                chipGroup.addView(chipAll);

                for (Category cat : response.body()) {
                    Chip chip = new Chip(getContext());
                    chip.setText(cat.getName());
                    chip.setCheckable(true);
                    chip.setOnClickListener(v -> {
                        selectedCategoryId = cat.getCategoryId();
                        currentPage = 1;
                        loadProducts();
                    });
                    chipGroup.addView(chip);
                }
            }

            @Override
            public void onFailure(Call<List<Category>> call, Throwable t) { }
        });
    }

    private void loadProducts() {
        progress.setVisibility(View.VISIBLE);
        ApiClient.getService().getProducts(searchQuery, selectedCategoryId, "name", currentPage)
                .enqueue(new Callback<ProductResponse>() {
                    @Override
                    public void onResponse(Call<ProductResponse> call, Response<ProductResponse> response) {
                        progress.setVisibility(View.GONE);
                        if (response.isSuccessful() && response.body() != null) {
                            ProductResponse data = response.body();
                            totalPages = data.getTotalPages();
                            products.clear();
                            products.addAll(data.getItems());
                            adapter.notifyDataSetChanged();
                            tvPage.setText("Page " + currentPage + "/" + totalPages);
                        }
                    }

                    @Override
                    public void onFailure(Call<ProductResponse> call, Throwable t) {
                        progress.setVisibility(View.GONE);
                    }
                });
    }
}
