import { useCallback, useEffect, useState } from 'react'
import { AlertaError } from '../components/AlertaError'
import { EncabezadoApp } from '../components/EncabezadoApp'
import { FormularioActivo } from '../components/FormularioActivo'
import { useAuth } from '../hooks/useAuth'
import { assetApi } from '../services/api'
import type { Activo, ActivoRequest, CategoriaActivo, ResumenPatrimonio } from '../types/activo'
import { moneda } from '../utils/formato'

/** Fecha corta (aaaa-mm-dd) sin pasar por Date, que la correría por zona horaria. */
function fechaCorta(iso: string) {
  const [anio, mes, dia] = iso.slice(0, 10).split('-')
  return `${dia}/${mes}/${anio}`
}

export function GarantiasPage() {
  const { token } = useAuth()

  const [categorias, setCategorias] = useState<CategoriaActivo[]>([])
  const [activos, setActivos] = useState<Activo[]>([])
  const [resumen, setResumen] = useState<ResumenPatrimonio | null>(null)

  const [formularioAbierto, setFormularioAbierto] = useState(false)
  const [enEdicion, setEnEdicion] = useState<Activo | null>(null)
  const [eliminando, setEliminando] = useState<string | null>(null)

  const [cargando, setCargando] = useState(true)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')
  const [errorFormulario, setErrorFormulario] = useState('')

  const cargar = useCallback(async () => {
    if (!token) return

    try {
      const [lista, totales] = await Promise.all([assetApi.listar(token), assetApi.resumen(token)])
      setActivos(lista)
      setResumen(totales)
      setError('')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudieron cargar tus garantías.')
    }
  }, [token])

  useEffect(() => {
    if (!token) return

    let vigente = true

    const cargarTodo = async () => {
      try {
        const catalogo = await assetApi.categorias(token)
        if (vigente) setCategorias(catalogo)
      } catch (e) {
        if (vigente) setError(e instanceof Error ? e.message : 'No se pudo cargar el catálogo.')
      }

      await cargar()
      if (vigente) setCargando(false)
    }

    void cargarTodo()

    return () => {
      vigente = false
    }
  }, [token, cargar])

  function abrirNuevo() {
    setEnEdicion(null)
    setErrorFormulario('')
    setFormularioAbierto(true)
  }

  function abrirEdicion(activo: Activo) {
    setEnEdicion(activo)
    setErrorFormulario('')
    setFormularioAbierto(true)
  }

  async function guardar(datos: ActivoRequest) {
    if (!token) return

    setGuardando(true)
    setErrorFormulario('')

    try {
      if (enEdicion) {
        await assetApi.actualizar(token, enEdicion.id, datos)
      } else {
        await assetApi.crear(token, datos)
      }

      await cargar()
      setFormularioAbierto(false)
      setEnEdicion(null)
    } catch (e) {
      setErrorFormulario(e instanceof Error ? e.message : 'No se pudo guardar la garantía.')
    } finally {
      setGuardando(false)
    }
  }

  async function eliminar(activo: Activo) {
    if (!token) return

    // Confirmación explícita: borrar un bien declarado no tiene vuelta atrás.
    if (!window.confirm(`¿Eliminar "${activo.nombre}"? Esta acción no se puede deshacer.`)) return

    setEliminando(activo.id)

    try {
      await assetApi.eliminar(token, activo.id)
      await cargar()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo eliminar la garantía.')
    } finally {
      setEliminando(null)
    }
  }

  return (
    <div className="min-h-full">
      <EncabezadoApp />

      <div className="border-b border-slate-200 bg-white">
        <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 sm:py-10">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
            Mis garantías
          </h1>
          <p className="mt-2 max-w-2xl text-lg text-slate-600">
            Registra los bienes que respaldan tu solicitud. El simulador mostrará tu patrimonio
            declarado junto al ingreso que exige cada cuota.
          </p>
        </div>
      </div>

      <main className="mx-auto max-w-6xl space-y-6 px-4 py-8 sm:px-6 sm:py-10">
        {error && <AlertaError mensaje={error} />}

        {resumen && resumen.cantidadActivos > 0 && (
          <section className="overflow-hidden rounded-2xl bg-marca-700 text-white shadow-sm">
            <div className="p-6">
              <p className="text-sm text-marca-100">Patrimonio declarado</p>
              <p className="mt-1 text-4xl font-bold tracking-tight tabular-nums">
                {moneda(resumen.valorTotal)}
              </p>
              <p className="mt-1 text-sm text-marca-100">
                {resumen.cantidadActivos} {resumen.cantidadActivos === 1 ? 'bien' : 'bienes'} en{' '}
                {resumen.porCategoria.length}{' '}
                {resumen.porCategoria.length === 1 ? 'categoría' : 'categorías'}
              </p>

              <dl className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {resumen.porCategoria.map((c) => (
                  <div key={c.categoria} className="rounded-lg bg-white/10 px-4 py-3 backdrop-blur-sm">
                    <dt className="text-xs text-marca-100">
                      {c.categoria} · {c.cantidad}
                    </dt>
                    <dd className="mt-0.5 text-lg font-semibold tabular-nums">{moneda(c.valor)}</dd>
                  </div>
                ))}
              </dl>
            </div>
          </section>
        )}

        {formularioAbierto ? (
          <FormularioActivo
            categorias={categorias}
            activo={enEdicion}
            guardando={guardando}
            error={errorFormulario}
            onGuardar={guardar}
            onCancelar={() => {
              setFormularioAbierto(false)
              setEnEdicion(null)
            }}
          />
        ) : (
          <div className="flex justify-end">
            <button
              onClick={abrirNuevo}
              disabled={categorias.length === 0}
              className="inline-flex items-center gap-2 rounded-xl bg-marca-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-marca-700 focus:ring-4 focus:ring-marca-200 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60"
            >
              <svg viewBox="0 0 20 20" className="size-5" fill="currentColor" aria-hidden="true">
                <path d="M10.75 4.75a.75.75 0 0 0-1.5 0v4.5h-4.5a.75.75 0 0 0 0 1.5h4.5v4.5a.75.75 0 0 0 1.5 0v-4.5h4.5a.75.75 0 0 0 0-1.5h-4.5v-4.5Z" />
              </svg>
              Registrar garantía
            </button>
          </div>
        )}

        <section aria-labelledby="titulo-lista">
          <h2 id="titulo-lista" className="text-xl font-bold text-slate-900">
            Bienes registrados
          </h2>

          {cargando ? (
            <div className="mt-4 flex items-center justify-center gap-3 rounded-2xl border border-slate-200 bg-white py-16">
              <span className="size-6 animate-spin rounded-full border-3 border-marca-100 border-t-marca-600" />
              <p className="text-sm text-slate-500">Cargando tus garantías…</p>
            </div>
          ) : activos.length === 0 ? (
            <div className="mt-4 rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
              <p className="font-semibold text-slate-800">Todavía no has registrado ningún bien</p>
              <p className="mx-auto mt-1 max-w-md text-sm text-slate-500">
                Registra un vehículo, un inmueble o cualquier otro activo para construir tu
                respaldo patrimonial.
              </p>
            </div>
          ) : (
            <div className="mt-4 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
              <div className="overflow-x-auto">
                <table className="w-full border-collapse text-sm">
                  <thead className="bg-slate-100 text-xs tracking-wide text-slate-600 uppercase">
                    <tr>
                      <th scope="col" className="px-4 py-3 text-left font-semibold">Bien</th>
                      <th scope="col" className="px-4 py-3 text-left font-semibold">Categoría</th>
                      <th scope="col" className="px-4 py-3 text-right font-semibold">Valor</th>
                      <th scope="col" className="px-4 py-3 text-left font-semibold">Adquirido</th>
                      <th scope="col" className="px-4 py-3 text-right font-semibold">Acciones</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {activos.map((activo) => (
                      <tr key={activo.id} className="transition-colors hover:bg-marca-50/60">
                        <td className="px-4 py-3">
                          <span className="block font-medium text-slate-900">{activo.nombre}</span>
                          {activo.descripcion && (
                            <span className="block text-xs text-slate-500">{activo.descripcion}</span>
                          )}
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap text-slate-600">
                          {activo.categoria.nombre}
                        </td>
                        <td className="px-4 py-3 text-right font-semibold tabular-nums whitespace-nowrap text-slate-900">
                          {moneda(activo.valorEstimado)}
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap text-slate-500">
                          {fechaCorta(activo.fechaAdquisicion)}
                        </td>
                        <td className="px-4 py-3 text-right whitespace-nowrap">
                          <button
                            onClick={() => abrirEdicion(activo)}
                            className="rounded-lg px-3 py-1.5 text-sm font-medium text-marca-700 transition hover:bg-marca-50"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => eliminar(activo)}
                            disabled={eliminando === activo.id}
                            className="rounded-lg px-3 py-1.5 text-sm font-medium text-red-600 transition hover:bg-red-50 disabled:opacity-50"
                          >
                            {eliminando === activo.id ? 'Eliminando…' : 'Eliminar'}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </section>
      </main>

      <footer className="mt-6 border-t border-slate-200 bg-white">
        <p className="mx-auto max-w-6xl px-4 py-6 text-sm text-slate-500 sm:px-6">
          Simulador de Créditos · Universidad Técnica de Ambato
        </p>
      </footer>
    </div>
  )
}
