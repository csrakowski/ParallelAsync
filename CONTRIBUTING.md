# Contributing to ParallelAsync

Thank you for your interest in contributing to ParallelAsync! This document provides guidelines and information to help you contribute effectively.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Environment](#development-environment)
- [How to Contribute](#how-to-contribute)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Communication](#communication)
- [License](#license)

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please be respectful and professional in all interactions.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Set up your development environment (see below)
4. Create a new branch for your changes
5. Make your changes and commit them
6. Push to your fork and submit a pull request

## Development Environment

### Prerequisites

- **.NET 10 SDK** - Required for building the project
- **Visual Studio 2026** (recommended) - Provides the best development experience
- **Visual Studio Code** with C# extensions (alternative) - Also supported for development

### Setup

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/ParallelAsync.git
cd ParallelAsync

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## How to Contribute

### Reporting Bugs

- Use the GitHub [issue templates](.github/ISSUE_TEMPLATE.md) provided
- Include a clear description of the issue
- Provide steps to reproduce the problem
- Include any relevant error messages or logs

### Suggesting Enhancements

- Use the GitHub [issue templates](.github/ISSUE_TEMPLATE.md) provided
- Clearly describe the enhancement and its benefits
- Explain why this enhancement would be useful to most users

### Submitting Changes

**For significant work:**
- Always create an issue first to discuss the proposed changes
- Reference the issue number in your commits and pull request

**For minor changes (typos, dependency bumps, etc.):**
- You may submit a pull request directly with a clear description

## Pull Request Process

1. **Target Branch**: Always target the `master` branch with your pull requests

2. **Before Submitting**:
   - Ensure all tests pass locally
   - Add tests for new features or bug fixes
   - Add benchmarks when applicable (for performance-related changes)
   - Follow the project's coding standards (see below)
   - Update documentation if needed

3. **PR Description**:
   - Provide a clear description of the changes
   - Reference related issues using `#issue-number`
   - Explain the reasoning behind the changes
   - Describe any breaking changes

4. **Review Process**:
   - Address any feedback from maintainers
   - Keep your branch up to date with master if needed
   - Be patient - reviews may take some time

5. **Merge Strategy**: This project uses merge commits to maintain a clear history

## Coding Standards

### C# Conventions

- Follow a slightly updated flavor of the **Microsoft C# Coding Conventions**
- The project includes an `.editorconfig` file that helps enforce these standards
- Ensure your IDE respects the `.editorconfig` settings
- In case you feel you have a valid reason to deviate, please justify in the related PR/comments

### General Guidelines

- Write clear, self-documenting code
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Avoid unnecessary complexity

## Testing Requirements

### Required for All Contributions

- **All existing tests must pass** - No exceptions
- **New features must include tests** - Demonstrate functionality and prevent regressions
- **Bug fixes should include tests** - Prevent the issue from reoccurring
- **Benchmarks when applicable** - Add benchmarks for performance-sensitive features

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Running Benchmarks

```bash
# Navigate to the benchmark project and run
dotnet run -c Release --project tests/CSRakowski.Parallel.Benchmarks
```

## Communication

- **Questions and Discussions**: Use GitHub Issues
- **Bug Reports**: Use GitHub Issues with the appropriate template
- **Feature Requests**: Use GitHub Issues with the appropriate template

Please keep all project-related communication in GitHub Issues to maintain transparency and allow others to benefit from the discussion.

## License

By submitting code to this project, you acknowledge that your contributions will be licensed under the same license as the project (see [LICENSE](LICENSE) file). You confirm that you have the right to submit the code and agree to the project's license terms.

---

Thank you for contributing to ParallelAsync! Your efforts help make this library better for everyone.
