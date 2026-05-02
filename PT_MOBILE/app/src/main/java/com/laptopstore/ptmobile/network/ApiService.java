package com.laptopstore.ptmobile.network;

import com.laptopstore.ptmobile.models.AuthResponse;
import com.laptopstore.ptmobile.models.Category;
import com.laptopstore.ptmobile.models.CheckoutRequest;
import com.laptopstore.ptmobile.models.LoginRequest;
import com.laptopstore.ptmobile.models.Order;
import com.laptopstore.ptmobile.models.ProductResponse;
import com.laptopstore.ptmobile.models.RegisterRequest;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface ApiService {

    @POST("api/auth/login")
    Call<AuthResponse> login(@Body LoginRequest request);

    @POST("api/auth/register")
    Call<AuthResponse> register(@Body RegisterRequest request);

    @GET("api/products")
    Call<ProductResponse> getProducts(
            @Query("search") String search,
            @Query("categoryId") Integer categoryId,
            @Query("sort") String sort,
            @Query("page") int page
    );

    @GET("api/products/{id}")
    Call<com.laptopstore.ptmobile.models.Product> getProductById(@Path("id") int id);

    @GET("api/products/categories")
    Call<List<Category>> getCategories();

    @POST("api/orders/checkout")
    Call<Order> checkout(@Body CheckoutRequest request);

    @GET("api/orders/my")
    Call<List<Order>> myOrders();
}
