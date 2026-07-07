# JobBoardApi - Redis-Powered Job Marketplace

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Redis](https://img.shields.io/badge/Redis-7.4-DC382D?style=flat-square&logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square)](./Dockerfile)

A **high-performance job marketplace API** built with .NET 10, showcasing advanced **Redis** patterns including caching, sorted sets for ranking, and real-time analytics. Perfect for learning production-grade Redis integration in .NET applications.

---

## Table of Contents

- [Features](#features)
- [Redis Architecture](#redis-architecture)
- [Quick Start](#quick-start)
- [Installation Guide](#installation-guide)
- [API Endpoints](#api-endpoints)
- [Redis Tools & Monitoring](#redis-tools--monitoring)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

---

## Features

### Core Features
- 🔴 **Redis Caching** - Intelligent caching layer for job listings with TTL management
- 📊 **View Ranking System** - Real-time job popularity tracking using Redis Sorted Sets
- ⚡ **High Performance** - Sub-millisecond response times with Redis backend
- 🔗 **Connection Pooling** - Singleton pattern for optimal Redis resource usage
- 📚 **OpenAPI Documentation** - Native .NET 10 OpenAPI with Scalar UI
- 🐳 **Docker Ready** - Complete Docker Compose setup with Redis Commander

### Redis Patterns Demonstrated
| Pattern | Use Case | Implementation |
|---------|----------|-----------------|
| **String Cache** | Job details storage | `StringGet` / `StringSet` with TTL |
| **Sorted Sets** | Job view ranking | `ZIncrBy` / `ZRevRange` |
| **Persistence** | Data durability | AOF (Append-Only File) |
| **Connection Pooling** | Resource optimization | StackExchange.Redis singleton |

---

## Redis Architecture

### Data Model

```
Redis Database (db: 0)
│
├── Strings (Cache Layer)
│   ├── jobs:cache:{jobId}  → JSON serialized JobOffer object (TTL: 5 min)
│   └── Example: jobs:cache:11111111-1111-1111-1111-111111111111
│
└── Sorted Sets (Ranking Layer)
    └── jobs:views:ranking  → {jobId: viewCount} (member: score)
        ├── "11111111-1111-1111-1111-111111111111": 42
        ├── "22222222-2222-2222-2222-222222222222": 28
        └── "33333333-3333-3333-3333-333333333333": 15
```

### Redis Commands Used

```redis
# Cache a job (String)
SET jobs:cache:{jobId} "{json}" EX 300

# Increment view count (Sorted Set)
ZINCRBY jobs:views:ranking 1 {jobId}

# Get top 10 most viewed jobs
ZREVRANGE jobs:views:ranking 0 9 WITHSCORES

# Delete from cache
DEL jobs:cache:{jobId}
```

### Benefits
✅ **Sub-millisecond latency** for cache hits  
✅ **Automatic expiration** with TTL  
✅ **Real-time ranking** without database queries  
✅ **Memory efficient** with compression support  
✅ **Persistence** via AOF for durability  

---

## Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac) or [Docker CLI](https://docs.docker.com/engine/install/) (Linux)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for local development)
- Git

### 30-Second Setup (Docker)
```bash
# Clone repository
git clone <repository-url>
cd JobBoardApi

# Start all services (API + Redis + Redis Commander)
docker-compose up -d

# Wait 10 seconds for services to initialize
sleep 10

# Test the API
curl http://localhost:8080/api/v1/jobs
```

**Success! 🎉**
- 🔗 API: http://localhost:5058/scalar/v1
- 🔴 Redis Commander: http://localhost:8081/
- 📊 Scalar API Docs: http://localhost:5058/docs/v1.json

---

## Installation Guide

### Option 1: Docker Compose (Recommended) 🐳

#### Step 1: Prerequisites Check
```powershell
# Windows PowerShell
docker --version
docker-compose --version
```

Expected output:
```
Docker version 24.0+
docker-compose version 2.20+
```

#### Step 2: Clone Repository
```bash
git clone <repository-url>
cd JobBoardApi
```

#### Step 3: Start Services
```bash
# Start all services in background
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (clean reset)
docker-compose down -v
```

#### Step 4: Verify Installation
```bash
# Check running containers
docker-compose ps

# Expected output:
# NAME                    STATUS
# jobboard-api           Up 2 minutes
# jobboard-redis         Up 2 minutes
# jobboard-redis-commander  Up 2 minutes

# Test API
http://localhost:8080/api/v1/jobs

```

---

### Option 2: Local Development (dotnet CLI) 💻

#### Step 1: Prerequisites
```powershell
# Windows PowerShell
dotnet --version  # Should be .NET 10.0+
redis-cli --version  # Optional, for local Redis testing
```

#### Step 2: Install Redis Locally

**Windows (via Chocolatey):**
```powershell
choco install redis-64 -y
# Start Redis service
redis-server
```

**macOS (via Homebrew):**
```bash
brew install redis
brew services start redis
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install redis-server
sudo systemctl start redis-server
```

**Using WSL on Windows:**
```bash
wsl
sudo apt-get update
sudo apt-get install redis-server
redis-server &
```

#### Step 3: Verify Redis Connection
```bash
redis-cli ping
# Response: PONG
```

#### Step 4: Build and Run API
```powershell
# Navigate to project directory
cd src/JobBoardApi

# Build project
dotnet build

# Run with .NET development server
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5058
```

#### Step 5: Test Endpoints
```bash
# Terminal 1: Run API
dotnet run

# Terminal 2: Test API
http://localhost:5058/scalar/v1

# You should see the sample job listings
```

---

### Option 3: Docker (Manual Build) 🔨

#### Step 1: Build Docker Image
```bash
# From project root
docker build -t jobboard-api:latest .

# Verify image
docker images | grep jobboard-api
```

#### Step 2: Start Redis Container
```bash
# Run Redis
docker run -d \
  --name jobboard-redis \
  -p 6379:6379 \
  redis:7.4-alpine redis-server --appendonly yes

# Verify Redis
docker exec jobboard-redis redis-cli ping
# Response: PONG
```

#### Step 3: Start API Container
```bash
# Run API (linked to Redis)
docker run -d \
  --name jobboard-api \
  -p 8080:8080 \
  --link jobboard-redis:redis \
  -e "Redis__Configuration=redis:6379,abortConnect=false" \
  jobboard-api:latest

# Check logs
docker logs -f jobboard-api
```

#### Step 4: Start Redis Commander
```bash
docker run -d \
  --name redis-commander \
  -p 8081:8081 \
  --link jobboard-redis:redis \
  -e "REDIS_HOSTS=local:redis:6379" \
  rediscommander/redis-commander:latest
```

---

## API Endpoints

### Base URL
- **Local**: `http://localhost:5058` (dotnet run)
- **Docker**: `http://localhost:8080` (docker-compose)

### Endpoints Reference

#### 1️⃣ Get All Jobs
```http
GET /api/v1/jobs
```

**Response:**
```json
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "title": "Senior .NET Developer",
    "company": "Tech Corp",
    "location": "Sao Paulo, BR",
    "description": "Work with .NET 10, Redis, and distributed architecture.",
    "salary": 18000,
    "postedAt": "2026-07-04T10:30:00Z"
  }
]
```

#### 2️⃣ Get Job by ID (with Caching)
```http
GET /api/v1/jobs/{id}
```

**Example:**
```bash
curl http://localhost:5058/api/v1/jobs/11111111-1111-1111-1111-111111111111
```

**Redis Operations:**
- ✅ Checks cache: `GET jobs:cache:{id}`
- ✅ If miss → fetches from repository
- ✅ Stores in cache: `SET jobs:cache:{id} {...} EX 300`
- ✅ Increments view count: `ZINCRBY jobs:views:ranking 1 {id}`

**Response:**
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "title": "Senior .NET Developer",
  "company": "Tech Corp",
  "location": "Sao Paulo, BR",
  "description": "Work with .NET 10, Redis, and distributed architecture.",
  "salary": 18000,
  "postedAt": "2026-07-04T10:30:00Z"
}
```

#### 3️⃣ Get Top Most-Viewed Jobs
```http
GET /api/v1/jobs/top?count=10
```

**Parameters:**
- `count` (optional): Number of results (default: 10, max: 100)

**Example:**
```bash
curl http://localhost:5058/api/v1/jobs/top?count=5
```

**Redis Operation:**
```redis
ZREVRANGE jobs:views:ranking 0 4 WITHSCORES
```

**Response:**
```json
[
  {
    "jobId": "11111111-1111-1111-1111-111111111111",
    "viewCount": 42
  },
  {
    "jobId": "22222222-2222-2222-2222-222222222222",
    "viewCount": 28
  },
  {
    "jobId": "33333333-3333-3333-3333-333333333333",
    "viewCount": 15
  }
]
```

---

## Redis Tools & Monitoring

### 1. Redis Commander (GUI) 🎨

**Access:** http://localhost:8081

**Features:**
- 🔍 Visual database browser
- ✏️ Real-time key editing
- 📊 Memory usage visualization
- 🎯 Pattern-based key filtering

**Usage:**
```
1. Navigate to http://localhost:8081
2. Select database (db: 0)
3. Expand "Sorted Sets" to see jobs:views:ranking
4. Expand "Strings" to see jobs:cache:* keys
5. Monitor changes in real-time
```

**View Rankings:**
```
Sorted Sets → jobs:views:ranking
Shows:
  Member: 11111111-1111-1111-1111-111111111111 → Score: 42
  Member: 22222222-2222-2222-2222-222222222222 → Score: 28
  Member: 33333333-3333-3333-3333-333333333333 → Score: 15
```

---

### 2. Redis CLI (Command Line)

#### Connection
```bash
# Docker
docker exec -it jobboard-redis redis-cli

# Local
redis-cli
```

#### Useful Commands

**Monitor Cache Activity:**
```redis
MONITOR
# Shows all commands in real-time as you call API endpoints
```

**View Job Rankings:**
```redis
ZREVRANGE jobs:views:ranking 0 -1 WITHSCORES
# Output:
# 1) "11111111-1111-1111-1111-111111111111"
# 2) "42"
# 3) "22222222-2222-2222-2222-222222222222"
# 4) "28"
```

**Check Cache Keys:**
```redis
KEYS jobs:cache:*
# Lists all cached job objects

GET jobs:cache:11111111-1111-1111-1111-111111111111
# Returns full JSON job object
```

**View Cache Statistics:**
```redis
INFO stats
# Shows:
# - total_commands_processed
# - total_connections_received
# - keyspace_hits
# - keyspace_misses
```

**Manual Cache Clear:**
```redis
DEL jobs:cache:11111111-1111-1111-1111-111111111111
# Removes single job from cache

FLUSHDB
# Clears entire database (use with caution!)
```

---

### 3. Redis Insights (Advanced Analytics) 📊

**For Production Environments:**

If using [Redis Cloud](https://redis.com/try-free/):
1. Connect to Redis Cloud instance
2. Use Insights in Redis Console
3. View metrics like:
   - Command latency
   - Memory usage trends
   - Top commands
   - Connected clients

---

### 4. Performance Testing

**Load Test with Wrk:**
```bash
# Install wrk
# Windows: choco install wrk
# macOS: brew install wrk

# Test cache hit rate
wrk -t4 -c100 -d30s http://localhost:5058/api/v1/jobs/11111111-1111-1111-1111-111111111111

# Expected results show >95% cache hits after first request
```

---

## Project Structure

```
JobBoardApi/
├── 📄 README.md                          # This file
├── 📄 Dockerfile                         # Multi-stage .NET build
├── 📄 docker-compose.yml                 # Complete stack orchestration
├── 📄 JobBoardApi.slnx                  # Solution file
│
├── src/
│   └── JobBoardApi/
│       ├── 📄 Program.cs                 # Dependency injection & pipeline setup
│       ├── 📄 appsettings.json          # Redis configuration
│       ├── 📄 appsettings.Development.json
│       ├── 📄 JobBoardApi.csproj        # NuGet dependencies
│       ├── 📄 JobBoardApi.http          # HTTP request examples
│       │
│       ├── Controllers/
│       │   └── JobsController.cs         # REST API endpoints
│       │                                 #   GET /api/jobs
│       │                                 #   GET /api/jobs/{id}
│       │                                 #   GET /api/jobs/top
│       │
│       ├── Models/
│       │   └── JobOffer.cs               # Data model for job listings
│       │
│       ├── Services/
│       │   ├── IRedisService.cs          # Redis abstraction interface
│       │   └── RedisService.cs           # Redis implementation
│       │                                 #   - StringGet/StringSet (caching)
│       │                                 #   - ZIncrBy (view tracking)
│       │                                 #   - ZRevRange (ranking)
│       │
│       └── Properties/
│           └── launchSettings.json       # Debug profiles
│
└── Configuration Files
    ├── 📄 .gitignore
    └── 📄 LICENSE (MIT)
```

---

## Configuration

### appsettings.json
```json
{
  "Redis": {
    "Configuration": "localhost:6379,abortConnect=false"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### Environment Variables (Docker)
```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
Redis__Configuration=redis:6379,abortConnect=false
```

### Connection String Formats

**Local Development:**
```
localhost:6379,abortConnect=false
```

**Docker Compose:**
```
redis:6379,abortConnect=false
```

**Redis Cloud (Example):**
```
redis-19999.c123.us-east-1-2.ec2.cloud.redislabs.com:19999,password=your-password,ssl=true,abortConnect=false
```

**Connection Options:**
- `abortConnect=false` - Don't throw on connection failure (retry enabled)
- `ssl=true` - Use TLS for cloud connections
- `password=xyz` - Authentication token

---

## Development

### Local Setup

```bash
# 1. Clone repository
git clone <repository-url>
cd JobBoardApi

# 2. Restore dependencies
dotnet restore

# 3. Start Redis (separate terminal)
# Windows (WSL)
wsl redis-server

# 4. Run API
dotnet run

# 5. Open Scalar UI
# Navigate to: http://localhost:5058/scalar/v1
```

### VS Code Debugging

**launch.json** already configured:
1. Press `F5` to start debugging
2. Set breakpoints in controllers/services
3. Step through Redis operations
4. Inspect cache state in Redis CLI

### Testing Redis Locally

```bash
# Terminal 1: Start Redis
redis-server

# Terminal 2: Start API
dotnet run

# Terminal 3: Monitor Redis
redis-cli MONITOR

# Terminal 4: Call API endpoints
curl http://localhost:5058/api/v1/jobs/11111111-1111-1111-1111-111111111111
curl http://localhost:5058/api/v1/jobs/top?count=3
```

### Making Code Changes

```bash
# Edit source files
code src/JobBoardApi/

# Rebuild
dotnet build

# Restart API (Ctrl+C, then dotnet run)

# Changes take effect immediately
```

---

## Troubleshooting

### Issue: "Redis connection refused"

**Symptoms:**
```
StackExchange.Redis.RedisConnectionException: Unable to connect to localhost:6379
```

**Solutions:**

1. **Check Redis is running:**
   ```bash
   # Docker
   docker-compose ps
   docker logs jobboard-redis

   # Local
   redis-cli ping
   ```

2. **Restart Redis:**
   ```bash
   # Docker
   docker-compose restart redis

   # Local
   redis-server  # Start in new terminal
   ```

3. **Check port conflicts:**
   ```bash
   # Windows
   netstat -ano | findstr :6379

   # macOS/Linux
   lsof -i :6379
   ```

---

### Issue: "Cache not working / null returns"

**Check:**
1. Redis is running: `docker-compose ps`
2. Verify connection in logs: `docker-compose logs api`
3. Check Redis state:
   ```redis
   KEYS *
   # Should show jobs:cache:* and jobs:views:ranking keys
   ```

4. Manually test cache:
   ```bash
   curl http://localhost:5058/api/v1/jobs/11111111-1111-1111-1111-111111111111
   # Call twice - second should be from cache
   ```

---

### Issue: "Docker image build fails"

**Solutions:**
```bash
# Clean build
docker-compose down -v
docker system prune -a

# Rebuild
docker-compose up -d --build

# View build logs
docker-compose logs api
```

---

### Issue: "Connection pool exhaustion"

**Symptoms:**
```
Timeout awaiting response (outbound=0, inbound=0, state=Connecting)
```

**Fix:**
- Uses singleton pattern (already optimized)
- Check for connection leaks in custom code
- Monitor with: `INFO clients`

---

### Health Check Commands

```bash
# Test API health
curl http://localhost:5058/api/v1/jobs

# Test Redis health
docker exec jobboard-redis redis-cli ping
# Response: PONG

# View all containers
docker-compose ps

# View service logs
docker-compose logs --tail=50
```

---

## Performance Optimization Tips

### Redis Tuning
```redis
# Set recommended settings
CONFIG SET maxmemory-policy allkeys-lru  # LRU eviction for cache
CONFIG SET tcp-backlog 511
CONFIG SET timeout 0

# Monitor performance
INFO stats
INFO clients
```

### Application Level
1. ✅ Keep cache TTL reasonable (5-15 minutes)
2. ✅ Use connection pooling (already configured)
3. ✅ Implement cache invalidation strategy
4. ✅ Monitor cache hit rate (target: >80%)

---

## Learning Resources

### Redis Documentation
- [Redis Commands](https://redis.io/commands/)
- [Redis Data Types](https://redis.io/docs/data-types/)
- [Sorted Sets Guide](https://redis.io/docs/data-types/sorted-sets/)

### .NET Integration
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [.NET 10 Documentation](https://learn.microsoft.com/dotnet/)
- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi)

### Docker & DevOps
- [Docker Compose Docs](https://docs.docker.com/compose/)
- [Redis Docker Image](https://hub.docker.com/_/redis)
- [Redis Commander](https://github.com/joeferner/redis-commander)

---

## Sample Workflow

### Complete Scenario: View Job Rankings

```bash
# 1. Start services
docker-compose up -d

# 2. Call same job multiple times (build views)
for i in {1..5}; do
  curl -s http://localhost:8080/api/v1/jobs/11111111-1111-1111-1111-111111111111
done

# 3. Check Redis cache
docker exec jobboard-redis redis-cli
# Inside Redis CLI:
KEYS jobs:cache:*
ZREVRANGE jobs:views:ranking 0 -1 WITHSCORES

# 4. View in Redis Commander
# Open http://localhost:8081
# Navigate: Sorted Sets → jobs:views:ranking

# 5. Get top jobs (should show updated ranking)
curl http://localhost:8080/api/v1/jobs/top?count=3

# 6. Stop services
docker-compose down
```

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Style
- Follow C# naming conventions
- Use async/await patterns
- Add XML documentation comments
- Include tests for new features

---

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## Next Steps

### For Beginners
1. ✅ Run with `docker-compose up`
2. ✅ Explore Redis Commander at http://localhost:8081
3. ✅ Call API endpoints and watch Redis data change
4. ✅ Read the `Program.cs` to understand DI setup

### For Advanced Users
1. 🔄 Implement cache invalidation strategies
2. 🔐 Add authentication/authorization
3. 📈 Build advanced analytics on view rankings
4. 🌍 Deploy to Kubernetes with persistent volumes
5. 🔗 Integrate with other services (message queues, databases)

---

## Support

For issues and questions:
- 📝 Check [Troubleshooting](#troubleshooting) section
- 🐛 Open an issue on GitHub
- 💬 Refer to [Redis Documentation](https://redis.io/docs/)
- 🔍 Check [StackExchange.Redis Wiki](https://stackexchange.github.io/StackExchange.Redis/)

---

