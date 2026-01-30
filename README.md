## OrderService (OrderLite)
OrderService is a backend-only service built as part of a self backend learning.

The goal of this project is not feature completeness, but depth of understanding around core backend correctness problems such as:

- Atomicity
- Idempotency
- Domain invariants
- Failure-safe request handling
- This service focuses on order creation only, under retry and failure scenarios.
- This service is a stand-alone service and doesnt serve any other backend/frontend.

## Scope
What this service does :
- Accepts a CreateOrder request
- Validates domain invariants
- Creates an Order in a single atomic boundary
- Enforces idempotency to prevent duplicate orders
- Returns the same result on safe retries

What this service does NOT do
- Payment processing
- Inventory management
- Shipping
- UI / frontend
- Microservices communication
- This is intentionally a single-service system

## Core Concepts
- Domain Invariants : enforced in constructor
- Idempotency : Client-provided key, one intent -> one order
- Immutability : Domain state is controlled and not mutable freely
- Clear Boundaries : Domain, Application, and Infrastructure are separated based on their responsibilities

## Current State
- Persistence in-memory only
- Transaction boundaries are explicitly designed, and enforced by DB
- Metrics are implemented
- Pressure/Admission Control are implemented in controller
- Concurrency Controller are implemented in controller

## Purpose of This Project
This project exists to answer questions like:
- How do retries behave under failure?
- Where should correctness be enforced?
- What happen if system crash?

It is a learning project focused on reasoning about backend systems, not shipping a product.

## Note
This is a learning project, so please expect some bug, minor changes, and a bit choke.

