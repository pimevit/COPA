import { ApiError, type ProblemDetails } from '../../../api/httpClient'

export type AuthErrorMessages = {
  general: string
  fieldErrors: Partial<Record<'name' | 'email' | 'password', string>>
}

const defaultMessage = 'Nao foi possivel concluir a autenticacao. Tente novamente.'

function normalizeFieldName(field: string): 'name' | 'email' | 'password' | null {
  const key = field.toLowerCase()

  if (key.endsWith('name')) {
    return 'name'
  }

  if (key.endsWith('email')) {
    return 'email'
  }

  if (key.endsWith('password')) {
    return 'password'
  }

  return null
}

function mapValidationErrors(body: ProblemDetails): AuthErrorMessages | null {
  if (!body.errors) {
    return null
  }

  const fieldErrors: AuthErrorMessages['fieldErrors'] = {}

  for (const [field, messages] of Object.entries(body.errors)) {
    const normalizedField = normalizeFieldName(field)

    if (normalizedField && messages.length > 0) {
      fieldErrors[normalizedField] = messages[0]
    }
  }

  return {
    general: Object.keys(fieldErrors).length > 0 ? 'Confira os campos destacados.' : 'Dados invalidos.',
    fieldErrors,
  }
}

function isProblemDetails(body: unknown): body is ProblemDetails {
  return typeof body === 'object' && body !== null
}

export function mapLoginError(error: unknown): AuthErrorMessages {
  if (error instanceof ApiError) {
    if (error.status === 401) {
      return {
        general: 'E-mail ou senha invalidos.',
        fieldErrors: {},
      }
    }

    if (isProblemDetails(error.body)) {
      const validationErrors = mapValidationErrors(error.body)

      if (validationErrors) {
        return validationErrors
      }
    }
  }

  return {
    general: defaultMessage,
    fieldErrors: {},
  }
}

export function mapRegisterError(error: unknown): AuthErrorMessages {
  if (error instanceof ApiError) {
    if (error.status === 409) {
      return {
        general: 'Este e-mail ja esta cadastrado. Fale com o admin para recuperar a senha.',
        fieldErrors: {
          email: 'Fale com o admin para recuperar a senha desta conta.',
        },
      }
    }

    if (isProblemDetails(error.body)) {
      const validationErrors = mapValidationErrors(error.body)

      if (validationErrors) {
        return validationErrors
      }
    }
  }

  return {
    general: defaultMessage,
    fieldErrors: {},
  }
}
