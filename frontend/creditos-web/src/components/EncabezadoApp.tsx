import { useAuth } from '../hooks/useAuth'
import { Isotipo } from './Isotipo'

function iniciales(nombre: string) {
  return nombre
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((parte) => parte[0]?.toUpperCase())
    .join('')
}

export function EncabezadoApp() {
  const { usuario, cerrarSesion } = useAuth()

  return (
    <header className="sticky top-0 z-30 border-b border-slate-200 bg-white/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
        <div className="flex items-center gap-3">
          <Isotipo />
          <div className="leading-tight">
            <p className="font-bold text-slate-900">Simulador de Créditos</p>
            <p className="hidden text-xs text-slate-500 sm:block">Universidad Técnica de Ambato</p>
          </div>
        </div>

        {usuario && (
          <div className="flex items-center gap-3">
            <div className="hidden text-right leading-tight sm:block">
              <p className="text-sm font-semibold text-slate-900">{usuario.nombreCompleto}</p>
              <p className="text-xs text-slate-500">{usuario.email}</p>
            </div>

            <span
              aria-hidden="true"
              className="flex size-9 items-center justify-center rounded-full bg-marca-100 text-sm font-bold text-marca-700"
            >
              {iniciales(usuario.nombreCompleto)}
            </span>

            <button
              onClick={cerrarSesion}
              className="rounded-lg px-3 py-2 text-sm font-semibold text-slate-600 transition hover:bg-slate-100 hover:text-slate-900"
            >
              Salir
            </button>
          </div>
        )}
      </div>
    </header>
  )
}
