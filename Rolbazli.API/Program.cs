using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoleBazli.Data;
using RoleBazli.Model.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var JWTSetting = builder.Configuration.GetSection("JWTSetting"); //Uygulaman�n konfig�rasyonundan JWT ayarlar�n� al�yoruz.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<AppUser, IdentityRole>() //Uygulama AppUser(IdentityUser'dan t�r�yen kullan�c�) ve IdentityRole s�f�n�lar�na g�re bir kimlik sistemi olu�turur.Kullan�c� giri�, ��k��, kay�t, �ifre i�lemleri vb. bu sistem �zerinden yap�lcak �ekilde ayarlan�r.Sonu� olarak burda bir kimlik sistemi olu�turulmu� olur.
    .AddEntityFrameworkStores<AppDbContext>() // Kullan�c� ve rol bilgilerini EntityFramework.Core kullanarak AppDbContext �zerinden veritaban�nda saklayacak �ekilde ayarlan�r.
    .AddDefaultTokenProviders(); // AddDefaultTokenProviders() metodu, �a�r�larak �ifre yenileme, email onay� gibi i�lemlerde kullan�lacak token �reticisi servisleri isteme eklenir.�re�in �ifre s�f�rlama.

/* ---------�zet----------
   Kulllan�c�(AppUser) ve rol(IdentityRole) y�netimi i�in asp.net core identity sistemini yap�land�r�r ve kimlik verilerini AppDbContext �zerinden EntityFrameworkCore ile veritaban�nda saklanmas�n� sa�lar.Bu yap� ile birlikte bir kullan�c� register/login sistemi kurulur.
*/

//JWT tabanl� kimlik do�rulamay� servislere ekleme
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // JWT kimlik do�rulama �emas�n i�in Bearer varsay�lan olarak kullan�l�r.
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // JWT kimlik do�rulama ba�ar�s�z ise kullan�lacak �emad�r.
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Uygulamada kullan�lacak varsay�lan �emad�r.
}).AddJwtBearer(opt =>  //JWT Bearer yap�land�rmas�
{
    opt.SaveToken = true; //Token'� do�rulad�ktan sonra saklay�p ba�ka yerlerde kullanabilmek i�in kaydediyoruz.
    opt.RequireHttpsMetadata = false; //HTTPS gereksinimini devre d��� b�rak�yoruz. (Geli�tirme(Developer) a�amas�nda kullan��l� olabilir)
    opt.TokenValidationParameters = new TokenValidationParameters  //Token do�rulama parametreleri
    {
        ValidateIssuer = true, //Token'�n verildi�i yeri do�rulamak i�in kullan�l�r.(Issuer)
        ValidateAudience = true, //Token'�n ald��� ki�iyi do�rulamak i�in kullan�l�r.
        ValidateLifetime = true, //Token'�n ge�erlilik s�resini do�rulamak i�in kullan�l�r.
        ValidateIssuerSigningKey = true, //Token'�n giri�i(imzay�) do�rulamak i�in kullan�l�r.
        ValidIssuer = JWTSetting["ValidIssuer"], //Token'�n verildi�i yer
        ValidAudience = JWTSetting["ValidAudience"], //Token'�n ald��� ki�i
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSetting.GetSection("secretkey").Value!)) //Token'�n imzas�(giri� anahtar�)
    };
});

builder.Services.AddControllers();
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(x=>
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization Example : `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9`", //Swagger UI'de g�z�kecek a��klama metini.
        Name = "Authorization", //Token'� nerede yer alaca��n� belirtelim.Header'da Authorization alan�nda yer alacak.
        In = ParameterLocation.Header, //Token'�n Header'da yer alaca��n� belirtelim.
        Type = SecuritySchemeType.ApiKey, //Token'�n bir API anahtar� oldu�unu belirtelim.G�venlik �emas� tipi.
        Scheme = "Bearer" //Token'�n Bearer �emas�nda oldu�unu belirtelim.
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme //bearer �emas�n� referans al�yoruz.
            {
                Reference = new OpenApiReference  //Swagger'a daha �nce tan�mlad��m�z "Bearer" g�venlik tan�m�n� referans almas�n� s�yl�yoruz.
                {
                    Type = ReferenceType.SecurityScheme,  // bu bir g�venlik �emas� referans�d�r.
                    Id = "Bearer"
                },
                Scheme = "Bearer", //header'da kullan�llan isim
                Name = "Bearer",
                In = ParameterLocation.Header //token'�n g�nderilece�i yer: Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(opt =>
{
    opt.AllowAnyHeader();
    opt.AllowAnyMethod();
    opt.AllowAnyOrigin();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
