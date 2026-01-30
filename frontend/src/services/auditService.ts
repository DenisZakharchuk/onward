import { GraphQLClient, gql } from 'graphql-request';

const graphqlClient = new GraphQLClient(
  import.meta.env.VITE_API_BASE_URL?.replace('/api', '') + '/graphql' || 'http://localhost:5002/graphql'
);

export interface AuditLog {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  userId: string;
  ipAddress?: string;
  timestamp: string;
}

export interface AuditLogFilters {
  fromDate?: string;
  toDate?: string;
  entityType?: string;
  action?: string;
  userId?: string;
}

const AUDIT_LOGS_QUERY = gql`
  query GetAuditLogs(
    $fromDate: DateTime
    $toDate: DateTime
    $entityType: String
    $action: String
    $userId: String
  ) {
    auditLogs(
      fromDate: $fromDate
      toDate: $toDate
      entityType: $entityType
      action: $action
      userId: $userId
    ) {
      id
      action
      entityType
      entityId
      userId
      ipAddress
      timestamp
    }
  }
`;

const AUDIT_LOG_BY_ID_QUERY = gql`
  query GetAuditLogById($id: String!) {
    auditLogById(id: $id) {
      id
      action
      entityType
      entityId
      userId
      ipAddress
      timestamp
    }
  }
`;

export const auditService = {
  async getAuditLogs(filters?: AuditLogFilters): Promise<AuditLog[]> {
    try {
      const data = await graphqlClient.request<{ auditLogs: AuditLog[] }>(
        AUDIT_LOGS_QUERY,
        filters || {}
      );
      return data.auditLogs;
    } catch (error) {
      console.error('Failed to fetch audit logs:', error);
      throw error;
    }
  },

  async getAuditLogById(id: string): Promise<AuditLog | null> {
    try {
      const data = await graphqlClient.request<{ auditLogById: AuditLog }>(
        AUDIT_LOG_BY_ID_QUERY,
        { id }
      );
      return data.auditLogById;
    } catch (error) {
      console.error('Failed to fetch audit log:', error);
      throw error;
    }
  },
};
