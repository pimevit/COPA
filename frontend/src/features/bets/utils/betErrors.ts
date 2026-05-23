import { ApiError, type ProblemDetails } from '../../../api/httpClient'

export type BetErrorKind = 'windowClosed' | 'duplicate' | 'validation' | 'generic'

export type BetErrorMessage = {
  kind: BetErrorKind
  message: string
}

function isProblemDetails(body: unknown): body is ProblemDetails {
  return typeof body === 'object' && body !== null
}

export function mapBetError(error: unknown): BetErrorMessage {
  if (error instanceof ApiError) {
    if (error.status === 422) {
      return {
        kind: 'windowClosed',
        message: 'A janela de palpites foi encerrada para esta partida.',
      }
    }

    if (error.status === 409) {
      return {
        kind: 'duplicate',
        message: 'Ja existe um palpite para esta partida. Atualize a pagina e tente editar o palpite existente.',
      }
    }

    if (error.status === 400 && isProblemDetails(error.body)) {
      return {
        kind: 'validation',
        message: error.body.title ?? 'Confira os valores informados.',
      }
    }
  }

  return {
    kind: 'generic',
    message: 'Nao foi possivel salvar o palpite. Tente novamente.',
  }
}
