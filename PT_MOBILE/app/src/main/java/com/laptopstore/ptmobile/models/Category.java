package com.laptopstore.ptmobile.models;

import com.google.gson.annotations.SerializedName;

public class Category {

    @SerializedName(value = "id", alternate = {"categoryId"})
    private int categoryId;

    @SerializedName("name")
    private String name;

    public int getCategoryId() { return categoryId; }
    public String getName() { return name; }
}
