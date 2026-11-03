# ADR 0001: Add Italian using the existing localization system

- Status: Accepted
- Date: 2026-11-02

## Context

The CYOA console game supports English, Spanish, and French. Adding Italian as a fourth language makes the game accessible to Italian-speaking players and extends the project's localization work.

The game already stores translated messages, input mappings, and display names in `language_data.json`. The `Messages` class loads the selected language, while gameplay uses shared message keys and canonical English identifiers for races, occupations, and weapons.

## Decision

Add Italian as option 4 in the language selection menu and store its translations in an `Italian` section of the existing JSON file. Use the same dictionary and mapping structure as the other languages and load it through `Messages`.

Preserve message keys, format placeholders such as `{0}`, canonical gameplay identifiers, and existing gameplay commands. The Italian lightning bolt spell display name is `incantesimo del fulmine`, with a space between `del` and `fulmine`.

## Reasoning

The existing system already provides the functionality Italian needs. Reusing it keeps translation data separate from gameplay logic and avoids duplicating loading, input normalization, and display translation code. Team members can maintain all four languages using the same structure and tests, which is appropriate for the scope of a college software engineering project.

## Alternatives considered

- **Create a separate Italian localization system:** This could support custom behavior, but Italian currently needs no separate architecture. It would duplicate code and increase the chance that languages behave differently after future changes.
- **Hard-code Italian strings in gameplay classes:** This would be quick for a few messages, but would mix translations with game logic and make updates harder to maintain.
- **Replace JSON with .NET resource files:** Resource files could support a more extensive localization workflow, but migrating all existing languages adds work beyond the needs of this addition.
- **Keep only three languages:** This avoids translation maintenance but does not meet the goal of supporting Italian-speaking players.

## Consequences and trade-offs

- Italian players receive translated prompts, narrative, and character display names without changes to game rules.
- New messages and mappings must be maintained across four languages. Translations must preserve placeholders and match the commands the game accepts.
- A shared JSON file is easy to inspect, but grows as languages are added and can cause merge conflicts when several contributors edit it.
- The existing loader depends on valid JSON and expected sections; it does not provide automatic English fallback for missing translations. Translation completeness and formatting therefore need verification.
- Reusing the current structure keeps this change small, while accepting its limitations for future language-specific grammar or formatting needs. A larger localization framework can be reconsidered if those needs arise.

## Validation

Use the existing Italian tests to check message coverage, placeholders, input mappings, display names, and console behavior. Run `dotnet build` and `dotnet test` after the change.
