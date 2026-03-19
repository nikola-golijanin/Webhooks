# Rewrite the Webhooks System in Go — Learner's TODO Checklist

You will write every line of code yourself. Each section is a numbered checklist.
Tick off tasks as you go. No code is provided — only instructions, explanations, and hints.

---

## Section 0 — What Are You Building?

Read this before writing a single line of code.

**What this system does:**

- Allows clients to **subscribe** to events by registering a webhook URL + event type
  (e.g. "call me at `https://myapp.com/hook` when an `order.created` event happens")
- Allows producers to **publish** events by POSTing an event type + payload to the API
- When an event is published, the system finds all matching subscriptions and sends an
  HTTP POST to each subscriber's URL with the event payload
- Every delivery attempt (success or failure) is recorded in the database

**Functional requirements — what you will build:**

1. `POST /api/webhooks/subscriptions` — register a webhook URL for an event type
2. `POST /api/webhooks/publish` — publish an event; fans out to all matching subscribers
3. `POST /api/orders` — creates an order and automatically publishes an `order.created` event
4. `GET /api/orders` — lists all orders (in-memory; resets on restart)
5. Webhook delivery happens **asynchronously** in the background — the publish endpoint
   returns immediately without waiting for HTTP delivery to complete
6. Failed deliveries are recorded (status code = null, success = false)

**Database tables:**

- `webhooks.subscriptions` — stores `(webhook_url, event_type)`
- `webhooks.delivery_attempts` — stores each delivery outcome (status code, success flag)

**What is explicitly out of scope (simplifications vs. the .NET version):**

- No RabbitMQ — a Go buffered channel replaces the message queue (in-process)
- No separate processing service — one binary handles both HTTP and delivery
- No retry logic — failed deliveries are recorded but not retried
- No auth — no API keys or signatures on outgoing webhooks

---

## Section 0b — Go vs C# Concept Map

Read this table before you start. Refer back to it whenever something feels unfamiliar.

| C# concept | Go equivalent | Notes |
|---|---|---|
| `class Foo { }` | `type Foo struct { }` | Go has no classes, only structs |
| `public` / `private` | Capital letter / lowercase | `Foo` = exported, `foo` = unexported |
| `interface IFoo { }` | `type Foo interface { }` | Implemented **implicitly** — no `implements` keyword |
| `throw new Exception()` | `return fmt.Errorf("...")` | Errors are values, not exceptions |
| `try/catch` | `if err != nil { }` | Every fallible function returns `(result, error)` |
| `async/await` | goroutines + channels | Concurrency is built into the language |
| `Task.Run(...)` | `go func()` | Launches a goroutine — much lighter than a thread |
| `Queue<T>` (bounded) | `make(chan T, N)` | Buffered channel with capacity N |
| `namespace Foo` | `package foo` | One package per directory |
| `using Foo;` | `import "path/to/foo"` | Explicit, no wildcards |
| `IServiceCollection` DI | manual wiring in `main.go` | No DI container — pass dependencies explicitly |
| `appsettings.json` | `os.Getenv(...)` | Read config from environment variables |
| `null` | `nil` | Same idea, but Go pointers make it explicit |
| `int?` (nullable int) | `*int` (pointer to int) | A pointer can be nil = "no value" |
| `List<T>` | `[]T` (slice) | Dynamic array; grow with `append(slice, item)` |
| `lock (obj) { }` | `mu.Lock()` / `mu.Unlock()` | `sync.Mutex` for protecting shared state |
| `[JsonPropertyName("x")]` | `` `json:"x"` `` struct tag | Controls JSON serialisation field name |
| `IHostApplicationLifetime` | `context.Context` + `signal.NotifyContext` | Cancel work on SIGTERM/SIGINT |

---

## Section 1 — Prerequisites

- [ ] Install Go 1.23 or later from `go.dev/dl`
- [ ] Verify installation: run `go version` in a terminal
- [ ] Install `psql` or a DB GUI (TablePlus or DBeaver work well)
- [ ] Install the **Go** extension (`golang.go`) in VS Code — it provides formatting,
      auto-imports, and inline error highlighting

---

## Section 2 — Project Bootstrap

- [ ] Create the project directory and enter it:
  ```
  mkdir webhooks-go && cd webhooks-go
  ```
- [ ] Initialise a Go module:
  ```
  go mod init webhooks-go
  ```
  > **What is `go.mod`?**
  > It is the Go equivalent of a `.csproj` file. It declares the module name and its
  > dependencies. You never edit dependency versions by hand — the `go get` and `go mod tidy`
  > commands manage it. `go.sum` is the lock file (like `packages.lock.json`) — never edit it.

- [ ] Create the folder structure:
  ```
  mkdir -p cmd/api internal/domain internal/database internal/server migrations
  ```
  The full tree you are building:
  ```
  webhooks-go/
  ├── cmd/
  │   └── api/
  │       └── main.go
  ├── internal/
  │   ├── domain/
  │   │   └── domain.go
  │   ├── database/
  │   │   ├── database.go
  │   │   └── database_test.go
  │   └── server/
  │       ├── server.go
  │       ├── routes.go
  │       └── routes_test.go
  ├── migrations/
  │   ├── 00001_create_subscriptions.sql
  │   └── 00002_create_delivery_attempts.sql
  ├── Makefile
  ├── docker-compose.yml
  ├── go.mod
  └── go.sum
  ```

  **Dependency direction — the most important Go rule for avoiding circular imports:**
  ```
  domain     ← no internal imports
  database   ← imports domain
  server     ← imports domain + database
  main       ← imports database + server
  ```
  Nothing ever imports `main`, and `domain` never imports anything internal.

- [ ] Install dependencies one by one. After each command, notice that `go.mod` and `go.sum`
      are updated automatically.

  ```
  go get github.com/gin-gonic/gin
  ```
  > **Gin** is the HTTP router. It handles JSON binding, URL params, and middleware.
  > Think of it as ASP.NET Core Minimal APIs — lightweight, no ceremony.

  ```
  go get github.com/jackc/pgx/v5
  ```
  > **pgx** is the PostgreSQL driver. It is the Go equivalent of Npgsql.
  > You will use `pgxpool` for a connection pool and `pgx/stdlib` for the standard
  > `database/sql` interface when goose needs it.

  ```
  go get github.com/pressly/goose/v3
  ```
  > **goose** manages database migrations. It is the Go equivalent of `dotnet ef migrations`.
  > Migration files are plain SQL with special comment directives.

---

## Section 3 — docker-compose.yml

- [ ] Create `docker-compose.yml` at the project root with a single Postgres service.
      Map port 5432, set environment variables for `POSTGRES_USER`, `POSTGRES_PASSWORD`,
      and `POSTGRES_DB` (use `webhooks` as the DB name).
  > **Why no RabbitMQ?**
  > The .NET version uses RabbitMQ as a message broker between two separate services.
  > In this Go version, a **buffered channel** (`chan DeliveryJob`) replaces the broker.
  > Channels are built into the language — no broker process, no network hop, no extra
  > dependency. This is idiomatic Go for in-process async work.

- [ ] Start Postgres:
  ```
  docker compose up -d
  ```
- [ ] Verify it is running:
  ```
  docker compose ps
  ```

---

## Section 4 — Makefile

- [ ] Create `Makefile` at the project root with these targets:

  | Target | Command it runs |
  |---|---|
  | `run` | `go run ./cmd/api` |
  | `build` | `go build -o bin/api ./cmd/api` |
  | `test` | `go test ./...` |
  | `migrate-up` | `goose -dir migrations postgres "$(DB_URL)" up` |
  | `migrate-down` | `goose -dir migrations postgres "$(DB_URL)" down` |

  > **Makefile tab rule:** Each recipe line *must* be indented with a real tab character,
  > not spaces. VS Code will warn you if you use spaces.

  > **`$(DB_URL)`:** This reads the shell environment variable `DB_URL` at the time
  > `make` is invoked. Set it with `export DB_URL="..."` before running any `make` target.
  > It is equivalent to `%DB_URL%` in a Windows batch file or `$env:DB_URL` in PowerShell.

---

## Section 5 — Database Migrations

- [ ] Create `migrations/00001_create_subscriptions.sql`.

  Every goose migration file must start with these two comment directives:
  ```sql
  -- +goose Up
  -- SQL to apply the migration goes here

  -- +goose Down
  -- SQL to roll back the migration goes here
  ```
  In the **Up** section, create the `webhooks` schema (if it does not exist) and then
  create the `webhooks.subscriptions` table with columns:
  - `id` — primary key, auto-incrementing integer
  - `webhook_url` — text, not null
  - `event_type` — text, not null
  - `created_at` — timestamp with time zone, default now()

  > **`GENERATED BY DEFAULT AS IDENTITY`** is the modern PostgreSQL way to create an
  > auto-incrementing primary key. It replaces the older `SERIAL` type and is equivalent
  > to EF Core's convention of annotating a property with `[Key]` + `ValueGeneratedOnAdd`.

  In the **Down** section, drop the table.

- [ ] Create `migrations/00002_create_delivery_attempts.sql`.

  In the **Up** section, create `webhooks.delivery_attempts` with columns:
  - `id` — primary key, auto-incrementing integer
  - `webhook_subscription_id` — integer, foreign key referencing `webhooks.subscriptions(id)`
  - `payload` — text, not null
  - `response_status_code` — integer, **nullable** (no NOT NULL constraint)
  - `success` — boolean, not null
  - `created_at` — timestamp with time zone, default now()

  > **Nullable column → pointer in Go:** Because `response_status_code` can be NULL in the
  > database, you will store it as `*int` in Go. A pointer can be `nil` — that is Go's way
  > of expressing "no value". Contrast with `int`, which always has a value (0 if not set).

  In the **Down** section, drop the table.

- [ ] Apply migrations:
  ```
  export DB_URL="postgres://postgres:postgres@localhost:5432/webhooks?sslmode=disable"
  make migrate-up
  ```
- [ ] Verify in psql or your GUI:
  ```sql
  \dt webhooks.*
  ```
  You should see both tables listed.

---

## Section 6 — internal/domain/domain.go

Write all your types first, before any logic. This prevents circular imports and gives
you a clear mental model of the data flowing through the system.

- [ ] Declare `package domain` at the top of the file.

- [ ] Write the `WebhookSubscription` struct with these fields:
  - `ID` int64
  - `WebhookURL` string
  - `EventType` string
  - `CreatedAt` time.Time

  Add JSON struct tags to each field.
  > **JSON tags:** `` `json:"id"` `` tells the Go JSON encoder to use `"id"` as the key
  > instead of `"ID"`. This is equivalent to `[JsonPropertyName("id")]` in C#.
  > Convention in Go APIs: use `snake_case` for JSON keys even though Go field names are
  > `PascalCase`.

- [ ] Write the `DeliveryAttempt` struct:
  - `ID` int64
  - `WebhookSubscriptionID` int64
  - `Payload` string
  - `ResponseStatusCode` *int  ← pointer, nullable
  - `Success` bool
  - `CreatedAt` time.Time

  > **`*int` for nullable:** When `ResponseStatusCode` is nil it means delivery failed before
  > an HTTP response was received. When it points to a value (e.g. `200`) it means a response
  > was received. Dereference with `*sub.ResponseStatusCode` to read the value.

- [ ] Write the `Order` struct (in-memory only, never persisted):
  - `ID` int64 (or string — your choice)
  - `CreatedAt` time.Time
  - Any other fields you want (e.g. a description)

- [ ] Write the `DeliveryJob` struct — this is the message passed through the Go channel
      from the HTTP handlers to the background workers:
  - `SubscriptionID` int64
  - `WebhookURL` string
  - `EventType` string
  - `Payload` any

  > **`any`** is an alias for `interface{}` added in Go 1.18. It means "any type" —
  > equivalent to `object` in C#. Use it here because the payload can be any JSON shape.

  > **No constructor needed:** Go zero-initialises all fields. Create a struct literal:
  > `domain.DeliveryJob{WebhookURL: "https://...", EventType: "order.created"}`.

---

## Section 7 — internal/database/database.go

Build the database layer one method at a time.

- [ ] Declare `package database` and write your imports.

  > **Blank import:** You will likely need:
  > ```go
  > import _ "github.com/jackc/pgx/v5/stdlib"
  > ```
  > The `_` means "import for side effects only" — this registers the pgx driver with
  > Go's `database/sql` package without you ever calling anything from it directly.
  > You only need this if you use `database/sql`; if you use `pgxpool` directly you can skip it.

- [ ] Define a `DB` struct that contains a `*pgxpool.Pool` field.
  > A pool manages multiple database connections. It is safe to use from multiple goroutines
  > concurrently — like registering `DbContext` as a singleton in ASP.NET Core.

- [ ] Write `New(ctx context.Context, connStr string) (*DB, error)`:
  - Call `pgxpool.New(ctx, connStr)` to create the pool
  - Call `pool.Ping(ctx)` to verify the connection is alive
  - Return a pointer to your `DB` struct wrapping the pool, or an error

  > **Multiple return values:** Go functions can return more than one value. The convention
  > for fallible functions is `(result, error)`. The caller checks `if err != nil` — there
  > are no exceptions to catch. `log.Fatal(err)` exits the program immediately if called
  > (use in `main.go` for unrecoverable startup errors).

- [ ] Write `CreateSubscription(ctx context.Context, webhookURL, eventType string) (*domain.WebhookSubscription, error)`:
  - Use `pool.QueryRow(ctx, sql, args...).Scan(&fields...)` with a `RETURNING` clause to
    insert the row and read back the generated `id` and `created_at` in one query
  - Return a pointer to the populated `domain.WebhookSubscription`

  > **`&sub.ID`:** The `&` operator takes the address of a variable (gives you a pointer).
  > `Scan` writes the query result into that memory location — similar to `out` parameters
  > in C#. You pass `&sub.ID, &sub.WebhookURL` etc. to tell Scan where to write each column.

- [ ] Write `GetSubscriptionsByEventType(ctx context.Context, eventType string) ([]domain.WebhookSubscription, error)`:
  - Use `pool.Query(ctx, sql, eventType)` which returns `(pgx.Rows, error)`
  - Add `defer rows.Close()` immediately after the error check

  > **`defer`:** A deferred call runs when the surrounding function returns — regardless of
  > whether it returns normally or with an error. It is the Go equivalent of a `finally`
  > block, or C#'s `using` statement for `IDisposable`. Use it to close rows, files, and
  > response bodies.

  - Loop: `for rows.Next() { rows.Scan(...) ; append to slice }`
  - After the loop: check `rows.Err()` — an error during iteration ends the loop early
    without returning from the loop body, so you must check it separately

- [ ] Write `RecordDelivery(ctx context.Context, subscriptionID int64, payload string, statusCode *int, success bool) error`:
  - Use `pool.Exec(ctx, sql, args...)` for an INSERT with no return value needed
  - Pass `statusCode` directly — pgx knows how to map a nil `*int` to SQL NULL

---

## Section 8 — internal/database/database_test.go

- [ ] Create `database_test.go` in the same package (`package database`).

  > **Go test conventions:**
  > - Test files end in `_test.go` — the compiler ignores them in normal builds
  > - Test functions are named `TestXxx(t *testing.T)` — the `go test` tool discovers them
  > - `t.Fatal(msg)` logs the message and stops the current test immediately (like an assert
  >   that aborts). `t.Error(msg)` logs and marks the test as failed but keeps running.

- [ ] Write `TestCreateSubscription(t *testing.T)`:
  - Read the test database URL from an environment variable: `os.Getenv("TEST_DB_URL")`
  - If it is empty, call `t.Skip("TEST_DB_URL not set")` to skip gracefully
  - Connect with `New(ctx, url)`, check err with `t.Fatal`
  - Call `db.CreateSubscription(...)`, check err
  - Assert the returned struct has a non-zero `ID` (delivery was inserted)

  > **Why a real database?** Mocks can pass even when the actual SQL is wrong.
  > Integration tests against a real DB catch schema mismatches, constraint violations,
  > and query bugs that mocks would silently allow.

- [ ] Write `TestGetSubscriptionsByEventType(t *testing.T)`:
  - Insert at least one subscription via `CreateSubscription`
  - Call `GetSubscriptionsByEventType` with the same event type
  - Assert the result slice is non-empty and contains the inserted row

- [ ] Run tests (set the env var first):
  ```
  export TEST_DB_URL="postgres://postgres:postgres@localhost:5432/webhooks?sslmode=disable"
  go test ./internal/database/... -v
  ```

---

## Section 9 — internal/server/server.go

- [ ] Declare `package server`.

- [ ] Define the `Server` struct with these fields:
  - `db *database.DB`
  - `router *gin.Engine`
  - `jobs chan domain.DeliveryJob`
  - `orders []domain.Order`
  - `mu sync.Mutex`
  - `port string`

  > **Buffered channel:** `make(chan domain.DeliveryJob, 100)` creates a channel with a
  > buffer of 100 jobs. Sending to it (`s.jobs <- job`) does not block as long as there is
  > space in the buffer — like a bounded `Queue<T>` in C#. If the buffer fills up, the send
  > blocks until a worker drains it.

- [ ] Write `New(db *database.DB, port string) *Server`:
  - Create the jobs channel with a reasonable buffer size
  - Create a Gin engine with `gin.Default()` — this includes the logger and recovery
    (panic → 500) middleware already wired in
  - Store everything in the struct
  - Call `s.registerRoutes()` (you will define this in `routes.go`)
  - Return the server pointer

- [ ] Write `Start(ctx context.Context) error`:
  - Call `s.startWorkers(ctx, 5)` to launch 5 background delivery workers
  - Call `s.router.Run(":" + s.port)` to start the HTTP server
  - Return any error from `Run`

- [ ] Write `startWorkers(ctx context.Context, n int)`:
  - Loop `n` times
  - In each iteration, launch a goroutine: `go s.runWorker(ctx)`

  > **`go` keyword:** Prefixing a function call with `go` launches it as a goroutine —
  > a lightweight concurrent function managed by the Go runtime. Goroutines are far cheaper
  > than OS threads. You can run thousands of them. This is the Go equivalent of `Task.Run`,
  > but without the overhead.

- [ ] Write `runWorker(ctx context.Context)`:
  - Loop forever with a `for { select { ... } }` construct
  - In the select, handle two cases:
    1. `case job := <-s.jobs:` — a delivery job arrived; call `s.deliver(job)`
    2. `case <-ctx.Done():` — the context was cancelled (SIGTERM received); `return`

  > **`select` statement:** Like a `switch`, but each case is a channel operation.
  > Go blocks until one case is ready, then executes it. If multiple cases are ready,
  > one is chosen at random. This is the idiomatic way to multiplex channel receives
  > with a cancellation signal.

  > **`ctx.Done()`:** Returns a channel that is closed when the context is cancelled.
  > Receiving from a closed channel returns the zero value immediately. This is how
  > `signal.NotifyContext` (in `main.go`) propagates Ctrl+C / SIGTERM to all workers.
  > It replaces `IHostApplicationLifetime.ApplicationStopping` from ASP.NET Core.

- [ ] Write `deliver(job domain.DeliveryJob)`:
  - Marshal `job.Payload` to JSON using `encoding/json`
  - Create an HTTP POST request to `job.WebhookURL` with the JSON body
  - Execute the request with an `http.Client` (or `http.DefaultClient` for now)
  - Add `defer resp.Body.Close()` immediately after checking the response error

  > **`defer resp.Body.Close()`:** You must always close an HTTP response body in Go,
  > or you will leak connections. Deferring it is the idiomatic way — even if you don't
  > read the body, close it.

  - If the request succeeded: extract the status code, set `success = (statusCode >= 200 && statusCode < 300)`
  - If the request failed (network error): leave `statusCode` as `nil`, set `success = false`
  - Call `s.db.RecordDelivery(context.Background(), job.SubscriptionID, string(payload), statusCode, success)`

---

## Section 10 — internal/server/routes.go

- [ ] Declare `package server`.

- [ ] Write `func (s *Server) registerRoutes()` — this is called from `New()`.
      Use `s.router.POST(path, handler)` and `s.router.GET(path, handler)` to register routes.

- [ ] `POST /api/webhooks/subscriptions` handler:
  - Define a local request struct with `WebhookURL` and `EventType` fields
  - Bind JSON from the request body: `c.ShouldBindJSON(&req)`
  - If binding fails, return `c.JSON(http.StatusBadRequest, ...)` and `return`
  - Call `s.db.CreateSubscription(c.Request.Context(), req.WebhookURL, req.EventType)`
  - Respond with `c.JSON(http.StatusCreated, sub)`

  > **`c.ShouldBindJSON`** is Gin's equivalent of `[FromBody]` in ASP.NET Core. It reads the
  > request body and unmarshals it into your struct using the `json` struct tags.

- [ ] `POST /api/webhooks/publish` handler:
  - Bind a request with `EventType` string and `Payload` any
  - Call `s.db.GetSubscriptionsByEventType(c.Request.Context(), req.EventType)`
  - For each subscription in the result, enqueue a delivery job:
    ```go
    s.jobs <- domain.DeliveryJob{
        SubscriptionID: sub.ID,
        WebhookURL:     sub.WebhookURL,
        EventType:      sub.EventType,
        Payload:        req.Payload,
    }
    ```
  - Respond with `c.Status(http.StatusAccepted)` — 202 signals async processing

  > **Non-blocking send:** Sending to a buffered channel does not block as long as there is
  > space. The HTTP handler returns immediately; a background worker picks up the job later.
  > This is the core async pattern replacing RabbitMQ.

- [ ] `POST /api/orders` handler:
  - Bind a request body
  - Lock the mutex: `s.mu.Lock()` / `defer s.mu.Unlock()`

  > **`sync.Mutex`:** Multiple goroutines (HTTP handlers run concurrently in Gin) can
  > reach this handler simultaneously. A mutex ensures only one goroutine modifies the
  > in-memory slice at a time — exactly like `lock (obj) { }` in C#.

  - Create a `domain.Order`, append to `s.orders`
  - Enqueue an `order.created` delivery job via `s.jobs`
  - Respond with the new order

- [ ] `GET /api/orders` handler:
  - Lock the mutex (even for reads, to avoid data races)
  - Respond with `c.JSON(http.StatusOK, s.orders)`

---

## Section 11 — internal/server/routes_test.go

- [ ] Declare `package server`.

  > **`net/http/httptest`:** Go's standard library includes a test HTTP package. You can
  > make requests to your Gin router without starting a real server — it is faster and
  > requires no port. This is equivalent to using `WebApplicationFactory<T>` in ASP.NET Core
  > integration tests.

- [ ] Write `TestCreateSubscriptionHandler(t *testing.T)`:
  - Create a `Server` instance. For unit tests, use a test database (or a mock if you
    prefer — but a real DB is more reliable; see Section 8's note on mocks)
  - Build a JSON request body with `webhookURL` and `eventType`
  - Create a recorder and a request:
    ```go
    w := httptest.NewRecorder()
    req := httptest.NewRequest(http.MethodPost, "/api/webhooks/subscriptions", body)
    req.Header.Set("Content-Type", "application/json")
    ```
  - Pass them to the router: `s.router.ServeHTTP(w, req)`
  - Assert `w.Code == http.StatusCreated`
  - Optionally decode `w.Body` and assert the returned JSON has a non-zero `id`

- [ ] Run the tests:
  ```
  go test ./internal/server/... -v
  ```

---

## Section 12 — cmd/api/main.go

- [ ] Declare `package main`.

  > **`package main`** is special — only a package named `main` can produce an executable.
  > Every other package in this project uses a different name (`domain`, `database`, `server`).

- [ ] Write `func main()`.

  > Go programs start at `main()` in `package main`. There is no `static void Main(string[] args)`,
  > no `Host.CreateDefaultBuilder`, no `Startup` class. You wire everything together yourself.

- [ ] Read configuration from environment variables using `os.Getenv`:
  ```go
  connStr := os.Getenv("DB_URL")
  port    := os.Getenv("PORT")
  ```
  - If `connStr` is empty, print a helpful error and exit (`log.Fatal(...)`)
  - Provide a default for `port` if it is empty (e.g. `"8080"`)

  > **Why no config library?** Environment variables are the 12-factor app standard.
  > `os.Getenv` is built in and needs no imports beyond `"os"`. For a small project,
  > keep it simple.

- [ ] Set up graceful shutdown with signal handling:
  ```go
  ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGTERM, syscall.SIGINT)
  defer stop()
  ```
  > This creates a context that is automatically cancelled when your process receives
  > Ctrl+C (`SIGINT`) or a termination signal from Docker/Kubernetes (`SIGTERM`).
  > The cancelled context propagates to your workers via `ctx.Done()`.
  > This replaces `IHostApplicationLifetime.ApplicationStopping` from ASP.NET Core.

- [ ] Connect to the database:
  ```go
  db, err := database.New(ctx, connStr)
  if err != nil {
      log.Fatal(err)
  }
  ```
- [ ] Create and start the server:
  ```go
  srv := server.New(db, port)
  if err := srv.Start(ctx); err != nil {
      log.Fatal(err)
  }
  ```

---

## Section 13 — Running Everything

- [ ] Start Postgres:
  ```
  docker compose up -d
  ```
- [ ] Export the database URL:
  ```
  export DB_URL="postgres://postgres:postgres@localhost:5432/webhooks?sslmode=disable"
  ```
- [ ] Apply migrations:
  ```
  make migrate-up
  ```
- [ ] Run the API:
  ```
  make run
  ```
- [ ] Test the endpoints with curl:

  Create a subscription:
  ```
  curl -X POST http://localhost:8080/api/webhooks/subscriptions \
       -H "Content-Type: application/json" \
       -d '{"webhook_url":"https://webhook.site/your-id","event_type":"order.created"}'
  ```

  Publish an event:
  ```
  curl -X POST http://localhost:8080/api/webhooks/publish \
       -H "Content-Type: application/json" \
       -d '{"event_type":"order.created","payload":{"order_id":42}}'
  ```

  Create an order (triggers `order.created` automatically):
  ```
  curl -X POST http://localhost:8080/api/orders \
       -H "Content-Type: application/json" \
       -d '{"description":"first order"}'
  ```

  List orders:
  ```
  curl http://localhost:8080/api/orders
  ```

- [ ] Check the database:
  ```sql
  SELECT * FROM webhooks.subscriptions;
  SELECT * FROM webhooks.delivery_attempts;
  ```

- [ ] Run all tests:
  ```
  make test
  ```

---

## Section 14 — What You Learned

Each concept you practised, and where it first appeared:

| Concept | File |
|---|---|
| Multiple return values + error handling | `database/database.go` |
| `defer` for cleanup (rows, response body) | `database/database.go`, `server/server.go` |
| Goroutines (`go func()`) | `server/server.go` — `startWorkers` |
| Buffered channels | `server/server.go` — `jobs chan` |
| Sending to a channel | `server/routes.go` — publish handler |
| `select` for channel multiplexing | `server/server.go` — `runWorker` |
| `sync.Mutex` for shared state | `server/routes.go` — order store |
| `context.Context` for cancellation | `main.go`, `server/server.go` |
| JSON binding and response | `server/routes.go` |
| Pointer as nullable (`*int`) | `domain/domain.go`, `database/database.go` |
| Struct tags for JSON | `domain/domain.go` |
| Real-DB integration tests | `database/database_test.go` |
| `httptest` for handler tests | `server/routes_test.go` |
