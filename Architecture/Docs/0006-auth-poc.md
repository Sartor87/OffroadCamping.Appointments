# Authentication Provider PoC (ADR 0006)

Status: Proposed — see `Architecture/ADR/0006-update-authentication-provider.md`.

Objective: run a short PoC to evaluate Keycloak (self-hosted) vs Okta (managed) and decide which provider fits security, compliance, and TCO needs.

Minimal PoC tasks:

1. Deploy Keycloak locally (Docker or Kubernetes) and create a realm for the Appointments service. For details check this video [Secure Your .NET Application With Keycloak: Step-by-Step Guide](https://www.youtube.com/watch?v=Blrn5JyAl6E); check if the Aspire package is out of preview [The Simplest Way to Add Keycloak Authentication to Your .NET API](https://www.youtube.com/watch?v=HAvCoQ0tOTs)
2. Integrate OIDC login with the API (verify token issuance and validation middleware).
3. Test login, refresh, logout flows, and role/claim mappings for `patient` / `admin` roles.
4. Validate audit and log export, backup/restore, and upgrade path for chosen Keycloak distribution.
5. Estimate 12/24 month TCO (infrastructure + ops) and compare to Okta pricing for equivalent feature set.

Acceptance criteria:

- Security posture meets team requirements for audit and logging.
- Operational tasks are automatable (backup, upgrade, HA).
- TCO is favourable for Keycloak or business accepts Okta's managed option.

Owner: DevOps + Architecture leads. Execute PoC with a cross-functional pair (Backend + Platform).