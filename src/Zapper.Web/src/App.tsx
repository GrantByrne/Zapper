import React from 'react'
import { Routes, Route } from 'react-router-dom'
import { Layout } from './components/Layout'
import { Dashboard } from './pages/Dashboard'
import { Devices } from './pages/Devices'
import { Activities } from './pages/Activities'
import { Remote } from './pages/Remote'

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/devices" element={<Devices />} />
        <Route path="/activities" element={<Activities />} />
        <Route path="/remote" element={<Remote />} />
      </Routes>
    </Layout>
  )
}

export default App