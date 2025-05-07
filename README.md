# README.md

**Created by [Mirlan Nurbekov](https://github.com/MirlanNurbekov)**

SentinelVision Enterprise Edition is an industrial-grade WPF/.NET 6 solution implementing a microservices-inspired architecture for building security management and hardware control. It features:

- **AI-driven Face Recognition** with pluggable ML backends (ONNX, TensorFlow)
- **Real-time CCTV Monitoring** via RTSP and TCP camera controllers
- **Secure Door & Elevator Access** through serial, TCP/IP, and HTTP controllers
- **Central Event Bus** (RabbitMQ) for decoupled event-driven workflows
- **Telemetry & Health Checks** exposing Prometheus metrics and liveness probes
- **Plugin Framework** enabling dynamic loading of new device controllers
- **Configuration & Logging** via Microsoft.Extensions.Configuration and Serilog
- **Automated Tests** covering unit, integration, and hardware simulation scenarios
