import type {
  CredencialesLogin,
  DatosRegistro,
  RespuestaAuth,
  Usuario,
} from '../types/auth'
import type {
  Simulacion,
  SimulacionHistorial,
  SimulacionRequest,
  TipoCredito,
} from '../types/credito'

/**
 * La SPA habla con un unico origen: el ApiGateway. El se encarga de dirigir
 * /api/auth hacia AuthService y /api/creditos hacia CreditService, asi que el
 * frontend no necesita conocer los puertos de cada microservicio.
 */
const API = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

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

  if (respuesta.status === 401) {
    mensaje = 'Tu sesión expiró. Vuelve a iniciar sesión.'
  }

  throw new ErrorApi(mensaje, respuesta.status)
}

async function pedir<T>(base: string, ruta: string, opciones: RequestInit = {}): Promise<T> {
  let respuesta: Response

  try {
    respuesta = await fetch(`${base}${ruta}`, {
      ...opciones,
      headers: {
        'Content-Type': 'application/json',
        ...opciones.headers,
      },
    })
  } catch {
    throw new ErrorApi(
      'No se pudo contactar al servidor. Verifica que los servicios estén en ejecución.',
      0,
    )
  }

  if (!respuesta.ok) await interpretarError(respuesta)

  return respuesta.json() as Promise<T>
}

const conToken = (token: string) => ({ Authorization: `Bearer ${token}` })

export const authApi = {
  registrar: (datos: DatosRegistro) =>
    pedir<RespuestaAuth>(API, '/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),

  login: (credenciales: CredencialesLogin) =>
    pedir<RespuestaAuth>(API, '/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(credenciales),
    }),

  perfil: (token: string) =>
    pedir<Usuario>(API, '/api/auth/me', { headers: conToken(token) }),
}

export const creditApi = {
  tipos: (token: string) =>
    pedir<TipoCredito[]>(API, '/api/creditos/tipos', { headers: conToken(token) }),

  simular: (token: string, datos: SimulacionRequest) =>
    pedir<Simulacion>(API, '/api/creditos/simular', {
      method: 'POST',
      headers: conToken(token),
      body: JSON.stringify(datos),
    }),

  historial: (token: string) =>
    pedir<SimulacionHistorial[]>(API, '/api/creditos/historial', {
      headers: conToken(token),
    }),
}
