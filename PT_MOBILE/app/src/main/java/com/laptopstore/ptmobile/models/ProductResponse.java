package com.laptopstore.ptmobile.models;

import com.google.gson.annotations.SerializedName;

import java.util.List;

public class ProductResponse {

    @SerializedName("items")
    private List<Product> items;

    @SerializedName("totalPages")
    private int totalPages;

    @SerializedName(value = "page", alternate = {"currentPage"})
    private int currentPage;

    @SerializedName(value = "total", alternate = {"totalItems"})
    private int totalItems;

    public List<Product> getItems() { return items; }
    public int getTotalPages() { return totalPages; }
    public int getCurrentPage() { return currentPage; }
    public int getTotalItems() { return totalItems; }
}
