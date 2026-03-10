## OrderService (OrderLite)

OrderService is a backend-only service built as part of a self-directed backend engineering study.

The goal of this project is not feature completeness, but to explore and implement core backend correctness guarantees such as:

- Atomicity
- Idempotency
- Domain invariants
- Failure-safe request handling
- Retry-safe behavior under network uncertainty
- System protection under load

The service focuses on **order creation under retry and failure scenarios**.  
It is intentionally designed as a **stand-alone backend service** with no external dependencies on frontend or other services.


## Scope

What this service DOES:

- Accepts a `CreateOrder` request
- Validates domain invariants
- Creates an Order within a single atomic transaction boundary
- Enforces idempotency using a client-provided key
- Returns the same result for safe retries
- Classifies failures with explicit error semantics
- Protects the system under load through admission control and concurrency limits


What this service DOES NOT do:

- Payment processing
- Inventory management
- Shipping
- UI / frontend
- Cross-service communication
- Microservice orchestration

This is intentionally a **single-service system focused on correctness guarantees**.


## System Guarantees

OrderService provides the following guarantees:

- **Atomic Order Creation**  
  Order creation either fully succeeds or leaves no partial state.

- **Idempotent Request Handling**  
  One IdempotencyKey corresponds to exactly one order intent and one outcome.

- **Retry Safety**  
  Duplicate requests caused by network retries return the same result without creating duplicate orders.

- **Explicit Failure Semantics**  
  The system classifies failures (invalid input, business rejection, infrastructure failure) so clients know how to react.

- **System Protection Under Load**  
  Admission control and concurrency limits prevent overload and protect shared resources.


## Core Concepts

- **Domain Invariants**  
  Enforced at object construction to guarantee valid domain state.

- **Idempotency**  
  Client-provided IdempotencyKey ensures  
  `1 key → 1 intent → 1 order`.

- **Immutability**  
  Domain state is controlled and cannot be freely mutated.

- **Clear Layered Boundaries**  
  Domain, Application, and Infrastructure layers are separated by responsibility.


## Architecture Overview

The system follows a layered architecture:

- **Domain Layer**  
  Contains domain entities and invariant enforcement.

- **Application Layer**  
  Handles use-case orchestration, transaction boundaries, and idempotency logic.

- **Infrastructure Layer**  
  Implements persistence, metrics, and external concerns.


## Failure Semantics

The system distinguishes between different failure categories:

- **Invalid** – Client sent invalid data (violates invariants)
- **Fail** – Valid request rejected by business or state rules
- **ServiceUnavailable** – System capacity or infrastructure problem
- **Error** – Unexpected system bug

Each failure type maps to clear HTTP responses and observability signals.


## Observability

The service emits metrics representing system decisions such as:

- Successful order creation
- Invalid client requests
- Business rule rejections
- Infrastructure failures
- Admission control rejections

Metrics are designed to support fast operational diagnosis.


## Current State

- Persistence: currently in-memory (designed for DB-backed transactions)
- Transaction boundaries explicitly modeled
- Idempotency enforcement implemented
- Metrics and observability included
- Admission control implemented
- Concurrency limits implemented


## Purpose of This Project

This project exists to explore questions such as:

- How should retries behave under failure?
- Where should correctness guarantees be enforced?
- What happens when a system crashes mid-request?
- How can a backend system remain safe under repeated requests?

The project focuses on **reasoning about backend system correctness**, not building a full commercial product.


## Project Status

This is a learning-focused project that prioritizes correctness guarantees and system behavior under failure scenarios rather than feature completeness.
