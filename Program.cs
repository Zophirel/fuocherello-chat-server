using SignalRChatServer.Hubs;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SignalRChatServer.Data;
using System.Security.Cryptography;
using SignalRChatServer.Singleton.JwtManager;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
string ConnString = builder.Configuration.GetConnectionString("DbContext")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnString);
await using var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(ConnString).UseSnakeCaseNamingConvention());

//import rsa key for jwt validation
string filename = "key";
using (RSA rsa = RSA.Create())
{
    byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
    File.WriteAllBytes(filename, privateKeyBytes);
}
RSA rsaKey = RSA.Create();
rsaKey.ImportRSAPrivateKey(File.ReadAllBytes(filename), out _);

//add jwt validator
builder.Services.AddSingleton<IJwtManager>(new JwtManager(rsaKey));

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapHub<ChatHub>("/chathub");

app.Run();
