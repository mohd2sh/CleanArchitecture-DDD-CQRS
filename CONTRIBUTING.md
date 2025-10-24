# Contributing

Thank you for your interest in contributing to this Clean Architecture template!

## How to Contribute

### Reporting Issues
- Search existing issues before creating new ones
- Use clear, descriptive titles
- Include steps to reproduce for bugs
- Specify your environment (.NET version, OS)

### Suggesting Enhancements
- Open an issue with the "enhancement" label
- Describe the use case and expected behavior
- Consider if it fits the template's scope (see [README](README.md#-whats-not-included-out-of-scope))

## Development

### Setup
1. Fork and clone the repository
2. Install .NET 8 SDK
3. Run `dotnet restore`
4. Run `dotnet test` to verify everything works

### Making Changes
1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make your changes
3. Ensure all tests pass: `dotnet test`
4. Update documentation if needed
5. Commit with clear messages

### Code Style
- Follow existing code patterns
- Add tests for new functionality
- Ensure architecture tests pass

### Pull Request Process
1. Update README.md if your changes affect the documentation
2. Add tests for new features
3. Ensure all tests pass
4. Submit a pull request with a clear description

## What We're Looking For

### Good Contributions
- Bug fixes
- Documentation improvements
- Test coverage
- Code quality improvements
- New features that fit the template's scope

### Out of Scope
- Authentication/Authorization (use external solutions)
- Message Bus implementations
- Production infrastructure configs
- Features that over-complicate the template

## Questions?

If you have questions about contributing, please open an issue and we'll be happy to help!

## License

By contributing, you agree that your contributions will be licensed under the same MIT License that covers the project.
