import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { AlertaError } from '../components/AlertaError'
import { CampoTexto } from '../components/CampoTexto'
import { PanelMarca } from '../components/PanelMarca'
import { useAuth } from '../hooks/useAuth'

interface ErroresCampo {
  email?: string
  password?: string
}

export function LoginPage() {
  const { login } = useAuth()
  const navegar = useNavigate()
  const ubicacion = useLocation() as { state?: { desde?: { pathname: string } } }

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errores, setErrores] = useState<ErroresCampo>({})
  const [errorGeneral, setErrorGeneral] = useState('')
  const [enviando, setEnviando] = useState(false)

  function validar(): boolean {
    const nuevos: ErroresCampo = {}

    if (!email.trim()) nuevos.email = 'Ingresa tu correo.'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
      nuevos.email = 'El formato del correo no es válido.'

    if (!password) nuevos.password = 'Ingresa tu contraseña.'

    setErrores(nuevos)
    return Object.keys(nuevos).length === 0
  }

  async function manejarEnvio(evento: React.FormEvent) {
    evento.preventDefault()
    setErrorGeneral('')

    if (!validar()) return

    setEnviando(true)
    try {
      await login({ email, password })
      navegar(ubicacion.state?.desde?.pathname ?? '/simulador', { replace: true })
    } catch (error) {
      setErrorGeneral(
        error instanceof Error ? error.message : 'No se pudo iniciar sesión.',
      )
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="grid h-full lg:grid-cols-2">
      <PanelMarca />

      <main className="flex items-center justify-center bg-slate-50 px-6 py-12">
        <div className="w-full max-w-md">
          <h2 className="text-3xl font-bold text-slate-900">Iniciar sesión</h2>
          <p className="mt-2 text-slate-600">
            Accede para simular tus créditos y comparar tablas de amortización.
          </p>

          <form onSubmit={manejarEnvio} noValidate className="mt-8 space-y-5">
            {errorGeneral && <AlertaError mensaje={errorGeneral} />}

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
              autoComplete="current-password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={errores.password}
              disabled={enviando}
            />

            <button
              type="submit"
              disabled={enviando}
              className="w-full rounded-lg bg-marca-600 px-4 py-2.5 font-semibold text-white shadow-sm transition
                hover:bg-marca-700 focus:ring-2 focus:ring-marca-300 focus:outline-none
                disabled:cursor-not-allowed disabled:opacity-60"
            >
              {enviando ? 'Verificando…' : 'Entrar'}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-600">
            ¿Aún no tienes cuenta?{' '}
            <Link
              to="/registro"
              className="font-semibold text-marca-600 hover:text-marca-700"
            >
              Regístrate aquí
            </Link>
          </p>
        </div>
      </main>
    </div>
  )
}
