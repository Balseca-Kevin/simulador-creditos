import { useAuth } from '../hooks/useAuth'

/**
 * Marcador de posición del Sprint 1.
 * El formulario de simulación y las tablas de amortización se construyen
 * en el Sprint 3, una vez que la Credit API exista (Sprint 2).
 */
export function SimuladorPage() {
  const { usuario, cerrarSesion } = useAuth()

  return (
    <div className="min-h-full bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
          <div>
            <p className="text-xs font-semibold tracking-widest text-marca-600 uppercase">
              Simulador de Créditos
            </p>
            <h1 className="text-lg font-semibold text-slate-900">
              Hola, {usuario?.nombreCompleto}
            </h1>
          </div>

          <button
            onClick={cerrarSesion}
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100"
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-6 py-12">
        <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center">
          <h2 className="text-xl font-semibold text-slate-900">
            Sesión iniciada correctamente
          </h2>
          <p className="mx-auto mt-3 max-w-lg text-slate-600">
            Tu token JWT está almacenado y validado contra la Auth API. El
            formulario de simulación y las tablas de amortización llegan en los
            siguientes sprints.
          </p>

          <dl className="mx-auto mt-8 grid max-w-sm gap-3 text-left text-sm">
            <div className="flex justify-between border-b border-slate-100 pb-2">
              <dt className="text-slate-500">Correo</dt>
              <dd className="font-medium text-slate-900">{usuario?.email}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-slate-500">Registrado el</dt>
              <dd className="font-medium text-slate-900">
                {usuario &&
                  new Date(usuario.fechaRegistro).toLocaleDateString('es-EC', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
              </dd>
            </div>
          </dl>
        </div>
      </main>
    </div>
  )
}
