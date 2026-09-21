import { createContext } from 'react'
import type { CredencialesLogin, DatosRegistro, Usuario } from '../types/auth'

export interface EstadoAuth {
  usuario: Usuario | null
  token: string | null
  cargando: boolean
  login: (credenciales: CredencialesLogin) => Promise<void>
  registrar: (datos: DatosRegistro) => Promise<void>
  cerrarSesion: () => void
}

/**
 * Vive en su propio archivo, separado del proveedor: si un módulo exporta a la
 * vez un contexto y un componente, Vite pierde el refresco en caliente.
 */
export const AuthContext = createContext<EstadoAuth | undefined>(undefined)

export const CLAVE_TOKEN = 'simulador.token'

/** Lectura tolerante del token: en modo privado localStorage puede lanzar. */
export function leerTokenGuardado(): string | null {
  try {
    return localStorage.getItem(CLAVE_TOKEN)
  } catch {
    return null
  }
}
