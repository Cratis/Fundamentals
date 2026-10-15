# Generation conformance vectors

Copied verbatim from Cratis/Screenplay commit `7b00cd9ffa72ff4f0eafaf56a0ef604b61da31bb`,
`Source/Screenplay/Compiler/Conformance/generation-patterns.json` and `generation-checksums.json`.

The runtime exercises every support, invalid, membership, checksum and validation vector.
The `assessments` section belongs to the binder's 101-year horizon and is not a runtime operation.
The `ranges` section tests parsing Screenplay source text; the runtime accepts typed Int64 bounds instead.
