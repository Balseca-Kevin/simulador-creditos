import type {
  CredencialesLogin,
  DatosRegistro,
  RespuestaAuth,
  Usuario,
} from '../types/auth'

const AUTH_API = import.meta.env.VITE_AUTH_API_URL ?? 'http://localhost:5080'

/** Error con el mensaje que el API devolvió, listo para mostrarse al usuario. */
export class ErrorApi extends Error {
  readonly estado: number

  constructor(mensaje: string, estado: number) {
    super(mensaje)
    this.name = 'ErrorApi'
    this.estado = estado
  }
}

/**
 * Traduce la respuesta del API a un mensaje en español.
 * ASP.NET devuelve los errores de validación en `errors`, y los nuestros en `mensaje`.
 */
async function interpretarError(respuesta: Response): Promise<never> {
  let mensaje = 'No se pudo completar la operación. Inténtalo de nuevo.'

  try {
    const cuerpo = await respuesta.json()

    if (typeof cuerpo?.mensaje === 'string') {
      mensaje = cuerpo.mensaje
    } else if (cuerpo?.errors && typeof cuerpo.errors === 'object') {
      const detalles = Object.values(cuerpo.errors).flat() as string[]
      if (detalles.length > 0) mensaje = detalles.join(' ')
    } else if (typeof cuerpo?.title === 'string') {
      mensaje = cuerpo.title
    }
  } catch {
    // El cuerpo no era JSON; se conserva el mensaje genérico.
  }

  throw new ErrorApi(mensaje, respuesta.status)
}

async function pedir<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
  let respuesta: Response

  try {
    respuesta = await fetch(`${AUTH_API}${ruta}`, {
      ...opciones,
      headers: {
        'Content-Type': 'application/json',
        ...opciones.headers,
      },
    })
  } catch {
    throw new ErrorApi(
      'No se pudo contactar al servidor. Verifica que la Auth API esté en ejecución.',
      0,
    )
  }

  if (!respuesta.ok) await interpretarError(respuesta)

  return respuesta.json() as Promise<T>
}

export const authApi = {
  registrar: (datos: DatosRegistro) =>
    pedir<RespuestaAuth>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),

  login: (credenciales: CredencialesLogin) =>
    pedir<RespuestaAuth>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(credenciales),
    }),

  perfil: (token: string) =>
    pedir<Usuario>('/api/auth/me', {
      headers: { Authorization: `Bearer ${token}` },
    }),
}
