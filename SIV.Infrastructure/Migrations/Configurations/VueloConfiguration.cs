using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure.Migrations.Configurations;

internal class VueloConfiguration : IEntityTypeConfiguration<Vuelo>
{
    public void Configure(EntityTypeBuilder<Vuelo> builder)
    {
        builder.ToTable("Vuelos");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Numero)
            .IsRequired()
            .HasMaxLength(20);

        // El número NO es único global: la regla del SAD (único por aerolínea y
        // fecha) la aplica VueloService. Se deja un índice compuesto no único para
        // acelerar esa consulta de existencia.
        builder.HasIndex(v => new { v.Numero, v.AerolineaId });

        builder.Property(v => v.AerolineaId).IsRequired();
        builder.Property(v => v.AeropuertoOrigenId).IsRequired();
        builder.Property(v => v.AeropuertoDestinoId).IsRequired();
        builder.Property(v => v.HorarioSalida).IsRequired();
        builder.Property(v => v.HorarioLlegada).IsRequired();
        builder.Property(v => v.Puerta).HasMaxLength(10);

        builder.Property(v => v.EstadoActual)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(v => v.CreadoEn).IsRequired();

        builder.OwnsMany(v => v.HistorialEstados, h =>
        {
            h.ToTable("VueloHistorialEstados");
            h.WithOwner().HasForeignKey(x => x.VueloId);
            h.HasKey(x => x.Id);
            h.Property(x => x.Id).ValueGeneratedNever();
            h.Property(x => x.EstadoAnterior).HasConversion<string>().HasMaxLength(20);
            h.Property(x => x.EstadoNuevo).HasConversion<string>().HasMaxLength(20);
            h.Property(x => x.OcurridoEn).IsRequired();
        });

        builder.Navigation(v => v.HistorialEstados)
            .HasField("_historialEstados")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(v => v.CambiosOperativos, c =>
        {
            c.ToTable("VueloCambiosOperativos");
            c.WithOwner().HasForeignKey(x => x.VueloId);
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).ValueGeneratedNever();
            c.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(30);
            c.Property(x => x.Motivo).IsRequired().HasMaxLength(500);
            c.Property(x => x.ValorAnterior).HasMaxLength(500);
            c.Property(x => x.ValorNuevo).HasMaxLength(500);
            c.Property(x => x.RegistradoEn).IsRequired();
        });

        builder.Navigation(v => v.CambiosOperativos)
            .HasField("_cambiosOperativos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
