import type { SimulacionHistorial } from '../types/credito'
import { fecha, moneda, porcentaje } from '../utils/formato'

/** Muestra las simulaciones guardadas del usuario, de la más reciente a la más antigua. */
export function HistorialSimulaciones({ historial }: { historial: SimulacionHistorial[] }) {
  if (historial.length === 0) {
    return (
      <section className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-center">
        <h2 className="font-semibold text-slate-900">Aún no tienes simulaciones</h2>
        <p className="mt-1 text-sm text-slate-500">
          Las que realices se guardarán aquí para que puedas compararlas después.
        </p>
      </section>
    )
  }

  return (
    <section className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <header className="border-b border-slate-100 px-5 py-4">
        <h2 className="font-semibold text-slate-900">Tus simulaciones anteriores</h2>
        <p className="mt-0.5 text-xs text-slate-500">
          Solo tú puedes verlas: se guardan asociadas a tu cuenta.
        </p>
      </header>

      <div className="max-h-80 overflow-auto">
        <table className="w-full border-collapse text-sm">
          <thead className="sticky top-0 bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
            <tr>
              <th scope="col" className="px-4 py-2.5 text-left font-medium">
                Fecha
              </th>
              <th scope="col" className="px-4 py-2.5 text-left font-medium">
                Tipo
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Monto
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Plazo
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Tasa
              </th>
              <th scope="col" className="px-4 py-2.5 text-right font-medium">
                Cuota fija
              </th>
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-100">
            {historial.map((registro) => (
              <tr key={registro.id} className="hover:bg-slate-50">
                <td className="px-4 py-2 text-left whitespace-nowrap text-slate-500">
                  {fecha(registro.fechaSimulacion)}
                </td>
                <td className="px-4 py-2 text-left text-slate-900">{registro.tipoCredito}</td>
                <td className="px-4 py-2 text-right tabular-nums text-slate-900">
                  {moneda(registro.monto)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                  {registro.plazoMeses} m
                </td>
                <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                  {porcentaje(registro.tasaAnualAplicada)}
                </td>
                <td className="px-4 py-2 text-right font-medium tabular-nums text-slate-900">
                  {moneda(registro.cuotaFija)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
