import { useLayoutEffect } from 'react'
import { usePageState } from './state'

export function formatTitle(...parts: (string | undefined)[]) {
  return [...parts.map(p => p?.trim()).filter(p => p), 'nhitomi'].join(' · ')
}

export function useTabTitle(...parts: (string | undefined)[]) {
  const [, setCurrent] = usePageState<string>('title')
  const value = formatTitle(...parts)

  useLayoutEffect(() => setCurrent(value), [setCurrent, value])
}

export const TitleSetter = () => {
  const [current] = usePageState<string>('title', formatTitle())

  useLayoutEffect(() => { document.title = current }, [current])

  return null
}
