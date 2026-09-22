import { useContext } from 'react'
import { AuthContext } from '../context/contextoAuth'

export function useAuth() {
  const contexto = useContext(AuthContext)

  if (!contexto) {
    throw new Error('useAuth debe usarse dentro de un AuthProvider.')
  }

  return contexto
}
