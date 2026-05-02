package com.laptopstore.ptmobile.adapters;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.models.CartItem;
import com.laptopstore.ptmobile.utils.CartManager;

import java.util.List;

public class CartAdapter extends RecyclerView.Adapter<CartAdapter.ViewHolder> {

    public interface OnCartChangedListener {
        void onCartChanged();
    }

    private final Context context;
    private final List<CartItem> items;
    private final OnCartChangedListener listener;

    public CartAdapter(Context context, List<CartItem> items, OnCartChangedListener listener) {
        this.context = context;
        this.items = items;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_cart, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        CartItem item = items.get(position);

        holder.tvName.setText(item.getProduct().getName());
        holder.tvPrice.setText(String.format("$%.0f", item.getProduct().getPrice()));
        holder.tvQty.setText(String.valueOf(item.getQuantity()));

        Glide.with(context)
                .load(item.getProduct().getImageUrl())
                .into(holder.ivProduct);

        holder.btnPlus.setOnClickListener(v -> {
            CartManager.getInstance().updateQuantity(item.getProduct().getProductId(), item.getQuantity() + 1);
            listener.onCartChanged();
        });

        holder.btnMinus.setOnClickListener(v -> {
            CartManager.getInstance().updateQuantity(item.getProduct().getProductId(), item.getQuantity() - 1);
            listener.onCartChanged();
        });

        holder.btnRemove.setOnClickListener(v -> {
            CartManager.getInstance().removeItem(item.getProduct().getProductId());
            listener.onCartChanged();
        });
    }

    @Override
    public int getItemCount() { return items.size(); }

    static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView ivProduct;
        TextView tvName, tvPrice, tvQty;
        View btnPlus, btnMinus;
        ImageButton btnRemove;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            ivProduct = itemView.findViewById(R.id.iv_product);
            tvName = itemView.findViewById(R.id.tv_name);
            tvPrice = itemView.findViewById(R.id.tv_price);
            tvQty = itemView.findViewById(R.id.tv_qty);
            btnPlus = itemView.findViewById(R.id.btn_plus);
            btnMinus = itemView.findViewById(R.id.btn_minus);
            btnRemove = itemView.findViewById(R.id.btn_remove);
        }
    }
}
