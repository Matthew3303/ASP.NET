using Microsoft.EntityFrameworkCore;

namespace PostMatchSummary.Services
{
    public class MatchHistoryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MatchHistoryBackgroundService> _logger;
        private readonly MatchCacheService _cache;

        public MatchHistoryBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MatchHistoryBackgroundService> logger,
            MatchCacheService cache)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cache = cache;
        }

        // Ne radi automatski — samo čeka dok se app ne zaustavi
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MatchHistoryBackgroundService registered (auto-timer disabled).");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public async Task RefreshAllPlayersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var riotService = scope.ServiceProvider.GetRequiredService<RiotService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<PostMatchSummaryDbContext>();

                var players = await dbContext.Players
                    .Where(p => p.RiotId != null && p.RiotId != "")
                    .ToListAsync(cancellationToken);

                _logger.LogInformation($"Refreshing match history for {players.Count} players...");
                int totalNew = 0;

                foreach (var player in players)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var matchIds = await riotService.GetMatchIdsByRiotIdAsync(
                            player.RiotId!, count: 20, cancellationToken: cancellationToken);

                        int newCount = 0;

                        foreach (var matchId in matchIds)
                        {
                            if (await dbContext.Matches.AnyAsync(m => m.MatchId == matchId, cancellationToken))
                                continue;

                            var match = await riotService.GetMatchAsync(matchId, dbContext, cancellationToken);
                            if (match != null)
                            {
                                dbContext.Matches.Add(match);
                                _cache.AddMatch(match);
                                newCount++;
                            }
                        }

                        await dbContext.SaveChangesAsync(cancellationToken);
                        totalNew += newCount;
                        _logger.LogInformation($"{player.SummonerName}: {newCount} new matches");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error for {player.SummonerName}: {ex.Message}");
                    }
                }

                _logger.LogInformation($"Refresh complete. Total new matches: {totalNew}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in background service: {ex.Message}");
            }
        }
    }
}