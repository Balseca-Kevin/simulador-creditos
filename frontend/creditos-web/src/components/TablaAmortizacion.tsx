import type { TablaAmortizacion as Tabla } from '../types/credito'
import { moneda } from '../utils/formato'

interface Props {
  tabla: Tabla
  conSeguro: boolean
}

const celda = 'px-4 py-2.5 text-right tabular-nums whitespace-nowrap'
const encabezado = 'px-4 py-3 text-right font-semibold whitespace-nowrap'

/**
 * Tabla de amortización con encabezado y totales fijos y desplazamiento propio:
 * un crédito a 480 meses son 480 filas, y sin esto se pierde de vista qué
 * significa cada columna. Las columnas de seguro solo aparecen si se contrató.
 */
export function TablaAmortizacion({ tabla, conSeguro }: Props) {
  return (
    <div className="max-h-[32rem] overflow-auto">
      <table className="w-full border-collapse text-sm">
        <caption className="sr-only">
          Tabla de amortización por el método {tabla.metodo === 'Frances' ? 'francés' : 'alemán'}
        </caption>

        <thead className="sticky top-0 z-10 bg-slate-100 text-xs tracking-wide text-slate-600 uppercase">
          <tr>
            <th scope="col" className="px-4 py-3 text-left font-semibold">
              N.º
            </th>
            <th scope="col" className={encabezado}>
              Capital
            </th>
            <th scope="col" className={encabezado}>
              Interés
            </th>
            <th scope="col" className={encabezado}>
              {conSeguro ? 'Cuota' : 'Cuota total'}
            </th>
            {conSeguro && (
              <>
                <th scope="col" className={encabezado}>
                  Seguro
                </th>
                <th scope="col" className={encabezado}>
                  Cuota total
                </th>
              </>
            )}
            <th scope="col" className={encabezado}>
              Saldo
            </th>
          </tr>
        </thead>

        <tbody className="divide-y divide-slate-100 bg-white">
          {tabla.cuotas.map((fila) => (
            <tr key={fila.periodo} className="transition-colors hover:bg-marca-50/60">
              <td className="px-4 py-2.5 text-left tabular-nums text-slate-400">{fila.periodo}</td>
              <td className={`${celda} text-slate-700`}>{moneda(fila.capital)}</td>
              <td className={`${celda} text-slate-700`}>{moneda(fila.interes)}</td>
              <td className={`${celda} ${conSeguro ? 'text-slate-700' : 'font-semibold text-slate-900'}`}>
                {moneda(fila.cuota)}
              </td>
              {conSeguro && (
                <>
                  <td className={`${celda} text-slate-500`}>{moneda(fila.seguro)}</td>
                  <td className={`${celda} font-semibold text-slate-900`}>{moneda(fila.cuotaTotal)}</td>
                </>
              )}
              <td className={`${celda} text-slate-500`}>{moneda(fila.saldoRestante)}</td>
            </tr>
          ))}
        </tbody>

        <tfoot className="sticky bottom-0 bg-marca-50 font-bold text-marca-900">
          <tr>
            <td className="px-4 py-3 text-left text-xs tracking-wide uppercase">Total</td>
            <td className={celda}>{moneda(tabla.totalCapital)}</td>
            <td className={celda}>{moneda(tabla.totalInteres)}</td>
            <td className={celda}>{moneda(tabla.totalCapital + tabla.totalInteres)}</td>
            {conSeguro && (
              <>
                <td className={celda}>{moneda(tabla.totalSeguro)}</td>
                <td className={celda}>{moneda(tabla.totalPagado)}</td>
              </>
            )}
            <td className={`${celda} font-normal text-marca-300`}>—</td>
          </tr>
        </tfoot>
      </table>
    </div>
  )
}
