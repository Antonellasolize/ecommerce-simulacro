namespace EcommerceSimulacro.DTOs;
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string Role, int? CompanyId);
public record LoginResponse(string Token, string Role, int? CompanyId);