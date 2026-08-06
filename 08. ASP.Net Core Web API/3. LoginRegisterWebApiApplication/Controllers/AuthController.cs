using LoginRegisterWebApiApplication.Helpers;
using LoginRegisterWebApiApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace LoginRegisterWebApiApplication.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _userDbContext;
        public AuthController(UserDbContext userDbContext)
        {
            this._userDbContext = userDbContext;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] User userObj)
        {
            if(userObj == null)
            {
                return BadRequest();
            }

            var user = await _userDbContext.Users.FirstOrDefaultAsync(u => u.Username == userObj.Username);

            if(user == null)
            {
                return NotFound(new { Message = "User Not Found" });
            }

            if (!PasswordHelper.Decode(userObj.Password, user.Password))
            {
                await Console.Out.WriteLineAsync(userObj.Password + " ----------- " + user.Password);
                return BadRequest("Password Is Incorrect");
            }

            user.Token = CreateJWT(userObj);

            return Ok( new { 
                Token = user.Token,
                Message = "User Logged In Successfylly" 
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] User userObj)
        {
            if (userObj == null)
            {
                return BadRequest();
            }
            if(await CheckUserNameExistsAsync(userObj.Username))
            {
                return BadRequest("Email Already Exists");
            }
            if(await CheckEmailExistsAsync(userObj.Email))
            {
                return BadRequest("Email Already Exists");
            }

            userObj.Password = PasswordHelper.Encode(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";
            await _userDbContext.AddAsync(userObj);
            await _userDbContext.SaveChangesAsync();

            return Ok("User Registered Successfully");
        }

        private async Task<bool> CheckUserNameExistsAsync(string username)
        {
            return await _userDbContext.Users.AnyAsync(u => u.Username == username);
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userDbContext.Users.AnyAsync(u => u.Email == email);
        }

        private string CreateJWT(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("this is my secret key new");

            var idenity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.Name,user.Name)
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256);

            var tokenDiscriptor = new SecurityTokenDescriptor()
            {
                Subject = idenity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDiscriptor);

            return jwtTokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _userDbContext.Users.ToListAsync();
        }

       
    }

  
}
