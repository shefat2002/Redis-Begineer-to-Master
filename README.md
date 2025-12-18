# Redis Learning Journey: Beginner to Master

Complete hands-on Redis learning with ASP.NET Core and SignalR.

## 📚 Overview

This repository contains a comprehensive learning path for mastering Redis, from fundamentals to advanced patterns with ASP.NET Core and SignalR integration.

## 🗺️ Learning Plan

See [REDIS-LEARNING-PLAN.md](REDIS-LEARNING-PLAN.md) for the complete 12-week curriculum covering:

- **Phase 1**: Redis Fundamentals (Week 1-2)
- **Phase 2**: Advanced Data Structures (Week 3-4)
- **Phase 3**: Caching Strategies (Week 5-6)
- **Phase 4**: SignalR with Redis Backplane (Week 7-8)
- **Phase 5**: Advanced SignalR + Redis Patterns (Week 9-10)
- **Phase 6**: Production-Ready Patterns (Week 11-12)

## 🚀 Projects

### ✅ Project 1: Simple Key-Value Store API (COMPLETED)

A RESTful API demonstrating fundamental Redis operations.

**Status**: ✅ Implemented and documented

**Quick Start**:
```bash
cd Project1.KeyValueStoreAPI
docker-compose up -d
dotnet run
```

**Documentation**:
- 📋 [Quick Summary](PROJECT1-SUMMARY.md) - Overview and quick start
- 📖 [Full Implementation Guide](PROJECT1-IMPLEMENTATION.md) - 21KB comprehensive guide
- 📝 [Project README](Project1.KeyValueStoreAPI/README.md) - In-project documentation

**What You'll Learn**:
- Redis string operations (SET, GET, DEL)
- ConnectionMultiplexer pattern
- TTL management
- JSON serialization
- Service layer architecture

**Tech Stack**: .NET 10, StackExchange.Redis 2.8.16, Redis 7

### ✅ Project 2: Real-Time Leaderboard System (COMPLETED)

A high-performance gaming leaderboard API using Redis Sorted Sets.

**Status**: ✅ Implemented and documented

**Quick Start**:
```bash
cd Project2.LeaderboardAPI
./run-leaderboard.sh
# Or manually:
docker run -d -p 6379:6379 redis:7-alpine
dotnet run
```

**Documentation**:
- 📋 [Quick Summary](PROJECT2-LEADERBOARD-SUMMARY.md) - Overview and architecture
- 📖 [Implementation Guide](PROJECT2-LEADERBOARD-IMPLEMENTATION.md) - Step-by-step tutorial
- 📝 [Project README](Project2.LeaderboardAPI/README.md) - API documentation
- 🧪 [Test Script](Project2.LeaderboardAPI/test-api.sh) - Automated testing
- 📊 [Performance Tests](Project2.TestConsole/Program.cs) - Load testing tool

**What You'll Learn**:
- Redis Sorted Sets (ZADD, ZRANGE, ZRANK, ZSCORE)
- Redis Hashes for metadata
- Atomic transactions (MULTI/EXEC)
- Ranking algorithms
- Performance optimization
- Load testing techniques

**Features**:
- Submit player scores with automatic ranking
- Get top N players (O(log N + N) complexity)
- Real-time rank calculation (O(log N) lookup)
- Player statistics tracking
- Range queries for leaderboard pagination
- Performance testing with 10,000+ operations

**Tech Stack**: .NET 10, StackExchange.Redis 2.8.16, Redis 7 Sorted Sets & Hashes

### 🔜 Project 3: E-Commerce Caching (Coming Next)

Gaming leaderboard with Sorted Sets - See REDIS-LEARNING-PLAN.md

### 🔜 Project 3: E-Commerce Product Catalog

Multi-level caching with cache invalidation - See REDIS-LEARNING-PLAN.md

### 🔜 Project 4: Real-Time Chat Application

SignalR with Redis backplane for scale-out - See REDIS-LEARNING-PLAN.md

### 🔜 Project 5: Collaborative Dashboard

Advanced patterns with presence and locking - See REDIS-LEARNING-PLAN.md

### 🔜 Project 6: Production Microservices

Enterprise-grade patterns and monitoring - See REDIS-LEARNING-PLAN.md

## 🛠️ Prerequisites

- **.NET 10 SDK**: https://dotnet.microsoft.com/download
- **Docker Desktop**: https://www.docker.com/products/docker-desktop
- **Redis CLI Tools** (optional): `brew install redis` (macOS) or `choco install redis` (Windows)

## 📦 Repository Structure

```
Redis-Begineer-to-Master/
├── REDIS-LEARNING-PLAN.md          # Complete 12-week curriculum
├── PROJECT1-SUMMARY.md             # Project 1 quick reference
├── PROJECT1-IMPLEMENTATION.md      # Project 1 deep dive guide
├── README.md                       # This file
│
├── Project1.KeyValueStoreAPI/      # ✅ Completed
│   ├── Controllers/                # API endpoints
│   ├── Models/                     # Data models
│   ├── Services/                   # Redis service layer
│   ├── docker-compose.yml          # Redis setup
│   ├── test-api.sh                 # Automated tests
│   └── README.md                   # Quick start
│
├── Project2.LeaderboardSystem/     # 🔜 Coming soon
├── Project3.ProductCatalog/        # 🔜 Coming soon
├── Project4.ChatApplication/       # 🔜 Coming soon
├── Project5.CollaborativeDashboard/# 🔜 Coming soon
└── Project6.Microservices/         # 🔜 Coming soon
```

## 🎯 Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd Redis-Begineer-to-Master
```

### 2. Read the Learning Plan
Start with [REDIS-LEARNING-PLAN.md](REDIS-LEARNING-PLAN.md) to understand the full curriculum.

### 3. Start Project 1
```bash
cd Project1.KeyValueStoreAPI
docker-compose up -d    # Start Redis
dotnet run              # Run the API
./test-api.sh          # Test all endpoints
```

### 4. Study the Documentation
- Read [PROJECT1-SUMMARY.md](PROJECT1-SUMMARY.md) for overview
- Deep dive into [PROJECT1-IMPLEMENTATION.md](PROJECT1-IMPLEMENTATION.md)
- Experiment with the code

## 📖 Key Learning Resources

### Documentation in This Repo
- **REDIS-LEARNING-PLAN.md**: 12-week structured curriculum
- **PROJECT1-IMPLEMENTATION.md**: 21KB guide with code explanations
- **PROJECT1-SUMMARY.md**: Quick reference and highlights

### External Resources
- [Redis Official Docs](https://redis.io/docs/)
- [StackExchange.Redis Docs](https://stackexchange.github.io/StackExchange.Redis/)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [SignalR Docs](https://docs.microsoft.com/aspnet/core/signalr/)

## 🎓 Learning Approach

This curriculum follows a **hands-on, project-based** approach:

1. **Read**: Study the theory and concepts
2. **Build**: Implement the project step-by-step
3. **Test**: Run automated and manual tests
4. **Experiment**: Modify code and observe behavior
5. **Debug**: Use Redis CLI and monitoring tools
6. **Document**: Take notes on learnings

## 📊 Progress Tracking

### Phase 1: Redis Fundamentals
- [x] Redis installed and running
- [x] Project 1 completed
- [x] Basic CRUD operations understood
- [x] ConnectionMultiplexer pattern learned
- [x] TTL management implemented
- [ ] All tests passing
- [ ] Redis CLI explored
- [ ] Redis Commander used

### Phases 2-6
See [REDIS-LEARNING-PLAN.md](REDIS-LEARNING-PLAN.md) for complete checklist.

## 💡 Tips for Success

1. **Work Through Projects in Order**: Each builds on previous knowledge
2. **Run the Code**: Reading is not enough - execute and experiment
3. **Use Redis Tools**: Monitor commands, explore data visually
4. **Take Notes**: Document your learnings and "aha!" moments
5. **Test Failures**: Simulate errors to understand resilience
6. **Join Communities**: Stack Overflow, Redis Discord, Reddit

## 🔧 Tools Included

- **Docker Compose**: Redis server + Redis Commander GUI
- **Test Scripts**: Automated API testing with bash/curl
- **Configuration**: Development and production configs
- **Logging**: Structured logging throughout
- **Error Handling**: Proper HTTP status codes and messages

## 📞 Getting Help

- **Issues**: Open an issue in this repository
- **Stack Overflow**: Use tags `[redis]`, `[stackexchange.redis]`, `[signalr]`
- **Redis Discord**: Join the official Redis community
- **Documentation**: Each project has comprehensive docs

## 🎉 What You'll Master

By completing this curriculum, you will:

✅ Understand Redis data structures deeply
✅ Build production-ready Redis applications
✅ Implement caching patterns effectively
✅ Scale SignalR with Redis backplane
✅ Design distributed systems with Redis
✅ Monitor and debug Redis applications
✅ Handle failures gracefully
✅ Optimize for performance

## 🚀 Next Steps

1. ✅ **Complete Project 1** - You've got the code and documentation
2. 📖 **Read Full Documentation** - [PROJECT1-IMPLEMENTATION.md](PROJECT1-IMPLEMENTATION.md)
3. 🧪 **Run All Tests** - Use test-api.sh and Redis CLI
4. 💡 **Try Exercises** - See learning exercises in documentation
5. ⏭️ **Move to Project 2** - Leaderboard with Sorted Sets

## 📄 License

This is an educational project for learning Redis with ASP.NET Core.

## 🙏 Acknowledgments

Built with:
- .NET 10
- StackExchange.Redis 2.8.16
- Redis 7
- ASP.NET Core
- Docker

---

**Ready to master Redis?** Start with Project 1 and work your way through! 🚀

*Last Updated: December 2024*
