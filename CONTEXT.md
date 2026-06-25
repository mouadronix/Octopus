# Octopus — Domain Glossary

## Terminal

The port facility being managed. Tracks the current simulation day and a planning horizon (default 30 days). Time advances one day at a time; advancing the day departs ships whose assignments have ended.

## Ship

A vessel requesting berth space. Has a size (S, M, L, XL), an arrival day, and a duration (3–15 days). Status progresses: Pending → Assigned → Departed. Ships are auto-generated with random size, arrival day, and duration — the operator provides only name and notes.

## Dock

A berthing location at the terminal. Has a size that determines which ships it can accommodate (dock size must be ≥ ship size). A dock can hold one ship at a time per assignment window.

## Assignment

A confirmed booking of a ship to a dock over a contiguous range of days (start day to end day). Created when the operator confirms a scheduling suggestion. One ship has at most one assignment at a time.

## Scheduling

The algorithm that finds the earliest available slot for a ship at a compatible dock. Uses first-fit greedy: scan a dock's existing assignments chronologically, find the first gap that fits the ship's duration. The suggestion loop iterates over all compatible docks and picks the earliest start day, tie-breaking by fewest existing assignments on the dock.

## Scheduling Module

A deep module owning all scheduling rules: slot-finding (first-fit greedy), size compatibility, conflict detection, and suggestion ranking. Pure functions — no DB dependency. The caller pre-fetches data and passes it in. One interface, two consumers (AssignmentService for assignment creation, the suggestion endpoint for the frontend).

## Assignment Repository

The data-access seam for AssignmentService. Hides the multi-entity fetching complexity behind use-case-specific methods: `GetAssignmentContext` (ship + dock + terminal for assignment creation) and `GetSuggestionContext` (ship + terminal + docks + grouped assignments for suggestions). The repository adds entities; the service controls when to save and manages transactions.
