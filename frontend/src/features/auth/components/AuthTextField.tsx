import { useId } from 'react'
import type { InputHTMLAttributes } from 'react'

type AuthTextFieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string
  error?: string
}

export function AuthTextField({ error, label, ...props }: AuthTextFieldProps) {
  const inputId = useId()
  const errorId = `${inputId}-error`

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-slate-800 dark:text-slate-100" htmlFor={inputId}>
        {label}
      </label>
      <input
        {...props}
        aria-describedby={error ? errorId : undefined}
        aria-invalid={Boolean(error)}
        className="min-h-11 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-base text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-emerald-600 focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:bg-slate-100 dark:border-slate-700 dark:bg-slate-950 dark:text-slate-50 dark:placeholder:text-slate-500 dark:focus:border-emerald-400"
        id={inputId}
      />
      {error ? (
        <p className="text-sm leading-5 text-red-700 dark:text-red-300" id={errorId}>
          {error}
        </p>
      ) : null}
    </div>
  )
}
