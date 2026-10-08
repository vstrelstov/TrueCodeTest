using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Entities;

namespace TrueCodeTest.Shared.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Currency> Currencies => Set<Currency>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserFavoriteCurrency> UserFavoriteCurrencies => Set<UserFavoriteCurrency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currency");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            // numeric(18, 6) покрывает даже самые мелкие курсы ЦБ, например
            // IRR, который публикуется как 0,0000486957 за единицу.
            entity.Property(x => x.Rate)
                .HasColumnName("rate")
                .HasPrecision(18, 6)
                .IsRequired();

            // Обновление курсов ищет валюту по названию, поэтому название
            // должно быть уникальным — иначе upsert не определён однозначно.
            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("ix_currency_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Password)
                .HasColumnName("password")
                .HasMaxLength(256)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("ix_user_name");
        });

        modelBuilder.Entity<UserFavoriteCurrency>(entity =>
        {
            entity.ToTable("user_favorite_currency");

            entity.HasKey(x => new { x.UserId, x.CurrencyId });

            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.CurrencyId).HasColumnName("currency_id");

            entity.HasOne(x => x.User)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Currency)
                .WithMany(x => x.FavoritedBy)
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.CurrencyId)
                .HasDatabaseName("ix_user_favorite_currency_currency_id");
        });

        base.OnModelCreating(modelBuilder);
    }
}
