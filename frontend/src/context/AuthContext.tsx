import { useCallback, useEffect, useMemo, useState } from 'react'
import { authApi } from '../services/api'
import type { CredencialesLogin, DatosRegistro, Usuario } from '../types/auth'
import { AuthContext, CLAVE_TOKEN, leerTokenGuardado } from './contextoAuth'

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [token, setToken] = useState<string | null>(leerTokenGuardado)

  // Solo hay algo que verificar si había un token guardado; así la pantalla de
  // carga no aparece para quien entra sin sesión.
  const [cargando, setCargando] = useState(() => leerTokenGuardado() !== null)

  // Al recargar la página se revalida el token guardado contra el API:
  // si expiró o fue manipulado, la sesión se descarta.
  useEffect(() => {
    if (!token) return

    let vigente = true

    authApi
      .perfil(token)
      .then((perfil) => {
        if (vigente) setUsuario(perfil)
      })
      .catch(() => {
        if (!vigente) return
        try {
          localStorage.removeItem(CLAVE_TOKEN)
        } catch {
          // Sin almacenamiento disponible basta con limpiar el estado en memoria.
        }
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
    try {
      localStorage.setItem(CLAVE_TOKEN, nuevoToken)
    } catch {
      // La sesión seguirá viva en memoria aunque no se pueda persistir.
    }
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
    try {
      localStorage.removeItem(CLAVE_TOKEN)
    } catch {
      // Igual que arriba: limpiar el estado es suficiente.
    }
    setToken(null)
    setUsuario(null)
  }, [])

  const valor = useMemo(
    () => ({ usuario, token, cargando, login, registrar, cerrarSesion }),
    [usuario, token, cargando, login, registrar, cerrarSesion],
  )

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}
