#!/bin/bash

# PNBP Application Docker Run Script
# Usage: ./docker-run.sh [command] [options]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
COMMAND=${1:-up}
COMPOSE_FILE="docker-compose.yml"
PROJECT_NAME="pnbp"

echo -e "${BLUE}🐳 PNBP Application Docker Runner${NC}"
echo -e "${BLUE}==================================${NC}"

# Check if Docker and Docker Compose are available
if ! command -v docker >/dev/null 2>&1; then
    echo -e "${RED}❌ Error: Docker is not installed${NC}"
    exit 1
fi

if ! command -v docker-compose >/dev/null 2>&1; then
    echo -e "${RED}❌ Error: Docker Compose is not installed${NC}"
    exit 1
fi

# Check if docker-compose.yml exists
if [ ! -f "$COMPOSE_FILE" ]; then
    echo -e "${RED}❌ Error: $COMPOSE_FILE not found${NC}"
    exit 1
fi

# Function to show help
show_help() {
    echo -e "${YELLOW}Available commands:${NC}"
    echo -e "  ${GREEN}up${NC}          - Start all services (default)"
    echo -e "  ${GREEN}down${NC}        - Stop and remove all services"
    echo -e "  ${GREEN}restart${NC}     - Restart all services"
    echo -e "  ${GREEN}build${NC}       - Build application image"
    echo -e "  ${GREEN}logs${NC}        - Show logs"
    echo -e "  ${GREEN}status${NC}      - Show service status"
    echo -e "  ${GREEN}shell${NC}       - Open shell in application container"
    echo -e "  ${GREEN}db-shell${NC}    - Open Oracle SQL*Plus shell"
    echo -e "  ${GREEN}clean${NC}       - Clean up containers, networks, and volumes"
    echo -e "  ${GREEN}help${NC}        - Show this help"
    echo ""
    echo -e "${YELLOW}Examples:${NC}"
    echo -e "  ./docker-run.sh up -d          # Start in detached mode"
    echo -e "  ./docker-run.sh logs -f        # Follow logs"
    echo -e "  ./docker-run.sh shell           # Open app shell"
    echo -e "  ./docker-run.sh db-shell        # Open Oracle shell"
}

# Function to wait for Oracle DB
wait_for_oracle() {
    echo -e "${YELLOW}⏳ Waiting for Oracle Database to be ready...${NC}"
    timeout=300
    while [ $timeout -gt 0 ]; do
        if docker-compose exec -T oracle-db sqlplus -L sys/OraclePassword123@//localhost:1521/ORCLCDB as sysdba <<< "SELECT 'OK' FROM dual;" >/dev/null 2>&1; then
            echo -e "${GREEN}✅ Oracle Database is ready!${NC}"
            return 0
        fi
        echo -e "${YELLOW}⏳ Still waiting... (${timeout}s remaining)${NC}"
        sleep 10
        timeout=$((timeout-10))
    done
    echo -e "${RED}❌ Timeout waiting for Oracle Database${NC}"
    return 1
}

# Execute command
case $COMMAND in
    "up")
        echo -e "${YELLOW}🚀 Starting PNBP application...${NC}"
        docker-compose -p "$PROJECT_NAME" up "${@:2}"
        ;;
    
    "down")
        echo -e "${YELLOW}🛑 Stopping PNBP application...${NC}"
        docker-compose -p "$PROJECT_NAME" down "${@:2}"
        ;;
    
    "restart")
        echo -e "${YELLOW}🔄 Restarting PNBP application...${NC}"
        docker-compose -p "$PROJECT_NAME" restart "${@:2}"
        ;;
    
    "build")
        echo -e "${YELLOW}🔨 Building PNBP application...${NC}"
        docker-compose -p "$PROJECT_NAME" build "${@:2}"
        ;;
    
    "logs")
        echo -e "${YELLOW}📋 Showing logs...${NC}"
        docker-compose -p "$PROJECT_NAME" logs "${@:2}"
        ;;
    
    "status")
        echo -e "${YELLOW}📊 Service Status:${NC}"
        docker-compose -p "$PROJECT_NAME" ps
        echo ""
        echo -e "${YELLOW}🌐 Access URLs:${NC}"
        echo -e "  Application: ${GREEN}http://localhost${NC}"
        echo -e "  Oracle EM:   ${GREEN}https://localhost:5500/em${NC}"
        echo -e "  Redis CLI:   ${GREEN}redis-cli -h localhost -p 6379 -a RedisPassword123${NC}"
        ;;
    
    "shell")
        echo -e "${YELLOW}🐚 Opening application shell...${NC}"
        docker-compose -p "$PROJECT_NAME" exec pnbp-app /bin/bash
        ;;
    
    "db-shell")
        echo -e "${YELLOW}🗄️  Opening Oracle SQL*Plus shell...${NC}"
        docker-compose -p "$PROJECT_NAME" exec oracle-db sqlplus pnbp/pnbp@//localhost:1521/ORCLPDB1
        ;;
    
    "clean")
        echo -e "${YELLOW}🧹 Cleaning up Docker resources...${NC}"
        docker-compose -p "$PROJECT_NAME" down -v --remove-orphans
        docker system prune -f
        echo -e "${GREEN}✅ Cleanup completed${NC}"
        ;;
    
    "help"|"-h"|"--help")
        show_help
        ;;
    
    *)
        echo -e "${RED}❌ Unknown command: $COMMAND${NC}"
        echo ""
        show_help
        exit 1
        ;;
esac