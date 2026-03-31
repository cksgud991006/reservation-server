## Load Testing

Tool: JMeter  
Scenario: Concurrent Ticket issuance  
Users: 1 → 1,000  
Duration: 5 minutes  
Ramp-Up period: 30 seconds

### Results
- Sustained throughput: ~2,318 req/s
- Average latency: ~409 ms
- Median latency: ~423 ms
- p90 latency: ~448 ms
- p95 latency: ~480 ms
- p99 latency: ~530 ms
- Error rate: 0.00%
- No overselling observed
- 0 data integrity violations

### Bottlenecks Identified
- Row-level database lock contention on Ticket inventory updates
- Throughput plateau observed under increased concurrency
- CPU utilization remained low (~5–10%), indicating a lock-bound rather than compute-bound workload

### Conclusion
Atomic SQL updates prevented race conditions under load.