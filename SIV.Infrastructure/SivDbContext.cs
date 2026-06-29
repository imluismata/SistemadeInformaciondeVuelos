using Microsoft.EntityFrameworkCore;
using SIV.Modules.Notificaciones.Domain;
using SIV.Modules.Usuarios.Domain;
using SIV.Modules.Vuelos.Domain;
using SeguimientoEntidad = SIV.Modules.Seguimiento.Domain.Seguimiento;

namespace SIV.Infrastructure;

public class SivDbContext(DbContextOptions<SivDbContext> options) : DbContext(options)
{
    // vuelos es de Luis, lo incluyo aqui para que EF lo reconozca
    public DbSet<Vuelo> Vuelos => Set<Vuelo>();

    // tablas de mis modulos
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<SeguimientoEntidad> Seguimientos => Set<SeguimientoEntidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SivDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
