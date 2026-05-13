using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartRentApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS – allow React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // required for SignalR
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // Allow SignalR to read JWT from query string (browser WebSocket requirement)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                {
                    context.Token = accessToken;
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

// Register Generic Repository and Services (Repository Pattern)
builder.Services.AddScoped(typeof(SmartRentApi.Repositories.IGenericRepository<>), typeof(SmartRentApi.Repositories.GenericRepository<>));
builder.Services.AddScoped<SmartRentApi.Services.IPropertyService, SmartRentApi.Services.PropertyService>();
builder.Services.AddScoped<SmartRentApi.Services.IAdminService, SmartRentApi.Services.AdminService>();
builder.Services.AddScoped<SmartRentApi.Services.IReviewService, SmartRentApi.Services.ReviewService>();
builder.Services.AddScoped<SmartRentApi.Services.IApplicationService, SmartRentApi.Services.ApplicationService>();
builder.Services.AddScoped<SmartRentApi.Services.IVisitService, SmartRentApi.Services.VisitService>();
builder.Services.AddScoped<SmartRentApi.Services.IFavoriteService, SmartRentApi.Services.FavoriteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("ReactApp");     
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SmartRentApi.Hubs.NotificationHub>("/notifications");

app.Run();

