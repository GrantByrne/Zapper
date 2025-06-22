import React from 'react'
import { NavLink } from 'react-router-dom'
import { Home, Tv, Play, Smartphone } from 'lucide-react'

interface LayoutProps {
  children: React.ReactNode
}

export function Layout({ children }: LayoutProps) {
  const navItems = [
    { to: '/', icon: Home, label: 'Dashboard' },
    { to: '/devices', icon: Tv, label: 'Devices' },
    { to: '/activities', icon: Play, label: 'Activities' },
    { to: '/remote', icon: Smartphone, label: 'Remote' },
  ]

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="flex">
        <nav className="w-64 bg-white dark:bg-gray-800 shadow-lg">
          <div className="p-6">
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
              Zapper
            </h1>
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Universal Remote Control
            </p>
          </div>
          <ul className="space-y-2 px-4 pb-6">
            {navItems.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  className={({ isActive }) =>
                    `flex items-center space-x-3 px-4 py-3 rounded-lg transition-colors duration-200 ${
                      isActive
                        ? 'bg-primary-50 text-primary-700 dark:bg-primary-900 dark:text-primary-300'
                        : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700'
                    }`
                  }
                >
                  <item.icon size={20} />
                  <span className="font-medium">{item.label}</span>
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>
        <main className="flex-1 p-8">
          {children}
        </main>
      </div>
    </div>
  )
}