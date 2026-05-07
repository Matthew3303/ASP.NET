using Microsoft.EntityFrameworkCore;
using PostMatchSummary.Models;

namespace PostMatchSummary
{
    public class PostMatchSummaryDbContext : DbContext
    {
        public PostMatchSummaryDbContext(DbContextOptions<PostMatchSummaryDbContext> options)
            : base(options)
        {
        }

        // ★ LAB3 - EF KONFIGURACIJA: DbSet<T> za sve entitete
        public DbSet<Match> Matches { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Champion> Champions { get; set; }

        // ★ LAB3 - EF KONFIGURACIJA: OnModelCreating za fluent API i veze
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Player -> Team (1-N veza): jedan Team ima mnogo Players
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Player -> Champion (1-N veza): jedan Champion ima mnogo Players
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Champion)
                .WithMany(c => c.Players)
                .HasForeignKey(p => p.ChampionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Team -> Match (N-1 veza): jedan Match ima mnogo Teams
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Match)
                .WithMany(m => m.Teams)
                .HasForeignKey(t => t.MatchId)
                .OnDelete(DeleteBehavior.NoAction);

            // Player -> Match (N-1 veza): jedan Match ima mnogo Players
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Match)
                .WithMany(m => m.Players)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index na MatchId u Match tablici
            modelBuilder.Entity<Match>()
                .HasIndex(m => m.MatchId)
                .IsUnique();
        }
    }
}