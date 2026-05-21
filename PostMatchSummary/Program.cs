using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using PostMatchSummary;
using PostMatchSummary.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MatchCacheService>();
builder.Services.AddSingleton<RiotService>();
builder.Services.AddHostedService<MatchHistoryBackgroundService>();

builder.Services.AddDbContext<PostMatchSummaryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PostMatchSummaryDbContext")));

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("hr"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Inicijaliziraj DB i napuni cache iz baze
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostMatchSummaryDbContext>();
    db.Database.Migrate();

    // Obriši sve podatke (Step 1)
    db.Players.RemoveRange(db.Players);
    db.Teams.RemoveRange(db.Teams);
    db.Matches.RemoveRange(db.Matches);
    db.Champions.RemoveRange(db.Champions);
    await db.SaveChangesAsync();

    // Reset IDENTITY seed na 0
    await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Champions', RESEED, 0)");
    await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Players', RESEED, 0)");
    await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Matches', RESEED, 0)");
    await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Teams', RESEED, 0)");

    // Seeduj sve Riot champions (Step 2)
    var riotService = scope.ServiceProvider.GetRequiredService<RiotService>();
    var champions = await riotService.FetchAllChampionsAsync();
    if (champions.Count > 0)
    {
        db.Champions.AddRange(champions);
        await db.SaveChangesAsync();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation($"✅ Seeded {champions.Count} champions from Riot DDragon API");
    }

    var loggerSys = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    loggerSys.LogInformation("✅ Database cleared, IDs reset to 0, and champions seeded on startup.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();