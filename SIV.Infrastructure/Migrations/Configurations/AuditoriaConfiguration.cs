using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Auditoria.Domain;

namespace SIV.Infrastructure.Migrations.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("Auditoria");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Modulo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Detalle).HasMaxLength(500);
        builder.Property(a => a.Resultado).IsRequired().HasMaxLength(20);
        builder.Property(a => a.FechaHora).IsRequired();

        builder.HasIndex(a => a.Modulo);
        builder.HasIndex(a => a.FechaHora);
    }
}
