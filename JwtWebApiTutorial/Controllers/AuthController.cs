using JwtWebApiTutorial.Data;
using JwtWebApiTutorial.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtWebApiTutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly DataContext _context;

        public AuthController(IConfiguration configuration, IUserService userService, DataContext context)
        {
            _configuration = configuration;
            _userService = userService;
            _context = context;

        }

        [HttpGet, Authorize]
        public ActionResult<string> GetMe()
        {
            var userName = _userService.GetMyName();
            return Ok(userName);
        }
        [HttpGet("getAddresses")]
        public ActionResult<Accounts> GetAllAddresses()
        {
         
            return Ok(_context.Addresses);

        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(Accounts request)
        {

            if (_context.Accounts == null)
            {
                return Problem("Entity set 'DataContext.Persons'  is null.");
            }
            _context.Accounts.Add(request);

            //user.Username = request.Username;
            var psH = Hashing(request.Password);

            request.Password = psH;
            /*user.PasswordSalt = passwordSalt;
               {
                  "id": 0,
                  "nume": "John",
                  "prenume": "Manu",
                  "email": "sdsds",
                  "nume_firma": "sdsds",
                  "username": "dsdsds",
                  "pass": "fffff",
                  "tip_pachet": "sdsdsd"
                }
           */
            await _context.SaveChangesAsync();

            Token tok = new Token();
            tok.token = CreateToken(request.id);
            tok.error = "";
            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);


            return Ok(tok);
          
        }

        

        public class LoginClass{
            public string name { get; set; }
            public string pass { get; set; }


            }
        public class Token
        {
            public string token { get; set; }
            public string error { get; set; }
            


        }
        [HttpPost("login")]
        public async Task<ActionResult<LoginClass>> Login(LoginClass lc)
        {

            Token tok = new Token();
            Accounts acc = _context.Accounts.Where(s => s.Username == lc.name).FirstOrDefault();
            if (acc == null)
            {
                
                tok.token = "";
                tok.error = "User not found.";
                return Ok(tok);
            }
            if (Hashing(lc.pass) != acc.Password)
            {

                tok.token = "";
                tok.error = "Wrong password.";
                return Ok(tok);
            }
            /* if (user.Username != request.Username)
             {
                 return BadRequest("User not found.");
             }
             var passHashed = Hashing(pass);
             if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
             {
                 return BadRequest("Wrong password.");
             }
            */


            tok.token = CreateToken(acc.id);
            tok.error = "";
            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);
            

            return Ok(tok);
        }
        public class Data
        {
            public string nume { get; set; }
            public string prenume { get; set; }
            public string adresa { get; set; }
        }

        [HttpPost("getData")]

        public async Task<ActionResult<Data>> GetData(int id)
        {
            Data dt = new Data();
            Accounts acc = _context.Accounts.Where(s => s.id == id).FirstOrDefault();
            dt.nume = acc.LastName;
            dt.prenume = acc.FirstName;
            dt.adresa = acc.Address;
            return Ok(dt);
        }


            [HttpPost("UAT")]
        public async Task<ActionResult<User>> changeUat(UAT request)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'DataContext.Persons'  is null.");
            }
            _context.UAT.Add(request);
            await _context.SaveChangesAsync();

            return Ok(request);
        }


        [HttpPost("Address")]
        public async Task<ActionResult<User>> changeAddress(string address, int uaTiD)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'DataContext.Persons'  is null.");
            }
            Addresses address2 = new Addresses();
            address2.UATiD = uaTiD;
            address2.Address = address;
            
            UAT ut    = _context.UAT.Where(s => s.id == uaTiD).FirstOrDefault();
           
            _context.Addresses.Add(address2);
            await _context.SaveChangesAsync();

            return Ok(ut);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if(user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }

            string token = CreateToken(1);
           
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);
           
            return Ok(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
        }

        private string CreateToken(int id)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("id", id.ToString())
                //new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string Hashing(string pass)
        {
            var sha = SHA256.Create();
            var asByteArray = Encoding.Default.GetBytes(pass);
            var hashedPassword = sha.ComputeHash(asByteArray);
            return Convert.ToBase64String(hashedPassword);

 
        }
        private string unHashing(string base64)
        {
            var b64 = Convert.FromBase64String(base64);
            var result = Encoding.Default.GetString(b64);
            return result;


           
        }
    }
}
