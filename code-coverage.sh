#!/bin/bash

# Remove existing results
rm -rf TestResults
rm -rf coverage-report

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory:./TestResults

# Find the coverage file
COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" | head -1)

# Generate the report
reportgenerator \
    -reports:$COVERAGE_FILE \
    -targetdir:coverage-report \
    -reporttypes:Html

echo "Code coverage report generated at coverage-report/index.html" 