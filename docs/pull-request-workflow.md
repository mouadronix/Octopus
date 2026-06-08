# Pull Request Workflow

## Branches

- `main` contains stable project code.
- `develop` collects completed sprint work before release.
- Feature branches should use the format `devN/card-number-short-title`.

Examples:

- `dev1/card-18-operator-dashboard`
- `dev2/card-23-berth-board`
- `dev3/card-05-ship-model`
- `dev4/card-04-database-context`

## Pull Requests

1. Create a branch from `develop`.
2. Implement one Trello card per pull request when possible.
3. Fill in the pull request template.
4. Request at least one review.
5. Wait for CI to pass.
6. Squash merge into `develop`.

## Definition of Done

- The card checklist is complete.
- The code builds locally.
- Tests or manual verification are documented.
- Related docs are updated.
