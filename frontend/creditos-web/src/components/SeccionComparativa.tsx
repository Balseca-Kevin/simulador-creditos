import type { Simulacion } from '../types/credito'
import { moneda } from '../utils/formato'

interface Props {
  simulacion: Simulacion
  abriendoReporte: boolean
  onAbrirReporte: () => void
  ref?: React.Ref<HTMLElement>
}

function IconoDocumento() {
  return (
    <svg viewBox="0 0 24 24" className="size-5" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true">
      <path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8l-5-5Z" strokeLinejoin="round" />
      <path d="M14 3v5h5M9 13h6M9 17h6" strokeLinecap="round" />
    </svg>
  )
}

/**
 * Comparación de los dos métodos. Las tablas completas no se muestran aquí:
 * viven en el reporte, donde el visor del navegador aporta paginación,
 * impresión y descarga sin que haya que construir esos controles.
 */
export function SeccionComparativa({ simulacion, abriendoReporte, onAbrirReporte, ref }: Props) {
  const { francesa, alemana, comparativo, incluyeSeguroDesgravamen } = simulacion
  const esUnica = simulacion.numeroCuotas === 1

  const filas: { concepto: string; frances: number; aleman: number; menorEsMejor?: boolean }[] = [
    { concepto: 'Primera cuota', frances: francesa.primeraCuotaTotal, aleman: alemana.primeraCuotaTotal },
    { concepto: 'Última cuota', frances: francesa.ultimaCuotaTotal, aleman: alemana.ultimaCuotaTotal },
    { concepto: 'Total de intereses', frances: francesa.totalInteres, aleman: alemana.totalInteres, menorEsMejor: true },
    ...(incluyeSeguroDesgravamen
      ? [
          {
            concepto: 'Seguro de desgravamen',
            frances: francesa.totalSeguro,
            aleman: alemana.totalSeguro,
            menorEsMejor: true,
          },
        ]
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
    <section
      ref={ref}
      className="scroll-mt-24 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
    >
      <header className="flex flex-col gap-4 px-5 pt-6 sm:flex-row sm:items-start sm:justify-between sm:px-7">
        <div>
          <h2 className="text-xl font-bold text-slate-900">Comparación de los dos métodos</h2>
          <p className="mt-1 text-sm text-slate-500">
            Las tablas de amortización completas están en el reporte, listas para imprimir o descargar.
          </p>
        </div>

        <button
          onClick={onAbrirReporte}
          disabled={abriendoReporte}
          className="inline-flex shrink-0 items-center justify-center gap-2 rounded-xl bg-marca-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-marca-700 focus:ring-4 focus:ring-marca-200 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60"
        >
          {abriendoReporte ? (
            <span className="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          ) : (
            <IconoDocumento />
          )}
          {abriendoReporte ? 'Generando…' : 'Ver tabla de amortización'}
        </button>
      </header>

      {esUnica ? (
        <p className="mx-auto max-w-lg px-5 py-8 text-center text-sm text-slate-600 sm:px-7">
          Con un <strong>pago único al vencimiento</strong> ambos métodos coinciden: se devuelve todo el
          capital con sus intereses en una sola cuota de{' '}
          <strong className="tabular-nums">{moneda(francesa.totalPagado)}</strong>.
        </p>
      ) : (
        <div className="px-5 py-6 sm:px-7">
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
                <strong className="tabular-nums">{moneda(comparativo.diferenciaTotalInteres)}</strong> en
                intereses. A cambio, su primera cuota es{' '}
                <strong className="tabular-nums">{moneda(diferenciaPrimeraCuota)}</strong> más alta y exige un
                ingreso <strong className="tabular-nums">{moneda(diferenciaIngreso)}</strong> mayor. Si tu
                presupuesto es ajustado, el francés ofrece una cuota más cómoda y predecible.
              </p>
            ) : (
              <p>
                El <strong>método francés</strong> te ahorra{' '}
                <strong className="tabular-nums">{moneda(comparativo.diferenciaTotalInteres)}</strong> en
                intereses y además mantiene la cuota fija.
              </p>
            )}
          </div>
        </div>
      )}
    </section>
  )
}
