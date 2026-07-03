## 1. Unit Test

The smallest unit — testing a single function or class's logic without external dependencies (DB, network).

**Example:** In a reservation system, if there's a function that "checks whether a requested date overlaps with an existing reservation," you test that function in isolation, without a DB connection.

**Libraries:** xUnit, NUnit, MSTest

### Test Coverage

1. **Happy path — at least 1**
   The basic success scenario.

2. **One test per branch (if/else) — at least 1 for each side**
   If the code has 3 conditionals, you need at least 3 tests (covering both true/false for each condition).

3. **Boundary values — always add if present**
   If the code uses `>=` or `<=` (anything comparing "equal to"), test that exact boundary. This is where bugs hide most often.

4. **Exception/error cases — always add if present**
   If there's a `throw`, test that the exception actually fires under the right condition.

5. **Null/empty cases — add if relevant**
   Empty list, null input, etc.

The key idea is replacing dependencies with Mocks/Stubs. For example, you swap out the DB call with a fake function, so you can verify the logic alone without a real DB.

---

## 2. Integration Test

Verifies that multiple components actually work together correctly when connected. Usually tested against a real DB (or a dedicated test DB).

**Example:** Sending a request to an API endpoint and confirming that a reservation row is actually created in the DB.

**Libraries:** WebApplicationFactory + Testcontainers

### Test Coverage
1. **Happy path — at least 1**
   The basic success scenario.

2. **Business-logic-specific failure cases**
   Ex: Concurrency issues, Overlapping attempts
