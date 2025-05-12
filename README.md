# SentinelVision Enterprise Edition

**Created by [Mirlan Nurbekov](https://github.com/MirlanNurbekov)**

Industrial-grade WPF/.NET 6 solution implementing a microservices-inspired architecture for building security and hardware control.

---

## 🚀 Features

- **AI-driven Face Recognition**  
  Pluggable ML backends (ONNX, TensorFlow) for high-accuracy identification.
- **Real-time CCTV Monitoring**  
  RTSP and TCP camera controllers with frame-level event hooks.
- **Secure Door & Elevator Access**  
  Serial, TCP/IP and HTTP adapters for lockdown, unlock, and elevator call.
- **Central Event Bus**  
  RabbitMQ-powered, event-driven decoupling of services and hardware.
- **Telemetry & Health Checks**  
  Prometheus metrics and liveness/readiness probes on every service.
- **Plugin Framework**  
  Discover and load new device controllers at runtime via MEF.
- **Configuration & Logging**  
  Microsoft.Extensions.Configuration + Serilog for structured, multi-sink logging.
- **Automated Test Suite**  
  Unit, integration, and hardware-simulation tests ensure reliability.

---

## 📦 Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)  
- [Docker & Docker Compose](https://docs.docker.com/compose/) (for RabbitMQ, PostgreSQL)  
- (Optional) RabbitMQ and PostgreSQL instances

### Clone & Configure

```bash
git clone https://github.com/MirlanNurbekov/SentinelVision.git
cd SentinelVision


🏗 Architecture
flowchart LR
  subgraph Core
    Domain
    Application
    Interfaces
  end

  subgraph Infrastructure
    Persistence
    HardwareAdapters
    MLAdapters
    EventBus
  end

  subgraph Services
    API
    Monitoring
    UI
    Plugins
    Notifications
  end

  Core --> Infrastructure --> Services
  Infrastructure --> Services
  Services --> Core


📁 Folder Layout
SentinelVision.sln
│
├── Core
│   ├── Core.Domain
│   ├── Core.Application
│   └── Core.Interfaces
│
├── Infrastructure
│   ├── Persistence       
│   ├── HardwareAdapters   
│   ├── MLAdapters         
│   └── EventBus           
│
├── Services
│   ├── Services.API
│   ├── Services.UI
│   ├── Services.Monitoring
│   ├── Services.Plugins
│   └── Services.Notifications
│
├── Tests
│   ├── UnitTests
│   ├── IntegrationTests
│   └── SimulationTests
│
└── Deployment
    ├── docker-compose.yml
    └── k8s-manifests