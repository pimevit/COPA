const defaultLocale = 'pt-BR'

function getUserLocale(): string {
  return typeof navigator === 'undefined' ? defaultLocale : navigator.language || defaultLocale
}

export function parseUtcDate(value: string): Date {
  const hasTimezone = /(?:z|[+-]\d{2}:?\d{2})$/i.test(value)
  return new Date(hasTimezone ? value : `${value}Z`)
}

export function formatMatchDateTime(value: string, locale = getUserLocale(), timeZone?: string): string {
  const formatter = new Intl.DateTimeFormat(locale, {
    dateStyle: 'short',
    timeStyle: 'short',
    timeZone,
  })

  return formatter.format(parseUtcDate(value))
}

function getDateParts(date: Date, locale: string, timeZone?: string): string {
  const formatter = new Intl.DateTimeFormat(locale, {
    day: '2-digit',
    month: '2-digit',
    timeZone,
    year: 'numeric',
  })

  const parts = formatter.formatToParts(date)
  const year = parts.find((part) => part.type === 'year')?.value ?? ''
  const month = parts.find((part) => part.type === 'month')?.value ?? ''
  const day = parts.find((part) => part.type === 'day')?.value ?? ''

  return `${year}-${month}-${day}`
}

export function isTodayMatch(
  matchDate: string,
  now = new Date(),
  locale = getUserLocale(),
  timeZone?: string,
): boolean {
  return getDateParts(parseUtcDate(matchDate), locale, timeZone) === getDateParts(now, locale, timeZone)
}
