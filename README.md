# 💬 SimpChat

<div align="center">

### *Real-time chat made simple, powerful, and beautiful*

A modern full-stack chat application with instant messaging, multiple rooms, and secure authentication

[![Backend](https://img.shields.io/badge/Backend-ASP.NET_Core-512BD4?style=for-the-badge&logo=.net)](https://github.com/MrJorjinio/simpchat.backend)
[![Frontend](https://img.shields.io/badge/Frontend-React-61DAFB?style=for-the-badge&logo=react)](https://github.com/MrJorjinio/simpchat.frontend)
[![Database](https://img.shields.io/badge/Database-PostgreSQL-4169E1?style=for-the-badge&logo=postgresql)](https://www.postgresql.org/)
[![Real-time](https://img.shields.io/badge/Real--time-SignalR-00ADD8?style=for-the-badge)](https://dotnet.microsoft.com/apps/aspnet/signalr)

</div>

---

## ✨ What is SimpChat?

SimpChat is a **lightning-fast real-time chat application** that brings conversations to life. Built with modern web technologies, it delivers instant messaging with the smoothness you'd expect from professional chat platforms. Whether you're looking to understand full-stack development, study real-time architectures, or just want a clean chat solution, SimpChat has you covered.

This isn't just another chat app—it's a demonstration of **production-ready architecture**, clean code practices, and seamless real-time communication.

---

## 🚀 Key Features

<table>
<tr>
<td width="50%">

### 🔐 **Secure Authentication**
- JWT token-based security
- Encrypted password storage
- Session management
- User registration & login

</td>
<td width="50%">

### ⚡ **Real-Time Messaging**
- Instant message delivery
- WebSocket-powered communication
- Zero-delay updates
- SignalR integration

</td>
</tr>
<tr>
<td width="50%">

### 💬 **Smart Chat Rooms**
- Multiple room support
- Private messaging capability
- User presence tracking
- Room-based conversations

</td>
<td width="50%">

### 💾 **Persistent Storage**
- PostgreSQL database
- Complete message history
- User data management
- Scalable architecture

</td>
</tr>
</table>

---

## 🛠️ Technology Stack

<div align="center">

| Layer | Technologies |
|-------|-------------|
| **Frontend** | React • TypeScript • TailwindCSS • Axios • SignalR Client |
| **Backend** | ASP.NET Core • Entity Framework Core • SignalR • JWT |
| **Database** | PostgreSQL |
| **DevOps** | Docker • Docker Compose • Git |

</div>

---

## 🎯 Getting Started

### Prerequisites

Before you begin, ensure you have installed:
- **Docker** and **Docker Compose** *(recommended)*, OR
- **.NET 6+** and **Node.js 16+** *(for manual setup)*
- **PostgreSQL** *(if running without Docker)*

### 🐳 Quick Start with Docker (Recommended)

The easiest way to get SimpChat running is with Docker Compose. Everything is configured to work together seamlessly:

```bash
# Clone both repositories
git clone https://github.com/MrJorjinio/simpchat.frontend
git clone https://github.com/MrJorjinio/simpchat.backend

# Navigate to your project root and run
docker-compose up --build
```

**That's it!** 🎉 Your application will be running at:
- **Frontend:** http://localhost:5173
- **Backend API:** http://localhost:5000
- **Database:** localhost:5432

### 💻 Manual Setup (Without Docker)

#### Backend Setup

```bash
# Navigate to backend directory
cd simpchat.backend

# Restore dependencies
dotnet restore

# Update database connection string in appsettings.json
# Run migrations
dotnet ef database update

# Start the backend
dotnet run
```

Backend will start at `http://localhost:5000`

#### Frontend Setup

```bash
# Navigate to frontend directory
cd simpchat.frontend

# Install dependencies
npm install

# Configure API endpoint in environment file
# Start the development server
npm start
```

Frontend will start at `http://localhost:5173`

---

## 📚 A Learning Journey

This project is a **real story of growth and learning**. Unlike polished, production-ready codebases, SimpChat's commit history tells an authentic story of development:

### 🌱 What Makes This Special

- **Early commits** show my initial attempts without deep architectural knowledge
- **Progressive improvements** demonstrate how I learned clean architecture principles
- **Refactoring evolution** reveals the journey from basic functionality to structured code
- **Problem-solving moments** captured in commit messages and code changes

### 🎓 Perfect for Learning

If you're learning full-stack development, exploring this repository chronologically offers insights into:

- How a developer evolves their understanding of software architecture
- Common mistakes and how to fix them
- The iterative process of building real applications
- Practical application of design patterns as knowledge grows

**💡 Tip:** Check out the commit history from oldest to newest to see the complete transformation from beginner patterns to more sophisticated approaches!

---

## 📖 How to Use

1. **Register an Account**
   - Open the application in your browser
   - Click "Sign Up" and create your account
   - Your password is securely hashed and stored

2. **Log In**
   - Use your credentials to log in
   - Receive a JWT token for authenticated sessions

3. **Join a Chat Room**
   - Browse available rooms or create a new one
   - Click to join and start chatting instantly

4. **Send Messages**
   - Type your message and hit send
   - Watch messages appear in real-time for all users
   - Enjoy seamless, lag-free conversations

5. **Private Messaging** *(if enabled)*
   - Select a user to start a private conversation
   - Keep your chats personal and secure

---

## 🏗️ Architecture Highlights

SimpChat follows **clean architecture principles** with clear separation of concerns:

- **Controllers** handle HTTP requests and real-time connections
- **Services** contain business logic and orchestration
- **Repositories** manage data access and persistence
- **Models** define data structures and entities

The application uses **Code First** approach with Entity Framework Core, making database management intuitive and version-controlled.

---

## 🌟 Why SimpChat?

- **Authentic Learning Journey:** Watch real development evolution through commit history
- **Portfolio Ready:** Shows growth mindset and continuous improvement
- **Educational Resource:** Perfect for learners to see realistic development progression
- **Transparent Process:** From messy first attempts to clean architecture
- **Learning Resource:** Study full-stack patterns and real-time communication
- **Extensible:** Built to be enhanced with new features
- **Production Practices:** Security, scalability, and maintainability achieved through iteration
- **Modern Stack:** Uses current industry-standard technologies

---

## 📦 Repository Links

<div align="center">

### [🎨 Frontend Repository](https://github.com/MrJorjinio/simpchat.frontend) | [⚙️ Backend Repository](https://github.com/MrJorjinio/simpchat.backend)

</div>

---

## 🤝 Contributing

Found a bug? Have a feature idea? Contributions are welcome! Feel free to open issues or submit pull requests on either repository.

---

## 📄 License

This project is open source and available for learning and portfolio purposes.

---

<div align="center">

### Built with 💙 by a solo developer on a learning journey

**SimpChat** - *Where real-time conversations come alive*

*"The best code isn't written perfectly the first time—it's refined through learning and iteration."*

</div>
