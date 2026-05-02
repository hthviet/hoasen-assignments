package com.laptopstore.ptmobile.fragments;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.activities.LoginActivity;
import com.laptopstore.ptmobile.adapters.OrderAdapter;
import com.laptopstore.ptmobile.models.Order;
import com.laptopstore.ptmobile.network.ApiClient;
import com.laptopstore.ptmobile.utils.SessionManager;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class OrdersFragment extends Fragment {

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_orders, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        ProgressBar progress = view.findViewById(R.id.progress);
        TextView tvEmpty = view.findViewById(R.id.tv_empty);
        RecyclerView rv = view.findViewById(R.id.rv_orders);
        rv.setLayoutManager(new LinearLayoutManager(getContext()));

        SessionManager session = new SessionManager(getContext());
        if (!session.isLoggedIn()) {
            tvEmpty.setText("Please login to see your orders.");
            tvEmpty.setVisibility(View.VISIBLE);
            return;
        }

        progress.setVisibility(View.VISIBLE);
        ApiClient.getService(session.getToken()).myOrders().enqueue(new Callback<List<Order>>() {
            @Override
            public void onResponse(Call<List<Order>> call, Response<List<Order>> response) {
                progress.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    List<Order> orders = response.body();
                    if (orders.isEmpty()) {
                        tvEmpty.setVisibility(View.VISIBLE);
                    } else {
                        rv.setAdapter(new OrderAdapter(getContext(), orders));
                    }
                }
            }

            @Override
            public void onFailure(Call<List<Order>> call, Throwable t) {
                progress.setVisibility(View.GONE);
                tvEmpty.setText(getString(R.string.error_network));
                tvEmpty.setVisibility(View.VISIBLE);
            }
        });
    }
}
