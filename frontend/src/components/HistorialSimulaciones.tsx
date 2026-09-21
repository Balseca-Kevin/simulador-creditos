import type { SimulacionHistorial } from '../types/credito'
import { fecha, moneda, opcionFrecuencia, plazoLegible, porcentaje } from '../utils/formato'

/** Simulaciones guardadas del usuario, de la más reciente a la más antigua. */
export function HistorialSimulaciones({ historial }: { historial: SimulacionHistorial[] }) {
  return (
    <section aria-labelledby="titulo-historial">
      <div className="flex items-end justify-between gap-4">
        <div>
          <h2 id="titulo-historial" className="text-xl font-bold text-slate-900">
            Tus simulaciones
          </h2>
          <p className="mt-1 text-sm text-slate-500">Se guardan en tu cuenta y solo tú puedes verlas.</p>
        </div>
        {historial.length > 0 && (
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">
            {historial.length} {historial.length === 1 ? 'registro' : 'registros'}
          </span>
        )}
      </div>

      {historial.length === 0 ? (
        <div className="mt-4 rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-10 text-center">
          <p className="font-semibold text-slate-800">Aún no tienes simulaciones</p>
          <p className="mt-1 text-sm text-slate-500">Las que calcules aparecerán aquí para que puedas compararlas.</p>
        </div>
      ) : (
        <div className="mt-4 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          <div className="max-h-96 overflow-auto">
            <table className="w-full border-collapse text-sm">
              <thead className="sticky top-0 bg-slate-100 text-xs tracking-wide text-slate-600 uppercase">
                <tr>
                  <th scope="col" className="px-4 py-3 text-left font-semibold">Fecha</th>
                  <th scope="col" className="px-4 py-3 text-left font-semibold">Crédito</th>
                  <th scope="col" className="px-4 py-3 text-right font-semibold">Monto</th>
                  <th scope="col" className="px-4 py-3 text-left font-semibold">Plazo y frecuencia</th>
                  <th scope="col" className="px-4 py-3 text-right font-semibold">Cuota fija</th>
                  <th scope="col" className="px-4 py-3 text-right font-semibold">Ingreso mínimo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {historial.map((registro) => (
                  <tr key={registro.id} className="transition-colors hover:bg-marca-50/60">
                    <td className="px-4 py-3 whitespace-nowrap text-slate-500">{fecha(registro.fechaSimulacion)}</td>
                    <td className="px-4 py-3">
                      <span className="block font-medium text-slate-900">{registro.tipoCredito}</span>
                      <span className="text-xs text-slate-500">
                        {porcentaje(registro.tasaAnualAplicada)}
                        {registro.incluyeSeguroDesgravamen ? ' · con seguro' : ''}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right font-medium tabular-nums whitespace-nowrap text-slate-900">
                      {moneda(registro.monto)}
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap text-slate-600">
                      {plazoLegible(registro.plazoMeses)}
                      <span className="block text-xs text-slate-400">
                        {opcionFrecuencia(registro.frecuenciaPago).etiqueta}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right tabular-nums whitespace-nowrap text-slate-700">
                      {moneda(registro.cuotaFija)}
                    </td>
                    <td className="px-4 py-3 text-right font-semibold tabular-nums whitespace-nowrap text-marca-700">
                      {moneda(registro.ingresoMinimoRequerido)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  )
}
