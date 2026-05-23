import type { ButtonHTMLAttributes } from 'react'

type AuthSubmitButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  isLoading: boolean
  loadingText: string
}

export function AuthSubmitButton({
  children,
  disabled,
  isLoading,
  loadingText,
  ...props
}: AuthSubmitButtonProps) {
  return (
    <button
      {...props}
      className="inline-flex min-h-11 w-full items-center justify-center rounded-lg bg-emerald-700 px-4 py-2 text-sm font-semibold text-white outline-none transition hover:bg-emerald-800 focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-400 disabled:text-slate-100 dark:focus-visible:ring-offset-slate-900"
      disabled={disabled || isLoading}
      type="submit"
    >
      {isLoading ? loadingText : children}
    </button>
  )
}
