import React from 'react'
import { useQuery } from '@tanstack/react-query'
import { Tv, Play, Activity } from 'lucide-react'
import { api } from '../services/api'

export function Dashboard() {
  const { data: devices } = useQuery({
    queryKey: ['devices'],
    queryFn: api.getDevices,
  })

  const { data: activities } = useQuery({
    queryKey: ['activities'],
    queryFn: api.getActivities,
  })

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Dashboard
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Overview of your connected devices and activities
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card p-6">
          <div className="flex items-center">
            <div className="p-3 bg-primary-100 dark:bg-primary-900 rounded-lg">
              <Tv className="h-6 w-6 text-primary-600 dark:text-primary-300" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Connected Devices
              </p>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">
                {devices?.length || 0}
              </p>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 dark:bg-green-900 rounded-lg">
              <Play className="h-6 w-6 text-green-600 dark:text-green-300" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Activities
              </p>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">
                {activities?.length || 0}
              </p>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 dark:bg-blue-900 rounded-lg">
              <Activity className="h-6 w-6 text-blue-600 dark:text-blue-300" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Status
              </p>
              <p className="text-2xl font-bold text-green-600">
                Online
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="card p-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Recent Activities
          </h2>
          <div className="space-y-3">
            {activities?.slice(0, 5).map((activity) => (
              <div key={activity.id} className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center">
                  <Play className="h-4 w-4 text-gray-500 mr-3" />
                  <span className="font-medium text-gray-900 dark:text-white">
                    {activity.name}
                  </span>
                </div>
                <button className="btn-primary text-sm py-1 px-3">
                  Start
                </button>
              </div>
            )) || (
              <p className="text-gray-500 dark:text-gray-400">
                No activities configured yet
              </p>
            )}
          </div>
        </div>

        <div className="card p-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Device Status
          </h2>
          <div className="space-y-3">
            {devices?.slice(0, 5).map((device) => (
              <div key={device.id} className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center">
                  <div className={`w-3 h-3 rounded-full mr-3 ${
                    device.isOnline ? 'bg-green-500' : 'bg-red-500'
                  }`} />
                  <span className="font-medium text-gray-900 dark:text-white">
                    {device.name}
                  </span>
                </div>
                <span className={`text-sm ${
                  device.isOnline ? 'text-green-600' : 'text-red-600'
                }`}>
                  {device.isOnline ? 'Online' : 'Offline'}
                </span>
              </div>
            )) || (
              <p className="text-gray-500 dark:text-gray-400">
                No devices connected yet
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}