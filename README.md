# getdue-buildingblocks

**Repo type:** Shared (per [getdue-docs · engineering/01-repositories.md §4](https://github.com/getdue-dev/getdue-docs/blob/main/engineering/01-repositories.md#4-shared-library-repo-getdue-buildingblocks))

Shared C# primitives published as NuGet packages under the `GetDue.BuildingBlocks.*` namespace. Consumed by every GetDue service via **pinned, published versions** — never a branch or a local path.

## Contents (planned)

| Package | Purpose |
|---|---|
| `GetDue.BuildingBlocks.Money` | `Money` value object (`decimal(19,4)` + ISO-4217 currency), JSON converter, EF Core value converter |
| `GetDue.BuildingBlocks.Outbox` | Transactional outbox + relay (Rebus + RabbitMQ) |
| `GetDue.BuildingBlocks.Idempotency` | RFC-style idempotency-key middleware ([API design §5](https://github.com/getdue-dev/getdue-docs/blob/main/phase-0/04-api-design.md#5-idempotency-keys)) |
| `GetDue.BuildingBlocks.Telemetry` | OpenTelemetry bootstrap (traces + metrics + logs, OTLP) |
| `GetDue.BuildingBlocks.Auth` | JWT bearer auth handlers + JWKS rotation |
| `GetDue.BuildingBlocks.Resilience` | Polly v8 resilience pipeline presets |
| `GetDue.BuildingBlocks.ProblemDetails` | RFC 9457 problem-details middleware |

## Rules

- **No business / domain code.** A symbol named after a domain entity (`BankAccount`, `LoanDebt`, `Property`, etc.) here is a build break — enforced by `NetArchTest`.
- **SemVer.** Breaking changes require a major bump and an ADR.
- **Pinned versions only** in consumer repos.

## Security

See [SECURITY.md](./SECURITY.md). Practices follow [GetDue Secure SDLC](https://github.com/getdue-dev/getdue-docs/blob/main/engineering/04-secure-sdlc.md).

## CI

`.github/workflows/ci.yml` calls the org-shared reusable workflows in [`getdue-dev/.github`](https://github.com/getdue-dev/.github/tree/main/.github/workflows). Today: `lint.yml` (actionlint). Once the .NET solution lands, `dotnet-build.yml` is added with the 100% line + branch coverage gate.
