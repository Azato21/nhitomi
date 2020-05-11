import { CommandFunc } from '.'
import { InteractiveMessage, RenderResult, ReactionTrigger } from '../interactive'
import { Locale } from '../locales'
import { BookQuery, BookSort, SortDirection, QueryMatchMode, Book, BookContent } from 'nhitomi-api'
import { AsyncArray } from '../asyncArray'
import { BookMessage } from './get'
import { ReadTrigger } from '../Triggers/read'
import { DestroyTrigger } from '../Triggers/destroy'
import { ListTrigger } from '../Triggers/list'

export class BookSearchMessage extends InteractiveMessage {
  readonly results = new AsyncArray<Book>(20, async (offset, limit) => (await this.context?.api.book.searchBooks(false, { ...this.baseQuery, offset, limit }))?.body.items || [])

  constructor(readonly baseQuery: BookQuery) { super() }

  position = 0

  book?: Book
  content?: BookContent

  protected async render(l: Locale): Promise<RenderResult> {
    this.book = await this.results.get(this.position)

    if (!this.book && !(this.book = this.results.getCached(this.position = Math.max(0, Math.min(this.results.loadedLength - 1, this.position))))) {
      //todo: empty results
      return { message: 'empty results' }
    }

    this.content = this.book.contents.filter(c => c.language === this.context?.user.language)[0] || this.book.contents[0]

    return BookMessage.renderStatic(l, this.book, this.content)
  }

  protected createTriggers(): ReactionTrigger[] {
    return [
      ...super.createTriggers(),

      new ReadTrigger(this),
      new ListTrigger(this, 'left'),
      new ListTrigger(this, 'right'),
      new DestroyTrigger()
    ]
  }
}

export const run: CommandFunc = (context, query) => {
  const baseQuery: BookQuery = {
    all: !query ? undefined : {
      values: [query],
      mode: QueryMatchMode.All
    },
    limit: 20,
    sorting: [{
      value: BookSort.CreatedTime,
      direction: SortDirection.Descending
    }]
  }

  return new BookSearchMessage(baseQuery).initialize(context)
}