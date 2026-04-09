using Microsoft.EntityFrameworkCore;
using BookingTravel.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BookingTravelDbContext>(options =>
{
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    options.UseMySql(connectionString, serverVersion);
});

// Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Cấu hình CORS — cho phép Frontend Vue gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // Vite dev server mặc định
                "http://localhost:5174",  // Vite fallback port
                "http://localhost:3000"   // Dự phòng
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Đăng ký Dependency Injection
builder.Services.AddScoped<BookingTravel.Application.Interfaces.IAuthService, BookingTravel.Infrastructure.Services.AuthService>();
builder.Services.AddScoped(typeof(BookingTravel.Application.Interfaces.IRepository<>), typeof(BookingTravel.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<BookingTravel.Application.Interfaces.IUnitOfWork, BookingTravel.Infrastructure.Repositories.UnitOfWork>();
builder.Services.AddScoped<BookingTravel.Application.Interfaces.ICategoryService, BookingTravel.Application.Services.CategoryService>();
builder.Services.AddScoped<BookingTravel.Application.Interfaces.IDestinationService, BookingTravel.Application.Services.DestinationService>();
builder.Services.AddScoped<BookingTravel.Application.Interfaces.ITourService, BookingTravel.Infrastructure.Services.TourService>();
builder.Services.AddScoped<BookingTravel.Application.Interfaces.IEmailService, BookingTravel.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<BookingTravel.Application.Interfaces.IBookingService, BookingTravel.Infrastructure.Services.BookingService>();
builder.Services.AddScoped<BookingTravel.Application.Interfaces.IUserService, BookingTravel.Infrastructure.Services.UserService>();
builder.Services.AddHostedService<BookingTravel.Infrastructure.Workers.BookingCleanupService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Cấu hình Swagger để nhập Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer' [khoảng trắng] và chuỗi token vào ô bên dưới."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Admin User Data tự động
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingTravel.Infrastructure.Data.BookingTravelDbContext>();
    
    // Đảm bảo không tạo trùng lặp admin
    if (!context.Users.Any(u => u.Role == BookingTravel.Domain.Enums.Role.Admin))
    {
        var adminUser = new BookingTravel.Domain.Entities.User
        {
            FullName = "Administrator (BacViet)",
            Email = "admin04@bacviet.com",
            Phone = "0999999999",
            PasswordHash = BookingTravel.Application.Helpers.PasswordHelper.HashPassword("Admin@123"),
            Role = BookingTravel.Domain.Enums.Role.Admin,
            IsActive = true
        };
        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}

app.Run();
