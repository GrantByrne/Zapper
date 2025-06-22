import axios from 'axios'

const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

export interface Device {
  id: string
  name: string
  type: string
  isOnline: boolean
  ipAddress?: string
  macAddress?: string
  capabilities: string[]
}

export interface Activity {
  id: string
  name: string
  description?: string
  devices: string[]
  commands: ActivityCommand[]
  isActive: boolean
}

export interface ActivityCommand {
  deviceId: string
  command: string
  parameters?: Record<string, any>
  delay?: number
}

export interface IRCode {
  id: string
  deviceType: string
  brand: string
  model: string
  command: string
  protocol: string
  data: string
}

export const api = {
  // Devices
  getDevices: async (): Promise<Device[]> => {
    const response = await apiClient.get('/devices')
    return response.data
  },

  getDevice: async (id: string): Promise<Device> => {
    const response = await apiClient.get(`/devices/${id}`)
    return response.data
  },

  addDevice: async (device: Omit<Device, 'id'>): Promise<Device> => {
    const response = await apiClient.post('/devices', device)
    return response.data
  },

  updateDevice: async (id: string, device: Partial<Device>): Promise<Device> => {
    const response = await apiClient.put(`/devices/${id}`, device)
    return response.data
  },

  deleteDevice: async (id: string): Promise<void> => {
    await apiClient.delete(`/devices/${id}`)
  },

  discoverDevices: async (): Promise<Device[]> => {
    const response = await apiClient.post('/devices/discover')
    return response.data
  },

  // Activities
  getActivities: async (): Promise<Activity[]> => {
    const response = await apiClient.get('/activities')
    return response.data
  },

  getActivity: async (id: string): Promise<Activity> => {
    const response = await apiClient.get(`/activities/${id}`)
    return response.data
  },

  createActivity: async (activity: Omit<Activity, 'id'>): Promise<Activity> => {
    const response = await apiClient.post('/activities', activity)
    return response.data
  },

  updateActivity: async (id: string, activity: Partial<Activity>): Promise<Activity> => {
    const response = await apiClient.put(`/activities/${id}`, activity)
    return response.data
  },

  deleteActivity: async (id: string): Promise<void> => {
    await apiClient.delete(`/activities/${id}`)
  },

  startActivity: async (id: string): Promise<void> => {
    await apiClient.post(`/activities/${id}/start`)
  },

  stopActivity: async (id: string): Promise<void> => {
    await apiClient.post(`/activities/${id}/stop`)
  },

  // Commands
  sendCommand: async (deviceId: string, command: string, parameters?: Record<string, any>): Promise<void> => {
    await apiClient.post(`/devices/${deviceId}/command`, { command, parameters })
  },

  // IR Codes
  getIRCodes: async (deviceType?: string, brand?: string): Promise<IRCode[]> => {
    const params = new URLSearchParams()
    if (deviceType) params.append('deviceType', deviceType)
    if (brand) params.append('brand', brand)
    
    const response = await apiClient.get(`/ir-codes?${params}`)
    return response.data
  },

  addIRCode: async (irCode: Omit<IRCode, 'id'>): Promise<IRCode> => {
    const response = await apiClient.post('/ir-codes', irCode)
    return response.data
  },
}