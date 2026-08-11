namespace SIV.Modules.Catalogo.Application;

public sealed record RegistrarAerolineaCommand(string Codigo, string Nombre);

public sealed record ActualizarAerolineaCommand(string Codigo, string Nombre);

public sealed record DesactivarAerolineaCommand();

public sealed record RegistrarAeropuertoCommand(string Codigo, string Nombre, string Pais);

public sealed record ActualizarAeropuertoCommand(string Codigo, string Nombre, string Pais);

public sealed record DesactivarAeropuertoCommand();

public sealed record RegistrarTerminalCommand(string Codigo, string Nombre, Guid AeropuertoId);

public sealed record ActualizarTerminalCommand(string Codigo, string Nombre);

// TerminalId nulo = puerta remota (rampa abierta).
public sealed record RegistrarPuertaCommand(string Codigo, Guid? TerminalId);

public sealed record ActualizarPuertaCommand(string Codigo, Guid? TerminalId);

