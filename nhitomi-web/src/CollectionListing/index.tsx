import React, { ComponentProps } from 'react'
import { TypedPrefetchLinkProps, PrefetchLink, usePostfetch, PrefetchGenerator } from '../Prefetch'
import { Book, User, Collection, ObjectType } from 'nhitomi-api'
import { useClient, useClientInfo } from '../ClientManager'
import { PageContainer } from '../Components/PageContainer'
import { Container } from '../Components/Container'
import { FormattedMessage } from 'react-intl'
import { BookSection } from './Book'
import { useTabTitle } from '../TitleSetter'
import { useLocalized } from '../LocaleManager'

export type PrefetchResult = { user: User, books: BookCollection[] }
export type PrefetchOptions = { id: string }

export type BookCollection = { collection: Collection, cover?: Book }

export const useCollectionListingPrefetch: PrefetchGenerator<PrefetchResult, PrefetchOptions> = ({ id }) => {
  const client = useClient()
  const { info, setInfo } = useClientInfo()

  return {
    destination: {
      path: `/users/${id}/collections`
    },

    fetch: async () => {
      const [user, collections] = await Promise.all([
        client.user.getUser({ id }),
        client.user.getUserCollections({ id }).then(x => x.items)
      ])

      if (info.authenticated && info.user.id === user.id)
        setInfo({ ...info, user })

      const bookCollections = collections.filter(c => c.type === ObjectType.Book)
      const bookCoverIds = bookCollections.map(c => c.items[0]).filter(x => x)
      const bookCovers = bookCoverIds.length ? (await client.book.getBooks({ getBookManyRequest: { ids: bookCoverIds } })).reduce((x, book) => { x[book.id] = book; return x }, {} as { [key: string]: Book }) : {}
      const books = bookCollections.map(c => ({ collection: c, cover: bookCovers[c.items[0]] }))

      return { user, books }
    }
  }
}

export const SelfCollectionListingLink = (props: Omit<ComponentProps<typeof CollectionListingLink>, 'id'>) => {
  const { info } = useClientInfo()

  if (info.authenticated)
    return <CollectionListingLink id={info.user.id} {...props} />

  return <>{props.children}</>
}

export const CollectionListingLink = ({ id, ...props }: TypedPrefetchLinkProps & PrefetchOptions) => (
  <PrefetchLink fetch={useCollectionListingPrefetch} options={{ id }} {...props} />
)

export const CollectionListing = (options: PrefetchOptions) => {
  const { result } = usePostfetch(useCollectionListingPrefetch, { requireAuth: true, ...options })

  if (!result)
    return null

  return (
    <PageContainer>
      <Loaded {...result} />
    </PageContainer>
  )
}

const Loaded = ({ user, books }: PrefetchResult) => {
  useTabTitle(useLocalized('pages.collectionListing.title'))

  const { permissions } = useClientInfo()

  return (
    <Container className='divide-y divide-gray-darkest'>
      <div className='p-2'>
        <div className='text-2xl'><FormattedMessage id='pages.collectionListing.title' /></div>
        <div className='text-xs text-gray-darker'><FormattedMessage id='pages.collectionListing.subtitle' values={{ user: user.username, mode: permissions.canManageCollections(user) ? 'manage' : 'view' }} /></div>
      </div>

      <div className='py-2 space-y-8'>
        <BookSection user={user} collections={books} />
      </div>
    </Container>
  )
}
