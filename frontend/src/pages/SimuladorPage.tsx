import { useCallback, useEffect, useRef, useState } from 'react'
import { AlertaError } from '../components/AlertaError'
import { EncabezadoApp } from '../components/EncabezadoApp'
import { FormularioSimulacion } from '../components/FormularioSimulacion'
import { HistorialSimulaciones } from '../components/HistorialSimulaciones'
import { type Metodo, PanelResultado } from '../components/PanelResultado'
import { PreguntasFrecuentes } from '../components/PreguntasFrecuentes'
import { type Pestana, SeccionTablas } from '../components/SeccionTablas'
import { useAuth } from '../hooks/useAuth'
import { creditApi } from '../services/api'
import type { Simulacion, SimulacionHistorial, SimulacionRequest, TipoCredito } from '../types/credito'

const BENEFICIOS = [
  'Compara el método francés y el alemán',
  'Incluye el seguro de desgravamen',
  'Conoce el ingreso que necesitas',
]

export function SimuladorPage() {
  const { token } = useAuth()

  const [tipos, setTipos] = useState<TipoCredito[]>([])
  const [historial, setHistorial] = useState<SimulacionHistorial[]>([])
  const [simulacion, setSimulacion] = useState<Simulacion | null>(null)

  // El método del panel y la pestaña de la tabla van sincronizados: elegir
  // "alemán" en uno lo muestra también en el otro. "Comparar" solo existe abajo.
  const [metodo, setMetodo] = useState<Metodo>('Frances')
  const [pestana, setPestana] = useState<Pestana>('Frances')

  const [cargandoCatalogo, setCargandoCatalogo] = useState(true)
  const [simulando, setSimulando] = useState(false)
  const [error, setError] = useState('')

  const panelRef = useRef<HTMLDivElement>(null)
  const tablasRef = useRef<HTMLElement>(null)

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

  function cambiarMetodo(nuevo: Metodo) {
    setMetodo(nuevo)
    setPestana(nuevo)
  }

  function cambiarPestana(nueva: Pestana) {
    setPestana(nueva)
    if (nueva !== 'Comparar') setMetodo(nueva)
  }

  async function simular(datos: SimulacionRequest) {
    if (!token) return

    setError('')
    setSimulando(true)

    try {
      const resultado = await creditApi.simular(token, datos)
      setSimulacion(resultado)
      void cargarHistorial()

      // En escritorio el panel ya está a la vista (queda fijo a la derecha); en
      // móvil está debajo del formulario, así que se lleva la vista hasta él.
      requestAnimationFrame(() => {
        const panel = panelRef.current
        if (!panel) return
        const { top, bottom } = panel.getBoundingClientRect()
        if (top < 0 || bottom > window.innerHeight) {
          panel.scrollIntoView({ behavior: 'smooth', block: 'start' })
        }
      })
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo realizar la simulación.')
    } finally {
      setSimulando(false)
    }
  }

  return (
    <div className="min-h-full">
      <EncabezadoApp />

      <div className="border-b border-slate-200 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-10">
          <nav aria-label="Ruta" className="text-sm text-slate-500">
            <span>Créditos</span>
            <span className="mx-2 text-slate-300">/</span>
            <span className="font-medium text-marca-700">Simulador</span>
          </nav>

          <h1 className="mt-3 text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
            Simulador de crédito
          </h1>
          <p className="mt-2 max-w-2xl text-lg text-slate-600">
            Calcula tu cuota, conoce el ingreso que necesitas y compara cuánto pagarías con cada método de
            amortización.
          </p>

          <ul className="mt-5 flex flex-wrap gap-x-6 gap-y-2">
            {BENEFICIOS.map((beneficio) => (
              <li key={beneficio} className="flex items-center gap-2 text-sm font-medium text-slate-700">
                <svg viewBox="0 0 20 20" className="size-5 text-marca-600" fill="currentColor" aria-hidden="true">
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm3.857-9.809a.75.75 0 0 0-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 1 0-1.06 1.061l2.5 2.5a.75.75 0 0 0 1.137-.089l4-5.5Z"
                    clipRule="evenodd"
                  />
                </svg>
                {beneficio}
              </li>
            ))}
          </ul>
        </div>
      </div>

      <main className="mx-auto max-w-7xl space-y-10 px-4 py-8 sm:px-6 sm:py-10">
        {error && <AlertaError mensaje={error} />}

        <div className="grid items-start gap-6 lg:grid-cols-12">
          <div className="lg:col-span-7 xl:col-span-8">
            {cargandoCatalogo ? (
              <div className="flex items-center justify-center gap-3 rounded-2xl border border-slate-200 bg-white py-24">
                <span className="size-6 animate-spin rounded-full border-3 border-marca-100 border-t-marca-600" />
                <p className="text-sm text-slate-500">Cargando tipos de crédito…</p>
              </div>
            ) : tipos.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-12 text-center">
                <h2 className="font-semibold text-slate-900">No hay tipos de crédito disponibles</h2>
                <p className="mt-1 text-sm text-slate-500">
                  Verifica que la Credit API esté en ejecución y tenga su catálogo cargado.
                </p>
              </div>
            ) : (
              <FormularioSimulacion tipos={tipos} simulando={simulando} onSimular={simular} />
            )}
          </div>

          <div ref={panelRef} className="scroll-mt-24 lg:col-span-5 xl:col-span-4">
            <PanelResultado
              simulacion={simulacion}
              metodo={metodo}
              onCambiarMetodo={cambiarMetodo}
              onVerTabla={() => tablasRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })}
            />
          </div>
        </div>

        {simulacion && (
          <SeccionTablas
            ref={tablasRef}
            simulacion={simulacion}
            pestana={pestana}
            onCambiarPestana={cambiarPestana}
          />
        )}

        <div className="flex gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-5 text-sm leading-relaxed text-amber-900">
          <svg viewBox="0 0 20 20" className="mt-0.5 size-5 shrink-0 text-amber-500" fill="currentColor" aria-hidden="true">
            <path
              fillRule="evenodd"
              d="M18 10a8 8 0 1 1-16 0 8 8 0 0 1 16 0Zm-7-4a1 1 0 1 1-2 0 1 1 0 0 1 2 0ZM9 9a.75.75 0 0 0 0 1.5h.253a.25.25 0 0 1 .244.304l-.459 2.066A1.75 1.75 0 0 0 10.747 15H11a.75.75 0 0 0 0-1.5h-.253a.25.25 0 0 1-.244-.304l.459-2.066A1.75 1.75 0 0 0 9.253 9H9Z"
              clipRule="evenodd"
            />
          </svg>
          <p>
            <strong>Esta información es una simulación.</strong> Los valores son referenciales y no constituyen
            una oferta ni un compromiso de crédito. Las condiciones finales dependen de la evaluación de cada
            institución financiera.
          </p>
        </div>

        <HistorialSimulaciones historial={historial} />

        <PreguntasFrecuentes />
      </main>

      <footer className="mt-6 border-t border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl flex-col gap-1 px-4 py-6 text-sm text-slate-500 sm:flex-row sm:justify-between sm:px-6">
          <p>Simulador de Créditos · Universidad Técnica de Ambato</p>
          <p>Proyecto académico de Metodologías de Desarrollo de Software</p>
        </div>
      </footer>
    </div>
  )
}
