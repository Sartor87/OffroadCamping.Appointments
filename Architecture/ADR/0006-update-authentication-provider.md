# 6. Update authentication provider: evaluate Keycloak vs Okta

## Date: 2026-01-23

## Status

Proposed / Pending decision

## Context

The project currently relies on a locally-managed Identity Provider (IdP) for JWT issuance and user management. We are evaluating whether to continue maintaining an internal IdP or adopt a third-party solution (self-hosted or managed) to reduce operational burden and improve features such as federation, SSO, and compliance.

Reference: Kiril Iliev's analysis comparing Keycloak and Okta — https://www.linkedin.com/pulse/why-keycloak-outperforms-okta-regulated-hybrid-microsoft-kiril-iliev-ugexf/?trackingId=QpNF%2FiEQHC0XJB2CCJYqYw%3D%3D

## Problem

Maintaining an in-house IdP adds continuous operational cost (patching, HA, backups), increases security responsibility, and slows feature delivery. We need a decision that balances operational effort, cost, vendor lock-in, extensibility, and compliance requirements.

## Decision (proposed)

Run a short proof-of-concept (PoC) evaluating Keycloak as the primary candidate and Okta as the managed SaaS alternative. Prefer Keycloak if the PoC validates that we can meet operational and compliance requirements with acceptable TCO; otherwise select Okta for a managed, lower-ops option.

Rationale for preferring Keycloak during evaluation:
- Open-source, no per-user licensing; lower recurring license cost for large user bases.
- Strong federation and AD/LDAP brokering capabilities for hybrid Microsoft environments.
- Full control over customization, themes, protocol extensions, and event/log access for auditing.
- Flexible deployment models (Kubernetes with Operator, VMs, managed vendors) that align with Aspire/local dev and cloud deployments.

## Consequences

Positive:
- Reduced vendor lock-in if self-hosted; full access to logs and data for compliance.
- Avoid per-seat licensing costs compared to Okta for larger user counts.
- Ability to extend and customize flows to match regulatory or domain-specific needs.

Negative / Risks:
- Increased operational responsibility (HA, upgrades, monitoring) if self-hosted Keycloak.
- Need to staff or acquire support (community vs paid vendor support).
- Migration complexity for existing users/tokens and for integrating with downstream services.

## Alternatives considered

- Okta (managed SaaS): Lower ops burden, enterprise SLAs and support, faster onboarding — higher recurring cost and potential vendor lock-in.
- Continue to operate local in-house IdP: Avoids migration work but retains full ops burden and slower feature development.

## Next steps / Acceptance criteria for PoC

1. Deploy Keycloak in a dev environment (Kubernetes or Docker) with a simple realm for the Appointments service.
2. Integrate OIDC login with `OffroadCamping.Appointments.API` (JWT issuance, token validation middleware). Verify logout and refresh flows.
3. Configure user federation (LDAP/AD broker) and SCIM provisioning if feasible.
4. Test MFA, role mapping, and claims transformation for patient/doctor/admin roles.
5. Validate audit/log export, backup/restore, and upgrade path for Keycloak operator or distribution chosen.
6. Document TCO (infra + ops) for 12/24 months and compare to Okta pricing for equivalent features.

Decision will be approved if PoC demonstrates: security posture meets requirements, operational tasks are automatable, and TCO is favourable compared to Okta or business accepts Okta's higher recurring cost for managed service benefits.

## Owner

Proposed owners: `DevOps` and `Architecture` leads; execute PoC with a cross-functional pair (Backend + Platform).
