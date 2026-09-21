import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { AlertaError } from '../components/AlertaError'
import { CampoTexto } from '../components/CampoTexto'
import { PanelMarca } from '../components/PanelMarca'
import { Isotipo } from '../components/Isotipo'
import { useAuth } from '../hooks/useAuth'

interface ErroresCampo {
  nombreCompleto?: string
  email?: string
  password?: string
  confirmacion?: string
}

export function RegistroPage() {
  const { registrar } = useAuth()
  const navegar = useNavigate()

  const [nombreCompleto, setNombreCompleto] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmacion, setConfirmacion] = useState('')
  const [errores, setErrores] = useState<ErroresCampo>({})
  const [errorGeneral, setErrorGeneral] = useState('')
  const [enviando, setEnviando] = useState(false)

  function validar(): boolean {
    const nuevos: ErroresCampo = {}

    if (nombreCompleto.trim().length < 3)
      nuevos.nombreCompleto = 'Ingresa tu nombre completo (mínimo 3 caracteres).'

    if (!email.trim()) nuevos.email = 'Ingresa tu correo.'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
      nuevos.email = 'El formato del correo no es válido.'

    if (password.length < 8)
      nuevos.password = 'La contraseña debe tener al menos 8 caracteres.'

    if (confirmacion !== password)
      nuevos.confirmacion = 'Las contraseñas no coinciden.'

    setErrores(nuevos)
    return Object.keys(nuevos).length === 0
  }

  async function manejarEnvio(evento: React.FormEvent) {
    evento.preventDefault()
    setErrorGeneral('')

    if (!validar()) return

    setEnviando(true)
    try {
      await registrar({ nombreCompleto, email, password })
      navegar('/simulador', { replace: true })
    } catch (error) {
      setErrorGeneral(
        error instanceof Error ? error.message : 'No se pudo crear la cuenta.',
      )
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="grid h-full lg:grid-cols-2">
      <PanelMarca />

      <main className="flex items-center justify-center overflow-y-auto bg-white px-6 py-12">
        <div className="w-full max-w-md">
          <div className="mb-10 flex items-center gap-3 lg:hidden">
            <Isotipo />
            <p className="font-bold text-slate-900">Simulador de Créditos</p>
          </div>

          <h2 className="text-3xl font-bold tracking-tight text-slate-900">Crear cuenta</h2>
          <p className="mt-2 text-slate-600">
            Regístrate para guardar y comparar tus simulaciones.
          </p>

          <form onSubmit={manejarEnvio} noValidate className="mt-8 space-y-5">
            {errorGeneral && <AlertaError mensaje={errorGeneral} />}

            <CampoTexto
              etiqueta="Nombre completo"
              name="nombreCompleto"
              autoComplete="name"
              placeholder="Kevin Balseca"
              value={nombreCompleto}
              onChange={(e) => setNombreCompleto(e.target.value)}
              error={errores.nombreCompleto}
              disabled={enviando}
            />

            <CampoTexto
              etiqueta="Correo electrónico"
              name="email"
              type="email"
              autoComplete="email"
              placeholder="nombre@correo.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              error={errores.email}
              disabled={enviando}
            />

            <CampoTexto
              etiqueta="Contraseña"
              name="password"
              type="password"
              autoComplete="new-password"
              placeholder="Mínimo 8 caracteres"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={errores.password}
              disabled={enviando}
            />

            <CampoTexto
              etiqueta="Repetir contraseña"
              name="confirmacion"
              type="password"
              autoComplete="new-password"
              placeholder="••••••••"
              value={confirmacion}
              onChange={(e) => setConfirmacion(e.target.value)}
              error={errores.confirmacion}
              disabled={enviando}
            />

            <button
              type="submit"
              disabled={enviando}
              className="w-full rounded-xl bg-marca-600 px-4 py-3 text-base font-semibold text-white shadow-sm transition hover:bg-marca-700 focus:ring-4 focus:ring-marca-200 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60"
            >
              {enviando ? 'Creando cuenta…' : 'Registrarme'}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-600">
            ¿Ya tienes cuenta?{' '}
            <Link
              to="/login"
              className="font-semibold text-marca-600 hover:text-marca-700"
            >
              Inicia sesión
            </Link>
          </p>
        </div>
      </main>
    </div>
  )
}
