using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using GraphQLDemo.Core.Inputs;
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GraphQLDemo.Infrastructure.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IValidator<RegisterInput> _registerValidator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration config, IValidator<RegisterInput> registerValidator, ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        public async Task<User> RegisterAsync(RegisterInput input)
        {
            var result = await _registerValidator.ValidateAsync(input);
            if (!result.IsValid)
            {
                var errorDict = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Core.Exceptions.ValidationException(errorDict);
            }

            if (await _context.Users.AnyAsync(u => u.Email == input.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} is already in use", input.Email);
                throw new Core.Exceptions.ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new[] { "Email is already in use" } }
                });
            }

            _logger.LogInformation("Registering user with email: {Email}", input.Email);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = input.Username,
                Email = input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                Role = "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string> LoginAsync(LoginInput input)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == input.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured");
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}

