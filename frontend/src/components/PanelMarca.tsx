const TASAS = [
  { tipo: 'Crédito de Consumo', tasa: '15.50 %' },
  { tipo: 'Crédito Inmobiliario', tasa: '8.50 %' },
  { tipo: 'Microcrédito', tasa: '22.00 %' },
]

/** Columna de presentación que acompaña a los formularios de acceso. */
export function PanelMarca() {
  return (
    <aside className="hidden flex-col justify-between bg-marca-700 p-12 text-white lg:flex">
      <div>
        <p className="text-sm font-semibold tracking-widest text-marca-100 uppercase">
          Universidad Técnica de Ambato
        </p>
        <h1 className="mt-6 text-4xl font-bold leading-tight">
          Simulador de Créditos
        </h1>
        <p className="mt-4 max-w-sm text-marca-100">
          Compara en segundos tu tabla de amortización por el método francés y el
          alemán, con las tasas referenciales vigentes.
        </p>
      </div>

      <dl className="space-y-3">
        {TASAS.map(({ tipo, tasa }) => (
          <div
            key={tipo}
            className="flex items-center justify-between rounded-lg bg-white/10 px-4 py-3 backdrop-blur-sm"
          >
            <dt className="text-sm text-marca-50">{tipo}</dt>
            <dd className="text-lg font-semibold tabular-nums">{tasa}</dd>
          </div>
        ))}
      </dl>
    </aside>
  )
}
