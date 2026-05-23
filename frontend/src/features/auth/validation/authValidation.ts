import type { LoginRequest, RegisterRequest } from '../../../types/auth'

export type AuthFieldErrors = Partial<Record<'name' | 'email' | 'password', string>>

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function validateLogin(values: LoginRequest): AuthFieldErrors {
  const errors: AuthFieldErrors = {}

  if (!values.email.trim()) {
    errors.email = 'Informe seu e-mail.'
  } else if (!emailPattern.test(values.email)) {
    errors.email = 'Informe um e-mail valido.'
  }

  if (!values.password) {
    errors.password = 'Informe sua senha.'
  }

  return errors
}

export function validateRegister(values: RegisterRequest): AuthFieldErrors {
  const errors: AuthFieldErrors = {}

  if (!values.name.trim()) {
    errors.name = 'Informe seu nome.'
  }

  if (!values.email.trim()) {
    errors.email = 'Informe seu e-mail.'
  } else if (!emailPattern.test(values.email)) {
    errors.email = 'Informe um e-mail valido.'
  }

  if (!values.password) {
    errors.password = 'Informe uma senha.'
  } else if (values.password.length < 6) {
    errors.password = 'Use pelo menos 6 caracteres.'
  }

  return errors
}

export function hasAuthFieldErrors(errors: AuthFieldErrors): boolean {
  return Object.keys(errors).length > 0
}
