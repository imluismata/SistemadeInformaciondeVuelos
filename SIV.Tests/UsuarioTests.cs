using SIV.Modules.Usuarios.Domain;
using Xunit;

namespace SIV.Tests;

/// <summary>
/// Pruebas unitarias de la entidad de dominio Usuario. No necesitan base de datos
/// ni mocks: se crea el objeto, se ejecuta un método y se comprueba el resultado
/// (patrón Arrange - Act - Assert).
/// </summary>
public class UsuarioTests
{
    // ---------- Creación ----------

    [Fact]
    public void Crear_ConDatosValidos_CreaUsuarioNoVerificado()
    {
        // Arrange & Act
        var usuario = Usuario.Crear("Ana Pérez", "ANA@Test.com ", "hash");

        // Assert
        Assert.Equal("Ana Pérez", usuario.Nombre);
        Assert.Equal("ana@test.com", usuario.Email); // se normaliza a minúsculas y sin espacios
        Assert.Equal(RolUsuario.UsuarioRegistrado, usuario.Rol);
        Assert.False(usuario.EmailConfirmado);        // recién creado, aún sin verificar
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Theory]
    [InlineData("", "ana@test.com", "hash")]   // nombre vacío
    [InlineData("Ana", "", "hash")]            // email vacío
    [InlineData("Ana", "ana@test.com", "")]    // hash vacío
    public void Crear_ConCampoObligatorioVacio_LanzaExcepcion(string nombre, string email, string hash)
    {
        Assert.Throws<ArgumentException>(() => Usuario.Crear(nombre, email, hash));
    }

    // ---------- Verificación de correo ----------

    [Fact]
    public void ConfirmarEmail_ConCodigoCorrecto_MarcaComoConfirmado()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hash");
        var codigo = usuario.GenerarCodigoVerificacion();

        usuario.ConfirmarEmail(codigo);

        Assert.True(usuario.EmailConfirmado);
    }

    [Fact]
    public void ConfirmarEmail_ConCodigoIncorrecto_LanzaExcepcionYNoConfirma()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hash");
        usuario.GenerarCodigoVerificacion();

        Assert.Throws<ArgumentException>(() => usuario.ConfirmarEmail("000000"));
        Assert.False(usuario.EmailConfirmado);
    }

    [Fact]
    public void ConfirmarEmail_SinCodigoGenerado_LanzaExcepcion()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hash");

        // No se generó ningún código, así que confirmar debe fallar.
        Assert.Throws<ArgumentException>(() => usuario.ConfirmarEmail("123456"));
    }

    [Fact]
    public void GenerarCodigoVerificacion_DevuelveSeisDigitos()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hash");

        var codigo = usuario.GenerarCodigoVerificacion();

        Assert.Matches(@"^\d{6}$", codigo);
    }

    // ---------- Recuperación de contraseña ----------

    [Fact]
    public void RestablecerPassword_ConCodigoValido_CambiaElHash()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hashViejo");
        var codigo = usuario.GenerarCodigoRecuperacion();

        usuario.RestablecerPassword(codigo, "hashNuevo");

        Assert.Equal("hashNuevo", usuario.PasswordHash);
    }

    [Fact]
    public void RestablecerPassword_ConCodigoInvalido_NoCambiaElHash()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hashViejo");
        usuario.GenerarCodigoRecuperacion();

        Assert.Throws<ArgumentException>(() => usuario.RestablecerPassword("999999", "hashNuevo"));
        Assert.Equal("hashViejo", usuario.PasswordHash); // el hash no debe cambiar si el código es inválido
    }

    // ---------- Roles y perfil ----------

    [Fact]
    public void CambiarRol_ActualizaElRol()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hash");

        usuario.CambiarRol(RolUsuario.Administrador);

        Assert.Equal(RolUsuario.Administrador, usuario.Rol);
    }

    [Fact]
    public void ActualizarNombre_ConValorVacio_LanzaExcepcion()
    {
        var usuario = Usuario.Crear("Ana", "ana@test.com", "hash");

        Assert.Throws<ArgumentException>(() => usuario.ActualizarNombre("   "));
    }
}
