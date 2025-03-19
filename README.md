#  Company.Cmms — Clean Architecture (DDD + CQRS)

> **Status:**  Work In Progress

This project aims to illustrate how to design and implement a **Computerized Maintenance Management System (CMMS)** using **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** principles — keeping simplicity and practicality at the core.

---

## Goal

The project demonstrates how to build a maintainable and scalable .NET application by organizing the codebase around **domain boundaries** such as Work Orders, Technicians, and Assets — all central concepts in a CMMS.

The goal is not to over-engineer but to show how **DDD and CQRS can be applied in balance**, with enough structure to gain clarity and flexibility, without unnecessary complexity.

---

## Domain Example — CMMS

The CMMS domain focuses on tracking **work orders**, **asset maintenance**, and **technician assignments**.  
For example:

- A **Work Order** is created to repair or inspect an **Asset**.  
- A **Technician** is assigned to perform the work.  
- The work order progresses through statuses like *Open → Assigned → In Progress → Completed*.  

This simple workflow is used to illustrate aggregates, entities, value objects, and domain events.

---

## CQRS in Action

The project adopts **CQRS (Command Query Responsibility Segregation)** to separate **write** and **read** concerns:

- **Write side:**
  - Uses transactional repositories implementing 
  - Changes are persisted via `IUnitOfWork.SaveChangesAsync()`.

- **Read side:**
  - Uses non transactional queries against read database or other sources.

This separation allows flexibility: the write model enforces business rules, while the read model optimizes for performance.

---

##  Domain-Driven Design Highlights

- **Aggregates & Entities:** Each domain module (WorkOrder, Technician, Asset) is modeled as an aggregate root with encapsulated behavior.
- **Value Objects:** Reusable, immutable concepts like `Location`, `SkillLevel`, or `AssetTag` ensure consistency.
- **Domain Events:** Capture meaningful changes such as `TechnicianAssignedEvent` or `AssetMaintenanceStartedEvent`.
- **Repositories:** Abstract away persistence details — they return aggregates, not database models.


---

## Architectural Tests

The project will include **ArchUnit-like tests** (using [NetArchTest.Rules](https://github.com/BenMorris/NetArchTest)) to enforce boundaries and prevent future violations:

- Application layer cannot depend on Infrastructure.
- Domain remains persistence-agnostic.
- QueryHandler should only use IReadRepository
- Application layer cannot return a domain entity
- CQRS handlers follow naming and structure conventions.
- Others..

This ensures future contributors don’t unintentionally break the architectural principles.

---

## Design Philosophy

The goal is **balance** — a realistic Clean Architecture setup that’s simple enough for daily work but structured enough to scale.

Focus on practicality:
- Avoiding over-engineering or unused abstractions.
- Showing a pragmatic balance between clean design and productivity.