package com.laptopstore.ptmobile.fragments;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.android.material.button.MaterialButton;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.activities.CheckoutActivity;
import com.laptopstore.ptmobile.activities.LoginActivity;
import com.laptopstore.ptmobile.adapters.CartAdapter;
import com.laptopstore.ptmobile.utils.CartManager;
import com.laptopstore.ptmobile.utils.SessionManager;

public class CartFragment extends Fragment {

    private CartAdapter adapter;
    private TextView tvTotal, tvEmpty;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_cart, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        tvTotal = view.findViewById(R.id.tv_total);
        tvEmpty = view.findViewById(R.id.tv_empty);
        RecyclerView rv = view.findViewById(R.id.rv_cart);
        rv.setLayoutManager(new LinearLayoutManager(getContext()));

        adapter = new CartAdapter(getContext(), CartManager.getInstance().getItems(), this::refreshUI);
        rv.setAdapter(adapter);

        MaterialButton btnCheckout = view.findViewById(R.id.btn_checkout);
        btnCheckout.setOnClickListener(v -> {
            if (!new SessionManager(getContext()).isLoggedIn()) {
                startActivity(new Intent(getContext(), LoginActivity.class));
                return;
            }
            startActivity(new Intent(getContext(), CheckoutActivity.class));
        });

        refreshUI();
    }

    private void refreshUI() {
        adapter.notifyDataSetChanged();
        tvTotal.setText(String.format("Total: $%.0f", CartManager.getInstance().getTotal()));
        tvEmpty.setVisibility(CartManager.getInstance().getItems().isEmpty() ? View.VISIBLE : View.GONE);
    }
}
