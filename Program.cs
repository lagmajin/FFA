using FFA.Components;
using FFA.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using FFA.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GuildService>();
builder.Services.AddScoped<DungeonService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<TownService>();
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
        // Recommended cookie settings for same-site browsers and production
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<BatchService>();
builder.Services.AddHostedService<TimeWeatherHostedService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DeveloperExceptionService>();
builder.Services.AddSingleton<AbilityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

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
