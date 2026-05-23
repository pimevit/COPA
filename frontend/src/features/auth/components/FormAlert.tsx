type FormAlertProps = {
  message: string
}

export function FormAlert({ message }: FormAlertProps) {
  return (
    <div
      className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm leading-6 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200"
      role="alert"
    >
      {message}
    </div>
  )
}
