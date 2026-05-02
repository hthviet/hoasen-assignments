package com.laptopstore.ptmobile.models;

import com.google.gson.annotations.SerializedName;

public class Product {

    @SerializedName(value = "id", alternate = {"productId"})
    private int productId;

    @SerializedName("name")
    private String name;

    @SerializedName("brand")
    private String brand;

    @SerializedName("price")
    private double price;

    @SerializedName(value = "stockQuantity", alternate = {"stock"})
    private int stock;

    @SerializedName("imageUrl")
    private String imageUrl;

    @SerializedName("description")
    private String description;

    @SerializedName("categoryId")
    private int categoryId;

    @SerializedName("categoryName")
    private String categoryName;

    public int getProductId() { return productId; }
    public String getName() { return name; }
    public String getBrand() { return brand; }
    public double getPrice() { return price; }
    public int getStock() { return stock; }
    public String getImageUrl() { return imageUrl; }
    public String getDescription() { return description; }
    public int getCategoryId() { return categoryId; }
    public String getCategoryName() { return categoryName; }
}
