# Mini Project 07 - Testing Data Access with EF Core and Dapper (.NET 10)

## Project Overview
This project validates data access correctness by implementing the same query with EF Core and Dapper and proving both return identical results against a real PostgreSQL database.

Context: technical portfolio and educational showcase focused on data access testing and tooling trade-offs.

Relevant for: Backend Engineers, Data Access and Persistence, Repository Testing, and SQL/ORM decision-making.

## Architecture & Design
- Architectural style: layered data access slice (Entities, Repositories, Models, Migrations) inside a single library.
- Separation of responsibilities:
  - Entities define the relational model.
  - AppDbContext maps entities to PostgreSQL tables.
  - Repositories encapsulate query logic (EF Core and Dapper).
  - Tests orchestrate database lifecycle and validate query contracts.
- Main flow:
  1. Test starts a PostgreSQL container.
  2. EF Core migrations are applied.
  3. Test data is inserted.
  4. Repository executes the price-range query.
  5. Assertions validate ordering and results.

## Tech Stack
- Language & runtime: C# on .NET 10
- ORM: Entity Framework Core 10
- Micro-ORM: Dapper 2.1.66
- Database driver: Npgsql 10
- Database: PostgreSQL (Testcontainers)
- Testing: xUnit, FluentAssertions, Testcontainers for .NET
- Infra/DevOps: Docker (required to run integration tests)

## Key Features
- Two repository implementations for the same query: EF Core and Dapper
- Explicit SQL and explicit LINQ projection with deterministic ordering
- Integration tests against a real PostgreSQL instance
- Per-test database creation with migrations applied
- Consistency tests that compare EF Core vs Dapper output
- Auto-skip integration tests when Docker is not available

## API / Application Behavior
This solution is a data access library and does not expose an HTTP API.

Repository behavior:
- Input: min and max price values
- Output: a list of ProductInRange records ordered by price and name
- Errors: database and query errors bubble up as exceptions (no custom error handling)
- Validations: no explicit validation beyond database constraints

## Testing Strategy
- Test type: integration tests only
- Tools: xUnit + FluentAssertions + Testcontainers
- Organization: repository-focused tests in 	ests/App.RepositoryTests
- Execution details:
  - Each test creates its own database
  - Migrations are applied before seeding
  - Tests are skipped when Docker is not available

## How to Run the Project
Prerequisites:
- .NET 10 SDK
- Docker (to execute integration tests)

Commands:
- dotnet test

If Docker is not running, tests are skipped automatically.

## Project Status
Completed as a technical portfolio project. No active feature development.

## Why This Project Matters
This project demonstrates:
- Strong understanding of data access correctness and repository testing
- Practical trade-offs between ORM and hand-written SQL
- Discipline in deterministic queries and integration testing with real infrastructure
