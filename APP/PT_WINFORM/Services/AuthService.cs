namespace PT_WINFORM.Services;

using System.Text;
using System.Text.Json;
using PT_WINFORM.Models;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string email, string password);
    string GetToken();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private string _token = string.Empty;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var payload = JsonSerializer.Serialize(new { email, password });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("/api/auth/login", content);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var login = JsonSerializer.Deserialize<LoginResponse>(body, _jsonOptions)
            ?? throw new InvalidOperationException("Login token missing from API response.");

        if (string.IsNullOrWhiteSpace(login.Token))
        {
            throw new InvalidOperationException("Login token missing from API response.");
        }

        _token = login.Token;
        return login;
    }

    public string GetToken() => _token;
}
