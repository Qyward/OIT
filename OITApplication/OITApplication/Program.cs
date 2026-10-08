using OITApplication.Models;
using OITApplication.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OITApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<SymetricKey>(builder.Configuration.GetSection("SymetricKey"));

            builder.Services.AddAuthentication().AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                        
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("V1zHUqkj9DbnIqA8/e4M0k4Li8oJ82NCmpKHn4JdF7M="))
                };
            });
            builder.Services.AddAuthorization();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=users.db");
            });


            builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
                p.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()));


            var app = builder.Build();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/getUserData", [Authorize](HttpContext context, AppDbContext db) =>
            {
                var user = db.Users.FirstOrDefault(u => u.Email == context.User.Identity.Name);      

                return Results.Ok(user);
            });

            app.MapPost("/login", (UserModel user, AppDbContext db) =>
            {
                User product = db.Users.FirstOrDefault(u => u.Email == user.Email);

                if(product == null)
                {
                    return Results.NotFound();
                }

                if(product.Password != user.Password)
                {
                    return Results.BadRequest("Неверный пароль");
                }

                var claims = new List<Claim>{
                    new Claim(ClaimTypes.Name, product.Email)
                };

                var jwt = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("V1zHUqkj9DbnIqA8/e4M0k4Li8oJ82NCmpKHn4JdF7M=")),
                        SecurityAlgorithms.HmacSha256));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Results.Text(encodedJwt);
            });

            app.MapPost("/registration", (UserRegistrationModel user, AppDbContext db) =>
            {
                var get = db.Users.FirstOrDefault(o =>  o.Email == user.Email);

                if(db.Users.FirstOrDefault(o => o.Email == user.Email) != null)
                {
                    return Results.BadRequest("почта занята");
                }
                else if(db.Users.FirstOrDefault(o => o.Name == user.Name) != null)
                {
                    return Results.BadRequest("имя занято");
                }

                if(get == null)
                {
                    if(user.Password.Length < 8 
                        || user.Password.IndexOf(' ') != -1)
                    {
                        return Results.BadRequest("Неправильно введён пароль");
                    }



                    User u = new User(user.Name, user.Email, user.Password);

                    db.Users.Add(u);
                    db.SaveChanges();


                    return Results.Created();
                }

                return Results.BadRequest("такой пользователь уже существует.");
            });

            app.Run();
        }
    }
}
