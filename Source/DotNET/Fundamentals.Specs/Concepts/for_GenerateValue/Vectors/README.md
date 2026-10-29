# Generation conformance vectors

Copied verbatim from Cratis/Screenplay commit `7b00cd9ffa72ff4f0eafaf56a0ef604b61da31bb`,
`Source/Screenplay/Compiler/Conformance/generation-patterns.json` and `generation-checksums.json`.

The runtime exercises every support, invalid, membership, checksum and validation vector.
The `assessments` section belongs to the binder's 101-year horizon and is not a runtime operation.
The `ranges` section tests parsing Screenplay source text; the runtime accepts typed Int64 bounds instead.

## UUID v5

Unlike the copied files, `uuid-v5.json` is authored here. Its sources are RFC 9562 Appendix A.4 and values
cross-computed with Python `uuid.uuid5` and Node `crypto`.

Names use UTF-8 without a BOM, Unicode normalization or case folding. Unpaired UTF-16 surrogates are rejected.
Namespaces use network byte order. Invalid names are UTF-16 code-unit arrays so readers can reconstruct them
without JSON string decoding replacing or rejecting lone surrogates.

Screenplay will copy this file verbatim to `Source/Screenplay/Compiler/Conformance/uuid-v5.json` when
its `derive uuidv5(…)` lands (Cratis/Screenplay#308). After that, whichever repository changes it first must
update the other and pin the source commit, as this README does for the pattern files.
