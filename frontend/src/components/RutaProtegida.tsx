import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

/**
 * Deja pasar solo a quien tenga una sesión válida.
 * Mientras se revalida el token guardado se muestra un estado de carga,
 * para no expulsar al usuario por un parpadeo.
 */
export function RutaProtegida() {
  const { usuario, cargando } = useAuth()
  const ubicacion = useLocation()

  if (cargando) {
    return (
      <div className="flex h-full items-center justify-center bg-slate-50">
        <div className="flex flex-col items-center gap-3">
          <span className="size-8 animate-spin rounded-full border-3 border-marca-100 border-t-marca-600" />
          <p className="text-sm text-slate-500">Verificando tu sesión…</p>
        </div>
      </div>
    )
  }

  return usuario ? <Outlet /> : <Navigate to="/login" replace state={{ desde: ubicacion }} />
}
