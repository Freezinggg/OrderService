# OrderService

OrderService is a backend-focused learning project built to study backend, distributed coordination, and operational behavior under failure and retry scenarios.

The project started as a simple order creation service, then gradually evolved (v2) into a multi-instance operational backend system using:
- PostgreSQL
- Redis
- Nginx
- Docker Compose

The focus of this project is not feature completeness or frontend development.

The main goal is understanding how backend systems behave under:
- retries
- concurrency
- distributed coordination
- infrastructure pressure
- multi-instance deployments


---

# Features

- Order creation
- Idempotent request handling
- PostgreSQL persistence
- Redis shared coordination
- Distributed rate limiting using Redis TTL
- Admission control
- Concurrency protection
- Explicit failure semantics
- Metrics and observability
- Multi-instance API topology
- Nginx reverse proxy and load balancing


---

# Current Topology
Browser -> Nginx -> API-1 / API-2 -> Redis -> PostgreSQL

# Tech Stack
- .NET 9
- PostgresSQL
- Docker (services such as Redis and Nginx)
