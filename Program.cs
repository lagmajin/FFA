using FFA.Components;
using FFA.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using FFA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GuildService>();
builder.Services.AddScoped<DungeonService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<TownService>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<FieldService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TimeWeatherService>();
builder.Services.AddSingleton<KarmaService>();
builder.Services.AddSingleton<QuestService>();
builder.Services.AddSingleton<DailyRewardService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "FFA.Auth";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = builder.Environment.IsDevelopment() 
        ? SameSiteMode.Lax 
        : SameSiteMode.Strict;
});
// Enable HSTS with recommended production settings
builder.Services.AddHsts(options =>
{
    // 1 year
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
    // Exclude common local development hosts
    options.ExcludedHosts.Add("localhost");
    options.ExcludedHosts.Add("127.0.0.1");
});
// Configure forwarded headers to correctly detect scheme/proxy information when behind a reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // If your proxy is on a known network, add KnownProxies or KnownNetworks entries here to harden
    // options.KnownProxies.Add(IPAddress.Parse("x.x.x.x"));
});
builder.Services.AddHttpClient();
builder.Services.AddHostedService<BatchService>();
builder.Services.AddHostedService<TimeWeatherHostedService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DeveloperExceptionService>();
builder.Services.AddSingleton<AbilityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// HTTPS リダイレクトは環境を問わず有効化（開発でも https プロファイルを使うため）
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    // In development capture exceptions to expose details in UI
    var devEx = app.Services.GetRequiredService<DeveloperExceptionService>();
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            devEx.Set(ex);
            throw;
        }
    });
    app.UseDeveloperExceptionPage();
}

// 静的ファイルの配信を有効化
// Use forwarded headers early so the scheme is correct when using a reverse proxy
app.UseForwardedHeaders();

app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/not-found");

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Minimal API endpoint for sign-in from Blazor components
app.MapPost("/signin", async (HttpContext http, LoginRequest req, UserService userService) =>
{
    if (req == null)
    {
        return Results.BadRequest();
    }
    string username = req.Username;
    string password = req.Password;
    var user = userService.Login(username, password);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        // grant admin role to specific username (simple approach)
        new Claim(ClaimTypes.Role, user.Username == "admin" ? "Admin" : "User"),
        new Claim("Gil", user.Gil.ToString()),
        new Claim("OldCoin", user.OldCoin.ToString()),
        new Claim("Job", user.Job.ToString())
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Ok();
});

// Karma endpoints
app.MapGet("/karma/{username}", (string username, KarmaService karma) =>
{
    var k = karma.GetKarma(username);
    return Results.Ok(k);
});

app.MapGet("/karma", (ClaimsPrincipal user, KarmaService karma) =>
{
    var name = user.Identity?.Name;
    if (string.IsNullOrEmpty(name)) return Results.Unauthorized();
    return Results.Ok(karma.GetKarma(name));
});

app.MapPost("/karma/adjust", (KarmaAdjustRequest req, KarmaService karma) =>
{
    if (req == null || string.IsNullOrEmpty(req.Username)) return Results.BadRequest();
    var val = karma.AdjustKarma(req.Username, req.Delta);
    return Results.Ok(val);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<FFA.Hubs.WorldHub>("/worldhub");

app.MapPost("/signout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.Run();

class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
