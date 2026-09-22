import { Isotipo } from './Isotipo'

const BENEFICIOS = [
  {
    titulo: 'Dos métodos, una comparación',
    detalle: 'Mira lado a lado tu tabla por el método francés y por el alemán.',
    icono: 'M3 13h4v8H3zM10 8h4v13h-4zM17 3h4v18h-4z',
  },
  {
    titulo: 'Sabe si calificas',
    detalle: 'Te decimos el ingreso mínimo que necesitas para cada cuota.',
    icono: 'M12 2 3 6v6c0 5 3.8 9.4 9 10 5.2-.6 9-5 9-10V6l-9-4Zm-1.2 13.6-3.5-3.5 1.4-1.4 2.1 2.1 4.9-4.9 1.4 1.4-6.3 6.3Z',
  },
  {
    titulo: 'Tu historial, siempre a mano',
    detalle: 'Cada simulación queda guardada en tu cuenta para revisarla después.',
    icono: 'M13 3a9 9 0 0 0-9 9H1l4 4 4-4H6a7 7 0 1 1 2.05 4.95l-1.42 1.42A9 9 0 1 0 13 3Zm-1 5v5l4.25 2.52.77-1.28-3.52-2.09V8H12Z',
  },
]

/**
 * Columna de presentación de las pantallas de acceso.
 * No muestra tasas: esas viven en la base de la Credit API y aquí quedarían
 * desactualizadas en cuanto cambiaran.
 */
export function PanelMarca() {
  return (
    <aside className="relative hidden overflow-hidden bg-marca-700 p-12 text-white lg:flex lg:flex-col lg:justify-between">
      {/* Trama decorativa de fondo */}
      <svg className="pointer-events-none absolute -right-24 -bottom-24 size-[28rem] text-white/5" viewBox="0 0 200 200" aria-hidden="true">
        <circle cx="100" cy="100" r="98" fill="none" stroke="currentColor" strokeWidth="2" />
        <circle cx="100" cy="100" r="70" fill="none" stroke="currentColor" strokeWidth="2" />
        <circle cx="100" cy="100" r="42" fill="none" stroke="currentColor" strokeWidth="2" />
      </svg>

      <div className="relative flex items-center gap-3">
        <Isotipo className="size-11" invertido />
        <div className="leading-tight">
          <p className="text-lg font-bold">Simulador de Créditos</p>
          <p className="text-sm text-marca-100">Universidad Técnica de Ambato</p>
        </div>
      </div>

      <div className="relative">
        <h1 className="max-w-md text-4xl leading-tight font-bold">
          Planifica tu crédito antes de solicitarlo.
        </h1>

        <ul className="mt-10 space-y-6">
          {BENEFICIOS.map(({ titulo, detalle, icono }) => (
            <li key={titulo} className="flex gap-4">
              <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-white/10">
                <svg viewBox="0 0 24 24" className="size-5" fill="currentColor" aria-hidden="true">
                  <path d={icono} />
                </svg>
              </span>
              <div>
                <p className="font-semibold">{titulo}</p>
                <p className="mt-0.5 text-sm text-marca-100">{detalle}</p>
              </div>
            </li>
          ))}
        </ul>
      </div>

      <p className="relative text-xs text-marca-200">
        Los valores de las simulaciones son referenciales y no constituyen una oferta de crédito.
      </p>
    </aside>
  )
}
