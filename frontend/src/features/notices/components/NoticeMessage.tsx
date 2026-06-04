type NoticeMessageProps = {
  message: string
}

type NoticeMessagePart =
  | {
      kind: 'text'
      text: string
    }
  | {
      href: string
      kind: 'link'
      text: string
    }

const markdownLinkPattern = /\[([^\]\n]+)\]\((https?:\/\/[^\s)]+|www\.[^\s)]+)\)/gi
const urlPattern = /(https?:\/\/[^\s<>()]+|www\.[^\s<>()]+)/gi
const trailingUrlPunctuationPattern = /[.,;:!?]+$/

export function NoticeMessage({ message }: NoticeMessageProps) {
  return (
    <>
      {getNoticeMessageParts(message).map((part, index) =>
        part.kind === 'link' ? (
          <a
            className="break-all font-semibold underline underline-offset-2 hover:text-amber-800 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-2 dark:hover:text-amber-50 dark:focus:ring-offset-amber-950"
            href={part.href}
            key={`${part.href}-${index}`}
            rel="noopener noreferrer"
            target="_blank"
          >
            {part.text}
          </a>
        ) : (
          part.text
        ),
      )}
    </>
  )
}

function getNoticeMessageParts(message: string): NoticeMessagePart[] {
  const parts: NoticeMessagePart[] = []
  let lastIndex = 0

  for (const match of message.matchAll(markdownLinkPattern)) {
    const matchIndex = match.index ?? 0
    appendAutoLinkedText(parts, message.slice(lastIndex, matchIndex))

    const linkText = match[1]
    const href = normalizeHref(match[2])

    if (isAllowedHref(href)) {
      parts.push({ href, kind: 'link', text: linkText })
    } else {
      parts.push({ kind: 'text', text: match[0] })
    }

    lastIndex = matchIndex + match[0].length
  }

  appendAutoLinkedText(parts, message.slice(lastIndex))

  return parts
}

function appendAutoLinkedText(parts: NoticeMessagePart[], text: string) {
  let lastIndex = 0

  for (const match of text.matchAll(urlPattern)) {
    const matchIndex = match.index ?? 0
    const rawUrl = match[0]
    const trailingPunctuation = rawUrl.match(trailingUrlPunctuationPattern)?.[0] ?? ''
    const linkText = trailingPunctuation ? rawUrl.slice(0, -trailingPunctuation.length) : rawUrl
    const href = normalizeHref(linkText)

    parts.push({ kind: 'text', text: text.slice(lastIndex, matchIndex) })

    if (isAllowedHref(href)) {
      parts.push({ href, kind: 'link', text: linkText })
    } else {
      parts.push({ kind: 'text', text: linkText })
    }

    if (trailingPunctuation) {
      parts.push({ kind: 'text', text: trailingPunctuation })
    }

    lastIndex = matchIndex + rawUrl.length
  }

  parts.push({ kind: 'text', text: text.slice(lastIndex) })
}

function normalizeHref(value: string): string {
  const trimmedValue = value.trim()

  if (trimmedValue.toLowerCase().startsWith('www.')) {
    return `https://${trimmedValue}`
  }

  return trimmedValue
}

function isAllowedHref(href: string): boolean {
  try {
    const url = new URL(href)

    return url.protocol === 'http:' || url.protocol === 'https:'
  } catch {
    return false
  }
}
