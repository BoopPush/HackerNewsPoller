# HackerNewsPoller
Small ASP.NET Core API that implements the Santander - Developer Coding Test.

---
## How to run

### IDE (Visual Studio/ Rider)

1. Open the solution.
2. Select the **HackerNewsPoller** run profile.
3. Run the project (F5).

### Using command line
From the repository root:

Run:

```bash
run.cmd
```
after running, keep the terminal open and navigate to: 'http://localhost:5151/swagger'
## Service Design Overview

This service retrieves the best **N** Hacker News stories, taking into account that Hacker News data changes dynamically and unpredictably. Both the list of IDs and individual stories can change at any time, so the service uses a short-lived **dynamic caching** strategy:

- best story IDs cached separately,
- individual stories cached independently,
- cache durations kept intentionally small to avoid stale data.

This balances data freshness and reduced load on the Hacker News API.

### Input Constraints
The endpoint:
GET /api/stories/best?n=... where 200 >= n > 0
because the official endpoint [beststories.json](https://hacker-news.firebaseio.com/v0/beststories.json) never returns more than ~200 IDs.

### No External NuGet Packages/Unit Tests (By Choice)

To keep the solution minimal for the coding test, no external libraries were used.  
With more time, the following would improve the project:

- **AutoMapper** – mapping models/DTOs  
- **Polly** – retries, bulkhead, timeouts  
- **xUnit + NSubstitute** – unit tests and mocks  

Tests are not included due to time constraints, but in production they would be mandatory.

---

### Future Improvements

- add XML documentation,
- move configuration outside the project root and support dynamic reload,
- add more null checks,
- fine-tune cache durations.

---
