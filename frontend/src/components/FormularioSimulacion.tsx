import { useState } from 'react'
import type { FrecuenciaPago, SimulacionRequest, TipoCredito } from '../types/credito'
import {
  FRECUENCIAS,
  frecuenciaCompatible,
  moneda,
  monedaEntera,
  opcionFrecuencia,
  plazoLegible,
  porcentaje,
} from '../utils/formato'

const MONTO_MINIMO = 100
const MONTO_MAXIMO = 1_000_000
const PLAZO_MINIMO = 1
const PLAZO_MAXIMO = 480
const MONTOS_SUGERIDOS = [5_000, 10_000, 25_000, 50_000, 100_000]
const PLAZOS_SUGERIDOS = [12, 24, 36, 60, 120, 240]

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

/** Encabezado numerado de cada paso del formulario. */
function Paso({
  numero,
  titulo,
  ayuda,
  children,
}: {
  numero: number
  titulo: string
  ayuda?: string
  children: React.ReactNode
}) {
  return (
    <section className="border-t border-slate-100 px-5 py-6 first:border-t-0 sm:px-7">
      <div className="flex items-start gap-3">
        <span className="flex size-7 shrink-0 items-center justify-center rounded-full bg-marca-600 text-sm font-bold text-white">
          {numero}
        </span>
        <div className="min-w-0 flex-1">
          <h3 className="text-base font-semibold text-slate-900">{titulo}</h3>
          {ayuda && <p className="mt-0.5 text-sm text-slate-500">{ayuda}</p>}
          <div className="mt-4">{children}</div>
        </div>
      </div>
    </section>
  )
}

function Atajo({
  activo,
  disabled,
  onClick,
  children,
}: {
  activo: boolean
  disabled: boolean
  onClick: () => void
  children: React.ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={`rounded-full border px-3 py-1 text-sm font-medium tabular-nums transition disabled:cursor-not-allowed disabled:opacity-50 ${
        activo
          ? 'border-marca-600 bg-marca-600 text-white'
          : 'border-slate-200 bg-white text-slate-600 hover:border-marca-300 hover:text-marca-700'
      }`}
    >
      {children}
    </button>
  )
}

function MensajeError({ children }: { children?: string }) {
  if (!children) return null
  return (
    <p role="alert" className="mt-2 text-sm font-medium text-red-600">
      {children}
    </p>
  )
}

export function FormularioSimulacion({ tipos, simulando, onSimular }: Props) {
  const [tipoId, setTipoId] = useState<number | null>(tipos[0]?.id ?? null)
  const [monto, setMonto] = useState('10000')
  const [plazo, setPlazo] = useState('24')
  const [frecuencia, setFrecuencia] = useState<FrecuenciaPago>('Mensual')
  const [conSeguro, setConSeguro] = useState(true)
  const [errores, setErrores] = useState<Errores>({})

  const tipoElegido = tipos.find((t) => t.id === tipoId)
  const plazoNumero = Number(plazo)
  const plazoValido = Number.isInteger(plazoNumero) && plazoNumero >= PLAZO_MINIMO

  /**
   * Si el nuevo plazo no admite la frecuencia elegida (10 meses no se divide en
   * trimestres), se vuelve a mensual en lugar de dejar el formulario en un
   * estado que el servidor rechazaría.
   */
  function cambiarPlazo(valor: string) {
    setPlazo(valor)
    const meses = Number(valor)
    if (Number.isInteger(meses) && !frecuenciaCompatible(opcionFrecuencia(frecuencia), meses)) {
      setFrecuencia('Mensual')
    }
  }

  function validar(): SimulacionRequest | null {
    const nuevos: Errores = {}

    if (tipoId === null) nuevos.tipo = 'Selecciona un tipo de crédito.'

    const montoNumero = Number(monto.replace(/\s/g, '').replace(',', '.'))
    if (!monto.trim() || Number.isNaN(montoNumero)) {
      nuevos.monto = 'Ingresa un monto válido.'
    } else if (montoNumero < MONTO_MINIMO || montoNumero > MONTO_MAXIMO) {
      nuevos.monto = `El monto debe estar entre ${monedaEntera(MONTO_MINIMO)} y ${monedaEntera(MONTO_MAXIMO)}.`
    }

    if (!plazo.trim() || !Number.isInteger(plazoNumero)) {
      nuevos.plazo = 'Ingresa un número entero de meses.'
    } else if (plazoNumero < PLAZO_MINIMO || plazoNumero > PLAZO_MAXIMO) {
      nuevos.plazo = `El plazo debe estar entre ${PLAZO_MINIMO} y ${PLAZO_MAXIMO} meses.`
    }

    setErrores(nuevos)
    if (Object.keys(nuevos).length > 0 || tipoId === null) return null

    return {
      tipoCreditoId: tipoId,
      monto: montoNumero,
      plazoMeses: plazoNumero,
      frecuenciaPago: frecuencia,
      incluirSeguroDesgravamen: conSeguro,
    }
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
      className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
    >
      <Paso numero={1} titulo="¿Qué tipo de crédito necesitas?" ayuda="La tasa de interés depende del tipo que elijas.">
        <div role="radiogroup" aria-label="Tipo de crédito" className="grid gap-3 md:grid-cols-3">
          {tipos.map((tipo) => {
            const seleccionado = tipo.id === tipoId

            return (
              <label
                key={tipo.id}
                className={`relative flex cursor-pointer flex-col rounded-xl border-2 p-4 transition ${
                  seleccionado
                    ? 'border-marca-600 bg-marca-50'
                    : 'border-slate-200 hover:border-marca-300'
                } ${simulando ? 'pointer-events-none opacity-60' : ''}`}
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

                <span
                  aria-hidden="true"
                  className={`absolute top-3 right-3 flex size-5 items-center justify-center rounded-full border-2 ${
                    seleccionado ? 'border-marca-600 bg-marca-600' : 'border-slate-300 bg-white'
                  }`}
                >
                  {seleccionado && <span className="size-2 rounded-full bg-white" />}
                </span>

                <span className="pr-6 text-sm font-semibold text-slate-900">{tipo.nombre}</span>
                <span className="mt-2 text-xs text-slate-500">Tasa referencial</span>
                <span className="text-2xl font-bold tabular-nums text-marca-700">
                  {porcentaje(tipo.tasaAnual)}
                </span>
                <span className="mt-2 text-xs leading-snug text-slate-500">{tipo.descripcion}</span>
              </label>
            )
          })}
        </div>
        <MensajeError>{errores.tipo}</MensajeError>
      </Paso>

      <Paso numero={2} titulo="¿Cuánto necesitas y en qué plazo?">
        <div className="grid gap-6 lg:grid-cols-2">
          <div>
            <label htmlFor="monto" className="text-sm font-medium text-slate-700">
              Monto del préstamo
            </label>
            <div className="relative mt-1.5">
              <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-lg font-semibold text-slate-400">
                $
              </span>
              <input
                id="monto"
                inputMode="decimal"
                value={monto}
                onChange={(e) => setMonto(e.target.value)}
                disabled={simulando}
                aria-invalid={Boolean(errores.monto)}
                aria-describedby="monto-rango"
                className={`w-full rounded-xl border-2 py-3 pr-4 pl-9 text-lg font-semibold tabular-nums text-slate-900 outline-none transition focus:ring-4 disabled:bg-slate-50 ${
                  errores.monto
                    ? 'border-red-400 focus:border-red-500 focus:ring-red-100'
                    : 'border-slate-200 focus:border-marca-500 focus:ring-marca-100'
                }`}
              />
            </div>
            <p id="monto-rango" className="mt-1.5 text-xs text-slate-500">
              Desde {monedaEntera(MONTO_MINIMO)} hasta {monedaEntera(MONTO_MAXIMO)}
            </p>
            <div className="mt-3 flex flex-wrap gap-2">
              {MONTOS_SUGERIDOS.map((valor) => (
                <Atajo
                  key={valor}
                  activo={Number(monto) === valor}
                  disabled={simulando}
                  onClick={() => setMonto(String(valor))}
                >
                  {monedaEntera(valor)}
                </Atajo>
              ))}
            </div>
            <MensajeError>{errores.monto}</MensajeError>
          </div>

          <div>
            <label htmlFor="plazo" className="text-sm font-medium text-slate-700">
              Plazo en meses
            </label>
            <div className="relative mt-1.5">
              <input
                id="plazo"
                inputMode="numeric"
                value={plazo}
                onChange={(e) => cambiarPlazo(e.target.value)}
                disabled={simulando}
                aria-invalid={Boolean(errores.plazo)}
                aria-describedby="plazo-legible"
                className={`w-full rounded-xl border-2 py-3 pr-24 pl-4 text-lg font-semibold tabular-nums text-slate-900 outline-none transition focus:ring-4 disabled:bg-slate-50 ${
                  errores.plazo
                    ? 'border-red-400 focus:border-red-500 focus:ring-red-100'
                    : 'border-slate-200 focus:border-marca-500 focus:ring-marca-100'
                }`}
              />
              <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-4 text-sm text-slate-400">
                meses
              </span>
            </div>
            <p id="plazo-legible" className="mt-1.5 text-xs text-slate-500">
              {plazoValido ? `Equivale a ${plazoLegible(plazoNumero)}` : 'Hasta 480 meses (40 años)'}
            </p>
            <div className="mt-3 flex flex-wrap gap-2">
              {PLAZOS_SUGERIDOS.map((meses) => (
                <Atajo
                  key={meses}
                  activo={plazoNumero === meses}
                  disabled={simulando}
                  onClick={() => cambiarPlazo(String(meses))}
                >
                  {plazoLegible(meses)}
                </Atajo>
              ))}
            </div>
            <MensajeError>{errores.plazo}</MensajeError>
          </div>
        </div>
      </Paso>

      <Paso
        numero={3}
        titulo="¿Cada cuánto quieres pagar?"
        ayuda="Solo se habilitan las frecuencias que dividen exactamente el plazo."
      >
        <div role="radiogroup" aria-label="Frecuencia de pago" className="grid grid-cols-2 gap-2 sm:grid-cols-5">
          {FRECUENCIAS.map((opcion) => {
            const compatible = plazoValido && frecuenciaCompatible(opcion, plazoNumero)
            const seleccionada = frecuencia === opcion.valor

            return (
              <button
                key={opcion.valor}
                type="button"
                role="radio"
                aria-checked={seleccionada}
                onClick={() => setFrecuencia(opcion.valor)}
                disabled={simulando || !compatible}
                title={compatible ? undefined : `El plazo no se divide en cuotas cada ${opcion.meses} meses`}
                className={`rounded-xl border-2 px-3 py-2.5 text-sm font-semibold transition disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300 ${
                  seleccionada
                    ? 'border-marca-600 bg-marca-50 text-marca-700'
                    : 'border-slate-200 text-slate-600 hover:border-marca-300'
                }`}
              >
                {opcion.etiqueta}
              </button>
            )
          })}
        </div>
      </Paso>

      <Paso numero={4} titulo="Seguro de desgravamen">
        <label className="flex cursor-pointer items-start gap-4 rounded-xl border border-slate-200 p-4 transition hover:border-marca-300">
          <span className="relative mt-0.5 inline-flex shrink-0">
            <input
              type="checkbox"
              checked={conSeguro}
              onChange={(e) => setConSeguro(e.target.checked)}
              disabled={simulando}
              className="peer sr-only"
            />
            <span className="h-6 w-11 rounded-full bg-slate-300 transition peer-checked:bg-marca-600 peer-focus-visible:ring-4 peer-focus-visible:ring-marca-100" />
            <span className="absolute top-0.5 left-0.5 size-5 rounded-full bg-white shadow transition peer-checked:translate-x-5" />
          </span>

          <span>
            <span className="block text-sm font-semibold text-slate-900">
              {conSeguro ? 'Incluir seguro de desgravamen' : 'Sin seguro de desgravamen'}
            </span>
            <span className="mt-0.5 block text-sm text-slate-500">
              Cubre el saldo pendiente de la deuda en caso de fallecimiento del titular.
              {tipoElegido &&
                ` Para ${tipoElegido.nombre.toLowerCase()} la prima es de ${porcentaje(
                  tipoElegido.tasaSeguroDesgravamenMensual,
                  4,
                )} mensual sobre el saldo.`}
            </span>
          </span>
        </label>
      </Paso>

      <div className="flex flex-col gap-3 border-t border-slate-100 bg-slate-50 px-5 py-5 sm:flex-row sm:items-center sm:justify-between sm:px-7">
        <p className="text-sm text-slate-500">
          {tipoElegido ? (
            <>
              Tasa aplicada: <strong className="text-slate-700">{porcentaje(tipoElegido.tasaAnual)} anual</strong>
            </>
          ) : (
            'Selecciona un tipo de crédito para continuar.'
          )}
        </p>

        <button
          type="submit"
          disabled={simulando}
          className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-marca-600 px-8 py-3 text-base font-semibold text-white shadow-sm transition hover:bg-marca-700 focus:ring-4 focus:ring-marca-200 focus:outline-none disabled:cursor-not-allowed disabled:opacity-60 sm:w-auto"
        >
          {simulando && (
            <span className="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          )}
          {simulando ? 'Calculando…' : 'Calcular cuota'}
        </button>
      </div>

      <p className="sr-only" aria-live="polite">
        {simulando ? `Calculando la simulación de ${moneda(Number(monto) || 0)}` : ''}
      </p>
    </form>
  )
}
