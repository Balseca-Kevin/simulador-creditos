import type { TablaAmortizacion as Tabla } from '../types/credito'
import { moneda } from '../utils/formato'

interface Props {
  tabla: Tabla
  titulo: string
  descripcion: string
  destacada?: boolean
}

/**
 * Tabla de amortización con encabezado fijo y desplazamiento propio:
 * un crédito a 480 meses son 480 filas, y sin esto la página se vuelve inmanejable.
 */
export function TablaAmortizacion({ tabla, titulo, descripcion, destacada = false }: Props) {
  return (
    <section
      className={`flex flex-col overflow-hidden rounded-xl border bg-white shadow-sm ${
        destacada ? 'border-marca-300 ring-1 ring-marca-300' : 'border-slate-200'
      }`}
    >
      <header className="border-b border-slate-100 px-5 py-4">
        <div className="flex items-start justify-between gap-3">
          <div>
            <h3 className="font-semibold text-slate-900">{titulo}</h3>
            <p className="mt-0.5 text-xs leading-snug text-slate-500">{descripcion}</p>
          </div>

          {destacada && (
            <span className="shrink-0 rounded-full bg-marca-100 px-2.5 py-1 text-xs font-semibold text-marca-700">
              Menos intereses
            </span>
          )}
        </div>

        <dl className="mt-4 grid grid-cols-3 gap-3 text-sm">
          <div>
            <dt className="text-xs text-slate-500">Primera cuota</dt>
            <dd className="font-semibold tabular-nums text-slate-900">
              {moneda(tabla.primeraCuota)}
            </dd>
          </div>
          <div>
            <dt className="text-xs text-slate-500">Última cuota</dt>
            <dd className="font-semibold tabular-nums text-slate-900">
              {moneda(tabla.ultimaCuota)}
            </dd>
          </div>
          <div>
            <dt className="text-xs text-slate-500">Total intereses</dt>
            <dd className="font-semibold tabular-nums text-slate-900">
              {moneda(tabla.totalInteres)}
            </dd>
          </div>
        </dl>
      </header>

      <div className="max-h-[28rem] overflow-auto">
        <table className="w-full border-collapse text-sm">
          <thead className="sticky top-0 z-10 bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
            <tr>
              <th scope="col" className="px-4 py-2.5 text-left font-medium">
                N.º
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Cuota
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Interés
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Capital
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Saldo
              </th>
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-100">
            {tabla.cuotas.map((fila) => (
              <tr key={fila.periodo} className="hover:bg-slate-50">
                <td className="px-4 py-2 text-left tabular-nums text-slate-500">{fila.periodo}</td>
                <td className="px-4 py-2 text-right font-medium tabular-nums text-slate-900">
                  {moneda(fila.cuota)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums text-amber-700">
                  {moneda(fila.interes)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums text-emerald-700">
                  {moneda(fila.capital)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                  {moneda(fila.saldoRestante)}
                </td>
              </tr>
            ))}
          </tbody>

          <tfoot className="sticky bottom-0 bg-slate-50 font-semibold text-slate-900">
            <tr>
              <td className="px-4 py-2.5 text-left text-xs uppercase tracking-wide text-slate-500">
                Totales
              </td>
              <td className="px-4 py-2.5 text-right tabular-nums">{moneda(tabla.totalPagado)}</td>
              <td className="px-4 py-2.5 text-right tabular-nums">{moneda(tabla.totalInteres)}</td>
              <td className="px-4 py-2.5 text-right tabular-nums">{moneda(tabla.totalCapital)}</td>
              <td className="px-4 py-2.5 text-right tabular-nums text-slate-400">—</td>
            </tr>
          </tfoot>
        </table>
      </div>
    </section>
  )
}
