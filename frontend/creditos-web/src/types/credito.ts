export type FrecuenciaPago = 'Mensual' | 'Bimensual' | 'Trimestral' | 'Semestral' | 'AlVencimiento'

export interface TipoCredito {
  id: number
  codigo: string
  nombre: string
  categoria: string
  tasaAnual: number
  tasaSeguroDesgravamenMensual: number
  descripcion: string
}

export interface CuotaAmortizacion {
  periodo: number
  /** Capital más interés. En el método francés es constante. */
  cuota: number
  interes: number
  capital: number
  seguro: number
  /** Lo que efectivamente se paga: cuota más seguro. */
  cuotaTotal: number
  saldoRestante: number
}

export interface TablaAmortizacion {
  metodo: 'Frances' | 'Aleman'
  cuotas: CuotaAmortizacion[]
  totalCapital: number
  totalInteres: number
  totalSeguro: number
  totalPagado: number
  primeraCuota: number
  ultimaCuota: number
  primeraCuotaTotal: number
  ultimaCuotaTotal: number
  cuotaTotalMaxima: number
  ingresoMinimoRequerido: number
}

export interface ComparativoMetodos {
  metodoMasEconomico: 'Frances' | 'Aleman'
  diferenciaTotalInteres: number
}

export interface Simulacion {
  id: string
  fechaSimulacion: string
  tipoCredito: TipoCredito
  monto: number
  plazoMeses: number
  frecuenciaPago: FrecuenciaPago
  numeroCuotas: number
  mesesPorPeriodo: number
  incluyeSeguroDesgravamen: boolean
  tasaAnualAplicada: number
  tasaPeriodicaAplicada: number
  relacionCuotaIngreso: number
  francesa: TablaAmortizacion
  alemana: TablaAmortizacion
  comparativo: ComparativoMetodos
}

export interface SimulacionRequest {
  tipoCreditoId: number
  monto: number
  plazoMeses: number
  frecuenciaPago: FrecuenciaPago
  incluirSeguroDesgravamen: boolean
}

export interface SimulacionHistorial {
  id: string
  fechaSimulacion: string
  tipoCredito: string
  monto: number
  plazoMeses: number
  frecuenciaPago: FrecuenciaPago
  incluyeSeguroDesgravamen: boolean
  tasaAnualAplicada: number
  cuotaFija: number
  totalInteresFrances: number
  totalInteresAleman: number
  ingresoMinimoRequerido: number
}

export interface EstimacionesRequest {
  tipoCreditoId: number
  monto: number
  plazoMeses: number
  frecuenciaPago: FrecuenciaPago
  incluirSeguroDesgravamen: boolean
}

export interface EstimacionTipo {
  tipoCreditoId: number
  nombre: string
  categoria: string
  descripcion: string
  tasaAnual: number
  cuotaEstimada: number | null
  ingresoMinimoRequerido: number | null
}

export interface EstimacionMonto {
  monto: number
  cuotaEstimada: number | null
}

export interface EstimacionPlazo {
  plazoMeses: number
  compatibleConFrecuencia: boolean
  cuotaEstimada: number | null
}

export interface Estimaciones {
  porTipo: EstimacionTipo[]
  porMonto: EstimacionMonto[]
  porPlazo: EstimacionPlazo[]
  metodo: string
}
