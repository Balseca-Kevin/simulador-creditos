import { useEffect, useId, useMemo, useRef, useState } from 'react'

export interface OpcionLista {
  /** Valor que se escribe en el campo al elegirla. */
  valor: string
  /** Texto principal de la opción. */
  etiqueta: string
  /** Texto secundario a la izquierda, bajo la etiqueta. */
  detalle?: string
  /** Cifra destacada a la derecha: la estimación de esa opción. */
  estimacion?: string
  /** Agrupador opcional; las opciones con el mismo grupo se muestran juntas. */
  grupo?: string
  deshabilitada?: boolean
  /** Motivo por el que está deshabilitada, mostrado en lugar de la estimación. */
  motivo?: string
}

interface Props {
  id: string
  etiqueta: string
  ayuda?: string
  valor: string
  opciones: OpcionLista[]
  /**
   * libre  : lo que se escribe es el valor (monto, plazo).
   * filtro : lo que se escribe solo filtra la lista; el valor debe salir de ella (tipo de crédito).
   */
  modo?: 'libre' | 'filtro'
  placeholder?: string
  prefijo?: string
  sufijo?: string
  deshabilitado?: boolean
  error?: string
  onCambio: (valor: string) => void
}

/**
 * Campo de texto con lista desplegable: se puede elegir una opción o escribir
 * un valor propio. Cada opción muestra a la derecha la estimación que produce,
 * para poder comparar antes de decidir.
 *
 * Se implementa a mano y no con <select> ni <datalist> porque ninguno de los
 * dos permite mostrar dos líneas de texto y una cifra alineada por opción, ni
 * darles estilo de forma consistente entre navegadores.
 */
export function ListaDesplegable({
  id,
  etiqueta,
  ayuda,
  valor,
  opciones,
  modo = 'libre',
  placeholder,
  prefijo,
  sufijo,
  deshabilitado = false,
  error,
  onCambio,
}: Props) {
  const [abierta, setAbierta] = useState(false)
  const [resaltada, setResaltada] = useState(0)
  const [filtro, setFiltro] = useState<string | null>(null)
  const contenedor = useRef<HTMLDivElement>(null)
  const listaRef = useRef<HTMLUListElement>(null)
  const idLista = useId()

  // Mientras se escribe, la lista se reduce a lo que coincide; al cerrar o
  // elegir, el filtro se descarta y vuelve a verse el catálogo completo.
  const visibles = useMemo(() => {
    if (filtro === null || filtro.trim() === '') return opciones
    const texto = filtro.toLowerCase()
    return opciones.filter(
      (o) =>
        o.etiqueta.toLowerCase().includes(texto) ||
        o.valor.toLowerCase().includes(texto) ||
        o.detalle?.toLowerCase().includes(texto) ||
        o.grupo?.toLowerCase().includes(texto),
    )
  }, [opciones, filtro])

  const seleccionada = opciones.find((o) => o.valor === valor)
  const textoCampo = filtro !== null ? filtro : (seleccionada?.etiqueta ?? valor)

  // Cerrar al hacer clic fuera.
  useEffect(() => {
    if (!abierta) return

    const alPulsar = (evento: MouseEvent) => {
      if (!contenedor.current?.contains(evento.target as Node)) {
        setAbierta(false)
        setFiltro(null)
      }
    }

    document.addEventListener('mousedown', alPulsar)
    return () => document.removeEventListener('mousedown', alPulsar)
  }, [abierta])

  // Mantener a la vista la opción resaltada al navegar con el teclado.
  useEffect(() => {
    if (!abierta) return
    listaRef.current?.querySelector('[data-resaltada="true"]')?.scrollIntoView({ block: 'nearest' })
  }, [abierta, resaltada])

  function elegir(opcion: OpcionLista) {
    if (opcion.deshabilitada) return
    onCambio(opcion.valor)
    setFiltro(null)
    setAbierta(false)
  }

  function alTeclear(evento: React.KeyboardEvent) {
    if (evento.key === 'ArrowDown' || evento.key === 'ArrowUp') {
      evento.preventDefault()
      if (!abierta) {
        setAbierta(true)
        return
      }
      const paso = evento.key === 'ArrowDown' ? 1 : -1
      setResaltada((actual) => {
        const total = visibles.length
        if (total === 0) return 0
        let siguiente = actual
        // Saltar las opciones deshabilitadas al navegar.
        for (let i = 0; i < total; i++) {
          siguiente = (siguiente + paso + total) % total
          if (!visibles[siguiente].deshabilitada) break
        }
        return siguiente
      })
    } else if (evento.key === 'Enter') {
      if (abierta && visibles[resaltada]) {
        evento.preventDefault()
        elegir(visibles[resaltada])
      }
    } else if (evento.key === 'Escape') {
      setAbierta(false)
      setFiltro(null)
    }
  }

  // El encabezado de grupo se decide antes de pintar: mutar una variable
  // durante el render haría que el resultado dependiera del orden de ejecución.
  const conEncabezado = useMemo(
    () =>
      visibles.map((opcion, indice) => ({
        opcion,
        encabezado:
          opcion.grupo && opcion.grupo !== visibles[indice - 1]?.grupo ? opcion.grupo : null,
      })),
    [visibles],
  )

  return (
    <div className="flex flex-col gap-1.5" ref={contenedor}>
      <label htmlFor={id} className="text-sm font-medium text-slate-700">
        {etiqueta}
      </label>

      <div className="relative">
        {prefijo && (
          <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-lg font-semibold text-slate-400">
            {prefijo}
          </span>
        )}

        <input
          id={id}
          type="text"
          role="combobox"
          aria-expanded={abierta}
          aria-controls={idLista}
          aria-autocomplete="list"
          aria-invalid={Boolean(error)}
          aria-describedby={ayuda ? `${id}-ayuda` : undefined}
          autoComplete="off"
          disabled={deshabilitado}
          placeholder={placeholder}
          value={textoCampo}
          onChange={(e) => {
            setFiltro(e.target.value)
            // En modo filtro la selección no cambia hasta elegir una opción:
            // un texto a medio escribir no es un tipo de crédito válido.
            if (modo === 'libre') onCambio(e.target.value)
            setAbierta(true)
            setResaltada(0)
          }}
          onFocus={() => setAbierta(true)}
          onKeyDown={alTeclear}
          className={`w-full rounded-xl border-2 py-3 text-lg font-semibold text-slate-900 outline-none transition focus:ring-4 disabled:bg-slate-50 ${
            prefijo ? 'pl-9' : 'pl-4'
          } ${sufijo ? 'pr-24' : 'pr-11'} ${modo === 'filtro' ? 'cursor-pointer' : ''} ${
            error
              ? 'border-red-400 focus:border-red-500 focus:ring-red-100'
              : 'border-slate-200 focus:border-marca-500 focus:ring-marca-100'
          }`}
        />

        {sufijo && (
          <span className="pointer-events-none absolute inset-y-0 right-9 flex items-center text-sm text-slate-400">
            {sufijo}
          </span>
        )}

        <button
          type="button"
          tabIndex={-1}
          aria-label={abierta ? 'Cerrar la lista' : 'Abrir la lista'}
          disabled={deshabilitado}
          onClick={() => {
            setAbierta((a) => !a)
            setFiltro(null)
          }}
          className="absolute inset-y-0 right-0 flex items-center px-3 text-slate-400 transition hover:text-marca-600 disabled:opacity-40"
        >
          <svg
            viewBox="0 0 20 20"
            className={`size-5 transition-transform ${abierta ? 'rotate-180' : ''}`}
            fill="currentColor"
            aria-hidden="true"
          >
            <path
              fillRule="evenodd"
              d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z"
              clipRule="evenodd"
            />
          </svg>
        </button>

        {abierta && (
          <ul
            id={idLista}
            ref={listaRef}
            role="listbox"
            aria-label={etiqueta}
            className="absolute z-40 mt-1 max-h-72 w-full overflow-auto rounded-xl border border-slate-200 bg-white py-1 shadow-lg"
          >
            {visibles.length === 0 && (
              <li className="px-4 py-3 text-sm text-slate-500">
                Ningún valor sugerido coincide. Puedes escribir el tuyo.
              </li>
            )}

            {conEncabezado.map(({ opcion, encabezado }, indice) => {
              return (
                <li key={opcion.valor}>
                  {encabezado && (
                    <p className="px-4 pt-2.5 pb-1 text-xs font-semibold tracking-wide text-slate-400 uppercase">
                      {encabezado}
                    </p>
                  )}

                  <button
                    type="button"
                    role="option"
                    aria-selected={opcion.valor === valor}
                    data-resaltada={indice === resaltada}
                    disabled={opcion.deshabilitada}
                    onMouseEnter={() => setResaltada(indice)}
                    onClick={() => elegir(opcion)}
                    className={`flex w-full items-center justify-between gap-4 px-4 py-2.5 text-left transition ${
                      opcion.deshabilitada
                        ? 'cursor-not-allowed opacity-45'
                        : indice === resaltada
                          ? 'bg-marca-50'
                          : ''
                    } ${opcion.valor === valor ? 'font-semibold' : ''}`}
                  >
                    <span className="min-w-0">
                      <span className="block truncate text-sm text-slate-900">{opcion.etiqueta}</span>
                      {opcion.detalle && (
                        <span className="block truncate text-xs text-slate-500">{opcion.detalle}</span>
                      )}
                    </span>

                    {opcion.deshabilitada && opcion.motivo ? (
                      <span className="shrink-0 text-xs text-slate-400">{opcion.motivo}</span>
                    ) : (
                      opcion.estimacion && (
                        <span className="shrink-0 text-right">
                          <span className="block text-sm font-semibold tabular-nums text-marca-700">
                            {opcion.estimacion}
                          </span>
                          <span className="block text-[11px] text-slate-400">por cuota</span>
                        </span>
                      )
                    )}
                  </button>
                </li>
              )
            })}
          </ul>
        )}
      </div>

      {ayuda && !error && (
        <p id={`${id}-ayuda`} className="text-xs text-slate-500">
          {ayuda}
        </p>
      )}

      {error && (
        <p role="alert" className="text-sm font-medium text-red-600">
          {error}
        </p>
      )}
    </div>
  )
}
