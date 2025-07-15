#!/bin/bash
# Script to run tests with code coverage locally

set -e

echo "🧪 Running tests with code coverage..."

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Navigate to src directory
cd "$(dirname "$0")/../src"

# Clean previous coverage reports
rm -rf ../coverage
rm -f **/*.cobertura.xml

# Run tests with coverage
echo -e "${YELLOW}Running tests...${NC}"
dotnet test --collect:"XPlat Code Coverage" --results-directory ../coverage

# Check if ReportGenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo -e "${YELLOW}Installing ReportGenerator...${NC}"
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generate HTML report
echo -e "${YELLOW}Generating coverage report...${NC}"
reportgenerator \
    -reports:"../coverage/**/coverage.cobertura.xml" \
    -targetdir:"../coverage/html" \
    -reporttypes:"Html;Cobertura;TextSummary"

# Display summary
echo -e "${GREEN}✅ Coverage report generated!${NC}"
cat ../coverage/html/Summary.txt 2>/dev/null || echo "Summary not available"

# Open report in browser
if [[ "$OSTYPE" == "darwin"* ]]; then
    open ../coverage/html/index.html
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    xdg-open ../coverage/html/index.html 2>/dev/null || echo "Please open coverage/html/index.html manually"
elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    start ../coverage/html/index.html
else
    echo "Please open coverage/html/index.html manually"
fi

echo -e "${GREEN}📊 Coverage report opened in browser${NC}"