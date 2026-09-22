export interface Usuario {
  id: string
  nombreCompleto: string
  email: string
  fechaRegistro: string
}

export interface RespuestaAuth {
  token: string
  expiraEn: string
  usuario: Usuario
}

export interface CredencialesLogin {
  email: string
  password: string
}

export interface DatosRegistro extends CredencialesLogin {
  nombreCompleto: string
}
