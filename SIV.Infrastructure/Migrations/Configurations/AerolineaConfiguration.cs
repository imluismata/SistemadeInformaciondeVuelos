using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Catalogo.Domain;

namespace SIV.Infrastructure.Migrations.Configurations;

public class AerolineaConfiguration : IEntityTypeConfiguration<Aerolinea>
{
    public void Configure(EntityTypeBuilder<Aerolinea> builder)
    {
        builder.ToTable("Aerolineas");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Codigo)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(a => a.Codigo).IsUnique();

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Activa).IsRequired();
    }
}
