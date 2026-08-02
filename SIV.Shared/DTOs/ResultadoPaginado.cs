namespace SIV.Shared.DTOs;

// Esto es lo que devuelvo cuando piden una pagina: los items de esa pagina y
// el total que hay, para saber cuantas paginas son en total. Asi no mando toda
// la tabla de una sola vez.
public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Pagina,
    int Tamano);
