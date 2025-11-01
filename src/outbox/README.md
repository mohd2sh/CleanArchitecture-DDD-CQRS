# Outbox Pattern

## What It Does

The outbox pattern stores integration events in a database table when business operations happen. A background worker processes these events later to ensure they're delivered even if the app crashes.

## Current Implementation

Simple setup:
- Events written to outbox table in same transaction as business data
- Background workers process events one at a time
- Failed events retry automatically
- After max retries, events move to dead letter queue
- Works with multiple workers processing in parallel without conflicts

See ADR-004 and main README for details.

## Alternative Approaches

**CDC/Streaming:**
- Read database transaction logs directly
- Stream changes to message broker
- No polling needed
- Better for microservices and high throughput

**Message Broker Bridge:**
- Keep current outbox table
- Processor publishes to message broker (RabbitMQ, Kafka, etc.)
- External services consume from broker
- Good migration path

Both approaches keep the transactional guarantee but change how events are delivered.

## Code Structure

- `CleanArchitecture.Outbox.Abstractions/` - Interfaces and entities
- `CleanArchitecture.Outbox/` - EF Core implementation and processor
- `tests/` - Integration tests


