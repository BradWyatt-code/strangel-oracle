using Microsoft.EntityFrameworkCore;
using StrangelOracle.Domain.Entities;

namespace StrangelOracle.Infrastructure.Data;

public class StrangelDbContext : DbContext
{
    public StrangelDbContext(DbContextOptions<StrangelDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<SoulLedgerEntry> SoulLedger => Set<SoulLedgerEntry>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoulLedgerEntry>(entity =>
        {
            entity.ToTable("soul_ledger");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
                
            entity.Property(e => e.SessionId)
                .HasColumnName("session_id")
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.Strangel)
                .HasColumnName("strangel")
                .HasConversion<string>()
                .HasMaxLength(50);
                
            entity.Property(e => e.Petition)
                .HasColumnName("petition")
                .HasMaxLength(1000);
                
            entity.Property(e => e.Response)
                .HasColumnName("response")
                .HasMaxLength(2000)
                .IsRequired();
                
            entity.Property(e => e.Outcome)
                .HasColumnName("outcome")
                .HasConversion<string>()
                .HasMaxLength(50);
                
            entity.Property(e => e.Intensity)
                .HasColumnName("intensity");
                
            entity.Property(e => e.BestowedAt)
                .HasColumnName("bestowed_at");
                
            // Index for querying a seeker's history
            entity.HasIndex(e => e.SessionId)
                .HasDatabaseName("ix_soul_ledger_session");
                
            // Index for Strangel-specific queries
            entity.HasIndex(e => e.Strangel)
                .HasDatabaseName("ix_soul_ledger_strangel");
        });
    }
}

