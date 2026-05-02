package com.laptopstore.ptmobile.network;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {

    // Use 10.0.2.2 for Android Emulator pointing to host machine localhost
    private static final String BASE_URL = "http://10.0.2.2:5226/";

    private static Retrofit retrofit = null;

    public static ApiService getService(String token) {
        HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
        logging.setLevel(HttpLoggingInterceptor.Level.BODY);

        OkHttpClient.Builder httpClient = new OkHttpClient.Builder()
                .addInterceptor(logging);

        if (token != null && !token.isEmpty()) {
            httpClient.addInterceptor(new AuthInterceptor(token));
        }

        retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .client(httpClient.build())
                .build();

        return retrofit.create(ApiService.class);
    }

    public static ApiService getService() {
        return getService(null);
    }
}
