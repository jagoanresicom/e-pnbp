#!/bin/bash

# PNBP Application Docker Build Script
# Usage: ./docker-build.sh [tag]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
TAG=${1:-latest}
IMAGE_NAME="pnbp-app"
BUILD_CONTEXT="."

echo -e "${BLUE}🐳 PNBP Application Docker Build${NC}"
echo -e "${BLUE}================================${NC}"

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo -e "${RED}❌ Error: Docker is not running${NC}"
    exit 1
fi

# Check if Dockerfile exists
if [ ! -f "Dockerfile" ]; then
    echo -e "${RED}❌ Error: Dockerfile not found${NC}"
    exit 1
fi

# Build info
echo -e "${YELLOW}📋 Build Information:${NC}"
echo -e "   Image Name: ${IMAGE_NAME}"
echo -e "   Tag: ${TAG}"
echo -e "   Context: ${BUILD_CONTEXT}"
echo ""

# Pre-build checks
echo -e "${YELLOW}🔍 Pre-build checks...${NC}"

# Check if packages directory exists
if [ ! -d "packages" ]; then
    echo -e "${YELLOW}⚠️  Warning: packages directory not found. Running NuGet restore...${NC}"
    if command -v mono >/dev/null 2>&1 && [ -f "nuget.exe" ]; then
        mono nuget.exe restore packages.config -PackagesDirectory packages
    else
        echo -e "${YELLOW}⚠️  NuGet restore skipped. Ensure packages are available.${NC}"
    fi
fi

# Check if solution file exists
if [ ! -f "Pnbp.sln" ]; then
    echo -e "${RED}❌ Error: Pnbp.sln not found${NC}"
    exit 1
fi

# Build Docker image
echo -e "${YELLOW}🔨 Building Docker image...${NC}"
echo ""

docker build \
    --tag "${IMAGE_NAME}:${TAG}" \
    --tag "${IMAGE_NAME}:latest" \
    --build-arg BUILD_DATE="$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
    --build-arg VCS_REF="$(git rev-parse --short HEAD 2>/dev/null || echo 'unknown')" \
    --build-arg VERSION="${TAG}" \
    "${BUILD_CONTEXT}"

# Check build result
if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ Build completed successfully!${NC}"
    echo ""
    echo -e "${YELLOW}📊 Image Information:${NC}"
    docker images "${IMAGE_NAME}:${TAG}" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
    echo ""
    echo -e "${YELLOW}🚀 Next Steps:${NC}"
    echo -e "   1. Run with Docker Compose: ${GREEN}docker-compose up -d${NC}"
    echo -e "   2. Run standalone: ${GREEN}docker run -p 80:80 ${IMAGE_NAME}:${TAG}${NC}"
    echo -e "   3. Push to registry: ${GREEN}docker push ${IMAGE_NAME}:${TAG}${NC}"
else
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi