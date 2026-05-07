using Microsoft.EntityFrameworkCore;
using PostMatchSummary;
using PostMatchSummary.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MatchCacheService>();
builder.Services.AddSingleton<RiotService>();

// ← JEDINO NOVO
builder.Services.AddDbContext<PostMatchSummaryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PostMatchSummaryDbContext")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostMatchSummaryDbContext>();
    db.Database.Migrate();

    var cache = scope.ServiceProvider.GetRequiredService<MatchCacheService>();
    var riot = scope.ServiceProvider.GetRequiredService<RiotService>();
    try { await cache.InitializeAsync(riot); }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning($"Greška pri inicijalizaciji cache-a: {ex.Message}");
    }
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