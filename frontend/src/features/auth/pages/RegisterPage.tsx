import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import type { FormEvent } from 'react'

import { useAuthStore } from '../../../stores/authStore'
import type { RegisterRequest } from '../../../types/auth'
import { mapRegisterError } from '../api/authErrors'
import { AuthLayout } from '../components/AuthLayout'
import { AuthSubmitButton } from '../components/AuthSubmitButton'
import { AuthTextField } from '../components/AuthTextField'
import { FormAlert } from '../components/FormAlert'
import { useRegisterMutation } from '../hooks/useAuthMutations'
import { hasAuthFieldErrors, validateRegister, type AuthFieldErrors } from '../validation/authValidation'

const initialValues: RegisterRequest = {
  name: '',
  email: '',
  password: '',
}

export function RegisterPage() {
  const [values, setValues] = useState<RegisterRequest>(initialValues)
  const [fieldErrors, setFieldErrors] = useState<AuthFieldErrors>({})
  const [generalError, setGeneralError] = useState<string | null>(null)
  const setSession = useAuthStore((state) => state.setSession)
  const registerMutation = useRegisterMutation()
  const navigate = useNavigate()

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validationErrors = validateRegister(values)
    setFieldErrors(validationErrors)
    setGeneralError(null)

    if (hasAuthFieldErrors(validationErrors)) {
      return
    }

    try {
      const session = await registerMutation.mutateAsync(values)
      setSession(session)
      navigate('/app', { replace: true })
    } catch (error) {
      const messages = mapRegisterError(error)
      setGeneralError(messages.general)
      setFieldErrors(messages.fieldErrors)
    }
  }

  return (
    <AuthLayout subtitle="Crie sua conta para entrar no bolao." title="Cadastro">
      <form className="space-y-4" noValidate onSubmit={handleSubmit}>
        {generalError ? <FormAlert message={generalError} /> : null}
        <AuthTextField
          autoComplete="name"
          disabled={registerMutation.isPending}
          error={fieldErrors.name}
          label="Nome"
          name="name"
          onChange={(event) => setValues((current) => ({ ...current, name: event.target.value }))}
          type="text"
          value={values.name}
        />
        <AuthTextField
          autoComplete="email"
          disabled={registerMutation.isPending}
          error={fieldErrors.email}
          label="E-mail"
          name="email"
          onChange={(event) => setValues((current) => ({ ...current, email: event.target.value }))}
          type="email"
          value={values.email}
        />
        <AuthTextField
          autoComplete="new-password"
          disabled={registerMutation.isPending}
          error={fieldErrors.password}
          label="Senha"
          name="password"
          onChange={(event) => setValues((current) => ({ ...current, password: event.target.value }))}
          type="password"
          value={values.password}
        />
        <AuthSubmitButton isLoading={registerMutation.isPending} loadingText="Cadastrando...">
          Criar conta
        </AuthSubmitButton>
        <p className="text-center text-sm text-slate-700 dark:text-slate-300">
          Ja tem conta?{' '}
          <Link className="font-semibold text-emerald-700 underline-offset-4 hover:underline dark:text-emerald-300" to="/login">
            Entrar
          </Link>
        </p>
      </form>
    </AuthLayout>
  )
}
