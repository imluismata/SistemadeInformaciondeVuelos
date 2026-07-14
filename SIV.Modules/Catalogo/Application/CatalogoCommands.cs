namespace SIV.Modules.Catalogo.Application;

public sealed record RegistrarAerolineaCommand(string Codigo, string Nombre);

public sealed record ActualizarAerolineaCommand(string Codigo, string Nombre);

public sealed record DesactivarAerolineaCommand();

public sealed record RegistrarAeropuertoCommand(string Codigo, string Nombre, string Pais);

public sealed record ActualizarAeropuertoCommand(string Codigo, string Nombre, string Pais);

public sealed record DesactivarAeropuertoCommand();

