const PREGUNTAS: { pregunta: string; respuesta: React.ReactNode }[] = [
  {
    pregunta: '¿Qué diferencia hay entre el método francés y el alemán?',
    respuesta: (
      <>
        En el <strong>francés</strong> pagas siempre la misma cuota: al inicio casi todo es interés y, con el
        tiempo, la mayor parte se va a capital. En el <strong>alemán</strong> amortizas la misma cantidad de
        capital cada vez, así que la cuota empieza más alta y va bajando. El alemán genera menos intereses en
        total, pero exige más capacidad de pago al principio.
      </>
    ),
  },
  {
    pregunta: '¿Qué es el seguro de desgravamen?',
    respuesta: (
      <>
        Es un seguro que cancela el saldo pendiente del crédito si el titular fallece, para que la deuda no
        pase a su familia. Se cobra como un porcentaje del saldo que todavía debes, por eso su valor baja a
        medida que pagas.
      </>
    ),
  },
  {
    pregunta: '¿Cómo se calcula el ingreso mínimo requerido?',
    respuesta: (
      <>
        Se toma la cuota más alta de tu tabla, se lleva a su equivalente mensual y se divide para 0,40. Es
        decir, la cuota no debería comprometer más del <strong>40 % de tus ingresos</strong>, que es el umbral
        de capacidad de pago habitual en la banca.
      </>
    ),
  },
  {
    pregunta: '¿Por qué algunas frecuencias de pago aparecen deshabilitadas?',
    respuesta: (
      <>
        Porque el plazo debe dividirse en un número exacto de cuotas. Un crédito a 10 meses no puede pagarse
        en trimestres, pero uno a 12 sí. Si cambias el plazo, las opciones se ajustan solas.
      </>
    ),
  },
  {
    pregunta: '¿Los valores de la simulación son definitivos?',
    respuesta: (
      <>
        No. Son <strong>referenciales</strong> y sirven para comparar alternativas. Las condiciones finales de
        un crédito dependen de la evaluación de cada institución financiera.
      </>
    ),
  },
]

export function PreguntasFrecuentes() {
  return (
    <section aria-labelledby="titulo-preguntas">
      <h2 id="titulo-preguntas" className="text-xl font-bold text-slate-900">
        Preguntas frecuentes
      </h2>

      <div className="mt-4 divide-y divide-slate-200 overflow-hidden rounded-2xl border border-slate-200 bg-white">
        {PREGUNTAS.map(({ pregunta, respuesta }) => (
          <details key={pregunta} className="group">
            <summary className="flex cursor-pointer list-none items-center justify-between gap-4 px-5 py-4 font-semibold text-slate-800 transition hover:bg-slate-50 sm:px-6 [&::-webkit-details-marker]:hidden">
              {pregunta}
              <svg
                viewBox="0 0 20 20"
                className="size-5 shrink-0 text-marca-600 transition-transform group-open:rotate-180"
                fill="currentColor"
                aria-hidden="true"
              >
                <path
                  fillRule="evenodd"
                  d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z"
                  clipRule="evenodd"
                />
              </svg>
            </summary>
            <p className="px-5 pb-5 text-sm leading-relaxed text-slate-600 sm:px-6">{respuesta}</p>
          </details>
        ))}
      </div>
    </section>
  )
}
