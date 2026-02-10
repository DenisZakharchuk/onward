# Semantic Search - Text to SQL Feature

**High-Level Description:** This feature enables external API consumers to query the ERP inventory database using natural language instead of writing SQL. Users ask questions like "show products in electronics" or "categories created last month," and the system uses OpenAI GPT-4 to automatically generate and execute safe PostgreSQL SELECT queries, returning structured JSON results. The implementation includes schema introspection, SQL validation for security, caching for cost optimization, and can be built either manually with full control or using LangChain for faster development with pre-built text-to-SQL components.

## Overview

Natural language query interface for the ERP inventorization module. External API consumers can ask questions in plain English (one sentence, up to 10 words) and receive structured data results. The system converts natural language to SQL SELECT queries using OpenAI GPT-4.

## Tech Stack

### Core Technologies
- **Backend**: ASP.NET MVC Web API (existing)
- **Database**: PostgreSQL with Entity Framework Core
- **LLM Provider**: OpenAI GPT-4
- **Language**: C#

### NuGet Packages
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider for EF Core (already installed)
- `Azure.AI.OpenAI` or `OpenAI` (Betalgo.OpenAI) - OpenAI API client
- `Microsoft.Extensions.Caching.Memory` - Cache common queries and schema metadata
- `Polly` - Retry policies for API calls
- `System.Text.Json` - JSON serialization (built-in)

### Optional/Recommended
- `Serilog` - Structured logging for query tracking
- `FluentValidation` - Input validation
- `Swashbuckle.AspNetCore` - API documentation (if not already installed)

## Database Schema

```csharp
public class Category
{
    public int Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public Category Category { get; set; }
}
```

## Implementation Plan

### 1. Add OpenAI NuGet Package
Install `Azure.AI.OpenAI` or `Betalgo.OpenAI` package for GPT-4 API access

### 2. Configuration Setup
Add to `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4",
    "MaxTokens": 500,
    "Temperature": 0.0,
    "Timeout": 30
  },
  "SemanticSearch": {
    "MaxQueryLength": 200,
    "CacheDurationMinutes": 60,
    "EnableQueryLogging": true
  }
}
```

Store API key in Environment Variables or Azure Key Vault for production

### 3. Create Schema Introspection Service
Build `ISchemaService` that:
- Generates database schema description on startup
- Formats schema as text for LLM context (table names, columns, data types, relationships)
- Caches schema description in memory
- Updates when schema changes detected
- Example output: "Categories table with columns: Id (int), CreateDate (datetime), Name (varchar), Code (varchar), Description (varchar). Products table with columns: Id (int), CategoryId (int FK to Categories), Name (varchar), Code (varchar), CreatedDate (datetime)."

### 4. Create Models
Define request/response models:
- `SemanticSearchRequest` with `Query` property (string, max 200 chars)
- `SemanticSearchResponse` with `GeneratedSQL`, `Results`, `ExecutionTime`, `Error` properties
- `QueryValidationResult` for SQL safety validation

### 5. Implement SQL Validator Service
Create `ISqlValidatorService` that:
- Parses generated SQL using basic string analysis or SQL parser library
- Ensures only SELECT statements allowed (no INSERT, UPDATE, DELETE, DROP, ALTER, etc.)
- Checks for dangerous patterns (`;`, `--`, `/*`, `xp_`, `EXEC`, etc.)
- Validates table/column names exist in schema
- Returns validation result with error messages if unsafe

### 6. Build Text-to-SQL Service
Create `ITextToSqlService` interface implementing:
- `Task<string> GenerateSqlAsync(string naturalLanguageQuery)` method
- System prompt engineering:
  ```
  You are a PostgreSQL expert. Convert natural language to SQL SELECT queries ONLY.
  
  Database Schema:
  {schema_description}
  
  Rules:
  - Generate ONLY valid PostgreSQL SELECT statements
  - Use table and column names EXACTLY as shown in schema
  - Use proper JOINs for related data
  - Do not include explanations, only return the SQL query
  - Limit results to 100 rows by default
  - Use parameterization where appropriate
  
  User Query: {user_query}
  
  SQL:
  ```
- Call OpenAI API with schema + user query
- Extract SQL from response (remove markdown formatting if present)
- Handle API errors and rate limits
- Apply Polly retry policy (3 attempts, exponential backoff)

### 7. Create Semantic Search Service
Implement `ISemanticSearchService` that orchestrates:
- Receive natural language query
- Check cache for identical query (return cached SQL + results)
- Call `ITextToSqlService` to generate SQL
- Call `ISqlValidatorService` to validate SQL
- If valid: execute query via EF Core `DbContext.Database.ExecuteSqlRaw()` or Dapper
- If invalid: return error with reason
- Store in cache with expiration
- Log query, SQL, execution time, success/failure
- Return results as JSON

### 8. Add API Controller
Create new `SemanticSearchController`:
- `POST /api/semantic-search` endpoint
- Accept `SemanticSearchRequest` body
- Validate input (max length, not empty, character whitelist)
- Call `ISemanticSearchService`
- Return `SemanticSearchResponse` with 200 OK or 400/500 with error
- Add rate limiting (per API consumer/IP)
- Add authentication/authorization for external API consumers

### 9. Implement Query Result Formatter
Create `IResultFormatterService`:
- Convert `DbDataReader` or `IEnumerable<T>` to JSON
- Support pagination (take first 100 results by default)
- Include metadata (row count, execution time, columns)
- Handle null values and data type conversions
- Optimize for large result sets (streaming if needed)

### 10. Add Caching Layer
Implement caching strategy:
- Use `IMemoryCache` for query → SQL mapping
- Use `IDistributedCache` (Redis) for SQL → results if high volume
- Cache key: hash of normalized query string
- Expiration: 60 minutes (configurable)
- Invalidation: clear cache on schema changes or manual trigger

### 11. Implement Security Measures
Add security layers:
- API key authentication for external consumers
- Rate limiting (e.g., 100 requests/hour per consumer)
- Input sanitization (remove special characters, limit length)
- SQL injection prevention (validator + parameterization)
- Query timeout (max 10 seconds execution)
- Audit logging (who, what, when, results count)
- Error message sanitization (don't expose schema details in errors to external users)

### 12. Create Integration Tests
Test scenarios:
- "Show all categories" → `SELECT * FROM Categories LIMIT 100`
- "Products in Electronics category" → `SELECT p.* FROM Products p JOIN Categories c ON p.CategoryId = c.Id WHERE c.Name = 'Electronics' LIMIT 100`
- "Categories created last month" → `SELECT * FROM Categories WHERE CreateDate >= NOW() - INTERVAL '1 month' LIMIT 100`
- "Products with code ABC" → `SELECT * FROM Products WHERE Code LIKE 'ABC%' LIMIT 100`
- Invalid queries (injection attempts, UPDATE statements)
- API error scenarios (OpenAI timeout, rate limit)
- Cache hit/miss scenarios

### 13. Add Monitoring and Logging
Implement telemetry:
- Log every query attempt (original query, generated SQL, success/failure)
- Track OpenAI API usage (tokens, cost)
- Monitor query execution time (database performance)
- Alert on validation failures (possible injection attempts)
- Dashboard metrics: queries/day, success rate, average execution time, cache hit rate
- Store failed queries for prompt improvement

### 14. Optimize Prompt Engineering
Iteratively improve system prompt:
- Collect failed conversions (query → wrong SQL)
- Add examples to prompt (few-shot learning)
- Refine schema description format
- Test with edge cases and adjust rules
- Version prompts for A/B testing
- Consider fine-tuning if many domain-specific queries

### 15. Add Fallback Mechanism
Implement graceful degradation:
- If OpenAI unavailable: return cached results only or error message
- If SQL validation fails: suggest corrections or return to traditional search
- If query execution fails: log error, return user-friendly message
- Provide "Did you mean..." suggestions for clarification

## LangChain Integration (Alternative Approach)

### Overview
LangChain provides ready-made components for text-to-SQL scenarios, significantly simplifying implementation. Instead of manually building prompt templates, SQL generation, and query execution logic, LangChain offers a unified framework with built-in best practices.

### Additional NuGet Packages for LangChain
```
LangChain.Core
LangChain.Providers.OpenAI (or LangChain.Providers.Azure)
LangChain.Databases.Postgres
LangChain.Memory
```

### Key LangChain Components for Text-to-SQL

#### 1. SQL Database Chain
LangChain's `SqlDatabaseChain` provides end-to-end text-to-SQL functionality:
- Automatic schema introspection
- Prompt template management
- SQL generation
- Query execution
- Result formatting

#### 2. Database Wrapper
`SqlDatabase` class handles database connections and schema retrieval:
- Connects to PostgreSQL via connection string
- Automatically extracts table schemas
- Limits accessible tables (security)
- Provides sample rows for context

#### 3. Prompt Templates
Pre-built prompt templates optimized for text-to-SQL:
- `SQL_POSTGRES_PROMPT` - PostgreSQL-specific templates
- Include few-shot examples
- Customizable for domain-specific needs

#### 4. Memory Components
Built-in conversation memory for multi-turn queries:
- `ConversationBufferMemory` - stores conversation history
- Enables follow-up questions ("show me the first 5", "sort by date")

### Simplified Implementation with LangChain

#### Step 1: Install LangChain Packages
```bash
dotnet add package LangChain.Core
dotnet add package LangChain.Providers.OpenAI
dotnet add package LangChain.Databases.Postgres
```

#### Step 2: Configure LangChain Service
```csharp
public class LangChainTextToSqlService : ITextToSqlService
{
    private readonly SqlDatabaseChain _chain;
    private readonly ILogger<LangChainTextToSqlService> _logger;

    public LangChainTextToSqlService(
        IConfiguration config,
        ILogger<LangChainTextToSqlService> logger)
    {
        // Initialize OpenAI provider
        var openAiProvider = new OpenAiProvider(
            apiKey: config["OpenAI:ApiKey"],
            model: "gpt-4"
        );

        // Initialize database connection
        var database = SqlDatabase.FromConnectionString(
            connectionString: config.GetConnectionString("PostgreSQL"),
            includeTables: new[] { "Categories", "Products" }, // Limit scope
            sampleRowsInTableInfo: 3 // Include sample data in schema context
        );

        // Create SQL database chain
        _chain = new SqlDatabaseChain(
            llm: openAiProvider,
            database: database,
            prompt: SqlPromptTemplate.PostgreSQL, // Pre-built Postgres template
            returnIntermediateSteps: true, // For debugging/logging
            returnDirect: false // Return formatted results
        );

        _logger = logger;
    }

    public async Task<string> GenerateSqlAsync(string query)
    {
        var result = await _chain.RunAsync(query);
        _logger.LogInformation("Generated SQL: {SQL}", result.IntermediateSteps.Sql);
        return result.IntermediateSteps.Sql;
    }

    public async Task<SemanticSearchResponse> ExecuteQueryAsync(string query)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _chain.RunAsync(query);
            stopwatch.Stop();

            return new SemanticSearchResponse
            {
                Query = query,
                GeneratedSql = result.IntermediateSteps.Sql,
                Results = result.Output,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Cached = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query execution failed");
            throw;
        }
    }
}
```

#### Step 3: Custom Prompt Template for ERP Domain
```csharp
var customPrompt = new PromptTemplate(
    template: @"
You are a PostgreSQL expert for an ERP inventory system.
Given an input question, create a syntactically correct PostgreSQL query.

Database Schema:
{schema}

Important Notes:
- Categories table contains product categories with Code, Name, Description
- Products table links to Categories via CategoryId
- Use ILIKE for case-insensitive text matching
- Always LIMIT results to 100 rows
- Only generate SELECT statements

Sample Questions and SQL:
Q: Show products in Electronics category
A: SELECT p.* FROM Products p JOIN Categories c ON p.CategoryId = c.Id WHERE c.Name ILIKE '%Electronics%' LIMIT 100

Q: Categories created this month
A: SELECT * FROM Categories WHERE CreateDate >= DATE_TRUNC('month', CURRENT_DATE) LIMIT 100

Question: {question}
SQL Query:",
    inputVariables: new[] { "schema", "question" }
);

var chain = new SqlDatabaseChain(
    llm: openAiProvider,
    database: database,
    prompt: customPrompt
);
```

#### Step 4: Add SQL Validation Layer
Even with LangChain, keep custom validation:
```csharp
public class ValidatedLangChainService : ITextToSqlService
{
    private readonly SqlDatabaseChain _chain;
    private readonly ISqlValidatorService _validator;

    public async Task<SemanticSearchResponse> ExecuteQueryAsync(string query)
    {
        // Generate SQL using LangChain
        var result = await _chain.RunAsync(query);
        var sql = result.IntermediateSteps.Sql;

        // Validate before execution
        var validation = await _validator.ValidateAsync(sql);
        if (!validation.IsValid)
        {
            throw new SecurityException($"Invalid SQL: {validation.ErrorMessage}");
        }

        // Return results
        return new SemanticSearchResponse
        {
            Query = query,
            GeneratedSql = sql,
            Results = result.Output,
            // ... other fields
        };
    }
}
```

#### Step 5: Add Caching Wrapper
Wrap LangChain with caching layer:
```csharp
public class CachedLangChainService : ITextToSqlService
{
    private readonly ValidatedLangChainService _innerService;
    private readonly IMemoryCache _cache;

    public async Task<SemanticSearchResponse> ExecuteQueryAsync(string query)
    {
        var cacheKey = $"semantic_search:{query.ToLowerInvariant().Trim()}";

        if (_cache.TryGetValue(cacheKey, out SemanticSearchResponse cached))
        {
            cached.Cached = true;
            return cached;
        }

        var result = await _innerService.ExecuteQueryAsync(query);
        
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(60));
        
        return result;
    }
}
```

#### Step 6: Register in DI Container
```csharp
// In Program.cs or Startup.cs
services.AddSingleton<SqlDatabaseChain>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var openAi = new OpenAiProvider(config["OpenAI:ApiKey"], "gpt-4");
    var db = SqlDatabase.FromConnectionString(
        config.GetConnectionString("PostgreSQL"),
        includeTables: new[] { "Categories", "Products" }
    );
    return new SqlDatabaseChain(openAi, db);
});

services.AddScoped<ValidatedLangChainService>();
services.AddScoped<CachedLangChainService>();
services.AddScoped<ITextToSqlService>(sp => 
    sp.GetRequiredService<CachedLangChainService>()
);
```

### Benefits of LangChain Approach

#### Simplified Implementation
- **Schema introspection**: Automatic, no manual service needed (Steps 3 → eliminated)
- **Prompt engineering**: Pre-built templates with best practices (Step 6 → simplified)
- **Query execution**: Built-in database handling (Step 7 → simplified)
- **Result formatting**: Automatic JSON conversion (Step 9 → eliminated)

#### Built-in Features
- **Few-shot learning**: Easily add examples to improve accuracy
- **Conversation memory**: Support multi-turn conversations
- **Error handling**: Standardized error patterns
- **Logging**: Intermediate steps captured automatically

#### Maintenance
- **Updates**: LangChain team maintains prompt templates for latest LLM capabilities
- **Testing**: Pre-tested components with community validation
- **Documentation**: Extensive examples and patterns

### What You Still Need with LangChain

Even with LangChain, you should still implement:

1. **SQL Validation** (Step 5) - Security critical, add custom validator
2. **Caching** (Step 10) - Cost optimization, wrap LangChain service
3. **API Controller** (Step 8) - REST API layer
4. **Security Measures** (Step 11) - Authentication, rate limiting, audit logging
5. **Monitoring** (Step 13) - Application-specific telemetry
6. **Configuration** (Step 2) - API keys, timeouts, settings

### LangChain vs Manual Implementation Comparison

| Aspect | Manual Implementation | LangChain Implementation |
|--------|----------------------|-------------------------|
| **Effort** | ~15 steps | ~6 steps |
| **Prompt Engineering** | Manual template creation | Pre-built templates + customization |
| **Schema Handling** | Build custom service | Automatic introspection |
| **Maintenance** | Update prompts manually | Benefit from community updates |
| **Flexibility** | Full control | Good control with abstractions |
| **Learning Curve** | Understand LLM internals | Learn LangChain API |
| **Debugging** | Custom logging | Built-in intermediate steps |
| **Testing** | Build all test cases | Some pre-tested components |

### Recommended Hybrid Approach

**Use LangChain for:**
- SQL generation and query execution core logic
- Prompt template management
- Database schema introspection

**Use Custom Implementation for:**
- SQL validation and security checks
- Caching strategy (application-specific)
- API authentication and authorization
- Business-specific logging and monitoring
- Rate limiting and cost controls

### Example: Hybrid Service Architecture

```
┌─────────────────────┐
│   API Controller    │
│ (Custom - Step 8)   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Caching Service    │
│ (Custom - Step 10)  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Validation Service  │
│  (Custom - Step 5)  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  LangChain Service  │
│ SqlDatabaseChain    │
│  - Schema loading   │
│  - SQL generation   │
│  - Query execution  │
└─────────────────────┘
```

### Migration Path

**Phase 1**: Start with manual implementation (15 steps)
- Full control and understanding
- Learn text-to-SQL patterns
- Identify specific needs

**Phase 2**: Evaluate LangChain integration
- Replace Steps 3, 6, 7, 9 with LangChain
- Keep custom validation, caching, security
- Compare results and performance

**Phase 3**: Optimize hybrid approach
- Use LangChain for core SQL generation
- Maintain custom layers for business logic
- Iterate on prompt templates

### LangChain Advanced Features for Future

Once basic implementation is working:

#### 1. SQL Agent (instead of Chain)
More sophisticated reasoning with tools:
```csharp
var agent = new SqlAgent(
    llm: openAiProvider,
    database: database,
    tools: new[] { 
        new SqlQueryTool(),      // Execute queries
        new SqlDescribeTool(),   // Describe tables
        new SqlInfoTool()        // Get schema info
    }
);
```

#### 2. Query Decomposition
Break complex queries into simpler sub-queries:
```csharp
var decomposer = new QueryDecomposer(llm: openAiProvider);
var subQueries = await decomposer.DecomposeAsync(complexQuery);
```

#### 3. Self-Healing Queries
Automatically fix SQL errors:
```csharp
var healingChain = new SqlDatabaseChain(
    llm: openAiProvider,
    database: database,
    maxIterations: 3,  // Retry with error feedback
    selfHeal: true
);
```

#### 4. Multi-Database Support
Query across multiple databases:
```csharp
var erp_db = SqlDatabase.FromConnectionString(erpConnectionString);
var analytics_db = SqlDatabase.FromConnectionString(analyticsConnectionString);

var multiDbAgent = new MultiDatabaseAgent(
    llm: openAiProvider,
    databases: new Dictionary<string, SqlDatabase> {
        { "erp", erp_db },
        { "analytics", analytics_db }
    }
);
```

## API Endpoint Specification

### Request
```http
POST /api/semantic-search
Content-Type: application/json
Authorization: Bearer {api_key}

{
  "query": "products in electronics"
}
```

### Response (Success)
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "query": "products in electronics",
  "generatedSql": "SELECT p.* FROM Products p JOIN Categories c ON p.CategoryId = c.Id WHERE c.Name ILIKE '%electronics%' LIMIT 100",
  "results": [
    {
      "id": 1,
      "categoryId": 5,
      "name": "Laptop",
      "code": "ELEC001",
      "createdDate": "2025-01-15T10:30:00Z"
    }
  ],
  "resultCount": 1,
  "executionTimeMs": 245,
  "cached": false
}
```

### Response (Validation Error)
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "Generated query contains unsafe operations",
  "details": "Only SELECT queries are allowed"
}
```

## Verification Checklist

- [ ] OpenAI API key configured and tested
- [ ] Schema introspection service generates accurate description
- [ ] SQL validator blocks all non-SELECT statements
- [ ] SQL validator detects injection patterns
- [ ] Generated SQL executes successfully against test database
- [ ] Results returned in expected JSON format
- [ ] Cache hit/miss working correctly
- [ ] Rate limiting enforced for API consumers
- [ ] Authentication working for external consumers
- [ ] Query logging capturing all required fields
- [ ] Unit tests pass (mock OpenAI responses)
- [ ] Integration tests pass (real OpenAI + test database)
- [ ] Performance acceptable (<2 seconds total for cached, <5 seconds for new queries)
- [ ] Error messages don't expose sensitive schema information
- [ ] API documentation updated (Swagger/OpenAPI)

## Design Decisions

### OpenAI GPT-4 as Primary Provider
- Best text-to-SQL performance in 2026
- Good understanding of PostgreSQL syntax
- Reliable with proper prompt engineering
- Can handle variations in user query phrasing

### Read-Only SELECT Queries Only
- Prevents data corruption from external consumers
- Reduces security risk surface
- Simplifies validation logic
- Appropriate for "search" use case

### Caching Strategy
- Reduces OpenAI API costs (GPT-4 token pricing)
- Improves response time for common queries
- 60-minute expiration balances freshness vs. efficiency

### Schema Context in Every Prompt
- Ensures LLM has current schema information
- Better SQL generation accuracy
- Handles schema changes without retraining

### Synchronous API Calls
- Simpler implementation for external API
- Acceptable latency (<5s) for search use case
- Can add async/queue pattern later if volume increases

### No Fine-Tuning Initially
- GPT-4 performs well with prompt engineering
- Fine-tuning complex and expensive
- Collect data first, evaluate need later

### External API Consumer Access
- Requires robust authentication and rate limiting
- Audit logging for compliance
- Error message sanitization to avoid information disclosure

## Security Considerations

### SQL Injection Prevention
1. **Validator Layer**: Blocks dangerous SQL patterns
2. **Allow-List**: Only SELECT statements pass
3. **Schema Validation**: Only existing tables/columns allowed
4. **Timeout**: Queries terminated after 10 seconds
5. **Result Limit**: Maximum 100 rows returned

### API Security
1. **Authentication**: API key or OAuth 2.0 bearer token
2. **Authorization**: Role-based access control (if needed)
3. **Rate Limiting**: Prevent abuse and cost overruns
4. **Input Validation**: Max length, character whitelist
5. **Audit Logging**: Track all queries for security review

### Data Privacy
1. **Error Sanitization**: Don't expose schema details in errors
2. **Query Logging**: Store securely, respect data retention policies
3. **Result Filtering**: Consider row-level security if needed

## Performance Optimization

### Caching
- In-memory cache for query → SQL (fast, low volume)
- Distributed cache (Redis) for SQL → results (optional, high volume)
- Cache invalidation on schema changes

### Database
- Ensure proper indexes on frequently queried columns (Category.Name, Product.Code)
- Query timeout to prevent long-running queries
- Connection pooling (EF Core default)

### OpenAI API
- Temperature 0.0 for deterministic output (reduces variability)
- Lower max_tokens (500) for faster response
- Retry with exponential backoff
- Monitor token usage for cost control

## Example Queries

### Simple Queries
- "all products" → `SELECT * FROM Products LIMIT 100`
- "all categories" → `SELECT * FROM Categories LIMIT 100`
- "products named laptop" → `SELECT * FROM Products WHERE Name ILIKE '%laptop%' LIMIT 100`

### Filtered Queries
- "categories created this year" → `SELECT * FROM Categories WHERE CreateDate >= '2026-01-01' LIMIT 100`
- "products with code starting ABC" → `SELECT * FROM Products WHERE Code LIKE 'ABC%' LIMIT 100`

### Joined Queries
- "products in electronics" → `SELECT p.* FROM Products p JOIN Categories c ON p.CategoryId = c.Id WHERE c.Name ILIKE '%electronics%' LIMIT 100`
- "products by category name" → `SELECT p.*, c.Name as CategoryName FROM Products p JOIN Categories c ON p.CategoryId = c.Id LIMIT 100`

### Aggregation Queries
- "count products per category" → `SELECT c.Name, COUNT(p.Id) FROM Categories c LEFT JOIN Products p ON c.Id = p.CategoryId GROUP BY c.Name LIMIT 100`
- "newest products" → `SELECT * FROM Products ORDER BY CreatedDate DESC LIMIT 100`

## Cost Estimation

### OpenAI API Costs (GPT-4, February 2026 pricing)
- Input: ~500 tokens per request (schema + prompt + query)
- Output: ~100 tokens per request (SQL query)
- Estimated: $0.03 - $0.06 per query
- With caching: ~$0.01 per query (assuming 50% cache hit rate)
- 1,000 queries/day = ~$10-$30/day

### Optimization Strategies
- Cache common queries (reduce API calls)
- Use GPT-3.5-turbo for simpler queries (lower cost)
- Batch similar queries if pattern emerges
- Set budget alerts

## Future Enhancements

### Phase 2 Considerations
- Support for multiple tables beyond Category/Product
- Query history and suggestions ("Users also searched...")
- Query refinement ("Did you mean...?")
- Export results (CSV, Excel)
- Visualization suggestions (chart type for aggregations)
- Multi-tenant schema support
- Custom domain-specific query templates

### Alternative Providers
- Google Gemini as fallback
- Azure OpenAI for enterprise customers
- Open-source text-to-SQL models (self-hosted)

---

**Created**: February 10, 2026  
**Status**: Planning Phase  
**Feature**: Semantic Search (Text-to-SQL)  
**Dependencies**: Feature One (Address Parser) is independent
