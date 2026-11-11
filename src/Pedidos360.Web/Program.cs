using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Pedidos360.Web.Data;
using Pedidos360.Web.Filters;
using Pedidos360.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var databaseProvider = builder.Configuration["Database:Provider"]?.Trim().ToLowerInvariant() ?? "sqlserver";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var aivenSchema = builder.Configuration["Database:AivenSchema"]?.Trim();

if (databaseProvider is "mysql" or "aiven")
{
    var connectionUri = builder.Configuration["Database:ConnectionUri"];
    if (!string.IsNullOrWhiteSpace(connectionUri))
    {
        connectionString = BuildMySqlConnectionString(connectionUri, aivenSchema);
    }
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings__DefaultConnection or, for Aiven MySQL, Database__ConnectionUri as a backend secret.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider is "mysql" or "aiven")
    {
        options.UseMySQL(connectionString);
        return;
    }

    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
        .CommandTimeout(90));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add<DatabaseUnavailableFilter>());
builder.Services.AddHostedService<DatabaseWarmupService>();
builder.Services.AddHostedService<IdentitySeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static string BuildMySqlConnectionString(string connectionUri, string? schemaOverride)
{
    if (!Uri.TryCreate(connectionUri, UriKind.Absolute, out var uri)
        || !uri.Scheme.Equals("mysql", StringComparison.OrdinalIgnoreCase)
        || string.IsNullOrWhiteSpace(uri.Host)
        || string.IsNullOrWhiteSpace(uri.UserInfo)
        || string.IsNullOrWhiteSpace(uri.AbsolutePath.Trim('/')))
    {
        throw new InvalidOperationException("Database__ConnectionUri must be a valid mysql:// URI.");
    }

    var credentials = uri.UserInfo.Split(':', 2);
    if (credentials.Length != 2)
    {
        throw new InvalidOperationException("Database__ConnectionUri must include a user and password.");
    }

    var databaseName = string.IsNullOrWhiteSpace(schemaOverride)
        ? uri.AbsolutePath.Trim('/')
        : schemaOverride;

    var connection = new MySqlConnectionStringBuilder
    {
        Server = uri.Host,
        Port = uri.IsDefaultPort ? 3306u : (uint)uri.Port,
        Database = databaseName,
        UserID = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = MySqlSslMode.Required
    };

    return connection.ConnectionString;
}
