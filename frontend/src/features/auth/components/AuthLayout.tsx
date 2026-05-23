import type { ReactNode } from 'react'

type AuthLayoutProps = {
  title: string
  subtitle: string
  children: ReactNode
}

export function AuthLayout({ title, subtitle, children }: AuthLayoutProps) {
  return (
    <main className="min-h-screen bg-slate-50 px-4 py-8 text-slate-950 dark:bg-slate-950 dark:text-slate-50">
      <section className="mx-auto flex min-h-[calc(100vh-4rem)] w-full max-w-md flex-col justify-center gap-6">
        <div className="space-y-2">
          <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
            Bolao Copa
          </p>
          <h1 className="text-3xl font-semibold tracking-normal">{title}</h1>
          <p className="text-base leading-7 text-slate-700 dark:text-slate-300">{subtitle}</p>
        </div>
        <div className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-800 dark:bg-slate-900">
          {children}
        </div>
      </section>
    </main>
  )
}
