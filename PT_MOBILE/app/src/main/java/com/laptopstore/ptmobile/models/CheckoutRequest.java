package com.laptopstore.ptmobile.models;

import java.util.List;

public class CheckoutRequest {
    private String shippingAddress;
    private String phoneNumber;
    private List<CheckoutItem> items;

    public CheckoutRequest(String shippingAddress, String phoneNumber, List<CheckoutItem> items) {
        this.shippingAddress = shippingAddress;
        this.phoneNumber = phoneNumber;
        this.items = items;
    }

    public static class CheckoutItem {
        private int productId;
        private int quantity;

        public CheckoutItem(int productId, int quantity) {
            this.productId = productId;
            this.quantity = quantity;
        }
    }
}
