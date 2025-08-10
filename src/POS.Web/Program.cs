using POS.Infrastructure;
using POS.Shared.Infrastructure;
using System.Reflection;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add modules
var moduleAssemblies = new[]
{
    Assembly.Load("POS.Modules.Authentication"),
    Assembly.Load("POS.Modules.Sales"),
    Assembly.Load("POS.Modules.Inventory"),
    Assembly.Load("POS.Modules.Customers"),
    Assembly.Load("POS.Modules.Branches"),
    Assembly.Load("POS.Modules.Reporting"),
    Assembly.Load("POS.Modules.Payments"),
    Assembly.Load("POS.Modules.Recipes"),
    Assembly.Load("POS.Modules.Promotions")
};

builder.Services.AddModules(moduleAssemblies);

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Initialize database
await POS.Infrastructure.DependencyInjection.InitializeDatabaseAsync(app.Services);

app.Run();