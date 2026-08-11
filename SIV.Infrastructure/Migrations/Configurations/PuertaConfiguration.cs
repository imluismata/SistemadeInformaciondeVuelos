using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Catalogo.Domain;

namespace SIV.Infrastructure.Migrations.Configurations;

internal class PuertaConfiguration : IEntityTypeConfiguration<Puerta>
{
    public void Configure(EntityTypeBuilder<Puerta> builder)
    {
        builder.ToTable("Puertas");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => p.Codigo).IsUnique();

        // TerminalId es nullable: una puerta sin terminal es una rampa abierta.
        builder.Property(p => p.TerminalId);
        builder.Property(p => p.Activa).IsRequired();

        // EsRampa es una propiedad derivada (TerminalId == null); no se persiste.
        builder.Ignore(p => p.EsRampa);
    }
}
