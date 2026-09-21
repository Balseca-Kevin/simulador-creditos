const MONEDA = new Intl.NumberFormat('es-EC', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const NUMERO = new Intl.NumberFormat('es-EC', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const FECHA = new Intl.DateTimeFormat('es-EC', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

export const moneda = (valor: number) => MONEDA.format(valor)

export const numero = (valor: number) => NUMERO.format(valor)

export const porcentaje = (valor: number) => `${NUMERO.format(valor)} %`

export const fecha = (iso: string) => FECHA.format(new Date(iso))
