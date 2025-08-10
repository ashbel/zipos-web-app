using POS.Infrastructure;
using POS.Shared.Infrastructure;
using System.Reflection;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "dev-secret"))
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(POS.Modules.Authentication.Authorization.Policies.CanManageUsers, policy => policy.Requirements.Add(new POS.Modules.Authentication.Authorization.PermissionRequirement("users:manage")));
    options.AddPolicy(POS.Modules.Authentication.Authorization.Policies.CanApproveRefunds, policy => policy.Requirements.Add(new POS.Modules.Authentication.Authorization.PermissionRequirement("sales:refund")));
    options.AddPolicy(POS.Modules.Authentication.Authorization.Policies.CanReprintReceipts, policy => policy.Requirements.Add(new POS.Modules.Authentication.Authorization.PermissionRequirement("sales:receipt:reprint")));
});

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

// Background jobs scheduling
using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
    // Stock alerts job per tenant can be triggered externally; schedule a control job if needed
    // Example: out-of-date tenant migration daily
    // To run per tenant, enqueue with specific org id from control-plane enumeration (future enhancement)
}

app.Run();