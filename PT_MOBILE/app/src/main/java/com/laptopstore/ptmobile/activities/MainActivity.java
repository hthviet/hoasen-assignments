package com.laptopstore.ptmobile.activities;

import android.os.Bundle;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.fragments.AccountFragment;
import com.laptopstore.ptmobile.fragments.CartFragment;
import com.laptopstore.ptmobile.fragments.HomeFragment;
import com.laptopstore.ptmobile.fragments.OrdersFragment;
import com.laptopstore.ptmobile.fragments.ProductListFragment;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        BottomNavigationView bottomNav = findViewById(R.id.bottom_nav);
        loadFragment(new HomeFragment());

        bottomNav.setOnItemSelectedListener(item -> {
            Fragment fragment;
            int id = item.getItemId();
            if (id == R.id.nav_home) {
                fragment = new HomeFragment();
            } else if (id == R.id.nav_products) {
                fragment = new ProductListFragment();
            } else if (id == R.id.nav_cart) {
                fragment = new CartFragment();
            } else if (id == R.id.nav_orders) {
                fragment = new OrdersFragment();
            } else {
                fragment = new AccountFragment();
            }
            loadFragment(fragment);
            return true;
        });
    }

    private void loadFragment(Fragment fragment) {
        getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.fragment_container, fragment)
                .commit();
    }
}
