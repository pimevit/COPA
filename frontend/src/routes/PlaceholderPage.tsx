type PlaceholderPageProps = {
  title: string
  description: string
}

export function PlaceholderPage({ title, description }: PlaceholderPageProps) {
  return (
    <main className="min-h-screen bg-slate-50 px-4 py-8 text-slate-950 dark:bg-slate-950 dark:text-slate-50">
      <section className="mx-auto flex min-h-[calc(100vh-4rem)] w-full max-w-3xl flex-col justify-center gap-4">
        <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
          Bolao Copa
        </p>
        <h1 className="text-3xl font-semibold tracking-normal sm:text-4xl">{title}</h1>
        <p className="max-w-2xl text-base leading-7 text-slate-700 dark:text-slate-300">{description}</p>
      </section>
    </main>
  )
}
