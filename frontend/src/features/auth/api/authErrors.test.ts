import { describe, expect, it } from 'vitest'

import { ApiError } from '../../../api/httpClient'
import { mapRegisterError } from './authErrors'

describe('mapRegisterError', () => {
  it('orients duplicate e-mail users to recover the password with the admin', () => {
    const result = mapRegisterError(new ApiError(409, {
      title: 'Email already registered.',
      detail: 'Email is already registered.',
    }))

    expect(result.general).toBe('Este e-mail ja esta cadastrado. Fale com o admin para recuperar a senha.')
    expect(result.fieldErrors.email).toBe('Fale com o admin para recuperar a senha desta conta.')
  })
})
