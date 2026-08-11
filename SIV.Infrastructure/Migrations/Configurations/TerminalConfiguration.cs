using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Catalogo.Domain;

namespace SIV.Infrastructure.Migrations.Configurations;

internal class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("Terminales");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Codigo)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.AeropuertoId).IsRequired();
        builder.Property(t => t.Activa).IsRequired();

        // El código de terminal es único dentro de un aeropuerto (A, B en SDQ).
        builder.HasIndex(t => new { t.AeropuertoId, t.Codigo }).IsUnique();
    }
}
