using PostMatchSummary.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<RiotService>();
builder.Services.AddSingleton<MatchCacheService>();

var app = builder.Build();

// Inicijalizacija cache-a pri startu aplikacije
using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<MatchCacheService>();
    var riot = scope.ServiceProvider.GetRequiredService<RiotService>();
    try
    {
        await cache.InitializeAsync(riot);
    }
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();