import React, { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Play, Square, Edit, Trash2 } from 'lucide-react'
import toast from 'react-hot-toast'
import { api, Activity } from '../services/api'

export function Activities() {
  const [activeActivity, setActiveActivity] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const { data: activities, isLoading } = useQuery({
    queryKey: ['activities'],
    queryFn: api.getActivities,
  })

  const startMutation = useMutation({
    mutationFn: api.startActivity,
    onSuccess: (_, activityId) => {
      setActiveActivity(activityId)
      toast.success('Activity started')
      queryClient.invalidateQueries({ queryKey: ['activities'] })
    },
    onError: () => {
      toast.error('Failed to start activity')
    },
  })

  const stopMutation = useMutation({
    mutationFn: api.stopActivity,
    onSuccess: () => {
      setActiveActivity(null)
      toast.success('Activity stopped')
      queryClient.invalidateQueries({ queryKey: ['activities'] })
    },
    onError: () => {
      toast.error('Failed to stop activity')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: api.deleteActivity,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activities'] })
      toast.success('Activity deleted')
    },
    onError: () => {
      toast.error('Failed to delete activity')
    },
  })

  const handleStart = (activity: Activity) => {
    if (activeActivity) {
      stopMutation.mutate(activeActivity)
    }
    startMutation.mutate(activity.id)
  }

  const handleStop = (activity: Activity) => {
    stopMutation.mutate(activity.id)
  }

  const handleDelete = (activity: Activity) => {
    if (confirm(`Are you sure you want to delete "${activity.name}"?`)) {
      deleteMutation.mutate(activity.id)
    }
  }

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Loading...</div>
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            Activities
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Create and manage your home theater activities
          </p>
        </div>
        <button className="btn-primary flex items-center space-x-2">
          <Plus size={18} />
          <span>New Activity</span>
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {activities?.map((activity) => (
          <div key={activity.id} className="card p-6">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {activity.name}
                </h3>
                {activity.description && (
                  <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                    {activity.description}
                  </p>
                )}
              </div>
              <div className="flex space-x-2">
                <button className="text-gray-500 hover:text-primary-600">
                  <Edit size={16} />
                </button>
                <button 
                  onClick={() => handleDelete(activity)}
                  className="text-gray-500 hover:text-red-600"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            </div>

            <div className="mb-4">
              <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                Devices ({activity.devices.length}):
              </p>
              <div className="flex flex-wrap gap-1">
                {activity.devices.map((deviceId) => (
                  <span 
                    key={deviceId}
                    className="inline-block px-2 py-1 text-xs bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300 rounded"
                  >
                    {deviceId}
                  </span>
                ))}
              </div>
            </div>

            <div className="mb-4">
              <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                Commands ({activity.commands.length}):
              </p>
              <div className="space-y-1 max-h-20 overflow-y-auto">
                {activity.commands.map((command, index) => (
                  <div key={index} className="text-xs text-gray-600 dark:text-gray-400">
                    {command.command}
                    {command.delay && <span className="text-gray-500"> (delay: {command.delay}ms)</span>}
                  </div>
                ))}
              </div>
            </div>

            <div className="flex space-x-2">
              {activity.isActive || activeActivity === activity.id ? (
                <button
                  onClick={() => handleStop(activity)}
                  disabled={stopMutation.isPending}
                  className="flex-1 bg-red-600 hover:bg-red-700 text-white font-medium py-2 px-4 rounded-md transition-colors duration-200 flex items-center justify-center space-x-2"
                >
                  <Square size={16} />
                  <span>Stop</span>
                </button>
              ) : (
                <button
                  onClick={() => handleStart(activity)}
                  disabled={startMutation.isPending}
                  className="flex-1 btn-primary flex items-center justify-center space-x-2"
                >
                  <Play size={16} />
                  <span>Start</span>
                </button>
              )}
            </div>
          </div>
        )) || []}
      </div>

      {(!activities || activities.length === 0) && (
        <div className="text-center py-12">
          <Play className="mx-auto h-12 w-12 text-gray-400" />
          <h3 className="mt-4 text-lg font-medium text-gray-900 dark:text-white">
            No activities yet
          </h3>
          <p className="mt-2 text-gray-600 dark:text-gray-400">
            Create your first activity to control multiple devices with one tap.
          </p>
          <button className="mt-4 btn-primary flex items-center space-x-2 mx-auto">
            <Plus size={18} />
            <span>Create Activity</span>
          </button>
        </div>
      )}
    </div>
  )
}