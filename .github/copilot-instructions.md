# Copilot instructions – Household Panel

See [PROJECT.md](../PROJECT.md) for the full architecture and design spec. These are the rules to follow when generating or editing code in this repository.

1. Follow the existing Clean Architecture layout: `src/HouseholdPanel.Domain` → `Application` → `Infrastructure` → `Api`. Dependencies only point inward.
2. Avoid over-engineering. Keep small features simple.
3. Keep the frontend extremely lightweight (Raspberry Pi Zero 2 W target).
4. All external data collection (weather, transport, calendar) happens in the backend. The frontend never calls external APIs directly.
5. Use dependency injection in the backend.
6. Use `async`/`await` for all I/O.
7. Use `CancellationToken` on backend service calls.
8. Put secrets in configuration/environment variables, never in Git.
9. Write tests for new backend logic (unit tests in `HouseholdPanel.UnitTests`, integration tests in `HouseholdPanel.IntegrationTests`).
10. Use Angular standalone components.
11. Use Angular Signals where they simplify state management (see `DashboardService`).
12. Avoid unnecessary frontend dependencies.
13. Optimize for the Raspberry Pi Zero 2 W: minimal JS, minimal DOM, no heavy animations or large images.
14. The GUI is touch-first and designed specifically for a 3.5" screen. No scrollbars.
15. Keep components small and focused.
16. Follow SOLID where it adds real value.
17. Use clear, descriptive names.
18. Don't put business logic in controllers.
19. Don't make HTTP calls from Angular components — only from `core/api` services, consumed via `core/services`.
20. The frontend only knows about the DTO models in `core/models`; it must never depend on backend domain models.
21. External data sources (weather, transport, calendar, schedule) must be implemented behind an interface (`Application/Abstractions`) so providers can be swapped later.
