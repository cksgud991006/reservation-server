# Ticket Booking Engine (Concurrency Mock)
A high-performance .NET backend designed to simulate and solve the challenges of high-concurrency ticket issuance. This project focuses on maintaining strict data integrity when hundreds of users compete for limited inventory (e.g., a single seat).

## Technical Focus
* Race Condition Mitigation: Implementing strategies to prevent "double-booking" during sub-millisecond request bursts.

* Distributed State Management: Leveraging Redis for high-speed atomic operations and seat-status caching.

* Dynamic Scenario Testing: A custom-built provider that swaps system behavior (High Contention vs. Normal Load) via environment-driven JSON configurations.

---

## Tech Stack
* Runtime: .NET 8 / C# (Asynchronous programming with IHostedService)

* Database: PostgreSQL (Relational integrity and ACID transactions)

* Caching/Locking: Redis (Distributed locking and concurrency control)

---

## System Architecture

The system uses a layered architecture with clear separation of concerns:

API (Controller)
→ Application Service
→ Repository (Data Access)
→ Database

The system handles requests in multiple layers:
1. Validation Layer: Validates seat availability in Redis.
2. Concurrency Layer: Uses Redis with Lua scripts to isolate the booking process.
3. Persistence Layer: Ensures the transaction is committed to PostgreSQL.

## Performance
The configurable scenario simulations are defined in `scenarios/*.json`.

| Scenario | Description | Key Metric | Full Report |
| :--- | :--- | :--- | :--- |
| **High Contention** | 1 Seat vs 100 Concurrent Requests | **Integrity**: Exactly 1 success, 99 failures | [View HTML](https://cksgud991006.github.io/ticket-issuance-service/performance/contention_test/html/index.html) |
| **Stress Test** | 1M volumes of flights and seats | **Throughput:** 1146.67 RPS <br> **Stability:** $Median \approx 118.00 ms$ | [View HTML](https://cksgud991006.github.io/ticket-issuance-service/performance/stress_test/html/index.html) |

Detailed metrics can be found in [performance](https://github.com/cksgud991006/ticket-issuance-service/tree/main/performance)
