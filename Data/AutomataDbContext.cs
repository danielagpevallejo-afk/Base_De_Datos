using Microsoft.EntityFrameworkCore;
using OllinLexer.Models;

namespace OllinLexer.Data;

public sealed class AutomataDbContext : DbContext
{
    public AutomataDbContext(DbContextOptions<AutomataDbContext> options) : base(options) { }

    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<Simbolo> Simbolos => Set<Simbolo>();
    public DbSet<Transicion> Transiciones => Set<Transicion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(8);
            entity.Property(e => e.Tipo).HasMaxLength(20);
            entity.Property(e => e.Categoria).HasMaxLength(50);
        });
        modelBuilder.Entity<Simbolo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasMaxLength(20);
            entity.HasIndex(e => e.Valor).IsUnique();
        });
        modelBuilder.Entity<Transicion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EstadoOrigenId, e.SimboloId, e.EstadoDestinoId }).IsUnique();
            entity.HasOne(e => e.EstadoOrigen).WithMany().HasForeignKey(e => e.EstadoOrigenId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.EstadoDestino).WithMany().HasForeignKey(e => e.EstadoDestinoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Simbolo).WithMany().HasForeignKey(e => e.SimboloId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
