using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace WebApi
{
    public class Program
    {
        private const string validIssuer = "WebApi";
        private const string validAudience = "http://localhost:5152";
        private const string key = "174D364E-C125-4F90-AF20-9217B00E4EC7";
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddOpenIddict().AddCore(options =>
            {


            }).AddServer(options =>
            {


            }).AddValidation(options =>
            {

            });

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = validIssuer,
                       ValidAudience = validAudience,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                   };
               });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }



            //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            app.MapGet("/todoitems/{id}", [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] (int id) => Results.Ok(id));

            app.MapGet("/auth/token", () =>
            {

                var singingAlgorithm = SecurityAlgorithms.HmacSha256;

                //signiture
                var secretByte = Encoding.UTF8.GetBytes(key);
                var signingkey = new SymmetricSecurityKey(secretByte);
                var singingCredentials = new SigningCredentials(signingkey, singingAlgorithm);
                var token = new JwtSecurityToken(
                    issuer: validIssuer,
                    audience: validAudience,
                    claims: null,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: singingCredentials
                    );
                var jsonToken = new JwtSecurityTokenHandler().WriteToken(token);
                return jsonToken;
            });

            app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
    }
}