# Senior .NET Take-Home Task

## Overview
Build a single API endpoint that lists pharmacy orders with filtering, sorting, pagination, and one business rule. Keep setup minimal and focus on senior-level judgement.

⏱️ Timebox: Aim for under three hours. Stop at three hours and note what remains.

## What You Will Build
- .NET 8 Web API with one endpoint: `GET /orders`
- In-memory data source loaded from `sample-orders.json`

## Dataset
Use the provided `sample-orders.json` with 1,000 realistic orders across 50 pharmacies. Load it in memory at startup. Do not add a database.

## Functional Requirements
- **Endpoint:** `GET /orders`
- **Query parameters:**
  - `pharmacyId` string, optional
  - `status` repeatable, optional (eg: `status=Pending&status=Shipped`)
  - `from` and `to` ISO dates, optional
  - `sort` in `{createdAt,totalCents}`, default `createdAt`
  - `dir` in `{asc,desc}`, default `desc`
  - `page` integer, default `1`
  - `pageSize` integer, default `20`, max `100`
- **Response shape:**
  - `items` array of orders
  - `page`, `pageSize`, `total`
- **Business rule:**
  - If an order's `totalCents` is greater than a configurable daily threshold, include `needsReview: true` on that item
  - Threshold set by config key `Review:DailyOrderThresholdCents`

## Non-Functional Requirements
- **Validation**
  - Validate query parameters and return a useful error for bad input
  - Enforce `pageSize` maximum 100
- **Default behaviour**
  - Default sort is `createdAt desc`
  - Pagination should be stable under repeated calls with the same parameters
- **Observability**
  - Log one structured line per request with correlation ID, validated params, elapsed ms, and item count
  - Include error logs for validation failures
- **Performance thinking**
  - Stay in memory for this exercise
  - In your README describe the indexes/query approach you would use in production and how you would test it

## Testing
Provide two focused unit tests:
1. `needsReview` flag is applied correctly using config threshold
2. Pagination returns stable results across repeated calls

## Deliverables
- Minimal .NET 8 solution that serves `GET /orders`
- `sample-orders.json` loaded at startup
- Unit tests as above
- **README** that covers:
  - How to run locally (3 steps or fewer)
  - Indexing/query approach you would use in production
  - Monitoring/alerting you would set up
  - Release flow notes (dev to staging to prod, config per env, no cherry-picks)
  - Trade-offs made due to time limit and what you would do next

## What To Ignore
- Authentication/authorisation
- Full repository or DAL patterns beyond basics
- Complex DI or layering
- Database setup

## Evaluation Rubric (0-4 each, total 20)
- **Correctness** - spec met and edge cases handled
- **Code quality** - clear structure, naming, cohesion
- **Tests** - targeted, easy to run
- **Senior judgement** - defaults, trade-offs, ops thinking
- **Developer experience** - README clarity, simple startup, useful logs

## Getting Started
- Use Minimal APIs or a simple controller
- Use `System.Text.Json`
- Generate a correlation ID per request (or accept `x-correlation-id`) and log it

### `sample-orders.json`

Include the 1,000-order dataset here (already generated).

### `.gitignore`

```gitignore
# .NET
bin/
obj/
*.user
*.suo
*.swp

# IDEs
.vscode/
.idea/

# OS
.DS_Store
Thumbs.db
