import { useState } from 'react'
import type { Activo, ActivoRequest, CategoriaActivo } from '../types/activo'
import { monedaEntera } from '../utils/formato'
import { AlertaError } from './AlertaError'
import { CampoTexto } from './CampoTexto'

const VALOR_MINIMO = 1
const VALOR_MAXIMO = 100_000_000

interface Props {
  categorias: CategoriaActivo[]
  /** Si viene, el formulario edita ese activo en lugar de crear uno nuevo. */
  activo: Activo | null
  guardando: boolean
  error: string
  onGuardar: (datos: ActivoRequest) => void
  onCancelar: () => void
}

interface Errores {
  categoria?: string
  nombre?: string
  valor?: string
  fecha?: string
}

const hoy = () => new Date().toISOString().slice(0, 10)

export function FormularioActivo({
  categorias,
  activo,
  guardando,
  error,
  onGuardar,
  onCancelar,
}: Props) {
  const [categoriaId, setCategoriaId] = useState(activo?.categoria.id ?? categorias[0]?.id ?? 0)
  const [nombre, setNombre] = useState(activo?.nombre ?? '')
  const [descripcion, setDescripcion] = useState(activo?.descripcion ?? '')
  const [valor, setValor] = useState(activo ? String(activo.valorEstimado) : '')
  const [fecha, setFecha] = useState(activo?.fechaAdquisicion ?? hoy())
  const [errores, setErrores] = useState<Errores>({})

  function validar(): ActivoRequest | null {
    const nuevos: Errores = {}

    if (!categoriaId) nuevos.categoria = 'Selecciona una categoría.'

    if (nombre.trim().length < 3) nuevos.nombre = 'El nombre debe tener al menos 3 caracteres.'

    const valorNumero = Number(valor.replace(/[\s,]/g, ''))
    if (!valor.trim() || Number.isNaN(valorNumero)) {
      nuevos.valor = 'Ingresa un valor válido.'
    } else if (valorNumero < VALOR_MINIMO || valorNumero > VALOR_MAXIMO) {
      nuevos.valor = `El valor debe estar entre ${monedaEntera(VALOR_MINIMO)} y ${monedaEntera(VALOR_MAXIMO)}.`
    }

    if (!fecha) {
      nuevos.fecha = 'Ingresa la fecha de adquisición.'
    } else if (fecha > hoy()) {
      // Se valida aquí y también en el servidor: el usuario ve el error al
      // instante, pero la regla real vive en el microservicio.
      nuevos.fecha = 'La fecha de adquisición no puede estar en el futuro.'
    }

    setErrores(nuevos)
    if (Object.keys(nuevos).length > 0) return null

    return {
      categoriaId,
      nombre: nombre.trim(),
      descripcion: descripcion.trim(),
      valorEstimado: valorNumero,
      fechaAdquisicion: fecha,
    }
  }

  function manejarEnvio(evento: React.FormEvent) {
    evento.preventDefault()
    const datos = validar()
    if (datos) onGuardar(datos)
  }

  return (
    <form
      onSubmit={manejarEnvio}
      noValidate
      className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-7"
    >
      <h2 className="text-lg font-semibold text-slate-900">
        {activo ? 'Editar garantía' : 'Registrar una garantía'}
      </h2>
      <p className="mt-1 text-sm text-slate-500">
        Los bienes que declares forman tu respaldo patrimonial. No salen de tu cuenta.
      </p>

      <div className="mt-6 space-y-5">
        {error && <AlertaError mensaje={error} />}

        <div className="flex flex-col gap-1.5">
          <label htmlFor="categoria" className="text-sm font-semibold text-slate-700">
            Categoría
          </label>
          <select
            id="categoria"
            value={categoriaId}
            onChange={(e) => setCategoriaId(Number(e.target.value))}
            disabled={guardando}
            className="w-full rounded-xl border-2 border-slate-200 px-4 py-3 text-slate-900 outline-none transition focus:border-marca-500 focus:ring-4 focus:ring-marca-100 disabled:bg-slate-50"
          >
            {categorias.map((c) => (
              <option key={c.id} value={c.id}>
                {c.nombre}
              </option>
            ))}
          </select>
          {errores.categoria && (
            <p role="alert" className="text-sm font-medium text-red-600">
              {errores.categoria}
            </p>
          )}
        </div>

        <CampoTexto
          etiqueta="Nombre del bien"
          name="nombre"
          placeholder="Ej. Camioneta Mazda BT-50"
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          error={errores.nombre}
          disabled={guardando}
        />

        <CampoTexto
          etiqueta="Descripción (opcional)"
          name="descripcion"
          placeholder="Detalles que ayuden a identificarlo"
          value={descripcion}
          onChange={(e) => setDescripcion(e.target.value)}
          disabled={guardando}
        />

        <div className="grid gap-5 sm:grid-cols-2">
          <CampoTexto
            etiqueta="Valor estimado"
            name="valor"
            inputMode="decimal"
            placeholder="25000"
            value={valor}
            onChange={(e) => setValor(e.target.value)}
            error={errores.valor}
            disabled={guardando}
          />

          <CampoTexto
            etiqueta="Fecha de adquisición"
            name="fecha"
            type="date"
            max={hoy()}
            value={fecha}
            onChange={(e) => setFecha(e.target.value)}
            error={errores.fecha}
            disabled={guardando}
          />
        </div>
      </div>

      <div className="mt-6 flex flex-col gap-3 border-t border-slate-100 pt-5 sm:flex-row sm:justify-end">
        <button
          type="button"
          onClick={onCancelar}
          disabled={guardando}
          className="rounded-xl border-2 border-slate-200 px-5 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-50 disabled:opacity-60"
        >
          Cancelar
        </button>

        <button
          type="submit"
          disabled={guardando}
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-marca-600 px-6 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-marca-700 focus:ring-4 focus:ring-marca-200 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60"
        >
          {guardando && (
            <span className="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          )}
          {guardando ? 'Guardando…' : activo ? 'Guardar cambios' : 'Registrar garantía'}
        </button>
      </div>
    </form>
  )
}
