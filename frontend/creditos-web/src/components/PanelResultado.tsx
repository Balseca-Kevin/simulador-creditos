import type { Simulacion } from '../types/credito'
import { moneda, opcionFrecuencia, plazoLegible, porcentaje } from '../utils/formato'

export type Metodo = 'Frances' | 'Aleman'

interface Props {
  simulacion: Simulacion | null
  metodo: Metodo
  abriendoReporte: boolean
  /** Valor total de las garantias declaradas; nulo si aun no se conoce. */
  patrimonio: number | null
  onCambiarMetodo: (metodo: Metodo) => void
  onVerReporte: () => void
}

function Fila({ etiqueta, valor, fuerte = false }: { etiqueta: string; valor: string; fuerte?: boolean }) {
  return (
    <div className="flex items-baseline justify-between gap-4 py-2">
      <dt className="text-sm text-slate-500">{etiqueta}</dt>
      <dd className={`text-right tabular-nums ${fuerte ? 'font-bold text-slate-900' : 'font-medium text-slate-700'}`}>
        {valor}
      </dd>
    </div>
  )
}

function Vacio() {
  return (
    <div className="flex flex-col items-center px-6 py-12 text-center">
      <span className="flex size-14 items-center justify-center rounded-2xl bg-marca-50">
        <svg viewBox="0 0 24 24" className="size-7 text-marca-600" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true">
          <rect x="4" y="3" width="16" height="18" rx="2.5" />
          <path d="M8 7h8M8 11h2M12 11h2M16 11h0M8 15h2M12 15h2M16 15h0M8 19h8" strokeLinecap="round" />
        </svg>
      </span>
      <h2 className="mt-4 text-lg font-semibold text-slate-900">Tu resultado aparecerá aquí</h2>
      <p className="mt-1 max-w-xs text-sm text-slate-500">
        Completa los cuatro pasos y presiona <strong className="text-slate-700">Calcular cuota</strong> para
        conocer tu cuota y el ingreso que necesitas.
      </p>
    </div>
  )
}

/**
 * Resumen que acompaña al formulario. En escritorio queda fijo a la derecha
 * mientras se ajustan los datos, como en los simuladores bancarios de referencia.
 */
export function PanelResultado({
  simulacion,
  metodo,
  abriendoReporte,
  patrimonio,
  onCambiarMetodo,
  onVerReporte,
}: Props) {
  return (
    <aside
      aria-live="polite"
      className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm lg:sticky lg:top-24"
    >
      {!simulacion ? (
        <Vacio />
      ) : (
        <Contenido
          simulacion={simulacion}
          metodo={metodo}
          abriendoReporte={abriendoReporte}
          patrimonio={patrimonio}
          onCambiarMetodo={onCambiarMetodo}
          onVerReporte={onVerReporte}
        />
      )}
    </aside>
  )
}

function Contenido({
  simulacion,
  metodo,
  abriendoReporte,
  patrimonio,
  onCambiarMetodo,
  onVerReporte,
}: Props & { simulacion: Simulacion }) {
  const tabla = metodo === 'Frances' ? simulacion.francesa : simulacion.alemana
  const frecuencia = opcionFrecuencia(simulacion.frecuenciaPago)
  const esUnica = simulacion.numeroCuotas === 1
  const conSeguro = simulacion.incluyeSeguroDesgravamen
  const umbral = Math.round(simulacion.relacionCuotaIngreso * 100)

  return (
    <>
      <div className="bg-marca-700 px-6 pt-5 pb-6 text-white">
        <div className="flex items-center justify-between gap-3">
          <p className="text-sm font-medium text-marca-100">{simulacion.tipoCredito.nombre}</p>

          {!esUnica && (
            <div role="tablist" aria-label="Método de amortización" className="flex rounded-lg bg-black/20 p-0.5">
              {(['Frances', 'Aleman'] as const).map((opcion) => (
                <button
                  key={opcion}
                  role="tab"
                  aria-selected={metodo === opcion}
                  onClick={() => onCambiarMetodo(opcion)}
                  className={`rounded-md px-3 py-1 text-xs font-semibold transition ${
                    metodo === opcion ? 'bg-white text-marca-700' : 'text-marca-100 hover:text-white'
                  }`}
                >
                  {opcion === 'Frances' ? 'Francés' : 'Alemán'}
                </button>
              ))}
            </div>
          )}
        </div>

        <p className="mt-4 text-sm text-marca-100">
          {esUnica
            ? 'Pago único al vencimiento'
            : ['Tu cuota', frecuencia.adjetivo, metodo === 'Aleman' && 'inicial', 'sería'].filter(Boolean).join(' ')}
        </p>
        <p className="mt-1 text-4xl font-bold tracking-tight tabular-nums">{moneda(tabla.primeraCuotaTotal)}</p>

        <p className="mt-2 text-sm text-marca-100">
          {esUnica
            ? `Capital e intereses al cabo de ${plazoLegible(simulacion.plazoMeses)}`
            : metodo === 'Frances'
              ? conSeguro
                ? `Cuota fija de ${moneda(tabla.primeraCuota)} más el seguro, que baja con el saldo`
                : 'Igual durante todo el plazo'
              : `Baja cada período hasta ${moneda(tabla.ultimaCuotaTotal)}`}
        </p>
      </div>

      <div className="border-b border-slate-100 bg-marca-50 px-6 py-4">
        <p className="text-sm font-medium text-marca-800">Ingreso mensual mínimo requerido</p>
        <p className="mt-0.5 text-2xl font-bold tabular-nums text-marca-800">
          {moneda(tabla.ingresoMinimoRequerido)}
        </p>
        <p className="mt-1 text-xs text-marca-700">
          Para que la cuota más alta no supere el {umbral} % de tus ingresos.
        </p>

        {/* El patrimonio lo aporta AssetService: la SPA une lo que devuelven dos
            microservicios sin que ellos se conozcan entre sí. */}
        {patrimonio !== null && (
          <div className="mt-3 flex items-baseline justify-between gap-3 border-t border-marca-200 pt-3">
            <span className="text-xs text-marca-700">Tu patrimonio declarado</span>
            <span className="text-sm font-semibold tabular-nums text-marca-800">
              {moneda(patrimonio)}
            </span>
          </div>
        )}
      </div>

      <dl className="divide-y divide-slate-100 px-6 py-2">
        <Fila etiqueta="Monto solicitado" valor={moneda(simulacion.monto)} />
        <Fila
          etiqueta="Plazo"
          valor={`${plazoLegible(simulacion.plazoMeses)} · ${simulacion.numeroCuotas} ${
            simulacion.numeroCuotas === 1 ? 'cuota' : 'cuotas'
          }`}
        />
        <Fila etiqueta="Frecuencia de pago" valor={frecuencia.etiqueta} />
        <Fila etiqueta="Tasa de interés anual" valor={porcentaje(simulacion.tasaAnualAplicada)} />
        <Fila etiqueta="Total de intereses" valor={moneda(tabla.totalInteres)} />
        <Fila
          etiqueta="Seguro de desgravamen"
          valor={conSeguro ? moneda(tabla.totalSeguro) : 'No incluido'}
        />
        <Fila etiqueta="Total a pagar" valor={moneda(tabla.totalPagado)} fuerte />
      </dl>

      <div className="px-6 pb-6">
        <button
          onClick={onVerReporte}
          disabled={abriendoReporte}
          className="inline-flex w-full items-center justify-center gap-2 rounded-xl border-2 border-marca-600 px-4 py-2.5 text-sm font-semibold text-marca-700 transition hover:bg-marca-50 focus:ring-4 focus:ring-marca-100 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60"
        >
          {abriendoReporte && (
            <span className="size-4 animate-spin rounded-full border-2 border-marca-200 border-t-marca-600" />
          )}
          {abriendoReporte ? 'Generando reporte…' : 'Ver tabla de amortización'}
        </button>

        <p className="mt-2 text-center text-xs text-slate-400">
          Se abre en una pestaña nueva, lista para imprimir o descargar.
        </p>
      </div>
    </>
  )
}
