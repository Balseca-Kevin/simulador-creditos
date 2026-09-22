using AuthService.Aplicacion.Contratos;
using AuthService.Aplicacion.Dtos;
using AuthService.Dominio;

namespace AuthService.Aplicacion.Servicios;

public interface IServicioAutenticacion
{
    Task<Resultado<AuthResponse>> Registrar(RegistroRequest solicitud);
    Task<Resultado<AuthResponse>> Login(LoginRequest solicitud);
    Task<Resultado<UsuarioResponse>> Perfil(Guid usuarioId);
}

/// <summary>
/// Casos de uso de identidad: registro, inicio de sesión y consulta de perfil.
/// Concentra aquí las reglas que antes vivían en el controlador, de modo que la
/// capa de presentación solo traduzca entre HTTP y estos casos de uso.
/// </summary>
public class ServicioAutenticacion(IRepositorioUsuarios repositorio, ITokenService tokenService)
    : IServicioAutenticacion
{
    public async Task<Resultado<AuthResponse>> Registrar(RegistroRequest solicitud)
    {
        var email = Normalizar(solicitud.Email);

        if (await repositorio.ExisteConEmail(email))
        {
            return Resultado<AuthResponse>.Fallo(
                MotivoFallo.Conflicto,
                "Ya existe una cuenta registrada con ese correo.");
        }

        var usuario = new Usuario
        {
            NombreCompleto = solicitud.NombreCompleto.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(solicitud.Password)
        };

        await repositorio.Agregar(usuario);

        return Resultado<AuthResponse>.Ok(ConstruirRespuesta(usuario));
    }

    public async Task<Resultado<AuthResponse>> Login(LoginRequest solicitud)
    {
        var usuario = await repositorio.BuscarPorEmail(Normalizar(solicitud.Email));

        // Se responde igual si el correo no existe o si la contraseña es incorrecta,
        // para no revelar qué correos están registrados.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(solicitud.Password, usuario.PasswordHash))
        {
            return Resultado<AuthResponse>.Fallo(
                MotivoFallo.NoAutorizado,
                "Correo o contraseña incorrectos.");
        }

        return Resultado<AuthResponse>.Ok(ConstruirRespuesta(usuario));
    }

    public async Task<Resultado<UsuarioResponse>> Perfil(Guid usuarioId)
    {
        var usuario = await repositorio.BuscarPorId(usuarioId);

        // El token es válido pero su titular ya no existe: la sesión no sirve.
        return usuario is null
            ? Resultado<UsuarioResponse>.Fallo(MotivoFallo.NoAutorizado, "La sesión ya no es válida.")
            : Resultado<UsuarioResponse>.Ok(Proyectar(usuario));
    }

    private static string Normalizar(string email) => email.Trim().ToLowerInvariant();

    private AuthResponse ConstruirRespuesta(Usuario usuario)
    {
        var (token, expiraEn) = tokenService.Generar(usuario);

        return new AuthResponse
        {
            Token = token,
            ExpiraEn = expiraEn,
            Usuario = Proyectar(usuario)
        };
    }

    private static UsuarioResponse Proyectar(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NombreCompleto = usuario.NombreCompleto,
        Email = usuario.Email,
        FechaRegistro = usuario.FechaRegistro
    };
}
