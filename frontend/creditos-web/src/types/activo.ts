export interface CategoriaActivo {
  id: number
  codigo: string
  nombre: string
  descripcion: string
}

export interface Activo {
  id: string
  categoria: CategoriaActivo
  nombre: string
  descripcion: string
  valorEstimado: number
  /** Fecha en formato ISO corto (aaaa-mm-dd). */
  fechaAdquisicion: string
  fechaRegistro: string
  fechaActualizacion: string | null
}

export interface ActivoRequest {
  categoriaId: number
  nombre: string
  descripcion: string
  valorEstimado: number
  fechaAdquisicion: string
}

export interface PatrimonioPorCategoria {
  categoria: string
  cantidad: number
  valor: number
}

export interface ResumenPatrimonio {
  cantidadActivos: number
  valorTotal: number
  porCategoria: PatrimonioPorCategoria[]
}
