---
applyTo: "**/*"
---

## Commands

```bash
dotnet build          # .NET packages
dotnet test           # specifications
yarn install && yarn build   # TypeScript packages (workspace root)
```

CI runs the .NET and JavaScript builds plus markdown verification; publishing
is label-driven (`major`/`minor`/`patch`) through the shared publish workflow.
