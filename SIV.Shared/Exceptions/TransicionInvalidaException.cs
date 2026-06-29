namespace SIV.Shared.Exceptions;

public sealed class TransicionInvalidaException : Exception
{
    public TransicionInvalidaException(string estadoAnterior, string estadoNuevo)
        : base($"La transición desde {estadoAnterior} hacia {estadoNuevo} no es válida.")
    {
    }
}