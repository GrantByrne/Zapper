import React, { useState } from 'react'
import { useQuery, useMutation } from '@tanstack/react-query'
import { 
  Power, 
  VolumeX, 
  Volume1, 
  Volume2, 
  ChevronUp, 
  ChevronDown, 
  ChevronLeft, 
  ChevronRight,
  Circle,
  RotateCcw,
  Home,
  Menu,
  ArrowLeft
} from 'lucide-react'
import toast from 'react-hot-toast'
import { api, Device } from '../services/api'

export function Remote() {
  const [selectedDevice, setSelectedDevice] = useState<string>('')
  
  const { data: devices } = useQuery({
    queryKey: ['devices'],
    queryFn: api.getDevices,
  })

  const commandMutation = useMutation({
    mutationFn: ({ deviceId, command, parameters }: { 
      deviceId: string; 
      command: string; 
      parameters?: Record<string, any> 
    }) => api.sendCommand(deviceId, command, parameters),
    onSuccess: () => {
      toast.success('Command sent')
    },
    onError: () => {
      toast.error('Failed to send command')
    },
  })

  const sendCommand = (command: string, parameters?: Record<string, any>) => {
    if (!selectedDevice) {
      toast.error('Please select a device first')
      return
    }
    commandMutation.mutate({ deviceId: selectedDevice, command, parameters })
  }

  const RemoteButton = ({ 
    children, 
    command, 
    className = '', 
    parameters,
    size = 'md' 
  }: {
    children: React.ReactNode
    command: string
    className?: string
    parameters?: Record<string, any>
    size?: 'sm' | 'md' | 'lg'
  }) => {
    const sizeClasses = {
      sm: 'p-2 text-sm',
      md: 'p-3',
      lg: 'p-4 text-lg'
    }
    
    return (
      <button
        onClick={() => sendCommand(command, parameters)}
        disabled={commandMutation.isPending}
        className={`bg-gray-800 hover:bg-gray-700 text-white rounded-lg transition-colors duration-200 flex items-center justify-center ${sizeClasses[size]} ${className}`}
      >
        {children}
      </button>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Remote Control
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Control your devices with the virtual remote
        </p>
      </div>

      <div className="card p-6">
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Select Device
          </label>
          <select
            value={selectedDevice}
            onChange={(e) => setSelectedDevice(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
          >
            <option value="">Choose a device...</option>
            {devices?.map((device) => (
              <option key={device.id} value={device.id}>
                {device.name} ({device.type})
              </option>
            ))}
          </select>
        </div>

        {selectedDevice && (
          <div className="max-w-sm mx-auto">
            {/* Power and Menu Row */}
            <div className="grid grid-cols-3 gap-3 mb-4">
              <RemoteButton command="power" className="col-start-2">
                <Power size={20} />
              </RemoteButton>
            </div>

            {/* Navigation Buttons */}
            <div className="grid grid-cols-3 gap-3 mb-4">
              <div></div>
              <RemoteButton command="up">
                <ChevronUp size={24} />
              </RemoteButton>
              <div></div>
              <RemoteButton command="left">
                <ChevronLeft size={24} />
              </RemoteButton>
              <RemoteButton command="select" size="lg">
                <Circle size={24} />
              </RemoteButton>
              <RemoteButton command="right">
                <ChevronRight size={24} />
              </RemoteButton>
              <div></div>
              <RemoteButton command="down">
                <ChevronDown size={24} />
              </RemoteButton>
              <div></div>
            </div>

            {/* Control Buttons */}
            <div className="grid grid-cols-3 gap-3 mb-4">
              <RemoteButton command="back">
                <ArrowLeft size={20} />
              </RemoteButton>
              <RemoteButton command="home">
                <Home size={20} />
              </RemoteButton>
              <RemoteButton command="menu">
                <Menu size={20} />
              </RemoteButton>
            </div>

            {/* Volume Controls */}
            <div className="grid grid-cols-3 gap-3 mb-4">
              <RemoteButton command="volume_down">
                <Volume1 size={20} />
              </RemoteButton>
              <RemoteButton command="mute">
                <VolumeX size={20} />
              </RemoteButton>
              <RemoteButton command="volume_up">
                <Volume2 size={20} />
              </RemoteButton>
            </div>

            {/* Channel Controls */}
            <div className="grid grid-cols-2 gap-3 mb-4">
              <RemoteButton command="channel_up" className="flex flex-col">
                <ChevronUp size={16} />
                <span className="text-xs">CH+</span>
              </RemoteButton>
              <RemoteButton command="channel_down" className="flex flex-col">
                <ChevronDown size={16} />
                <span className="text-xs">CH-</span>
              </RemoteButton>
            </div>

            {/* Number Pad */}
            <div className="grid grid-cols-3 gap-3 mb-4">
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 0].map((num) => (
                <RemoteButton 
                  key={num} 
                  command="number" 
                  parameters={{ number: num }}
                  className={num === 0 ? "col-start-2" : ""}
                >
                  {num}
                </RemoteButton>
              ))}
            </div>

            {/* Utility Buttons */}
            <div className="grid grid-cols-2 gap-3">
              <RemoteButton command="last" className="text-sm">
                Last
              </RemoteButton>
              <RemoteButton command="info" className="text-sm">
                Info
              </RemoteButton>
            </div>
          </div>
        )}

        {!selectedDevice && (
          <div className="text-center py-12">
            <div className="mx-auto h-12 w-12 bg-gray-200 dark:bg-gray-700 rounded-lg flex items-center justify-center mb-4">
              <Circle className="h-6 w-6 text-gray-400" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              Select a device to start
            </h3>
            <p className="text-gray-600 dark:text-gray-400">
              Choose a device from the dropdown above to use the remote control.
            </p>
          </div>
        )}
      </div>
    </div>
  )
}