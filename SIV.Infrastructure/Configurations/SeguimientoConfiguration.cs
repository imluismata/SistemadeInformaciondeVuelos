using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeguimientoEntidad = SIV.Modules.Seguimiento.Domain.Seguimiento;

namespace SIV.Infrastructure.Configurations;

public class SeguimientoConfiguration : IEntityTypeConfiguration<SeguimientoEntidad>
{
    public void Configure(EntityTypeBuilder<SeguimientoEntidad> builder)
    {
        builder.ToTable("Seguimientos");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UsuarioId)
            .IsRequired();

        builder.Property(s => s.VueloId)
            .IsRequired();

        builder.Property(s => s.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.CreadoEn)
            .IsRequired();

        builder.Property(s => s.CanceladoEn)
            .IsRequired(false);

        builder.HasIndex(s => new { s.UsuarioId, s.VueloId })
            .HasDatabaseName("IX_Seguimientos_UsuarioId_VueloId");
    }
}
