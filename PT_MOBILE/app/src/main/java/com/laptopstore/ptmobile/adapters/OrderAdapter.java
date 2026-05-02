package com.laptopstore.ptmobile.adapters;

import android.content.Context;
import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.laptopstore.ptmobile.R;
import com.laptopstore.ptmobile.models.Order;
import com.laptopstore.ptmobile.models.OrderItem;

import java.util.List;

public class OrderAdapter extends RecyclerView.Adapter<OrderAdapter.ViewHolder> {

    private final Context context;
    private final List<Order> orders;

    public OrderAdapter(Context context, List<Order> orders) {
        this.context = context;
        this.orders = orders;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_order, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        Order order = orders.get(position);

        holder.tvOrderId.setText("Order #" + order.getOrderId());
        holder.tvDate.setText(order.getOrderDate() != null ? order.getOrderDate().substring(0, 10) : "");
        holder.tvTotal.setText(String.format("Total: $%.0f", order.getTotalAmount()));
        holder.tvStatus.setText(order.getStatus());

        // Color code status
        switch (order.getStatus()) {
            case "Paid":
                holder.tvStatus.setBackgroundColor(Color.parseColor("#388E3C"));
                break;
            case "Shipped":
                holder.tvStatus.setBackgroundColor(Color.parseColor("#1565C0"));
                break;
            default:
                holder.tvStatus.setBackgroundColor(Color.parseColor("#F57C00"));
                break;
        }

        // Build items summary
        if (order.getOrderItems() != null) {
            StringBuilder sb = new StringBuilder();
            for (OrderItem item : order.getOrderItems()) {
                sb.append(item.getProductName()).append(" x").append(item.getQuantity()).append("\n");
            }
            holder.tvItems.setText(sb.toString().trim());
        }
    }

    @Override
    public int getItemCount() { return orders.size(); }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvOrderId, tvStatus, tvDate, tvItems, tvTotal;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvOrderId = itemView.findViewById(R.id.tv_order_id);
            tvStatus = itemView.findViewById(R.id.tv_status);
            tvDate = itemView.findViewById(R.id.tv_date);
            tvItems = itemView.findViewById(R.id.tv_items);
            tvTotal = itemView.findViewById(R.id.tv_total);
        }
    }
}
