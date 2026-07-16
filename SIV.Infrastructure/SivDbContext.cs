using Microsoft.EntityFrameworkCore;
using SIV.Infrastructure.Migrations.Configurations;
using SIV.Modules.Auditoria.Domain;
using SIV.Modules.Catalogo.Domain;
using SIV.Modules.Notificaciones.Domain;
using SIV.Modules.Usuarios.Domain;
using SIV.Modules.Vuelos.Domain;
using SeguimientoEntidad = SIV.Modules.Seguimiento.Domain.Seguimiento;

namespace SIV.Infrastructure;

public class SivDbContext(DbContextOptions<SivDbContext> options) : DbContext(options)
{
    internal DbSet<Vuelo> Vuelos => Set<Vuelo>();
    internal DbSet<Aerolinea> Aerolineas => Set<Aerolinea>();
    internal DbSet<Aeropuerto> Aeropuertos => Set<Aeropuerto>();
    internal DbSet<RegistroAuditoria> Auditoria => Set<RegistroAuditoria>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<SeguimientoEntidad> Seguimientos => Set<SeguimientoEntidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SivDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
