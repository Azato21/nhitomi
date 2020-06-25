import { Divider, Layout, Menu, PageHeader, Tooltip, Button } from 'antd'
import React, { useContext, useLayoutEffect } from 'react'
import { useLocation } from 'react-router-dom'
import { useShortcut, useShortcutKeyName } from './shortcuts'
import { NotificationContext } from './NotificationContext'
import { ClientContext } from './ClientContext'
import { LayoutContext } from './LayoutContext'
import { BarsOutlined, ReadOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { FormattedMessage } from 'react-intl'
import { BookListingLink } from './BookListing'
import { AboutLink } from './About'

export const SideBarWidth = 200

export const SideBar = () => {
  const { pathname } = useLocation()
  const client = useContext(ClientContext)
  const { alert } = useContext(NotificationContext)
  const { sidebar, setSidebar, mobile } = useContext(LayoutContext)

  // collapse on url change if mobile
  // we use setTimeout to avoid a bug in antd where menu shows a tooltip that can't be disabled
  useLayoutEffect(() => { mobile && sidebar && setTimeout(() => setSidebar(false), 100) }, [pathname]) // eslint-disable-line

  // collapse on shortcut
  useShortcut('sidebarKey', () => {
    setSidebar(!sidebar)

    if (sidebar)
      alert.info(<FormattedMessage id='sidebar.collapsed' />)
    else
      alert.info(<FormattedMessage id='sidebar.opened' />)
  })

  const toggleKey = useShortcutKeyName('sidebarKey')

  return <Layout.Sider
    theme='dark'
    breakpoint='md'
    onBreakpoint={v => setSidebar(!v)}
    width={SideBarWidth}
    collapsible
    collapsed={!sidebar}
    collapsedWidth={0}
    onCollapse={v => setSidebar(!v)}
    trigger={null}
    style={{
      position: 'fixed',
      height: '100vh',
      zIndex: 100,
      top: 0,
      left: 0,
      boxShadow: sidebar ? '-3px 0 6px 0 #555' : undefined
    }}>

    <Tooltip
      className='ant-layout-sider-zero-width-trigger ant-layout-sider-zero-width-trigger-left'
      title={<FormattedMessage id='sidebar.pressToToggle' values={{ key: toggleKey }} />}
      placement={sidebar ? 'left' : 'right'}
      mouseEnterDelay={0.5}
      mouseLeaveDelay={0}>

      <BarsOutlined
        style={{
          lineHeight: '46px' // this is a hack
        }}
        onClick={() => setSidebar(!sidebar)} />
    </Tooltip>

    <div style={{
      opacity: sidebar ? 1 : 0,
      transition: sidebar ? 'opacity 0.5s' : undefined
    }}>
      <BookListingLink>
        <PageHeader
          backIcon={false}
          style={{ minWidth: SideBarWidth }}
          title='nhitomi' />
      </BookListingLink>

      <Divider style={{ margin: 0 }} />

      <Menu theme='dark' mode='inline' selectedKeys={[pathname.split('/')[1]]}>
        <Menu.Item key='books'>
          <BookListingLink>
            <ReadOutlined />
            <FormattedMessage id='bookListing.header.title' />
          </BookListingLink>
        </Menu.Item>

        <Menu.Item key='about'>
          <AboutLink>
            <InfoCircleOutlined />
            <FormattedMessage id='about.title' />
          </AboutLink>
        </Menu.Item>

        {/*<Menu.Item key='images'>
        <ImageListingLink>
          <PictureOutlined />
          <span>Images</span>
        </ImageListingLink>
      </Menu.Item>

      <Menu.Item key='wiki'>
        <WikiHomeLink>
          <WikiOutlined />
          <span>Wiki</span>
        </WikiHomeLink>
      </Menu.Item>

      <Menu.Item key='uploads'>
        <UploadListingLink>
          <CloudUploadOutlined />
          <span>Uploads</span>
        </UploadListingLink>
      </Menu.Item>

      <Menu.Item key='users'>
        <UserSelfInfoLink>
          <UserOutlined />
          <span>Profile</span>
        </UserSelfInfoLink>
      </Menu.Item>

      {user && isElevatedUser(user) &&
        <Menu.Item key='_internal'>
          <AdminPanelLink>
            <ControlOutlined />
            <span>Administration</span>
          </AdminPanelLink>
        </Menu.Item>} */}
      </Menu>

      <a target='_blank' rel="noopener noreferrer" href={`https://github.com/chiyadev/nhitomi/commit/${client.currentInfo.version.hash}`}>
        <Button type='text' style={{
          position: 'absolute',
          left: 0,
          bottom: 0,
          minWidth: SideBarWidth,
          textAlign: 'center'
        }}>
          <small>ver. <code>{client.currentInfo.version.shortHash}</code></small>
        </Button>
      </a>
    </div>
  </Layout.Sider>
}
