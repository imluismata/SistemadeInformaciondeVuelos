using Microsoft.EntityFrameworkCore;
using SIV.Infrastructure.Migrations.Configurations;
using SIV.Modules.Catalogo.Domain;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure;

public class SivDbContext(DbContextOptions<SivDbContext> options) : DbContext(options)
{
    public DbSet<Vuelo> Vuelos => Set<Vuelo>();
    public DbSet<Aerolinea> Aerolineas => Set<Aerolinea>();
    public DbSet<Aeropuerto> Aeropuertos => Set<Aeropuerto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VueloConfiguration());
        modelBuilder.ApplyConfiguration(new AerolineaConfiguration());
        modelBuilder.ApplyConfiguration(new AeropuertoConfiguration());
    }
}
