import type { Simulacion } from '../types/credito'
import { moneda, porcentaje } from '../utils/formato'

/** Cifra destacada del encabezado de resultados. */
function Dato({
  etiqueta,
  valor,
  detalle,
}: {
  etiqueta: string
  valor: string
  detalle?: string
}) {
  return (
    <div className="rounded-lg bg-white/10 px-4 py-3 backdrop-blur-sm">
      <p className="text-xs text-marca-100">{etiqueta}</p>
      <p className="mt-1 text-xl font-bold tabular-nums">{valor}</p>
      {detalle && <p className="mt-0.5 text-xs text-marca-100">{detalle}</p>}
    </div>
  )
}

export function ResumenSimulacion({ simulacion }: { simulacion: Simulacion }) {
  const { francesa, alemana, comparativo } = simulacion
  const alemanGanador = comparativo.metodoMasEconomico === 'Aleman'

  return (
    <section className="overflow-hidden rounded-xl bg-marca-700 text-white shadow-sm">
      <div className="p-6">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <h2 className="text-lg font-semibold">{simulacion.tipoCredito.nombre}</h2>
          <p className="text-sm text-marca-100">
            {moneda(simulacion.monto)} a {simulacion.plazoMeses} meses ·{' '}
            {porcentaje(simulacion.tasaAnualAplicada)} anual
          </p>
        </div>

        <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <Dato
            etiqueta="Cuota fija (francés)"
            valor={moneda(francesa.primeraCuota)}
            detalle="Igual todos los meses"
          />
          <Dato
            etiqueta="Cuota alemana"
            valor={moneda(alemana.primeraCuota)}
            detalle={`Baja hasta ${moneda(alemana.ultimaCuota)}`}
          />
          <Dato
            etiqueta="Intereses francés"
            valor={moneda(francesa.totalInteres)}
            detalle={`Total: ${moneda(francesa.totalPagado)}`}
          />
          <Dato
            etiqueta="Intereses alemán"
            valor={moneda(alemana.totalInteres)}
            detalle={`Total: ${moneda(alemana.totalPagado)}`}
          />
        </div>
      </div>

      <p className="border-t border-white/15 bg-black/10 px-6 py-3 text-sm">
        {comparativo.diferenciaTotalInteres === 0 ? (
          <>Ambos métodos generan el mismo total de intereses en este escenario.</>
        ) : (
          <>
            El método{' '}
            <strong className="font-semibold">{alemanGanador ? 'alemán' : 'francés'}</strong> ahorra{' '}
            <strong className="font-semibold tabular-nums">
              {moneda(comparativo.diferenciaTotalInteres)}
            </strong>{' '}
            en intereses
            {alemanGanador
              ? ', pero exige cuotas más altas al principio.'
              : ', manteniendo además la cuota fija todos los meses.'}
          </>
        )}
      </p>
    </section>
  )
}
