import React, { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Search, Wifi, WifiOff, Edit, Trash2 } from 'lucide-react'
import toast from 'react-hot-toast'
import { api, Device } from '../services/api'

export function Devices() {
  const [searchTerm, setSearchTerm] = useState('')
  const [isDiscovering, setIsDiscovering] = useState(false)
  const queryClient = useQueryClient()

  const { data: devices, isLoading } = useQuery({
    queryKey: ['devices'],
    queryFn: api.getDevices,
  })

  const discoverMutation = useMutation({
    mutationFn: api.discoverDevices,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] })
      toast.success('Device discovery completed')
      setIsDiscovering(false)
    },
    onError: () => {
      toast.error('Failed to discover devices')
      setIsDiscovering(false)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: api.deleteDevice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] })
      toast.success('Device removed')
    },
    onError: () => {
      toast.error('Failed to remove device')
    },
  })

  const handleDiscover = () => {
    setIsDiscovering(true)
    discoverMutation.mutate()
  }

  const handleDelete = (device: Device) => {
    if (confirm(`Are you sure you want to remove ${device.name}?`)) {
      deleteMutation.mutate(device.id)
    }
  }

  const filteredDevices = devices?.filter(device =>
    device.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    device.type.toLowerCase().includes(searchTerm.toLowerCase())
  ) || []

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Loading...</div>
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            Devices
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Manage your connected devices
          </p>
        </div>
        <div className="flex space-x-3">
          <button
            onClick={handleDiscover}
            disabled={isDiscovering}
            className="btn-secondary flex items-center space-x-2"
          >
            <Search size={18} />
            <span>{isDiscovering ? 'Discovering...' : 'Discover'}</span>
          </button>
          <button className="btn-primary flex items-center space-x-2">
            <Plus size={18} />
            <span>Add Device</span>
          </button>
        </div>
      </div>

      <div className="card p-6">
        <div className="mb-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input
              type="text"
              placeholder="Search devices..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredDevices.map((device) => (
            <div key={device.id} className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
              <div className="flex justify-between items-start mb-3">
                <div className="flex items-center">
                  {device.isOnline ? (
                    <Wifi className="h-5 w-5 text-green-500 mr-2" />
                  ) : (
                    <WifiOff className="h-5 w-5 text-red-500 mr-2" />
                  )}
                  <h3 className="font-semibold text-gray-900 dark:text-white">
                    {device.name}
                  </h3>
                </div>
                <div className="flex space-x-2">
                  <button className="text-gray-500 hover:text-primary-600">
                    <Edit size={16} />
                  </button>
                  <button 
                    onClick={() => handleDelete(device)}
                    className="text-gray-500 hover:text-red-600"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>
              
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-600 dark:text-gray-400">Type:</span>
                  <span className="text-gray-900 dark:text-white">{device.type}</span>
                </div>
                {device.ipAddress && (
                  <div className="flex justify-between">
                    <span className="text-gray-600 dark:text-gray-400">IP:</span>
                    <span className="text-gray-900 dark:text-white">{device.ipAddress}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span className="text-gray-600 dark:text-gray-400">Status:</span>
                  <span className={device.isOnline ? 'text-green-600' : 'text-red-600'}>
                    {device.isOnline ? 'Online' : 'Offline'}
                  </span>
                </div>
              </div>

              <div className="mt-3 flex flex-wrap gap-1">
                {device.capabilities.map((capability) => (
                  <span 
                    key={capability}
                    className="inline-block px-2 py-1 text-xs bg-primary-100 text-primary-800 dark:bg-primary-900 dark:text-primary-200 rounded"
                  >
                    {capability}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>

        {filteredDevices.length === 0 && (
          <div className="text-center py-12">
            <Wifi className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-4 text-lg font-medium text-gray-900 dark:text-white">
              No devices found
            </h3>
            <p className="mt-2 text-gray-600 dark:text-gray-400">
              {searchTerm ? 'Try adjusting your search term.' : 'Get started by discovering or adding a device.'}
            </p>
          </div>
        )}
      </div>
    </div>
  )
}