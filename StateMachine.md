User: explain async state machine in .net
cover all important aspects, performance impact, known issues, key methods of related classes, best practices and tunung strategies

GitHub Copilot: # Async State Machines in .NET

## What Is an Async State Machine?

When you write an `async` method, the C# (or VB.NET/F#) compiler **transforms it into a state machine** — a struct (or class in debug mode) that implements `IAsyncStateMachine`. The original method body is split at each `await` point into numbered states. The runtime resumes execution at the correct state when an awaited task completes.

### Compiler Transformation Example

```csharp
// What you write:
public async Task<int> GetDataAsync()
{
    var a = await FetchAAsync();
    var b = await FetchBAsync(a);
    return a + b;
}
```

```csharp
// What the compiler generates (simplified):
[CompilerGenerated]
private struct <GetDataAsync>d__0 : IAsyncStateMachine
{
    public int <>1__state;          // current state: -1=start, -2=done, 0,1=await points
    public AsyncTaskMethodBuilder<int> <>t__builder;
    private int <a>5__1;
    private int <b>5__2;
    private TaskAwaiter<int> <>u__1;

    void IAsyncStateMachine.MoveNext()
    {
        int result;
        try
        {
            switch (<>1__state)
            {
                case -1: goto state_start;
                case 0:  goto state_resume_a;
                case 1:  goto state_resume_b;
            }

            state_start:
            var awaiter1 = FetchAAsync().GetAwaiter();
            if (!awaiter1.IsCompleted) { <>1__state = 0; <>t__builder.AwaitUnsafeOnCompleted(ref awaiter1, ref this); return; }

            state_resume_a:
            <a>5__1 = awaiter1.GetResult();
            var awaiter2 = FetchBAsync(<a>5__1).GetAwaiter();
            if (!awaiter2.IsCompleted) { <>1__state = 1; <>t__builder.AwaitUnsafeOnCompleted(ref awaiter2, ref this); return; }

            state_resume_b:
            <b>5__2 = awaiter2.GetResult();
            result = <a>5__1 + <b>5__2;
        }
        catch (Exception ex) { <>1__state = -2; <>t__builder.SetException(ex); return; }

        <>1__state = -2;
        <>t__builder.SetResult(result);
    }
}
```

---

## Key Classes and Interfaces

### `IAsyncStateMachine`
The interface every generated state machine implements.
```csharp
public interface IAsyncStateMachine
{
    void MoveNext();                // called to advance to next state
    void SetStateMachine(IAsyncStateMachine stateMachine); // boxing notification
}
```

### `AsyncTaskMethodBuilder<T>` / `AsyncTaskMethodBuilder` / `AsyncValueTaskMethodBuilder<T>`
The **builder** mediates between the state machine and the returned `Task`/`ValueTask`.

| Method | Purpose |
|---|---|
| `Create()` | Allocates the builder |
| `Start<TStateMachine>()` | Runs `MoveNext()` once synchronously |
| `AwaitOnCompleted()` / `AwaitUnsafeOnCompleted()` | Schedules resumption via `TaskScheduler` or without `ExecutionContext` flow |
| `SetResult(T)` | Completes the backing `Task` successfully |
| `SetException(Exception)` | Faults the backing `Task` |
| `Task` property | Returns the `Task<T>` exposed to callers |

`AwaitUnsafeOnCompleted` skips `ExecutionContext` capture — used internally when the awaiter doesn't need it (e.g., `ConfigureAwait(false)`).

### `TaskAwaiter<T>` / `ConfiguredTaskAwaitable<T>.Awaiter`
Produced by `task.GetAwaiter()`. Key members:
- `IsCompleted` — checked before deciding whether to suspend
- `GetResult()` — retrieves the result or re-throws exceptions
- `OnCompleted(Action)` — schedules the continuation (captures `SynchronizationContext`)
- `UnsafeOnCompleted(Action)` — same but no `ExecutionContext` flow

### `ExecutionContext`
Captured at each `await` suspension point and restored on resumption. Contains:
- `AsyncLocal<T>` values
- Security context

---

## Lifecycle: Step by Step

```
caller calls async method
        │
        ▼
Builder.Start() → MoveNext() [state = -1]
        │
   IsCompleted?──Yes──► skip suspension, continue inline
        │No
        ▼
   state machine boxed to heap (if struct)
   ExecutionContext captured
   awaiter.UnsafeOnCompleted(continuation)
   MoveNext returns → "Task" returned to caller
        │
   ... I/O completes on thread pool ...
        │
        ▼
   continuation queued to ThreadPool / SynchronizationContext
        │
        ▼
   MoveNext() [state = N] resumes
        │
   more awaits? ──Yes──► repeat
        │No
        ▼
   Builder.SetResult() → Task transitions to RanToCompletion
```

---

## Performance Impact

### 1. Allocation Cost
- In **release mode**, the state machine is a `struct` — it lives on the stack until the first actual suspension.
- On suspension, the struct is **boxed to the heap** (allocated as an `IAsyncStateMachine` object).
- `AsyncTaskMethodBuilder` also allocates a `Task<T>` object per call.
- **Hot path impact**: Even with zero allocations on the fast path (synchronous completion), there is still potential for JIT inlining failures due to the generated complexity.

### 2. `ValueTask<T>` to Reduce Allocations
```csharp
// Avoids Task<T> allocation when result is synchronously available
public ValueTask<int> GetCachedDataAsync()
{
    if (_cache.TryGet(out var v)) return new ValueTask<int>(v); // zero alloc
    return new ValueTask<int>(SlowPathAsync());
}
```
**Warning**: `ValueTask` must not be awaited more than once; do not `.Result` or `await` it twice.

### 3. `IValueTaskSource<T>` (Advanced)
Allows pooled/reusable completion sources — used internally by `Socket`, `Pipe`, and `Channel`. Eliminates per-operation `Task` allocations entirely.

### 4. Synchronous Completion Fast Path
If `awaiter.IsCompleted == true`, the state machine **never suspends** — no heap allocation occurs. Libraries like `MemoryCache` and `Channel<T>` are optimized to complete synchronously when data is ready.

### 5. `ExecutionContext` Overhead
Each suspension captures `ExecutionContext`. If you use many `AsyncLocal<T>` values, each capture/restore is more expensive. Use sparingly in hot paths.

---

## `SynchronizationContext` and `ConfigureAwait`

By default, `await` captures the current `SynchronizationContext` (e.g., UI thread in WPF/WinForms, ASP.NET Classic request context) and **posts the continuation back to it**.

```csharp
// Library code — always use this to avoid deadlocks and improve perf
var result = await SomeIOAsync().ConfigureAwait(false);
```

`ConfigureAwait(false)`:
- Does **not** capture `SynchronizationContext`
- Continuation runs on the thread pool thread that completed the I/O
- **Prevents deadlocks** in environments with single-threaded sync contexts (WPF, WinForms, ASP.NET Classic)
- Reduces context switching overhead

**Classic deadlock scenario** (ASP.NET Classic / WPF):
```csharp
// DEADLOCK: sync context thread is blocked waiting, but continuation
// needs that same thread to resume
var result = GetDataAsync().Result; // blocks thread
// GetDataAsync awaits something → tries to post back to the blocked thread → deadlock
```

**In ASP.NET Core**: there is no `SynchronizationContext` by default, so `ConfigureAwait(false)` has no deadlock-prevention effect — but still slightly reduces overhead.

---

## Known Issues and Gotchas

### 1. Exception Handling
Exceptions thrown before the first `await` are **captured into the returned Task**, not thrown synchronously — even if no async operation occurs.
```csharp
public async Task DoWorkAsync()
{
    throw new InvalidOperationException(); // goes into Task, not thrown here
    await Task.Delay(1);
}
// Caller must await to observe the exception
```

### 2. `async void` — Avoid Unless Required
```csharp
async void OnButtonClick(object sender, EventArgs e) // only acceptable pattern
async void FireAndForget() // DANGEROUS: exceptions crash the process
```
Exceptions in `async void` go to `SynchronizationContext.UnhandledException` or crash the app. Use `async Task` instead and `_ = FireAndForget()` to discard intentionally.

### 3. Partial Trust / `AwaitUnsafeOnCompleted`
In legacy security sandboxes, `UnsafeOnCompleted` skips `ExecutionContext` flow, which can cause security context leaking. Not relevant in modern .NET but a historical concern.

### 4. `CancellationToken` Propagation
Failing to pass `CancellationToken` through all async calls means cancellation won't propagate:
```csharp
// Wrong — outer cancellation has no effect on DB query
public async Task<Data> GetAsync(CancellationToken ct)
{
    return await _db.QueryAsync("SELECT ...");
}

// Correct
public async Task<Data> GetAsync(CancellationToken ct)
{
    return await _db.QueryAsync("SELECT ...", ct);
}
```

### 5. `await` in `lock` / `Monitor` — Compile Error by Design
```csharp
lock (_obj)
{
    await SomethingAsync(); // compile error — correct, locks can't span threads
}
// Use SemaphoreSlim.WaitAsync() instead
```

### 6. Struct State Machine Boxing
Even though the compiler uses a struct, the first genuine suspension boxes it. Overly long async methods with many locals can produce large structs → large heap objects. Prefer breaking large methods into smaller ones.

### 7. Stack Dive / Synchronous Recursion
If every awaited operation completes synchronously, the state machine keeps calling `MoveNext` inline, potentially growing the stack. This is rare but worth noting in deeply recursive async code.

---

## Best Practices

| Practice | Reason |
|---|---|
| `ConfigureAwait(false)` in libraries | Avoids deadlocks, reduces context switching |
| Prefer `ValueTask<T>` for hot/cached paths | Eliminates `Task` allocation on sync path |
| Pass `CancellationToken` everywhere | Enables cooperative cancellation |
| Avoid `async void` | Unobservable exceptions |
| Don't block on async (`.Result`, `.Wait()`) | Causes deadlocks with sync contexts |
| Keep async methods focused | Reduces state machine struct size |
| Use `Task.WhenAll` for parallelism | Avoids sequential awaiting of independent work |
| Prefer `SemaphoreSlim` over `lock` | `lock` can't span `await` boundaries |
| Use `CancellationTokenSource.CancelAfter()` | Enforce timeouts without manual timers |

---

## Tuning Strategies

### 1. Measure First
Use **dotnet-trace**, **PerfView**, or **BenchmarkDotNet** with memory diagnoser to identify async allocation hotspots before optimizing.

### 2. Pool `IValueTaskSource` for Ultra-High-Throughput
```csharp
// Pattern used by System.IO.Pipelines internally
class PooledValueTaskSource : IValueTaskSource<int>
{
    // reset and reuse instead of allocating per operation
}
```

### 3. Avoid Async for Trivial Synchronous Methods
```csharp
// Bad: needless state machine + Task allocation
public async Task<int> GetConstantAsync() => await Task.FromResult(42);

// Good: no overhead
public Task<int> GetConstantAsync() => Task.FromResult(42);
// or
public ValueTask<int> GetConstantAsync() => new ValueTask<int>(42);
```

### 4. `Task.CompletedTask` / `Task.FromResult` Cached Instances
```csharp
// Task.FromResult(true/false) is cached internally in .NET 6+
// Task.FromResult(0) and Task.FromResult<T>(default) may not be — cache manually if needed
private static readonly Task<bool> _cachedTrue = Task.FromResult(true);
```

### 5. Thread Pool Tuning
Async I/O completes on IOCP threads (Windows) or thread pool threads (Linux). Ensure `ThreadPool.SetMinThreads()` is set appropriately for burst workloads to avoid thread injection delay.

### 6. `TaskCreationOptions.RunContinuationsAsynchronously`
When using `TaskCompletionSource`, this prevents inline continuation execution on the completing thread, avoiding unexpected stack growth or lock contention:
```csharp
var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
```

### 7. Avoid `Task.Run` Wrapping in ASP.NET Core
```csharp
// Unnecessary — wastes a thread pool thread
await Task.Run(() => SomeAlreadyAsyncMethod());
```
`Task.Run` is for CPU-bound work, not I/O-bound work.

---

## Summary Diagram

```
async method call
     │
     ▼
[State = -1] MoveNext() ──► sync path? ──Yes──► SetResult (no alloc)
     │No
     ▼
[Box struct to heap, capture ExecutionContext]
[Register continuation with awaiter]
     │
     ▼
... suspend, return Task to caller ...
     │
[I/O / timer completes]
     │
     ▼
[Restore ExecutionContext]
[State = N] MoveNext() ──► more awaits? ──Yes──► loop
     │No
     ▼
SetResult / SetException ──► Task transitions state ──► caller's continuation runs
```

The async state machine is effectively zero-overhead on the synchronous fast path, and its cost on the async path is dominated by the heap allocation of the boxed state machine and the `Task<T>` object — which `ValueTask<T>` and `IValueTaskSource<T>` can eliminate.
