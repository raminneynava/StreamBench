# StreamBench

**StreamBench** is a .NET project that benchmarks **traditional Streams** vs **System.IO.Pipelines** for high-throughput data processing.

It demonstrates how Pipelines can improve performance by reducing memory allocations and GC pressure when reading large streams of data over TCP.

---

## Features

- TCP Server sending large amounts of data.
- TCP Client reading data using:
  - **Traditional Stream**
  - **System.IO.Pipelines**
- Benchmarking execution time and total bytes read.
- Zero-allocation data processing with Pipelines.
- Easy to extend for WebSocket or file streaming scenarios.

---

## Why Pipelines?

Traditional streams often create temporary buffers for each read operation, causing:

- High memory allocations
- Frequent GC pauses
- Lower throughput

Pipelines reuse pooled memory segments to avoid repeated allocations, resulting in:

- Reduced memory usage
- Faster processing for high-throughput streams
- Smooth performance under heavy load

---

## Getting Started

### Prerequisites

- .NET 8 SDK or later

### Run TCP Server

```bash
cd TcpServer
dotnet run
