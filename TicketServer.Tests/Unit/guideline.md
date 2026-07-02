Test Coverages
1) Happy path — at least 1
The basic success scenario.
2) One test per branch (if/else) — at least 1 for each side
If the code has 3 conditionals, you need at least 3 tests (covering both true/false for each condition).
3) Boundary values — always add if present
If the code uses >= or <= (anything comparing "equal to"), test that exact boundary. This is where bugs hide most often.
4) Exception/error cases — always add if present
If there's a throw, test that the exception actually fires under the right condition.
5) Null/empty cases — add if relevant
Empty list, null input, etc.