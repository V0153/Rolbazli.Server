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

var JWTSetting = builder.Configuration.GetSection("JWTSetting"); //Uygulamanýn konfigürasyonundan JWT ayarlarýný alýyoruz.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<AppUser, IdentityRole>() //Uygulama AppUser(IdentityUser'dan türüyen kullanýcý) ve IdentityRole sýfýnýlarýna göre bir kimlik sistemi oluþturur.Kullanýcý giriþ, çýkýþ, kayýt, þifre iþlemleri vb. bu sistem üzerinden yapýlcak þekilde ayarlanýr.Sonuç olarak burda bir kimlik sistemi oluþturulmuþ olur.
    .AddEntityFrameworkStores<AppDbContext>() // Kullanýcý ve rol bilgilerini EntityFramework.Core kullanarak AppDbContext üzerinden veritabanýnda saklayacak þekilde ayarlanýr.
    .AddDefaultTokenProviders(); // AddDefaultTokenProviders() metodu, çaðrýlarak þifre yenileme, email onayý gibi iþlemlerde kullanýlacak token üreticisi servisleri isteme eklenir.Öreðin þifre sýfýrlama.

/* ---------Özet----------
   Kulllanýcý(AppUser) ve rol(IdentityRole) yönetimi için asp.net core identity sistemini yapýlandýrýr ve kimlik verilerini AppDbContext üzerinden EntityFrameworkCore ile veritabanýnda saklanmasýný saðlar.Bu yapý ile birlikte bir kullanýcý register/login sistemi kurulur.
*/

//JWT tabanlý kimlik doðrulamayý servislere ekleme
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // JWT kimlik doðrulama þemasýn için Bearer varsayýlan olarak kullanýlýr.
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // JWT kimlik doðrulama baþarýsýz ise kullanýlacak þemadýr.
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Uygulamada kullanýlacak varsayýlan þemadýr.
}).AddJwtBearer(opt =>  //JWT Bearer yapýlandýrmasý
{
    opt.SaveToken = true; //Token'ý doðruladýktan sonra saklayýp baþka yerlerde kullanabilmek için kaydediyoruz.
    opt.RequireHttpsMetadata = false; //HTTPS gereksinimini devre dýþý býrakýyoruz. (Geliþtirme(Developer) aþamasýnda kullanýþlý olabilir)
    opt.TokenValidationParameters = new TokenValidationParameters  //Token doðrulama parametreleri
    {
        ValidateIssuer = true, //Token'ýn verildiði yeri doðrulamak için kullanýlýr.(Issuer)
        ValidateAudience = true, //Token'ýn aldýðý kiþiyi doðrulamak için kullanýlýr.
        ValidateLifetime = true, //Token'ýn geçerlilik süresini doðrulamak için kullanýlýr.
        ValidateIssuerSigningKey = true, //Token'ýn giriþi(imzayý) doðrulamak için kullanýlýr.
        ValidIssuer = JWTSetting["ValidIssuer"], //Token'ýn verildiði yer
        ValidAudience = JWTSetting["ValidAudience"], //Token'ýn aldýðý kiþi
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSetting.GetSection("secretkey").Value!)) //Token'ýn imzasý(giriþ anahtarý)
    };
});

builder.Services.AddControllers();
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(x=>
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization Example : `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9`", //Swagger UI'de gözükecek açýklama metini.
        Name = "Authorization", //Token'ý nerede yer alacaðýný belirtelim.Header'da Authorization alanýnda yer alacak.
        In = ParameterLocation.Header, //Token'ýn Header'da yer alacaðýný belirtelim.
        Type = SecuritySchemeType.ApiKey, //Token'ýn bir API anahtarý olduðunu belirtelim.Güvenlik þemasý tipi.
        Scheme = "Bearer" //Token'ýn Bearer þemasýnda olduðunu belirtelim.
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme //bearer þemasýný referans alýyoruz.
            {
                Reference = new OpenApiReference  //Swagger'a daha önce tanýmladýýmýz "Bearer" güvenlik tanýmýný referans almasýný söylüyoruz.
                {
                    Type = ReferenceType.SecurityScheme,  // bu bir güvenlik þemasý referansýdýr.
                    Id = "Bearer"
                },
                Scheme = "Bearer", //header'da kullanýllan isim
                Name = "Bearer",
                In = ParameterLocation.Header //token'ýn gönderileceði yer: Header
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
