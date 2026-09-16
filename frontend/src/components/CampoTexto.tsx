interface Props extends React.InputHTMLAttributes<HTMLInputElement> {
  etiqueta: string
  error?: string
}

export function CampoTexto({ etiqueta, error, id, ...resto }: Props) {
  const idCampo = id ?? resto.name ?? etiqueta

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={idCampo} className="text-sm font-medium text-slate-700">
        {etiqueta}
      </label>

      <input
        id={idCampo}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${idCampo}-error` : undefined}
        className={`w-full rounded-lg border px-3.5 py-2.5 text-slate-900 shadow-sm outline-none transition
          placeholder:text-slate-400
          focus:ring-2 focus:ring-marca-300
          disabled:cursor-not-allowed disabled:bg-slate-50
          ${
            error
              ? 'border-red-400 focus:border-red-500 focus:ring-red-200'
              : 'border-slate-300 focus:border-marca-500'
          }`}
        {...resto}
      />

      {error && (
        <p id={`${idCampo}-error`} role="alert" className="text-sm text-red-600">
          {error}
        </p>
      )}
    </div>
  )
}
