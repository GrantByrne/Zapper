// ZapperHub Web Application

class ZapperHubApp {
    constructor() {
        this.connection = null;
        this.devices = [];
        this.activities = [];
        this.currentDevice = null;
        this.init();
    }

    async init() {
        this.setupSignalR();
        this.setupEventHandlers();
        await this.loadInitialData();
        this.showSection('devices');
    }

    // SignalR Connection
    setupSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/zapper")
            .withAutomaticReconnect()
            .build();

        this.connection.start().then(() => {
            console.log('SignalR Connected');
            this.updateConnectionStatus('connected');
        }).catch(err => {
            console.error('SignalR Connection Error:', err);
            this.updateConnectionStatus('disconnected');
        });

        // Event handlers
        this.connection.on("DeviceStatusChanged", (data) => {
            this.handleDeviceStatusChanged(data);
        });

        this.connection.on("DeviceCommandExecuted", (data) => {
            this.handleDeviceCommandExecuted(data);
        });

        this.connection.on("ActivityStatusChanged", (data) => {
            this.handleActivityStatusChanged(data);
        });

        this.connection.on("ActivityStepExecuted", (data) => {
            this.handleActivityStepExecuted(data);
        });

        this.connection.on("SystemMessage", (data) => {
            this.showNotification(data.Message, data.Level);
        });

        this.connection.onreconnecting(() => {
            this.updateConnectionStatus('connecting');
        });

        this.connection.onreconnected(() => {
            this.updateConnectionStatus('connected');
        });

        this.connection.onclose(() => {
            this.updateConnectionStatus('disconnected');
        });
    }

    updateConnectionStatus(status) {
        const badge = document.getElementById('connection-status');
        badge.className = `badge bg-${status === 'connected' ? 'success' : status === 'connecting' ? 'warning' : 'danger'}`;
        badge.textContent = status.charAt(0).toUpperCase() + status.slice(1);
    }

    // Event Handlers
    setupEventHandlers() {
        // Form submission handlers
        document.getElementById('addDeviceForm').addEventListener('change', (e) => {
            if (e.target.name === 'connectionType') {
                this.toggleConnectionFields(e.target.value);
            }
        });

        // Load devices for remote selector
        this.loadRemoteDevices();
    }

    toggleConnectionFields(connectionType) {
        const networkGroup = document.getElementById('networkAddressGroup');
        const macGroup = document.getElementById('macAddressGroup');
        
        // Show/hide fields based on connection type
        if (['2', '3', '6'].includes(connectionType)) { // TCP, WebSocket, WebOS
            networkGroup.style.display = 'block';
        } else {
            networkGroup.style.display = 'none';
        }

        if (connectionType === '4') { // Bluetooth
            macGroup.style.display = 'block';
        } else {
            macGroup.style.display = 'none';
        }
    }

    // Data Loading
    async loadInitialData() {
        await Promise.all([
            this.loadDevices(),
            this.loadActivities(),
            this.loadSystemStatus()
        ]);
    }

    async loadDevices() {
        try {
            const response = await fetch('/api/devices');
            this.devices = await response.json();
            this.renderDevices();
            this.loadRemoteDevices();
        } catch (error) {
            console.error('Error loading devices:', error);
            this.showNotification('Failed to load devices', 'error');
        }
    }

    async loadActivities() {
        try {
            const response = await fetch('/api/activities');
            this.activities = await response.json();
            this.renderActivities();
        } catch (error) {
            console.error('Error loading activities:', error);
            this.showNotification('Failed to load activities', 'error');
        }
    }

    async loadSystemStatus() {
        try {
            const response = await fetch('/api/system/status');
            const status = await response.json();
            this.renderSystemStatus(status);
        } catch (error) {
            console.error('Error loading system status:', error);
        }
    }

    // Rendering
    renderDevices() {
        const container = document.getElementById('devices-list');
        container.innerHTML = '';

        if (this.devices.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center py-4">
                        <i class="bi bi-tv" style="font-size: 3rem; color: #ccc;"></i>
                        <p class="mt-3 text-muted">No devices configured</p>
                        <button class="btn btn-primary" onclick="showAddDeviceModal()">
                            <i class="bi bi-plus-lg me-1"></i>Add Your First Device
                        </button>
                    </div>
                </div>
            `;
            return;
        }

        this.devices.forEach(device => {
            const deviceCard = this.createDeviceCard(device);
            container.appendChild(deviceCard);
        });
    }

    createDeviceCard(device) {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4 mb-3';

        const typeIcons = {
            0: 'tv', 1: 'speaker', 2: 'router', 3: 'cast',
            4: 'controller', 5: 'speaker', 8: 'smart-watch'
        };

        col.innerHTML = `
            <div class="card device-card ${device.isOnline ? 'online' : 'offline'}" onclick="selectDevice(${device.id})">
                <div class="card-body position-relative">
                    <div class="device-status ${device.isOnline ? 'online' : 'offline'}"></div>
                    <div class="text-center">
                        <i class="bi bi-${typeIcons[device.type] || 'device-hdd'} device-icon"></i>
                        <h6 class="card-title">${device.name}</h6>
                        <p class="card-text text-muted">${device.brand} ${device.model}</p>
                        <small class="text-muted">
                            ${this.getConnectionTypeText(device.connectionType)} • 
                            ${device.isOnline ? 'Online' : 'Offline'}
                        </small>
                    </div>
                </div>
                <div class="card-footer bg-transparent">
                    <div class="btn-group w-100" role="group">
                        <button class="btn btn-sm btn-outline-primary" onclick="event.stopPropagation(); testDevice(${device.id})">
                            <i class="bi bi-activity"></i> Test
                        </button>
                        <button class="btn btn-sm btn-outline-secondary" onclick="event.stopPropagation(); editDevice(${device.id})">
                            <i class="bi bi-pencil"></i> Edit
                        </button>
                        <button class="btn btn-sm btn-outline-danger" onclick="event.stopPropagation(); deleteDevice(${device.id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;

        return col;
    }

    renderActivities() {
        const container = document.getElementById('activities-list');
        container.innerHTML = '';

        if (this.activities.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center py-4">
                        <i class="bi bi-play-circle" style="font-size: 3rem; color: #ccc;"></i>
                        <p class="mt-3 text-muted">No activities configured</p>
                        <button class="btn btn-primary" onclick="showAddActivityModal()">
                            <i class="bi bi-plus-lg me-1"></i>Create Your First Activity
                        </button>
                    </div>
                </div>
            `;
            return;
        }

        this.activities.forEach(activity => {
            const activityCard = this.createActivityCard(activity);
            container.appendChild(activityCard);
        });
    }

    createActivityCard(activity) {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4 mb-3';

        col.innerHTML = `
            <div class="card activity-card" onclick="executeActivity(${activity.id})">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <h6 class="card-title">${activity.name}</h6>
                            <p class="card-text text-muted">${activity.description || 'No description'}</p>
                        </div>
                        <div class="dropdown">
                            <button class="btn btn-sm btn-outline-secondary" onclick="event.stopPropagation()" data-bs-toggle="dropdown">
                                <i class="bi bi-three-dots-vertical"></i>
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item" href="#" onclick="editActivity(${activity.id})">
                                    <i class="bi bi-pencil me-2"></i>Edit
                                </a></li>
                                <li><a class="dropdown-item" href="#" onclick="duplicateActivity(${activity.id})">
                                    <i class="bi bi-files me-2"></i>Duplicate
                                </a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a class="dropdown-item text-danger" href="#" onclick="deleteActivity(${activity.id})">
                                    <i class="bi bi-trash me-2"></i>Delete
                                </a></li>
                            </ul>
                        </div>
                    </div>
                    <div class="mt-3">
                        <small class="text-muted">
                            ${activity.steps?.length || 0} steps • 
                            ${activity.isEnabled ? 'Enabled' : 'Disabled'}
                        </small>
                    </div>
                </div>
            </div>
        `;

        return col;
    }

    renderSystemStatus(status) {
        const container = document.getElementById('system-status');
        container.innerHTML = `
            <div class="status-item">
                <span>Status</span>
                <span class="status-value">${status.status}</span>
            </div>
            <div class="status-item">
                <span>Uptime</span>
                <span class="status-value">${this.formatUptime(status.system.uptime)}</span>
            </div>
            <div class="status-item">
                <span>Platform</span>
                <span class="status-value">${status.system.platform}</span>
            </div>
            <div class="status-item">
                <span>Connected Clients</span>
                <span class="status-value">${status.connectedClients}</span>
            </div>
        `;
    }

    loadRemoteDevices() {
        const select = document.getElementById('remote-device-select');
        select.innerHTML = '<option value="">Select Device...</option>';
        
        this.devices.forEach(device => {
            const option = document.createElement('option');
            option.value = device.id;
            option.textContent = `${device.name} (${device.brand})`;
            option.disabled = !device.isOnline;
            select.appendChild(option);
        });
    }

    // Utility Functions
    getConnectionTypeText(type) {
        const types = ['IR', 'RF', 'TCP', 'WebSocket', 'Bluetooth', 'USB', 'WebOS'];
        return types[type] || 'Unknown';
    }

    formatUptime(uptimeString) {
        // Parse uptime string and format nicely
        const match = uptimeString.match(/(\d+):(\d+):(\d+)/);
        if (match) {
            const hours = parseInt(match[1]);
            const minutes = parseInt(match[2]);
            if (hours > 0) {
                return `${hours}h ${minutes}m`;
            }
            return `${minutes}m`;
        }
        return uptimeString;
    }

    // Navigation
    showSection(sectionName) {
        // Hide all sections
        document.querySelectorAll('.content-section').forEach(section => {
            section.classList.add('d-none');
        });

        // Show selected section
        document.getElementById(`${sectionName}-section`).classList.remove('d-none');

        // Update navigation
        document.querySelectorAll('.nav-link').forEach(link => {
            link.classList.remove('active');
        });
        document.querySelector(`[onclick="showSection('${sectionName}')"]`).classList.add('active');
    }

    // Event Handlers
    handleDeviceStatusChanged(data) {
        const device = this.devices.find(d => d.id === data.DeviceId);
        if (device) {
            device.isOnline = data.IsOnline;
            this.renderDevices();
            this.loadRemoteDevices();
        }
        this.showNotification(`${data.DeviceName} is now ${data.IsOnline ? 'online' : 'offline'}`, 'info');
    }

    handleDeviceCommandExecuted(data) {
        this.showNotification(
            `${data.DeviceName}: ${data.CommandName} ${data.Success ? 'successful' : 'failed'}`,
            data.Success ? 'success' : 'error'
        );
    }

    handleActivityStatusChanged(data) {
        this.showNotification(`Activity ${data.ActivityName} ${data.Status}`, 'info');
    }

    handleActivityStepExecuted(data) {
        console.log('Activity step executed:', data);
    }

    // Actions
    async addDevice() {
        const form = document.getElementById('addDeviceForm');
        const formData = new FormData(form);
        
        const deviceData = {
            name: formData.get('name'),
            brand: formData.get('brand'),
            model: formData.get('model'),
            type: parseInt(formData.get('type')),
            connectionType: parseInt(formData.get('connectionType')),
            networkAddress: formData.get('networkAddress') || null,
            macAddress: formData.get('macAddress') || null,
            isOnline: false
        };

        try {
            const response = await fetch('/api/devices', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(deviceData)
            });

            if (response.ok) {
                this.showNotification('Device added successfully', 'success');
                bootstrap.Modal.getInstance(document.getElementById('addDeviceModal')).hide();
                form.reset();
                await this.loadDevices();
            } else {
                this.showNotification('Failed to add device', 'error');
            }
        } catch (error) {
            console.error('Error adding device:', error);
            this.showNotification('Error adding device', 'error');
        }
    }

    async discoverDevices() {
        this.showNotification('Discovering devices...', 'info');
        
        try {
            // Try WebOS discovery
            const webosResponse = await fetch('/api/devices/discover/webos', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ timeoutSeconds: 10 })
            });

            if (webosResponse.ok) {
                const webosDevices = await webosResponse.json();
                this.showNotification(`Found ${webosDevices.length} WebOS devices`, 'success');
            }

            // Try Bluetooth discovery
            const bluetoothResponse = await fetch('/api/devices/discover/bluetooth');
            if (bluetoothResponse.ok) {
                const bluetoothDevices = await bluetoothResponse.json();
                this.showNotification(`Found ${bluetoothDevices.length} Bluetooth devices`, 'success');
            }

        } catch (error) {
            console.error('Error discovering devices:', error);
            this.showNotification('Error during device discovery', 'error');
        }
    }

    async sendCommand(commandType) {
        const deviceSelect = document.getElementById('remote-device-select');
        const deviceId = deviceSelect.value;

        if (!deviceId) {
            this.showNotification('Please select a device first', 'warning');
            return;
        }

        try {
            const response = await fetch(`/api/devices/${deviceId}/command`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ commandName: commandType })
            });

            if (!response.ok) {
                this.showNotification(`Failed to send ${commandType} command`, 'error');
            }
        } catch (error) {
            console.error('Error sending command:', error);
            this.showNotification('Error sending command', 'error');
        }
    }

    async executeActivity(activityId) {
        try {
            const response = await fetch(`/api/activities/${activityId}/execute`, {
                method: 'POST'
            });

            if (!response.ok) {
                this.showNotification('Failed to execute activity', 'error');
            }
        } catch (error) {
            console.error('Error executing activity:', error);
            this.showNotification('Error executing activity', 'error');
        }
    }

    // UI Helpers
    showNotification(message, type = 'info') {
        const toast = document.getElementById('notification-toast');
        const toastBody = toast.querySelector('.toast-body');
        
        toastBody.textContent = message;
        
        // Update toast styling based on type
        toast.className = `toast ${type === 'error' ? 'border-danger' : type === 'success' ? 'border-success' : 'border-info'}`;
        
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
    }
}

// Global functions for HTML onclick handlers
let app;

function showSection(section) {
    app.showSection(section);
}

function showAddDeviceModal() {
    const modal = new bootstrap.Modal(document.getElementById('addDeviceModal'));
    modal.show();
}

function showAddActivityModal() {
    app.showNotification('Activity creation coming soon!', 'info');
}

function addDevice() {
    app.addDevice();
}

function discoverDevices() {
    app.discoverDevices();
}

function sendCommand(command) {
    app.sendCommand(command);
}

function executeActivity(activityId) {
    app.executeActivity(activityId);
}

function selectDevice(deviceId) {
    app.currentDevice = deviceId;
    app.showNotification(`Selected device ${deviceId}`, 'info');
}

function testDevice(deviceId) {
    app.showNotification(`Testing device ${deviceId}...`, 'info');
}

function editDevice(deviceId) {
    app.showNotification('Device editing coming soon!', 'info');
}

function deleteDevice(deviceId) {
    if (confirm('Are you sure you want to delete this device?')) {
        app.showNotification(`Deleting device ${deviceId}...`, 'info');
    }
}

function editActivity(activityId) {
    app.showNotification('Activity editing coming soon!', 'info');
}

function duplicateActivity(activityId) {
    app.showNotification('Activity duplication coming soon!', 'info');
}

function deleteActivity(activityId) {
    if (confirm('Are you sure you want to delete this activity?')) {
        app.showNotification(`Deleting activity ${activityId}...`, 'info');
    }
}

// Initialize app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    app = new ZapperHubApp();
});