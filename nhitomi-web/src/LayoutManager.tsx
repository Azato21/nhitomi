import React, { ReactNode, createContext, useMemo, useContext } from 'react'
import { useWindowSize } from 'react-use'
import { StripWidth } from './Sidebar/Strip'

export type ScreenType = 'sm' | 'lg'

/** Width after which the device will be considered large. */
export const ScreenBreakpoint = 768

export const SmallBreakpoints = [320, 480, 640]
export const LargeBreakpoints = [640, 768, 1024, 1280].map(n => n - StripWidth)

/** Layout information context. */
const LayoutContext = createContext<{
  width: number
  height: number
  sidebar: number
  screen: ScreenType
  breakpoint?: number
}>(undefined as any)

export function useLayout() {
  return useContext(LayoutContext)
}

export const LayoutManager = ({ children }: { children?: ReactNode }) => {
  const { width: windowWidth, height } = useWindowSize()

  let screen: ScreenType = 'sm'
  let width = windowWidth
  let sidebar = 0
  let breakpoint = getBreakpoint(SmallBreakpoints, width)

  if (width >= ScreenBreakpoint) {
    screen = 'lg'
    sidebar = StripWidth
    width -= sidebar // on large screens there is a sidebar
    breakpoint = getBreakpoint(LargeBreakpoints, width)
  }

  return (
    <LayoutContext.Provider
      value={useMemo(() => ({
        width,
        height,
        sidebar,
        screen,
        breakpoint
      }), [width, height, sidebar, screen, breakpoint])}
      children={children} />
  )
}

export function getBreakpoint(breakpoints: number[], value: number) {
  let breakpoint: number | undefined

  for (const br of breakpoints) {
    if (value >= br)
      breakpoint = br

    else break
  }

  return breakpoint
}