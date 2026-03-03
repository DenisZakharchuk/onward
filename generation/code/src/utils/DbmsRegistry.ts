/**
 * Registry of supported database management systems and EF Core providers.
 *
 * - DBMS entries drive docker-compose service generation (image, ports, env vars, healthcheck).
 * - EF provider entries drive Program.cs and .csproj generation (UseXxx() method, NuGet package).
 */

// ---------------------------------------------------------------------------
// DBMS (Docker compose) types
// ---------------------------------------------------------------------------

export interface DbmsEnvVar {
  key: string;
  value: string;
}

export interface DbmsConfig {
  /** Docker image name + tag. */
  image: string;
  /** Port exposed inside the container. */
  containerPort: number;
  /** Path to data volume mount inside the container. */
  dataPath: string;
  /** Healthcheck test array rendered as a YAML inline sequence, e.g. '["CMD-SHELL", "..."]' */
  healthcheck: string;
  /** Returns the environment variables needed to initialise the database. */
  getEnvVars: (databaseName: string) => DbmsEnvVar[];
}

// ---------------------------------------------------------------------------
// EF Core provider types
// ---------------------------------------------------------------------------

export interface EfProviderConfig {
  /** DbContextOptionsBuilder extension method, e.g. 'UseNpgsql'. */
  useMethod: string;
  /** NuGet package name. */
  package: string;
  /** NuGet package version. */
  packageVersion: string;
}

// ---------------------------------------------------------------------------
// Registries
// ---------------------------------------------------------------------------

const DBMS_REGISTRY: Record<string, DbmsConfig> = {
  postgres: {
    image: 'postgres:16',
    containerPort: 5432,
    dataPath: '/var/lib/postgresql/data',
    healthcheck: '["CMD-SHELL", "pg_isready -U postgres"]',
    getEnvVars: (databaseName) => [
      { key: 'POSTGRES_DB', value: databaseName },
      { key: 'POSTGRES_USER', value: 'postgres' },
      { key: 'POSTGRES_PASSWORD', value: 'postgres' },
    ],
  },
  mssql: {
    image: 'mcr.microsoft.com/mssql/server:2022-latest',
    containerPort: 1433,
    dataPath: '/var/opt/mssql/data',
    healthcheck:
      '["/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrong!Passw0rd", "-Q", "SELECT 1"]',
    getEnvVars: (_databaseName) => [
      { key: 'ACCEPT_EULA', value: 'Y' },
      { key: 'SA_PASSWORD', value: 'YourStrong!Passw0rd' },
      { key: 'MSSQL_PID', value: 'Developer' },
    ],
  },
  mysql: {
    image: 'mysql:8',
    containerPort: 3306,
    dataPath: '/var/lib/mysql',
    healthcheck: '["CMD", "mysqladmin", "ping", "-h", "localhost"]',
    getEnvVars: (databaseName) => [
      { key: 'MYSQL_DATABASE', value: databaseName },
      { key: 'MYSQL_USER', value: 'mysql' },
      { key: 'MYSQL_PASSWORD', value: 'mysql' },
      { key: 'MYSQL_ROOT_PASSWORD', value: 'root' },
    ],
  },
};

const EF_PROVIDER_REGISTRY: Record<string, EfProviderConfig> = {
  npgsql: {
    useMethod: 'UseNpgsql',
    package: 'Npgsql.EntityFrameworkCore.PostgreSQL',
    packageVersion: '8.0.0',
  },
  sqlserver: {
    useMethod: 'UseSqlServer',
    package: 'Microsoft.EntityFrameworkCore.SqlServer',
    packageVersion: '8.0.0',
  },
  sqlite: {
    useMethod: 'UseSqlite',
    package: 'Microsoft.EntityFrameworkCore.Sqlite',
    packageVersion: '8.0.0',
  },
};

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export class DbmsRegistry {
  /**
   * Look up docker compose config for the given management system identifier.
   * Throws a descriptive error for unknown keys (fail-fast at generation time).
   */
  static getDbmsConfig(managementSystem: string): DbmsConfig {
    const key = managementSystem.toLowerCase();
    const config = DBMS_REGISTRY[key];
    if (!config) {
      throw new Error(
        `Unknown managementSystem '${managementSystem}'. ` +
          `Supported values: ${Object.keys(DBMS_REGISTRY).join(', ')}.`
      );
    }
    return config;
  }

  /**
   * Look up EF Core provider config for the given provider identifier.
   * Throws a descriptive error for unknown keys (fail-fast at generation time).
   */
  static getProviderConfig(provider: string): EfProviderConfig {
    const key = provider.toLowerCase();
    const config = EF_PROVIDER_REGISTRY[key];
    if (!config) {
      throw new Error(
        `Unknown EF provider '${provider}'. ` +
          `Supported values: ${Object.keys(EF_PROVIDER_REGISTRY).join(', ')}.`
      );
    }
    return config;
  }
}
