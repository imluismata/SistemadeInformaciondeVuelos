namespace SIV.Shared.Contracts;

public interface ISeguimientoConsulta
{
    Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId);

    // Seguidores activos por vuelo, para el reporte de vuelos más seguidos (CU-REP-03).
    Task<IReadOnlyList<SeguidoresPorVuelo>> ContarSeguidoresActivosAsync();
}

public sealed record SeguidoresPorVuelo(Guid VueloId, int Seguidores);
