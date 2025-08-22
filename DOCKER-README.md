# 🐳 PNBP Application - Docker Setup

Panduan lengkap untuk menjalankan aplikasi PNBP menggunakan Docker dengan Oracle Database 19c.

## 📋 Prerequisites

- Docker Engine 20.10+ 
- Docker Compose 1.29+
- Minimal 8GB RAM (untuk Oracle DB)
- Minimal 20GB disk space

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Nginx Proxy   │    │  PNBP App       │    │  Oracle 19c     │
│   (Port 80)     │◄──►│  (Port 9000)    │◄──►│  (Port 1521)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌─────────────────┐
                       │  Redis Cache    │
                       │  (Port 6379)    │
                       └─────────────────┘
```

## 🚀 Quick Start

### 1. Build Application Image
```bash
./docker-build.sh
```

### 2. Start All Services
```bash
./docker-run.sh up -d
```

### 3. Check Status
```bash
./docker-run.sh status
```

### 4. Access Application
- **Web Application**: http://localhost
- **Oracle Enterprise Manager**: https://localhost:5500/em
- **Direct App Port**: http://localhost:9000

## 📦 Services

### PNBP Application
- **Image**: `pnbp-app:latest`
- **Ports**: 80 (Nginx), 9000 (App)
- **Runtime**: Mono + XSP4
- **Oracle Client**: 19.19.0.0

### Oracle Database 19c
- **Image**: `container-registry.oracle.com/database/enterprise:19.3.0.0`
- **Ports**: 1521 (DB), 5500 (EM)
- **SID**: ORCLCDB
- **PDB**: ORCLPDB1
- **Users**: pnbp/pnbp, kkpwebdev/kkpwebdev

### Redis Cache
- **Image**: `redis:7-alpine`
- **Port**: 6379
- **Password**: RedisPassword123

## 🛠️ Available Commands

### Build Commands
```bash
# Build application image
./docker-build.sh [tag]

# Build with Docker Compose
./docker-run.sh build
```

### Run Commands
```bash
# Start all services
./docker-run.sh up -d

# Stop all services
./docker-run.sh down

# Restart services
./docker-run.sh restart

# View logs
./docker-run.sh logs -f

# Check status
./docker-run.sh status
```

### Maintenance Commands
```bash
# Open application shell
./docker-run.sh shell

# Open Oracle SQL*Plus
./docker-run.sh db-shell

# Clean up everything
./docker-run.sh clean
```

## 🔧 Configuration

### Environment Variables

#### Application (.env file)
```env
# Application
ASPNET_ENVIRONMENT=Production
TZ=Asia/Jakarta

# Oracle Database
ORACLE_CONNECTION_STRING=User Id=pnbp;Password=pnbp;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle-db)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCLPDB1)));

# Redis
REDIS_CONNECTION_STRING=redis-cache:6379,password=RedisPassword123
```

#### Oracle Database
```env
ORACLE_SID=ORCLCDB
ORACLE_PDB=ORCLPDB1  
ORACLE_PWD=OraclePassword123
INIT_SGA_SIZE=2048
INIT_PGA_SIZE=1024
ORACLE_CHARACTERSET=AL32UTF8
```

### Volume Mounts
- `oracle-data:/opt/oracle/oradata` - Database files
- `oracle-backup:/opt/oracle/backup` - Backup location  
- `redis-data:/data` - Redis persistence
- `./Uploads:/app/Uploads` - Application uploads
- `./logs:/app/logs` - Application logs

## 🗄️ Database Setup

### Initial Setup
Database akan otomatis diinisialisasi dengan:
1. User `pnbp` dengan password `pnbp`
2. User `kkpwebdev` dengan password `kkpwebdev`
3. Privileges yang diperlukan untuk aplikasi

### Manual Database Access
```bash
# Via Docker
./docker-run.sh db-shell

# Via SQL*Plus (dalam container)
docker-compose exec oracle-db sqlplus pnbp/pnbp@//localhost:1521/ORCLPDB1

# Via SQL Developer (external)
Host: localhost
Port: 1521
Service: ORCLPDB1
User: pnbp
Password: pnbp
```

## 📊 Monitoring & Logs

### View Logs
```bash
# All services
./docker-run.sh logs -f

# Specific service
docker-compose logs -f pnbp-app
docker-compose logs -f oracle-db
docker-compose logs -f redis-cache
```

### Health Checks
- **Application**: http://localhost:9000/health
- **Oracle**: Built-in health check via SQL query
- **Redis**: Built-in health check via ping

### Performance Monitoring
```bash
# Container stats
docker stats

# Service status
./docker-run.sh status

# Disk usage
docker system df
```

## 🔒 Security

### Default Credentials
⚠️ **IMPORTANT**: Change these in production!

- **Oracle SYS**: `sys/OraclePassword123`
- **Oracle PNBP**: `pnbp/pnbp`
- **Redis**: `RedisPassword123`

### Network Security
- Services communicate via internal Docker network
- Only necessary ports exposed to host
- Oracle EM accessible via HTTPS

## 🚢 Production Deployment

### 1. Environment Setup
```bash
# Create production environment file
cp .env.example .env.production
```

### 2. SSL/TLS Configuration
```bash
# Add SSL certificates to nginx config
# Update docker/nginx.conf for HTTPS
```

### 3. Resource Limits
```yaml
# In docker-compose.yml
services:
  pnbp-app:
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'
  oracle-db:
    deploy:
      resources:
        limits:
          memory: 4G
          cpus: '2.0'
```

### 4. Backup Strategy
```bash
# Database backup
docker-compose exec oracle-db rman target /
RMAN> backup database;

# Volume backup
docker run --rm -v pnbp_oracle-data:/data -v $(pwd):/backup alpine tar czf /backup/oracle-backup.tar.gz /data
```

## 🐛 Troubleshooting

### Common Issues

#### Oracle Database Won't Start
```bash
# Check logs
docker-compose logs oracle-db

# Common fixes
docker-compose down
docker volume rm pnbp_oracle-data
docker-compose up -d oracle-db
```

#### Application Can't Connect to Oracle
```bash
# Check network connectivity
docker-compose exec pnbp-app ping oracle-db

# Check Oracle listener
docker-compose exec oracle-db lsnrctl status
```

#### Build Failures
```bash
# Clear Docker cache
docker system prune -a

# Rebuild from scratch
./docker-build.sh --no-cache
```

### Debug Mode
```bash
# Run application in debug mode
docker-compose -f docker-compose.yml -f docker-compose.debug.yml up
```

## 📈 Scaling

### Horizontal Scaling
```bash
# Scale application instances
docker-compose up -d --scale pnbp-app=3

# Use load balancer profile
docker-compose --profile loadbalancer up -d
```

### Vertical Scaling
```yaml
# Increase resources in docker-compose.yml
services:
  oracle-db:
    environment:
      - INIT_SGA_SIZE=4096
      - INIT_PGA_SIZE=2048
```

## 📝 Development

### Local Development
```bash
# Start only database
docker-compose up -d oracle-db redis-cache

# Run application locally
xsp4 --port=9000 --root=. --applications=/:. --nonstop
```

### Code Changes
```bash
# Rebuild after code changes
./docker-build.sh dev
./docker-run.sh restart pnbp-app
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Update Docker configs if needed
4. Test with `./docker-run.sh build && ./docker-run.sh up -d`
5. Submit pull request

## 📞 Support

Untuk bantuan lebih lanjut:
- Check logs: `./docker-run.sh logs -f`
- Open issue di repository
- Contact: admin@pnbp.go.id

---

**Happy Dockerizing! 🐳**