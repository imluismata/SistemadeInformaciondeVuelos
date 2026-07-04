using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Notificaciones.Domain;

namespace SIV.Infrastructure.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UsuarioId)
            .IsRequired();

        builder.Property(n => n.VueloId)
            .IsRequired();

        builder.Property(n => n.Mensaje)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue("NoLeida");

        builder.Property(n => n.GeneradaEn)
            .IsRequired();

        builder.Property(n => n.LeidaEn)
            .IsRequired(false);

        builder.HasIndex(n => n.UsuarioId)
            .HasDatabaseName("IX_Notificaciones_UsuarioId");
    }
}