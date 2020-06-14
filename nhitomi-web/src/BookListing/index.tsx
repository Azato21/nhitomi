import { BookOutlined } from '@ant-design/icons'
import { PageHeader } from 'antd'
import React, { Dispatch, useContext, useRef, useState, createContext, useMemo, useLayoutEffect } from 'react'
import { useTabTitle } from '../hooks'
import { Prefetch, PrefetchLink, PrefetchLinkProps, usePrefetch } from '../Prefetch'
import { ProgressContext } from '../Progress'
import { useScrollShortcut } from '../shortcuts'
import { GridListing } from './Grid'
import { Search } from './Search'
import { ClientContext } from '../ClientContext'
import { LayoutContent } from '../Layout'
import { SearchManager, SearchState } from './searchManager'

export function getBookListingPrefetch(): Prefetch<SearchState> {
  return {
    path: '/books',

    func: async client => {
      const manager = new SearchManager(client)

      await manager.refresh()

      return manager.state
    }
  }
}

export const BookListing = () => {
  const { result, dispatch } = usePrefetch(getBookListingPrefetch())

  if (result)
    return <Loaded state={result} dispatch={dispatch} />

  return null
}

export const BookListingLink = (props: PrefetchLinkProps) => <PrefetchLink fetch={getBookListingPrefetch()} {...props} />

export const BookListingContext = createContext<{ manager: SearchManager }>(undefined as any)

const Loaded = ({ state, dispatch }: { state: SearchState, dispatch: Dispatch<SearchState> }) => {
  useTabTitle('Books')
  useScrollShortcut()

  const client = useContext(ClientContext)
  const { start, stop } = useContext(ProgressContext)

  const manager = useRef(new SearchManager(client)).current
  manager.setState(state)

  useLayoutEffect(() => {
    const onloading = (v: boolean) => { if (v) start(); else stop() }
    const onstate = () => dispatch(manager.state)

    manager.on('loading', onloading)
    manager.on('state', onstate)

    return () => {
      manager.off('loading', onloading)
      manager.off('state', onstate)
    }
  }, [dispatch, manager, start, stop])

  const [selected, setSelected] = useState<string>()

  return <BookListingContext.Provider value={useMemo(() => ({ manager }), [manager])}>
    <PageHeader
      avatar={{ icon: <BookOutlined />, shape: 'square' }}
      title='Books'
      subTitle='List of all books'
      extra={<Search />} />

    <LayoutContent>
      <GridListing
        selected={selected}
        setSelected={setSelected} />
    </LayoutContent>
  </BookListingContext.Provider>
}
