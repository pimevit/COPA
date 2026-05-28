import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'

const scoreRows = [
  ['Placar exato', '5'],
  ['Acertou apenas o vencedor ou empate', '2'],
  ['Acertou os gols de apenas um time', '1'],
  ['Errou tudo', '0'],
] as const

const multiplierRows = [
  ['Grupos', 'x1'],
  ['Oitavas', 'x2'],
  ['Quartas', 'x3'],
  ['Semifinal', 'x4'],
  ['Final', 'x5'],
] as const

export function RulesPage() {
  return (
    <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
      <section className="mx-auto flex w-full max-w-5xl flex-col gap-5">
        <header className="flex flex-col gap-3 border-b border-slate-200 pb-4 dark:border-slate-800">
          <AuthenticatedNav activePage="rules" />
          <div>
            <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
              Bolao
            </p>
            <h1 className="text-2xl font-semibold tracking-normal sm:text-3xl">Regras</h1>
            <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
              Pontuacao, fechamento de palpites, visibilidade e desempate.
            </p>
          </div>
        </header>

        <section className="grid gap-4 lg:grid-cols-2">
          <RulesTable title="Pontuacao base" rows={scoreRows} firstHeader="Situacao" secondHeader="Pontos" />
          <RulesTable title="Multiplicador por fase" rows={multiplierRows} firstHeader="Fase" secondHeader="Multiplicador" />
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
          <h2 className="text-lg font-semibold tracking-normal">Janela de palpites</h2>
          <ul className="mt-3 grid gap-2 text-sm leading-6 text-slate-700 dark:text-slate-300">
            <li>* Os palpites sao bloqueados automaticamente 15 minutos antes na fase de grupos e 30 minutos antes no mata-mata.</li>
            <li>A API tambem recusa criacao ou edicao de palpites depois do fechamento.</li>
            <li>Administradores podem bloquear manualmente os palpites de uma partida antes do fechamento automatico.</li>
          </ul>
        </section>

        <section className="grid gap-4 lg:grid-cols-2">
          <article className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
            <h2 className="text-lg font-semibold tracking-normal">Visibilidade</h2>
            <p className="mt-3 text-sm leading-6 text-slate-700 dark:text-slate-300">
              Palpites ficam ocultos antes do inicio da partida e podem ser exibidos apos o inicio.
            </p>
          </article>

          <article className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
            <h2 className="text-lg font-semibold tracking-normal">Desempate</h2>
            <ol className="mt-3 list-decimal space-y-2 pl-5 text-sm leading-6 text-slate-700 dark:text-slate-300">
              <li>Mais placares exatos.</li>
              <li>Mais acertos de vencedor ou empate.</li>
              <li>Melhor sequencia de acertos consecutivos.</li>
              <li>Quem cadastrou os palpites primeiro.</li>
            </ol>
          </article>
        </section>
      </section>
    </main>
  )
}

function RulesTable({
  firstHeader,
  rows,
  secondHeader,
  title,
}: {
  firstHeader: string
  rows: readonly (readonly [string, string])[]
  secondHeader: string
  title: string
}) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
      <h2 className="text-lg font-semibold tracking-normal">{title}</h2>
      <div className="mt-3 overflow-x-auto">
        <table className="w-full border-collapse text-left text-sm">
          <thead>
            <tr className="border-b border-slate-200 text-slate-600 dark:border-slate-800 dark:text-slate-300">
              <th className="py-2 pr-3 font-semibold">{firstHeader}</th>
              <th className="py-2 font-semibold">{secondHeader}</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(([label, value]) => (
              <tr className="border-b border-slate-100 last:border-b-0 dark:border-slate-900" key={label}>
                <td className="py-2 pr-3 text-slate-700 dark:text-slate-300">{label}</td>
                <td className="py-2 font-semibold text-slate-950 dark:text-slate-50">{value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
