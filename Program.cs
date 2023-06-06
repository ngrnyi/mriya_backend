using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MessengerBackend.Data;
using MessengerBackend.Controllers;
using MessengerBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using MessengerBackend.Helpers;
using MessengerBackend;
using Hangfire;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MessengerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddSignalR()
        .AddAzureSignalR(builder.Configuration.GetConnectionString("AzureSignalR"));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<MessengerDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<AnalyzeMessagesService>();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessengerBackend", Version = "v1" });
});

// Configure JWT authentication
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);

var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed.");
                Console.WriteLine(context.Exception);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    var contentModerationService = services.GetService<AnalyzeMessagesService>();
}

app.UseHangfireServer();
var jobClient = app.Services.GetService<IBackgroundJobClient>();
RecurringJob.AddOrUpdate(() => new AnalyzeMessagesService(app.Services.CreateScope().ServiceProvider.GetService<MessengerDbContext>()).AnalyzeNewMessages(), Cron.MinuteInterval(10));
app.UseHangfireDashboard(); // This line enables the Hangfire Dashboard
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Add this line before UseAuthorization
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessengerBackend v1");
});


app.MapControllers();
app.UseEndpoints(endpoints =>
{
        endpoints.MapHub<ChatHub>("/chatHub");
});
app.Run();
