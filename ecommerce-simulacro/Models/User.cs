namespace EcommerceSimulacro.Models;
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // ⚠️ Solo para simulacro
    public Role Role { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
}