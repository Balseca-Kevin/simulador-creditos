using AssetService.Aplicacion.Contratos;
using AssetService.Aplicacion.Dtos;
using AssetService.Dominio;

namespace AssetService.Aplicacion.Servicios;

public interface IServicioActivos
{
    Task<IReadOnlyList<CategoriaActivoResponse>> Categorias();
    Task<IReadOnlyList<ActivoResponse>> Listar(Guid usuarioId);
    Task<Resultado<ActivoResponse>> Buscar(Guid id, Guid usuarioId);
    Task<Resultado<ActivoResponse>> Crear(Guid usuarioId, ActivoRequest solicitud);
    Task<Resultado<ActivoResponse>> Actualizar(Guid id, Guid usuarioId, ActivoRequest solicitud);
    Task<Resultado<bool>> Eliminar(Guid id, Guid usuarioId);
    Task<ResumenPatrimonioResponse> Resumen(Guid usuarioId);
}

/// <summary>
/// Casos de uso de los activos declarados: consultar el catálogo de categorías,
/// registrar, modificar y eliminar bienes, y resumir el patrimonio.
///
/// Cada operación recibe el usuario que la solicita y el repositorio filtra por
/// él, de modo que un identificador ajeno no devuelve nada en lugar de devolver
/// el bien de otra persona.
/// </summary>
public class ServicioActivos(IRepositorioActivos repositorio) : IServicioActivos
{
    public async Task<IReadOnlyList<CategoriaActivoResponse>> Categorias()
    {
        var categorias = await repositorio.ListarCategorias();
        return [.. categorias.Select(ProyectarCategoria)];
    }

    public async Task<IReadOnlyList<ActivoResponse>> Listar(Guid usuarioId)
    {
        var activos = await repositorio.ListarDelUsuario(usuarioId);
        return [.. activos.Select(Proyectar)];
    }

    public async Task<Resultado<ActivoResponse>> Buscar(Guid id, Guid usuarioId)
    {
        var activo = await repositorio.Buscar(id, usuarioId);

        return activo is null
            ? NoEncontrado<ActivoResponse>()
            : Resultado<ActivoResponse>.Ok(Proyectar(activo));
    }

    public async Task<Resultado<ActivoResponse>> Crear(Guid usuarioId, ActivoRequest solicitud)
    {
        if (!await repositorio.ExisteCategoria(solicitud.CategoriaId))
        {
            return Resultado<ActivoResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                "La categoría seleccionada no existe o no está disponible.");
        }

        if (EsFutura(solicitud.FechaAdquisicion))
        {
            return Resultado<ActivoResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                "La fecha de adquisición no puede estar en el futuro.");
        }

        var activo = new Activo
        {
            UsuarioId = usuarioId,
            CategoriaId = solicitud.CategoriaId,
            Nombre = solicitud.Nombre.Trim(),
            Descripcion = solicitud.Descripcion.Trim(),
            ValorEstimado = solicitud.ValorEstimado,
            FechaAdquisicion = solicitud.FechaAdquisicion
        };

        await repositorio.Agregar(activo);

        // Se relee para devolverlo con su categoría cargada.
        var guardado = await repositorio.Buscar(activo.Id, usuarioId);

        return Resultado<ActivoResponse>.Ok(Proyectar(guardado ?? activo));
    }

    public async Task<Resultado<ActivoResponse>> Actualizar(Guid id, Guid usuarioId, ActivoRequest solicitud)
    {
        var activo = await repositorio.Buscar(id, usuarioId);

        if (activo is null) return NoEncontrado<ActivoResponse>();

        if (!await repositorio.ExisteCategoria(solicitud.CategoriaId))
        {
            return Resultado<ActivoResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                "La categoría seleccionada no existe o no está disponible.");
        }

        if (EsFutura(solicitud.FechaAdquisicion))
        {
            return Resultado<ActivoResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                "La fecha de adquisición no puede estar en el futuro.");
        }

        activo.CategoriaId = solicitud.CategoriaId;
        activo.Nombre = solicitud.Nombre.Trim();
        activo.Descripcion = solicitud.Descripcion.Trim();
        activo.ValorEstimado = solicitud.ValorEstimado;
        activo.FechaAdquisicion = solicitud.FechaAdquisicion;
        activo.FechaActualizacion = DateTime.UtcNow;

        await repositorio.Actualizar(activo);

        var guardado = await repositorio.Buscar(id, usuarioId);

        return Resultado<ActivoResponse>.Ok(Proyectar(guardado ?? activo));
    }

    public async Task<Resultado<bool>> Eliminar(Guid id, Guid usuarioId)
    {
        var activo = await repositorio.Buscar(id, usuarioId);

        if (activo is null) return NoEncontrado<bool>();

        await repositorio.Eliminar(activo);

        return Resultado<bool>.Ok(true);
    }

    public async Task<ResumenPatrimonioResponse> Resumen(Guid usuarioId)
    {
        var activos = await repositorio.ListarDelUsuario(usuarioId);

        var porCategoria = activos
            .GroupBy(a => a.Categoria?.Nombre ?? "Sin categoría")
            .Select(g => new PatrimonioPorCategoria
            {
                Categoria = g.Key,
                Cantidad = g.Count(),
                Valor = g.Sum(a => a.ValorEstimado)
            })
            .OrderByDescending(g => g.Valor)
            .ToList();

        return new ResumenPatrimonioResponse
        {
            CantidadActivos = activos.Count,
            ValorTotal = activos.Sum(a => a.ValorEstimado),
            PorCategoria = porCategoria
        };
    }

    /// <summary>Un bien no puede haberse adquirido en una fecha que todavía no llega.</summary>
    private static bool EsFutura(DateOnly fecha) => fecha > DateOnly.FromDateTime(DateTime.UtcNow);

    private static Resultado<T> NoEncontrado<T>() =>
        Resultado<T>.Fallo(MotivoFallo.NoEncontrado, "El activo no existe o no pertenece a tu cuenta.");

    private static ActivoResponse Proyectar(Activo activo) => new()
    {
        Id = activo.Id,
        Categoria = activo.Categoria is null
            ? new CategoriaActivoResponse { Id = activo.CategoriaId, Codigo = "", Nombre = "", Descripcion = "" }
            : ProyectarCategoria(activo.Categoria),
        Nombre = activo.Nombre,
        Descripcion = activo.Descripcion,
        ValorEstimado = activo.ValorEstimado,
        FechaAdquisicion = activo.FechaAdquisicion,
        FechaRegistro = activo.FechaRegistro,
        FechaActualizacion = activo.FechaActualizacion
    };

    private static CategoriaActivoResponse ProyectarCategoria(CategoriaActivo categoria) => new()
    {
        Id = categoria.Id,
        Codigo = categoria.Codigo,
        Nombre = categoria.Nombre,
        Descripcion = categoria.Descripcion
    };
}
