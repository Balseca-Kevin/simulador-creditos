/** Isotipo del simulador: una gráfica ascendente, sin referencias a marcas reales. */
export function Isotipo({ className = 'size-9', invertido = false }: { className?: string; invertido?: boolean }) {
  return (
    <svg viewBox="0 0 40 40" className={className} aria-hidden="true">
      <rect width="40" height="40" rx="10" className={invertido ? 'fill-white' : 'fill-marca-600'} />
      <path
        d="M10 27 L17 20 L22 24 L30 14"
        fill="none"
        className={invertido ? 'stroke-marca-700' : 'stroke-white'}
        strokeWidth="3"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <circle cx="30" cy="14" r="2.6" className={invertido ? 'fill-marca-700' : 'fill-white'} />
    </svg>
  )
}
