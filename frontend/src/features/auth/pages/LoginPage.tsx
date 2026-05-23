import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import type { FormEvent } from 'react'
import type { Location } from 'react-router-dom'

import { useAuthStore } from '../../../stores/authStore'
import type { LoginRequest } from '../../../types/auth'
import { mapLoginError } from '../api/authErrors'
import { AuthLayout } from '../components/AuthLayout'
import { AuthSubmitButton } from '../components/AuthSubmitButton'
import { AuthTextField } from '../components/AuthTextField'
import { FormAlert } from '../components/FormAlert'
import { useLoginMutation } from '../hooks/useAuthMutations'
import { hasAuthFieldErrors, validateLogin, type AuthFieldErrors } from '../validation/authValidation'

type RedirectState = {
  from?: Location
}

const initialValues: LoginRequest = {
  email: '',
  password: '',
}

function getRedirectPath(state: unknown): string {
  const from = (state as RedirectState | null)?.from
  const path = from ? `${from.pathname}${from.search}${from.hash}` : '/app'

  return path === '/login' || path === '/register' ? '/app' : path
}

export function LoginPage() {
  const [values, setValues] = useState<LoginRequest>(initialValues)
  const [fieldErrors, setFieldErrors] = useState<AuthFieldErrors>({})
  const [generalError, setGeneralError] = useState<string | null>(null)
  const setSession = useAuthStore((state) => state.setSession)
  const loginMutation = useLoginMutation()
  const navigate = useNavigate()
  const location = useLocation()

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validationErrors = validateLogin(values)
    setFieldErrors(validationErrors)
    setGeneralError(null)

    if (hasAuthFieldErrors(validationErrors)) {
      return
    }

    try {
      const session = await loginMutation.mutateAsync(values)
      setSession(session)
      navigate(getRedirectPath(location.state), { replace: true })
    } catch (error) {
      const messages = mapLoginError(error)
      setGeneralError(messages.general)
      setFieldErrors(messages.fieldErrors)
    }
  }

  return (
    <AuthLayout subtitle="Use seu e-mail e senha para acessar sua conta." title="Entrar">
      <form className="space-y-4" noValidate onSubmit={handleSubmit}>
        {generalError ? <FormAlert message={generalError} /> : null}
        <AuthTextField
          autoComplete="email"
          disabled={loginMutation.isPending}
          error={fieldErrors.email}
          label="E-mail"
          name="email"
          onChange={(event) => setValues((current) => ({ ...current, email: event.target.value }))}
          type="email"
          value={values.email}
        />
        <AuthTextField
          autoComplete="current-password"
          disabled={loginMutation.isPending}
          error={fieldErrors.password}
          label="Senha"
          name="password"
          onChange={(event) => setValues((current) => ({ ...current, password: event.target.value }))}
          type="password"
          value={values.password}
        />
        <AuthSubmitButton isLoading={loginMutation.isPending} loadingText="Entrando...">
          Entrar
        </AuthSubmitButton>
        <p className="text-center text-sm text-slate-700 dark:text-slate-300">
          Ainda nao tem conta?{' '}
          <Link className="font-semibold text-emerald-700 underline-offset-4 hover:underline dark:text-emerald-300" to="/register">
            Cadastre-se
          </Link>
        </p>
      </form>
    </AuthLayout>
  )
}
