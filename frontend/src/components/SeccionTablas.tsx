import type { Simulacion } from '../types/credito'
import { moneda } from '../utils/formato'
import { TablaAmortizacion } from './TablaAmortizacion'

export type Pestana = 'Frances' | 'Aleman' | 'Comparar'

interface Props {
  simulacion: Simulacion
  pestana: Pestana
  onCambiarPestana: (pestana: Pestana) => void
  ref?: React.Ref<HTMLElement>
}

const PESTANAS: { valor: Pestana; etiqueta: string }[] = [
  { valor: 'Frances', etiqueta: 'Método francés' },
  { valor: 'Aleman', etiqueta: 'Método alemán' },
  { valor: 'Comparar', etiqueta: 'Comparar' },
]

const DESCRIPCIONES: Record<'Frances' | 'Aleman', string> = {
  Frances:
    'Cuota fija durante todo el plazo. Al inicio pagas más interés y, con el tiempo, más capital.',
  Aleman:
    'Amortizas la misma cantidad de capital en cada cuota. Como el interés se calcula sobre un saldo cada vez menor, la cuota baja período a período.',
}

export function SeccionTablas({ simulacion, pestana, onCambiarPestana, ref }: Props) {
  const esUnica = simulacion.numeroCuotas === 1

  return (
    <section
      ref={ref}
      className="scroll-mt-24 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
    >
      <header className="px-5 pt-6 sm:px-7">
        <h2 className="text-xl font-bold text-slate-900">Tabla de amortización</h2>
        <p className="mt-1 text-sm text-slate-500">
          Detalle de cada cuota: cuánto va a capital, cuánto a intereses y cuánto debes después de pagarla.
        </p>

        <div role="tablist" aria-label="Vista de la tabla" className="mt-5 flex gap-1 overflow-x-auto border-b border-slate-200">
          {PESTANAS.map(({ valor, etiqueta }) => {
            const activa = pestana === valor
            return (
              <button
                key={valor}
                role="tab"
                aria-selected={activa}
                onClick={() => onCambiarPestana(valor)}
                className={`-mb-px border-b-2 px-4 py-2.5 text-sm font-semibold whitespace-nowrap transition ${
                  activa
                    ? 'border-marca-600 text-marca-700'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                }`}
              >
                {etiqueta}
              </button>
            )
          })}
        </div>
      </header>

      {pestana === 'Comparar' ? (
        <Comparativo simulacion={simulacion} esUnica={esUnica} />
      ) : (
        <div role="tabpanel">
          <p className="px-5 py-4 text-sm text-slate-600 sm:px-7">{DESCRIPCIONES[pestana]}</p>
          <TablaAmortizacion
            tabla={pestana === 'Frances' ? simulacion.francesa : simulacion.alemana}
            conSeguro={simulacion.incluyeSeguroDesgravamen}
          />
        </div>
      )}
    </section>
  )
}

function Comparativo({ simulacion, esUnica }: { simulacion: Simulacion; esUnica: boolean }) {
  const { francesa, alemana, comparativo, incluyeSeguroDesgravamen } = simulacion

  if (esUnica) {
    return (
      <div role="tabpanel" className="px-5 py-8 text-center sm:px-7">
        <p className="mx-auto max-w-md text-sm text-slate-600">
          Con un <strong>pago único al vencimiento</strong> ambos métodos coinciden: se devuelve todo el
          capital con sus intereses en una sola cuota de{' '}
          <strong className="tabular-nums">{moneda(francesa.totalPagado)}</strong>.
        </p>
      </div>
    )
  }

  const filas: { concepto: string; frances: number; aleman: number; menorEsMejor?: boolean }[] = [
    { concepto: 'Primera cuota', frances: francesa.primeraCuotaTotal, aleman: alemana.primeraCuotaTotal },
    { concepto: 'Última cuota', frances: francesa.ultimaCuotaTotal, aleman: alemana.ultimaCuotaTotal },
    { concepto: 'Total de intereses', frances: francesa.totalInteres, aleman: alemana.totalInteres, menorEsMejor: true },
    ...(incluyeSeguroDesgravamen
      ? [{ concepto: 'Seguro de desgravamen', frances: francesa.totalSeguro, aleman: alemana.totalSeguro, menorEsMejor: true }]
      : []),
    { concepto: 'Total a pagar', frances: francesa.totalPagado, aleman: alemana.totalPagado, menorEsMejor: true },
    {
      concepto: 'Ingreso mínimo requerido',
      frances: francesa.ingresoMinimoRequerido,
      aleman: alemana.ingresoMinimoRequerido,
      menorEsMejor: true,
    },
  ]

  const alemanAhorra = comparativo.metodoMasEconomico === 'Aleman'
  const diferenciaPrimeraCuota = alemana.primeraCuotaTotal - francesa.primeraCuotaTotal
  const diferenciaIngreso = alemana.ingresoMinimoRequerido - francesa.ingresoMinimoRequerido

  return (
    <div role="tabpanel" className="px-5 py-6 sm:px-7">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-slate-200 text-xs tracking-wide text-slate-500 uppercase">
              <th scope="col" className="py-3 pr-4 text-left font-semibold">
                Concepto
              </th>
              <th scope="col" className="px-4 py-3 text-right font-semibold">
                Francés
              </th>
              <th scope="col" className="py-3 pl-4 text-right font-semibold">
                Alemán
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filas.map(({ concepto, frances, aleman, menorEsMejor }) => {
              const ganaFrances = menorEsMejor && frances < aleman
              const ganaAleman = menorEsMejor && aleman < frances
              return (
                <tr key={concepto}>
                  <th scope="row" className="py-3 pr-4 text-left font-medium text-slate-700">
                    {concepto}
                  </th>
                  <td
                    className={`px-4 py-3 text-right tabular-nums ${ganaFrances ? 'font-bold text-emerald-700' : 'text-slate-700'}`}
                  >
                    {moneda(frances)}
                  </td>
                  <td
                    className={`py-3 pl-4 text-right tabular-nums ${ganaAleman ? 'font-bold text-emerald-700' : 'text-slate-700'}`}
                  >
                    {moneda(aleman)}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      <div className="mt-6 rounded-xl border border-marca-200 bg-marca-50 p-4 text-sm leading-relaxed text-marca-900">
        {comparativo.diferenciaTotalInteres === 0 ? (
          <p>En este escenario ambos métodos generan el mismo total de intereses.</p>
        ) : alemanAhorra ? (
          <p>
            El <strong>método alemán</strong> te ahorra{' '}
            <strong className="tabular-nums">{moneda(comparativo.diferenciaTotalInteres)}</strong> en intereses.
            A cambio, su primera cuota es{' '}
            <strong className="tabular-nums">{moneda(diferenciaPrimeraCuota)}</strong> más alta y exige un
            ingreso{' '}
            <strong className="tabular-nums">{moneda(diferenciaIngreso)}</strong> mayor. Si tu presupuesto es
            ajustado, el francés ofrece una cuota más cómoda y predecible.
          </p>
        ) : (
          <p>
            El <strong>método francés</strong> te ahorra{' '}
            <strong className="tabular-nums">{moneda(comparativo.diferenciaTotalInteres)}</strong> en intereses y
            además mantiene la cuota fija.
          </p>
        )}
      </div>
    </div>
  )
}
