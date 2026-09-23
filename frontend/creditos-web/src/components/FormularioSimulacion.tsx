import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useAuth } from '../hooks/useAuth'
import { creditApi } from '../services/api'
import type { Estimaciones, FrecuenciaPago, SimulacionRequest, TipoCredito } from '../types/credito'
import {
  FRECUENCIAS,
  frecuenciaCompatible,
  moneda,
  monedaEntera,
  opcionFrecuencia,
  plazoLegible,
  porcentaje,
} from '../utils/formato'
import { ListaDesplegable, type OpcionLista } from './ListaDesplegable'

const MONTO_MINIMO = 100
const MONTO_MAXIMO = 1_000_000
const PLAZO_MINIMO = 1
const PLAZO_MAXIMO = 480

/** Espera antes de pedir estimaciones, para no lanzar una llamada por tecla. */
const RETARDO_ESTIMACIONES = 400

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

const aNumero = (texto: string) => Number(texto.replace(/[\s.$]/g, '').replace(',', '.'))

export function FormularioSimulacion({ tipos, simulando, onSimular }: Props) {
  const { token } = useAuth()

  const [tipoId, setTipoId] = useState<number | null>(tipos[0]?.id ?? null)
  const [monto, setMonto] = useState('10000')
  const [plazo, setPlazo] = useState('24')
  const [frecuencia, setFrecuencia] = useState<FrecuenciaPago>('Mensual')
  const [conSeguro, setConSeguro] = useState(true)
  const [errores, setErrores] = useState<Errores>({})
  const [estimaciones, setEstimaciones] = useState<Estimaciones | null>(null)

  const tipoElegido = tipos.find((t) => t.id === tipoId)
  const montoNumero = aNumero(monto)
  const plazoNumero = Number(plazo)
  const plazoValido = Number.isInteger(plazoNumero) && plazoNumero >= PLAZO_MINIMO && plazoNumero <= PLAZO_MAXIMO
  const montoValido = !Number.isNaN(montoNumero) && montoNumero >= MONTO_MINIMO && montoNumero <= MONTO_MAXIMO

  const temporizador = useRef<number | undefined>(undefined)

  /**
   * Pide al servidor la cuota que daría cada opción. Se calcula allí, con el
   * mismo motor que la simulación final, para que lo que anticipa la lista no
   * pueda diferir del resultado real.
   */
  const pedirEstimaciones = useCallback(async () => {
    if (!token || tipoId === null || !montoValido || !plazoValido) return

    try {
      setEstimaciones(
        await creditApi.estimaciones(token, {
          tipoCreditoId: tipoId,
          monto: montoNumero,
          plazoMeses: plazoNumero,
          frecuenciaPago: frecuencia,
          incluirSeguroDesgravamen: conSeguro,
        }),
      )
    } catch {
      // Las estimaciones son una ayuda: si fallan, el formulario sigue usable
      // y las listas se muestran sin la cifra de la derecha.
      setEstimaciones(null)
    }
  }, [token, tipoId, montoNumero, plazoNumero, frecuencia, conSeguro, montoValido, plazoValido])

  useEffect(() => {
    window.clearTimeout(temporizador.current)
    temporizador.current = window.setTimeout(() => void pedirEstimaciones(), RETARDO_ESTIMACIONES)
    return () => window.clearTimeout(temporizador.current)
  }, [pedirEstimaciones])

  const opcionesTipo: OpcionLista[] = useMemo(() => {
    const porTipo = estimaciones?.porTipo ?? []

    return tipos.map((t) => {
      const cuota = porTipo.find((e) => e.tipoCreditoId === t.id)?.cuotaEstimada ?? null

      return {
        valor: String(t.id),
        etiqueta: t.nombre,
        detalle: `${porcentaje(t.tasaAnual)} anual · ${t.descripcion}`,
        estimacion: cuota !== null ? moneda(cuota) : undefined,
        grupo: t.categoria,
      }
    })
  }, [tipos, estimaciones])

  const opcionesMonto: OpcionLista[] = useMemo(
    () =>
      (estimaciones?.porMonto ?? []).map((e) => ({
        valor: String(e.monto),
        etiqueta: monedaEntera(e.monto),
        estimacion: e.cuotaEstimada !== null ? moneda(e.cuotaEstimada) : undefined,
      })),
    [estimaciones],
  )

  const opcionesPlazo: OpcionLista[] = useMemo(
    () =>
      (estimaciones?.porPlazo ?? []).map((e) => ({
        valor: String(e.plazoMeses),
        etiqueta: `${e.plazoMeses} meses`,
        detalle: plazoLegible(e.plazoMeses),
        estimacion: e.cuotaEstimada !== null ? moneda(e.cuotaEstimada) : undefined,
        deshabilitada: !e.compatibleConFrecuencia,
        motivo: !e.compatibleConFrecuencia ? 'No divisible' : undefined,
      })),
    [estimaciones],
  )

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
      <Paso
        numero={1}
        titulo="¿Qué tipo de crédito necesitas?"
        ayuda="La tasa de interés depende del tipo que elijas. Escribe para buscar entre los segmentos."
      >
        <ListaDesplegable
          id="tipo-credito"
          etiqueta="Tipo de crédito"
          modo="filtro"
          valor={tipoId !== null ? String(tipoId) : ''}
          opciones={opcionesTipo}
          deshabilitado={simulando}
          error={errores.tipo}
          placeholder="Busca un tipo de crédito"
          ayuda={
            tipoElegido
              ? `${porcentaje(tipoElegido.tasaAnual)} anual · desgravamen ${porcentaje(tipoElegido.tasaSeguroDesgravamenMensual, 4)} mensual`
              : undefined
          }
          onCambio={(v) => setTipoId(Number(v))}
        />
      </Paso>

      <Paso
        numero={2}
        titulo="¿Cuánto necesitas y en qué plazo?"
        ayuda="Elige un valor sugerido o escribe el tuyo. La cifra de la derecha es la cuota que resultaría."
      >
        <div className="grid gap-6 lg:grid-cols-2">
          <ListaDesplegable
            id="monto"
            etiqueta="Monto del préstamo"
            valor={monto}
            opciones={opcionesMonto}
            prefijo="$"
            deshabilitado={simulando}
            error={errores.monto}
            ayuda={`Desde ${monedaEntera(MONTO_MINIMO)} hasta ${monedaEntera(MONTO_MAXIMO)}`}
            onCambio={setMonto}
          />

          <ListaDesplegable
            id="plazo"
            etiqueta="Plazo"
            valor={plazo}
            opciones={opcionesPlazo}
            sufijo="meses"
            deshabilitado={simulando}
            error={errores.plazo}
            ayuda={plazoValido ? `Equivale a ${plazoLegible(plazoNumero)}` : 'Hasta 480 meses (40 años)'}
            onCambio={cambiarPlazo}
          />
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
    </form>
  )
}
