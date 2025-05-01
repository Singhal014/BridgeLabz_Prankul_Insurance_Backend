using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RepoLayer.Context;
using RepoLayer.Interfaces;
using RepoLayer.Services;
using RabbitMQConsumer;
using System.Text;
using BusinessLogicLayer.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Layer DI
builder.Services.AddScoped<IAdminRL, AdminRL>();
builder.Services.AddScoped<IEmployeeRL, EmployeeRL>();
builder.Services.AddScoped<IAgentRL, AgentRL>();
builder.Services.AddScoped<ICustomerRL, CustomerRL>();
builder.Services.AddScoped<IPolicyRL, PolicyRL>();
builder.Services.AddScoped<IPremiumRL, PremiumRL>();
builder.Services.AddScoped<ICommissionRL, CommissionRL>();
builder.Services.AddScoped<IPaymentRL, PaymentRL>();
builder.Services.AddScoped<ISchemeRL, SchemeRL>();
builder.Services.AddScoped<IPlanRL, PlanRL>();

// Business Layer DI
builder.Services.AddScoped<IAdminBL, AdminBL>();
builder.Services.AddScoped<IEmployeeBL, EmployeeBL>();
builder.Services.AddScoped<IAgentBL, AgentBL>();
builder.Services.AddScoped<ICustomerBL, CustomerBL>();
builder.Services.AddScoped<IOtpServiceBL, OtpServiceBL>();
builder.Services.AddScoped<IPolicyBL, PolicyBL>();
builder.Services.AddScoped<IPremiumBL, PremiumBL>();
builder.Services.AddScoped<ICommissionBL, CommissionBL>();
builder.Services.AddScoped<IPaymentBL, PaymentBL>();
builder.Services.AddScoped<ISchemeBL, SchemeBL>();
builder.Services.AddScoped<IPlanBL, PlanBL>();

// RabbitMQ Consumer
builder.Services.AddHostedService<RabbitMQConsumerService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Insurance API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Insurance API V1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();