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

import com.google.android.material.button.MaterialButton;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.activities.LoginActivity;
import com.laptopstore.ptmobile.utils.SessionManager;

public class AccountFragment extends Fragment {

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_account, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        TextView tvName = view.findViewById(R.id.tv_name);
        TextView tvEmail = view.findViewById(R.id.tv_email);
        MaterialButton btnLogout = view.findViewById(R.id.btn_logout);
        MaterialButton btnLogin = view.findViewById(R.id.btn_login);

        SessionManager session = new SessionManager(getContext());

        if (session.isLoggedIn()) {
            tvName.setText(session.getFullName());
            tvEmail.setText(session.getEmail());
            btnLogout.setVisibility(View.VISIBLE);
            btnLogin.setVisibility(View.GONE);

            btnLogout.setOnClickListener(v -> {
                session.clearSession();
                tvName.setText("Guest");
                tvEmail.setText("");
                btnLogout.setVisibility(View.GONE);
                btnLogin.setVisibility(View.VISIBLE);
            });
        } else {
            tvName.setText("Guest");
            tvEmail.setText("Not logged in");
            btnLogout.setVisibility(View.GONE);
            btnLogin.setVisibility(View.VISIBLE);

            btnLogin.setOnClickListener(v ->
                    startActivity(new Intent(getContext(), LoginActivity.class)));
        }
    }
}
