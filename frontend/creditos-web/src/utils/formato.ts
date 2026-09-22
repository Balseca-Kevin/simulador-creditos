import type { FrecuenciaPago } from '../types/credito'

const MONEDA = new Intl.NumberFormat('es-EC', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const MONEDA_ENTERA = new Intl.NumberFormat('es-EC', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
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

/** Para cifras de referencia donde los centavos solo agregan ruido (atajos de monto). */
export const monedaEntera = (valor: number) => MONEDA_ENTERA.format(valor)

export const numero = (valor: number) => NUMERO.format(valor)

export const porcentaje = (valor: number, decimales = 2) =>
  `${valor.toLocaleString('es-EC', {
    minimumFractionDigits: decimales,
    maximumFractionDigits: decimales,
  })} %`

export const fecha = (iso: string) => FECHA.format(new Date(iso))

/** Plazo legible: 36 -> "3 años", 18 -> "1 año y 6 meses", 7 -> "7 meses". */
export function plazoLegible(meses: number): string {
  const anios = Math.floor(meses / 12)
  const resto = meses % 12
  const textoAnios = anios === 1 ? '1 año' : `${anios} años`
  const textoMeses = resto === 1 ? '1 mes' : `${resto} meses`

  if (anios === 0) return textoMeses
  if (resto === 0) return textoAnios
  return `${textoAnios} y ${textoMeses}`
}

export interface OpcionFrecuencia {
  valor: FrecuenciaPago
  etiqueta: string
  /** Sustantivo para la cuota: "Cuota mensual", "Cuota trimestral"… */
  adjetivo: string
  /** Meses por cuota; null cuando el período es el plazo completo. */
  meses: number | null
}

export const FRECUENCIAS: OpcionFrecuencia[] = [
  { valor: 'Mensual', etiqueta: 'Mensual', adjetivo: 'mensual', meses: 1 },
  { valor: 'Bimensual', etiqueta: 'Bimensual', adjetivo: 'bimensual', meses: 2 },
  { valor: 'Trimestral', etiqueta: 'Trimestral', adjetivo: 'trimestral', meses: 3 },
  { valor: 'Semestral', etiqueta: 'Semestral', adjetivo: 'semestral', meses: 6 },
  { valor: 'AlVencimiento', etiqueta: 'Al vencimiento', adjetivo: 'única', meses: null },
]

export const opcionFrecuencia = (valor: FrecuenciaPago) =>
  FRECUENCIAS.find((f) => f.valor === valor) ?? FRECUENCIAS[0]

/** Una frecuencia es válida si el plazo se divide exactamente en sus cuotas. */
export const frecuenciaCompatible = (frecuencia: OpcionFrecuencia, plazoMeses: number) =>
  frecuencia.meses === null || (plazoMeses > 0 && plazoMeses % frecuencia.meses === 0)
