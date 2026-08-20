---
applyTo: "**/*.kt"
---

# Kotlin Conventions

## Naming

### No Postfixes

**Do not postfix class names** — this is a hard rule across the entire Kotlin client.

- No `*Service` suffix. A class that registers reactors is `Reactors`, not `ReactorsService`. A class that registers reducers is `Reducers`, not `ReducersService`.
- No `*Annotation` suffix on annotation classes. An annotation that marks a reactor is `@Reactor`, not `@ReactorAnnotation`.
- No `*Manager`, `*Handler`, `*Helper`, `*Utils` — these are all content-free names that say nothing about what the class actually does.

Name classes after what they **represent in the domain** — typically the plural form of the concept:

| Concept | Correct name | Wrong name |
|---|---|---|
| Registers and manages reactors | `Reactors` | `ReactorsService` |
| Registers and manages reducers | `Reducers` | `ReducersService` |
| Registers event types | `EventTypes` | `EventTypesService` |
| Registers projections | `Projections` | `ProjectionsService` |
| Manages constraints | `Constraints` | `ConstraintsService` |
| Marks a class as a reactor | `@Reactor` | `@ReactorAnnotation` |
| Marks a class as a reducer | `@Reducer` | `@ReducerAnnotation` |
| Marks a class as an event type | `@EventType` | `@EventTypeAnnotation` |

The only exception is when the class IS genuinely a service (e.g., a gRPC stub wrapper intended exclusively as an internal infrastructure adapter), but even then, prefer naming it after the protocol or transport concept.

## Annotations

- Annotation classes must not carry an `Annotation` suffix. Use `@EventType`, `@Reactor`, `@Reducer`, `@Projection`, `@Constraint`, `@ReadModel`, `@Seeder`, `@Pii`, etc.
- All identity/id fields on annotation classes must be named `id` for consistency — never `reactorId`, `reducerId`, `projectionId`, `readModelId`, or `name`.
- All id fields on annotations default to `""`. The service layer uses reflection (`simpleName` or `qualifiedName`) as the fallback, so callers never need to specify an id explicitly unless they want to override the default.

## File Organization

- One top-level declaration per file.
- File name matches the top-level class or annotation name exactly.
- Extension functions on a type live in the same file as the type, unless they are so numerous that a dedicated `*Extensions.kt` file is warranted.

## Java compatibility

Java is a first-class target for this client. Kotlin compiling and the specs passing prove nothing
about it — Java has broken twice on changes that were green everywhere else.

**Every new public type, annotation or member ships a Java usage fixture.** Add it to
`Source/src/test/java/io/cratis/chronicle/conformance/JavaConformance.java`, which is never run —
compiling it *is* the assertion — or to a narrower fixture under `src/test/java` when the area
already has one.

The recurring hazards, each of which has bitten:

| Kotlin construct | What Java sees |
|---|---|
| Default arguments | Nothing — every parameter is required unless `@JvmOverloads` |
| `@Repeatable` | No usable container unless `@JvmRepeatable` names an explicit one |
| Annotation element not named `value` | No array or single-value shorthand |
| `suspend fun` | Unusable — needs a blocking bridge in `io.cratis.chronicle.java` |
| `Flow<T>` | Uncollectable — needs a callback bridge |
| `@JvmInline value class` | Mangled constructors and accessors — never put one in the Java surface |
| `KClass<T>` parameter | Awkward — add a `Class<T>` overload |
| `internal` members | Name-mangled |

`Source/api/Source.api` is the checked-in public ABI, produced by the binary-compatibility-validator.
A change to it shows up in the diff, which is how an accidental break gets caught in review. Run
`gradle apiDump` after any intentional public API change and commit the result; `gradle apiCheck`
runs as part of `gradle build` and fails when the dump is stale.
