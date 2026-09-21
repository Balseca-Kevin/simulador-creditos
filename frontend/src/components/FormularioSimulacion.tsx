import { useState } from 'react'
import type { SimulacionRequest, TipoCredito } from '../types/credito'
import { moneda, porcentaje } from '../utils/formato'

const MONTO_MINIMO = 100
const MONTO_MAXIMO = 1_000_000
const PLAZO_MINIMO = 1
const PLAZO_MAXIMO = 480
const PLAZOS_SUGERIDOS = [12, 24, 36, 60, 120]

interface Props {
  tipos: TipoCredito[]
  simulando: boolean
  onSimular: (datos: SimulacionRequest) => void
}

interface Errores {
  tipo?: string
  monto?: string
  plazo?: string
}

export function FormularioSimulacion({ tipos, simulando, onSimular }: Props) {
  const [tipoId, setTipoId] = useState<number | null>(tipos[0]?.id ?? null)
  const [monto, setMonto] = useState('10000')
  const [plazo, setPlazo] = useState('12')
  const [errores, setErrores] = useState<Errores>({})

  const tipoElegido = tipos.find((t) => t.id === tipoId)

  function validar(): SimulacionRequest | null {
    const nuevos: Errores = {}

    if (tipoId === null) nuevos.tipo = 'Selecciona un tipo de crédito.'

    const montoNumero = Number(monto.replace(',', '.'))
    if (!monto.trim() || Number.isNaN(montoNumero)) {
      nuevos.monto = 'Ingresa un monto válido.'
    } else if (montoNumero < MONTO_MINIMO || montoNumero > MONTO_MAXIMO) {
      nuevos.monto = `El monto debe estar entre ${moneda(MONTO_MINIMO)} y ${moneda(MONTO_MAXIMO)}.`
    }

    const plazoNumero = Number(plazo)
    if (!plazo.trim() || !Number.isInteger(plazoNumero)) {
      nuevos.plazo = 'Ingresa un número entero de meses.'
    } else if (plazoNumero < PLAZO_MINIMO || plazoNumero > PLAZO_MAXIMO) {
      nuevos.plazo = `El plazo debe estar entre ${PLAZO_MINIMO} y ${PLAZO_MAXIMO} meses.`
    }

    setErrores(nuevos)
    if (Object.keys(nuevos).length > 0 || tipoId === null) return null

    return { tipoCreditoId: tipoId, monto: montoNumero, plazoMeses: plazoNumero }
  }

  function manejarEnvio(evento: React.FormEvent) {
    evento.preventDefault()
    const datos = validar()
    if (datos) onSimular(datos)
  }

  return (
    <form
      onSubmit={manejarEnvio}
      noValidate
      className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
    >
      <h2 className="text-lg font-semibold text-slate-900">Datos del crédito</h2>
      <p className="mt-1 text-sm text-slate-500">
        El tipo que elijas determina la tasa de interés aplicada.
      </p>

      <fieldset className="mt-6">
        <legend className="text-sm font-medium text-slate-700">Tipo de crédito</legend>

        <div className="mt-3 grid gap-3 sm:grid-cols-3">
          {tipos.map((tipo) => {
            const seleccionado = tipo.id === tipoId

            return (
              <label
                key={tipo.id}
                className={`cursor-pointer rounded-lg border p-4 transition ${
                  seleccionado
                    ? 'border-marca-500 bg-marca-50 ring-2 ring-marca-300'
                    : 'border-slate-200 hover:border-marca-300 hover:bg-slate-50'
                }`}
              >
                <input
                  type="radio"
                  name="tipoCredito"
                  value={tipo.id}
                  checked={seleccionado}
                  onChange={() => setTipoId(tipo.id)}
                  disabled={simulando}
                  className="sr-only"
                />

                <span className="block text-2xl font-bold tabular-nums text-marca-700">
                  {porcentaje(tipo.tasaAnual)}
                </span>
                <span className="mt-1 block text-sm font-semibold text-slate-900">
                  {tipo.nombre}
                </span>
                <span className="mt-1 block text-xs leading-snug text-slate-500">
                  {tipo.descripcion}
                </span>
              </label>
            )
          })}
        </div>

        {errores.tipo && (
          <p role="alert" className="mt-2 text-sm text-red-600">
            {errores.tipo}
          </p>
        )}
      </fieldset>

      <div className="mt-6 grid gap-5 sm:grid-cols-2">
        <div className="flex flex-col gap-1.5">
          <label htmlFor="monto" className="text-sm font-medium text-slate-700">
            Monto a financiar
          </label>

          <div className="relative">
            <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-400">
              $
            </span>
            <input
              id="monto"
              inputMode="decimal"
              value={monto}
              onChange={(e) => setMonto(e.target.value)}
              disabled={simulando}
              aria-invalid={Boolean(errores.monto)}
              className={`w-full rounded-lg border py-2.5 pr-3.5 pl-7 tabular-nums text-slate-900 shadow-sm outline-none transition focus:ring-2 disabled:bg-slate-50 ${
                errores.monto
                  ? 'border-red-400 focus:border-red-500 focus:ring-red-200'
                  : 'border-slate-300 focus:border-marca-500 focus:ring-marca-300'
              }`}
            />
          </div>

          {errores.monto && (
            <p role="alert" className="text-sm text-red-600">
              {errores.monto}
            </p>
          )}
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="plazo" className="text-sm font-medium text-slate-700">
            Plazo en meses
          </label>

          <input
            id="plazo"
            inputMode="numeric"
            value={plazo}
            onChange={(e) => setPlazo(e.target.value)}
            disabled={simulando}
            aria-invalid={Boolean(errores.plazo)}
            className={`w-full rounded-lg border px-3.5 py-2.5 tabular-nums text-slate-900 shadow-sm outline-none transition focus:ring-2 disabled:bg-slate-50 ${
              errores.plazo
                ? 'border-red-400 focus:border-red-500 focus:ring-red-200'
                : 'border-slate-300 focus:border-marca-500 focus:ring-marca-300'
            }`}
          />

          <div className="flex flex-wrap gap-1.5">
            {PLAZOS_SUGERIDOS.map((meses) => (
              <button
                key={meses}
                type="button"
                onClick={() => setPlazo(String(meses))}
                disabled={simulando}
                className={`rounded-md px-2.5 py-1 text-xs font-medium transition ${
                  plazo === String(meses)
                    ? 'bg-marca-600 text-white'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                {meses} m
              </button>
            ))}
          </div>

          {errores.plazo && (
            <p role="alert" className="text-sm text-red-600">
              {errores.plazo}
            </p>
          )}
        </div>
      </div>

      <div className="mt-6 flex flex-col items-start gap-3 border-t border-slate-100 pt-5 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-slate-500">
          {tipoElegido
            ? `Se aplicará una tasa anual de ${porcentaje(tipoElegido.tasaAnual)}.`
            : 'Selecciona un tipo de crédito para continuar.'}
        </p>

        <button
          type="submit"
          disabled={simulando}
          className="inline-flex w-full items-center justify-center gap-2 rounded-lg bg-marca-600 px-6 py-2.5 font-semibold text-white shadow-sm transition hover:bg-marca-700 focus:ring-2 focus:ring-marca-300 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60 sm:w-auto"
        >
          {simulando && (
            <span className="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          )}
          {simulando ? 'Calculando…' : 'Simular crédito'}
        </button>
      </div>
    </form>
  )
}
