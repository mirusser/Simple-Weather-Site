using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuth;

public class AuthenticationManager(string key)
{
	// TODO: real database (this is for dev/tests purposes only
	private readonly Dictionary<string, string> users = new()
	{
		{"test", "password"}
	};

	public bool TryAuthenticate(
		string username,
		string password,
		out string? token)
	{
		token = null;

		if (!users.Any(u => u.Key == username && u.Value == password))
		{
			return false;
		}

		JwtSecurityTokenHandler tokenHandler = new();
		var tokenKey = Encoding.ASCII.GetBytes(key);

		SecurityTokenDescriptor tokenDescriptor = new()
		{
			Subject = new ClaimsIdentity(
			[
				new(ClaimTypes.Name, username),
			]),
			Expires = DateTime.UtcNow.AddHours(1), // TODO: add to settings
			SigningCredentials = new(
				new SymmetricSecurityKey(tokenKey),
				SecurityAlgorithms.HmacSha256Signature),
		};
		var securityToken = tokenHandler.CreateToken(tokenDescriptor);

		token = tokenHandler.WriteToken(securityToken);

		return true;
	}
}