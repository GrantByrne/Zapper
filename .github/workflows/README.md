# GitHub Actions Workflows

This directory contains the automated workflows for the Zapper project.

## Workflows

### CI/CD Pipeline (`ci.yml`)
**Triggers:** Push to main/develop, Pull requests to main/develop

This is the main continuous integration workflow that:
- Builds the solution
- Runs all tests
- Checks code formatting
- Uploads code coverage reports (on push events)

For pull requests, formatting issues will show as warnings but won't fail the build. The auto-format workflow will fix them automatically.

### Auto Format (`auto-format.yml`)
**Triggers:** Pull requests (opened or synchronized)

This workflow automatically formats code in pull requests:
- Runs `dotnet format` on the entire solution
- Commits any formatting changes back to the PR branch
- Adds a comment to the PR when formatting is applied
- Only runs on PRs from the same repository (not forks)

### Release (`release.yml`)
**Triggers:** Push of version tags (v*.*.*)

This workflow creates releases:
- Generates changelog from commits
- Builds release assets for multiple platforms
- Creates GitHub release with artifacts
- Updates installation scripts

## How Auto-Formatting Works

1. When you open a PR with formatting issues:
   - The CI pipeline will detect them but won't fail
   - The auto-format workflow will run separately
   - Any formatting fixes will be committed to your PR
   - You'll see a comment confirming the formatting was applied

2. For direct pushes to main/develop:
   - Formatting is strictly enforced
   - The push will fail if formatting issues exist
   - Run `dotnet format` locally before pushing

## Running Formatting Locally

To format your code before committing:

```bash
cd src
dotnet format
```

To check formatting without making changes:

```bash
cd src
dotnet format --verify-no-changes
```

## Permissions

The auto-format workflow requires write permissions to:
- Push commits to PR branches
- Add comments to PRs

These permissions are granted automatically for PRs from the same repository. For security reasons, the auto-format workflow doesn't run on PRs from forks.