import { useCallback, useEffect, useRef, useState } from 'react'
import { AlertaError } from '../components/AlertaError'
import { FormularioSimulacion } from '../components/FormularioSimulacion'
import { HistorialSimulaciones } from '../components/HistorialSimulaciones'
import { ResumenSimulacion } from '../components/ResumenSimulacion'
import { TablaAmortizacion } from '../components/TablaAmortizacion'
import { useAuth } from '../hooks/useAuth'
import { creditApi } from '../services/api'
import type { Simulacion, SimulacionHistorial, SimulacionRequest, TipoCredito } from '../types/credito'

export function SimuladorPage() {
  const { usuario, token, cerrarSesion } = useAuth()

  const [tipos, setTipos] = useState<TipoCredito[]>([])
  const [historial, setHistorial] = useState<SimulacionHistorial[]>([])
  const [simulacion, setSimulacion] = useState<Simulacion | null>(null)

  const [cargandoCatalogo, setCargandoCatalogo] = useState(true)
  const [simulando, setSimulando] = useState(false)
  const [error, setError] = useState('')

  const resultadosRef = useRef<HTMLDivElement>(null)

  const cargarHistorial = useCallback(async () => {
    if (!token) return
    try {
      setHistorial(await creditApi.historial(token))
    } catch {
      // El historial es complementario: si falla, la simulación sigue siendo utilizable.
    }
  }, [token])

  // Catálogo e historial se piden una sola vez, al entrar.
  useEffect(() => {
    if (!token) return

    let vigente = true

    const cargarDatosIniciales = async () => {
      try {
        const catalogo = await creditApi.tipos(token)
        if (vigente) setTipos(catalogo)
      } catch (e) {
        if (vigente) setError(e instanceof Error ? e.message : 'No se pudo cargar el catálogo.')
      } finally {
        if (vigente) setCargandoCatalogo(false)
      }

      await cargarHistorial()
    }

    void cargarDatosIniciales()

    return () => {
      vigente = false
    }
  }, [token, cargarHistorial])

  async function simular(datos: SimulacionRequest) {
    if (!token) return

    setError('')
    setSimulando(true)

    try {
      const resultado = await creditApi.simular(token, datos)
      setSimulacion(resultado)
      void cargarHistorial()

      // Lleva la vista a los resultados: en móvil quedan debajo del formulario.
      requestAnimationFrame(() =>
        resultadosRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' }),
      )
    } catch (e) {
      setSimulacion(null)
      setError(e instanceof Error ? e.message : 'No se pudo realizar la simulación.')
    } finally {
      setSimulando(false)
    }
  }

  const alemanGanador = simulacion?.comparativo.metodoMasEconomico === 'Aleman'

  return (
    <div className="min-h-full bg-slate-50">
      <header className="sticky top-0 z-20 border-b border-slate-200 bg-white/90 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-3.5">
          <div>
            <p className="text-xs font-semibold tracking-widest text-marca-600 uppercase">
              Simulador de Créditos
            </p>
            <h1 className="text-base font-semibold text-slate-900">
              Hola, {usuario?.nombreCompleto}
            </h1>
          </div>

          <button
            onClick={cerrarSesion}
            className="shrink-0 rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100"
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-6xl space-y-6 px-6 py-8">
        {error && <AlertaError mensaje={error} />}

        {cargandoCatalogo ? (
          <div className="flex items-center justify-center gap-3 rounded-xl border border-slate-200 bg-white py-16">
            <span className="size-6 animate-spin rounded-full border-3 border-marca-100 border-t-marca-600" />
            <p className="text-sm text-slate-500">Cargando tipos de crédito…</p>
          </div>
        ) : tipos.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center">
            <h2 className="font-semibold text-slate-900">No hay tipos de crédito disponibles</h2>
            <p className="mt-1 text-sm text-slate-500">
              Verifica que la Credit API esté en ejecución y tenga su catálogo cargado.
            </p>
          </div>
        ) : (
          <FormularioSimulacion tipos={tipos} simulando={simulando} onSimular={simular} />
        )}

        <div ref={resultadosRef} className="space-y-6 scroll-mt-24">
          {simulacion && (
            <>
              <ResumenSimulacion simulacion={simulacion} />

              <div className="grid gap-6 xl:grid-cols-2">
                <TablaAmortizacion
                  tabla={simulacion.francesa}
                  titulo="Método Francés"
                  descripcion="Cuota mensual fija: el interés disminuye y el capital aumenta con el tiempo."
                  destacada={!alemanGanador}
                />
                <TablaAmortizacion
                  tabla={simulacion.alemana}
                  titulo="Método Alemán"
                  descripcion="Amortización de capital fija: la cuota total decrece cada mes."
                  destacada={alemanGanador}
                />
              </div>
            </>
          )}
        </div>

        <HistorialSimulaciones historial={historial} />
      </main>

      <footer className="border-t border-slate-200 bg-white">
        <p className="mx-auto max-w-6xl px-6 py-5 text-center text-xs text-slate-500">
          Universidad Técnica de Ambato · Las tasas mostradas son referenciales y
          los resultados no constituyen una oferta de crédito.
        </p>
      </footer>
    </div>
  )
}
