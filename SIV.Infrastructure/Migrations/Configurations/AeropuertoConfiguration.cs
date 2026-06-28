using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Catalogo.Domain;

namespace SIV.Infrastructure.Migrations.Configurations;

public class AeropuertoConfiguration : IEntityTypeConfiguration<Aeropuerto>
{
    public void Configure(EntityTypeBuilder<Aeropuerto> builder)
    {
        builder.ToTable("Aeropuertos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Codigo)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(a => a.Codigo).IsUnique();

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Pais)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Activo).IsRequired();
    }
}
