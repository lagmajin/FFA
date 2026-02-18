using FFA.Components;
using FFA.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using FFA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;

// スキル習得リクエスト型
// LearnSkillRequest moved to Models/LearnSkillRequest.cs to avoid top-level declaration ordering issues

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor services for UI components
builder.Services.AddMudServices();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GuildService>();
builder.Services.AddScoped<GuildEnhancementService>();
builder.Services.AddSingleton<WorldService>();
builder.Services.AddSingleton<StaminaService>();
builder.Services.AddSingleton<ConsumableItemService>();
builder.Services.AddScoped<DungeonService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<TownService>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<FieldService>();
builder.Services.AddScoped<MarketService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TimeWeatherService>();
builder.Services.AddSingleton<KarmaService>();
builder.Services.AddSingleton<QuestService>();
builder.Services.AddSingleton<DailyRewardService>();
// Register RockSmashService
builder.Services.AddScoped<RockSmashService>();
// Register AuctionService and CountryWarService
builder.Services.AddScoped<AuctionService>();
builder.Services.AddSingleton<CountryWarService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "FFA.Auth";
        options.Cookie.SameSite = builder.Environment.IsDevelopment() 
            ? SameSiteMode.Lax 
            : SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
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
builder.Services.AddSingleton<ChatService>();
builder.Services.AddScoped<ExplorationService>();
builder.Services.AddScoped<CaravanService>();
builder.Services.AddScoped<WorldEventService>();
builder.Services.AddScoped<CompanionService>();
builder.Services.AddScoped<InstanceService>();
builder.Services.AddScoped<IdlenessService>();
builder.Services.AddSingleton<MonsterService>();
builder.Services.AddSingleton<NotoriousMonsterService>();

// In development bind to standard HTTPS port 443 so https://localhost resolves
if (builder.Environment.IsDevelopment())
{
    try
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            // Listen on 0.0.0.0:443 or localhost:443 depending on environment
            options.ListenLocalhost(443, listenOptions => listenOptions.UseHttps());
        });
    }
    catch
    {
        // Ignore failures here (likely due to permissions or port in use); app will still use configured endpoints
    }
}

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

// Guild Enhancement API endpoints
app.MapGet("/guild/info", (ClaimsPrincipal user, GuildService guildService, GuildEnhancementService guildEnhancement) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    var userService = new UserService();
    var userEntity = userService.GetByUsername(username);
    if (userEntity == null || userEntity.GuildId == null) return Results.NotFound("ギルドに参加していません");
    
    var guild = guildService.GetUserGuild(userEntity);
    if (guild == null) return Results.NotFound("ギルドが見つかりません");
    
    var info = guildEnhancement.GetGuildInfo(guild);
    return Results.Ok(info);
});

app.MapGet("/guild/skills", (GuildEnhancementService guildEnhancement) =>
{
    return Results.Ok(GuildEnhancementService.GuildSkills);
});

app.MapPost("/guild/skill/learn", (LearnSkillRequest req, ClaimsPrincipal user, GuildService guildService, GuildEnhancementService guildEnhancement) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    var userService = new UserService();
    var userEntity = userService.GetByUsername(username);
    if (userEntity == null || userEntity.GuildId == null) return Results.NotFound("ギルドに参加していません");
    
    var guild = guildService.GetUserGuild(userEntity);
    if (guild == null) return Results.NotFound("ギルドが見つかりません");
    
    var result = guildEnhancement.AddGuildSkill(guild, req.SkillId);
    return Results.Ok(result);
});

// World/Map API endpoints
app.MapGet("/world/map", (ClaimsPrincipal user, WorldService worldService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var mapInfo = worldService.GetWorldMapInfo(username);
    return Results.Ok(mapInfo);
});

app.MapGet("/world/surroundings", (ClaimsPrincipal user, WorldService worldService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var surroundings = worldService.GetSurroundings(username);
    return Results.Ok(surroundings);
});

app.MapGet("/world/location", (ClaimsPrincipal user, WorldService worldService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var locationName = worldService.GetCurrentLocationName(username);
    return Results.Ok(new { Location = locationName });
});

app.MapPost("/world/move/{direction}", (string direction, ClaimsPrincipal user, WorldService worldService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var result = worldService.MovePlayer(username, direction);
    return Results.Ok(result);
});

// Stamina API endpoints
app.MapGet("/stamina", (ClaimsPrincipal user, StaminaService staminaService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var info = staminaService.GetStaminaInfo(username);
    return Results.Ok(info);
});

app.MapGet("/stamina/{type}", (string type, ClaimsPrincipal user, StaminaService staminaService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    if (!Enum.TryParse<FFA.Models.StaminaType>(type, true, out var staminaType))
        return Results.BadRequest("無効なスタミナタイプです");
    
    var status = staminaService.GetStaminaStatus(username, staminaType);
    return Results.Ok(status);
});

app.MapPost("/stamina/use/{type}", (string type, ClaimsPrincipal user, StaminaService staminaService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    if (!Enum.TryParse<FFA.Models.StaminaType>(type, true, out var staminaType))
        return Results.BadRequest("無効なスタミナタイプです");
    
    var result = staminaService.UseStamina(username, staminaType);
    return Results.Ok(result);
});

app.MapPost("/stamina/recover/{type}", (string type, ClaimsPrincipal user, StaminaService staminaService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    if (!Enum.TryParse<FFA.Models.StaminaType>(type, true, out var staminaType))
        return Results.BadRequest("無効なスタミナタイプです");
    
    // 回復量は固定（アイテム等で使用）
    var result = staminaService.RecoverStaminaItem(username, staminaType, 20);
    return Results.Ok(result);
});

// ConsumableItem API endpoints
app.MapGet("/items/consumable", (ConsumableItemService itemService) =>
{
    return Results.Ok(ConsumableItemService.ConsumableItems);
});

app.MapGet("/items/consumable/{id}", (int id, ConsumableItemService itemService) =>
{
    var item = itemService.GetItem(id);
    if (item == null) return Results.NotFound("アイテムが見つかりません");
    return Results.Ok(item);
});

app.MapPost("/items/use/{itemId}", (int itemId, ClaimsPrincipal user, ConsumableItemService itemService, UserService userService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var userEntity = userService.GetByUsername(username);
    if (userEntity == null) return Results.NotFound("ユーザーが見つかりません");
    
    var result = itemService.UseItem(userEntity, itemId);
    if (result.Success)
    {
        userService.UpdateUser(userEntity);
    }
    return Results.Ok(result);
});

app.MapGet("/items/buffs", (ClaimsPrincipal user, ConsumableItemService itemService) =>
{
    var username = user.Identity?.Name;
    if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
    
    var buffs = itemService.GetActiveBuffs(username);
    return Results.Ok(buffs);
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

// GET signout for direct browser navigation (fallback)
app.MapGet("/signout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// Admin API: list users (sanitized)
app.MapGet("/admin/users", (ClaimsPrincipal user, UserService userService) =>
{
    if (!user.IsInRole("Admin")) return Results.Unauthorized();
    var list = userService.GetAllUsers()
        .Select(u => new {
            u.Username,
            u.Level,
            u.Gil,
            u.IsChampion,
            Job = u.Job.ToString()
        })
        .ToList();
    return Results.Ok(list);
});

// Admin API: delete user
app.MapPost("/admin/user/delete", async (HttpContext http, UserService userService) =>
{
    if (!http.User.IsInRole("Admin")) return Results.Unauthorized();
    var body = await http.Request.ReadFromJsonAsync<Dictionary<string, string>>();
    if (body == null || !body.TryGetValue("username", out var username) || string.IsNullOrEmpty(username))
        return Results.BadRequest();
    var ok = userService.DeleteUser(username);
    return ok ? Results.Ok() : Results.NotFound();
});

// Admin API: set champion (make username the champion)
app.MapPost("/admin/user/setchampion", async (HttpContext http, UserService userService) =>
{
    if (!http.User.IsInRole("Admin")) return Results.Unauthorized();
    var body = await http.Request.ReadFromJsonAsync<Dictionary<string, string>>();
    if (body == null || !body.TryGetValue("username", out var username) || string.IsNullOrEmpty(username))
        return Results.BadRequest();

    // clear previous champions
    foreach (var u in userService.GetAllUsers())
    {
        if (u.IsChampion)
        {
            u.IsChampion = false;
            userService.UpdateUser(u);
        }
    }

    var newChampion = userService.GetByUsername(username);
    if (newChampion == null) return Results.NotFound();
    newChampion.IsChampion = true;
    userService.UpdateUser(newChampion);
    return Results.Ok();
});

// Ensure there is an initial champion (CPU) if none exists
try
{
    var userService = new UserService();
    var all = userService.GetAllUsers().ToList();
    var existingChampion = all.FirstOrDefault(u => u.IsChampion);
    if (existingChampion == null)
    {
        // create or mark a CPU champion user
        var cpu = all.FirstOrDefault(u => u.Username == "CPU_Champion");
        if (cpu == null)
        {
            userService.Register("CPU_Champion", Guid.NewGuid().ToString(), Job.Warrior);
            cpu = userService.GetByUsername("CPU_Champion");
        }
        if (cpu != null)
        {
            cpu.IsChampion = true;
            userService.UpdateUser(cpu);
            Console.WriteLine("Initialized CPU_Champion as Sky Arena champion");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Champion initialization failed: {ex.Message}");
}

// Seed monster templates
try
{
    var monsterService = app.Services.GetRequiredService<MonsterService>();
    monsterService.SeedDefaultTemplates();
}
catch (Exception ex)
{
    Console.WriteLine($"Monster seeding failed: {ex.Message}");
}

// Seed NM defaults
try
{
    var nmService = app.Services.GetRequiredService<NotoriousMonsterService>();
    nmService.SeedDefaults();
}
catch (Exception ex)
{
    Console.WriteLine($"NM seeding failed: {ex.Message}");
}

app.Run();

class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
