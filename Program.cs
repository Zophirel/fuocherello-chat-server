using SignalRChatServer.Hubs;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SignalRChatServer.Data;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
string ConnString = builder.Configuration.GetConnectionString("DbContext")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnString);
await using var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(ConnString).UseSnakeCaseNamingConvention());
// Add services to the container.
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
