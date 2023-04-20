using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        // En samling (collection) i MongoDB-databasen for brugere.
        private readonly IMongoCollection<User> _users;

        // En logger, der bruges til at logge beskeder i applikationen.
        private readonly ILogger<AuthController> _logger;

        // App-konfigurationen, som kan indeholde forskellige indstillinger og oplysninger.
        private readonly IConfiguration _config;


        // Konstruktør til AuthController-klassen, der håndterer godkendelseslogik
        public AuthController(ILogger<AuthController> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;

            // Opret en forbindelse til MongoDB-databasen - admin gøres til miljøvariabel:
            var mongoClient = new MongoClient(_config["connectionsstring"]);
            //database gøres til en miljøvariabel
            var database = mongoClient.GetDatabase(_config["database"]);

            // Opret en samling (collection) for brugere - users bliver gjort til en miljøvariabel:
            _users = database.GetCollection<User>(_config["collection"]);
        }

        // Angiver, at denne metode ikke kræver godkendelse for at blive kaldt.
        [AllowAnonymous]
        // Angiver, at denne metode svarer på HTTP POST-anmodninger til ruten 'login'.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            // Finder brugeren i databasen baseret på det indtastede brugernavn.
            var user = await _users.Find(u => u.Username == login.Username).FirstOrDefaultAsync<User>();
            _logger.LogInformation($"{user}");
            _logger.LogInformation($"{user.Username}, {user.Password}");
            // Hvis brugeren ikke findes eller adgangskoden er forkert, sendes en 401 (unauthorized) statuskode.
            if (user == null || user.Password != login.Password)
            {
                return Unauthorized();
            }
            // Genererer en JWT-token til brugeren.
            var token = GenerateJwtToken(user.Username);
            // Returnerer en 200 OK statuskode sammen med JWT-token.
            return Ok(new { token });
        }

        // Angiver, at denne metode ikke kræver godkendelse for at blive kaldt.
        [AllowAnonymous]
        // Angiver, at denne metode svarer på HTTP POST-anmodninger til ruten 'validate'.
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateJwtToken([FromBody] string? token)
        {
            // Hvis tokenet er tomt eller null, sendes en 400 BadRequest statuskode med en fejlmeddelelse.
            if (token.IsNullOrEmpty())
                return BadRequest("Invalid token submitted.");

            // Opretter en JwtSecurityTokenHandler og henter hemmeligheden fra app-konfigurationen.
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["secret"]!);
            try
            {
                // Validerer JWT-tokenet ved at bruge hemmeligheden og andre tokenvalideringsparametre.
                // Hvis tokenet er gyldigt, udtrækkes bruger-ID'en fra tokenet og returneres som svar på anmodningen.
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = jwtToken.Claims.First(
                    x => x.Type == ClaimTypes.NameIdentifier).Value;
                return Ok(accountId);
            }
            catch (Exception ex)
            {
                // Hvis valideringen fejler, logges en fejlmeddelelse, og der returneres en 404 Not Found statuskode.
                _logger.LogError(ex, ex.Message);
                return StatusCode(404);
            }
        }

        // Metode, der bruges til at generere et JSON Web Token (JWT) for en bruger.
        private string GenerateJwtToken(string username)
        {
            // Opretter en hemmelighed baseret på app-konfigurationens JWT-secret --> sercret = miljøvariabel
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["secret"]));
            // Opretter et SigningCredentials-objekt, der bruger hemmeligheden og HMACSHA256-algoritmen.
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // Opretter en bruger-ID-claim til at inkludere i tokenet.
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username)
            };
            // Opretter en JWT, der inkluderer udstederen (Issuer), publikum (Audience), brugerens claims og udløbstiden.
            // Tokenet underskrives med credentials-objektet. --> issuer = miljøvariabel
            var token = new JwtSecurityToken(
                _config["issuer"],
                "http://localhost",
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);
            // Returnerer tokenet i form af en JWT-streng.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}