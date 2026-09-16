import { createContext, useCallback, useEffect, useMemo, useState } from 'react'
import { authApi } from '../services/api'
import type { CredencialesLogin, DatosRegistro, Usuario } from '../types/auth'

interface EstadoAuth {
  usuario: Usuario | null
  token: string | null
  cargando: boolean
  login: (credenciales: CredencialesLogin) => Promise<void>
  registrar: (datos: DatosRegistro) => Promise<void>
  cerrarSesion: () => void
}

export const AuthContext = createContext<EstadoAuth | undefined>(undefined)

const CLAVE_TOKEN = 'simulador.token'

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem(CLAVE_TOKEN),
  )
  const [cargando, setCargando] = useState(true)

  // Al recargar la página se revalida el token guardado contra el API:
  // si expiró o fue manipulado, la sesión se descarta.
  useEffect(() => {
    if (!token) {
      setCargando(false)
      return
    }

    let vigente = true

    authApi
      .perfil(token)
      .then((perfil) => {
        if (vigente) setUsuario(perfil)
      })
      .catch(() => {
        if (!vigente) return
        localStorage.removeItem(CLAVE_TOKEN)
        setToken(null)
        setUsuario(null)
      })
      .finally(() => {
        if (vigente) setCargando(false)
      })

    return () => {
      vigente = false
    }
  }, [token])

  const guardarSesion = useCallback((nuevoToken: string, nuevoUsuario: Usuario) => {
    localStorage.setItem(CLAVE_TOKEN, nuevoToken)
    setToken(nuevoToken)
    setUsuario(nuevoUsuario)
    setCargando(false)
  }, [])

  const login = useCallback(
    async (credenciales: CredencialesLogin) => {
      const respuesta = await authApi.login(credenciales)
      guardarSesion(respuesta.token, respuesta.usuario)
    },
    [guardarSesion],
  )

  const registrar = useCallback(
    async (datos: DatosRegistro) => {
      const respuesta = await authApi.registrar(datos)
      guardarSesion(respuesta.token, respuesta.usuario)
    },
    [guardarSesion],
  )

  const cerrarSesion = useCallback(() => {
    localStorage.removeItem(CLAVE_TOKEN)
    setToken(null)
    setUsuario(null)
  }, [])

  const valor = useMemo(
    () => ({ usuario, token, cargando, login, registrar, cerrarSesion }),
    [usuario, token, cargando, login, registrar, cerrarSesion],
  )

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}
