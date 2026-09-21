export interface TipoCredito {
  id: number
  codigo: string
  nombre: string
  tasaAnual: number
  descripcion: string
}

export interface CuotaAmortizacion {
  periodo: number
  cuota: number
  interes: number
  capital: number
  saldoRestante: number
}

export interface TablaAmortizacion {
  metodo: string
  cuotas: CuotaAmortizacion[]
  totalCapital: number
  totalInteres: number
  totalPagado: number
  primeraCuota: number
  ultimaCuota: number
}

export interface ComparativoMetodos {
  metodoMasEconomico: string
  diferenciaTotalInteres: number
}

export interface Simulacion {
  id: string
  fechaSimulacion: string
  tipoCredito: TipoCredito
  monto: number
  plazoMeses: number
  tasaAnualAplicada: number
  tasaMensualAplicada: number
  francesa: TablaAmortizacion
  alemana: TablaAmortizacion
  comparativo: ComparativoMetodos
}

export interface SimulacionRequest {
  tipoCreditoId: number
  monto: number
  plazoMeses: number
}

export interface SimulacionHistorial {
  id: string
  fechaSimulacion: string
  tipoCredito: string
  monto: number
  plazoMeses: number
  tasaAnualAplicada: number
  cuotaFija: number
  totalInteresFrances: number
  totalInteresAleman: number
}
