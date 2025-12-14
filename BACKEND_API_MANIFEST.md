# Simpchat Backend API Documentation

**Project:** Simpchat Messaging Platform
**Backend Location:** `simpchat.backend`
**API Base URL:** `http://localhost:5000/api`
**Documentation Generated:** 2025-12-02

---

## Table of Contents

1. [Authentication](#authentication)
2. [Response Format](#response-format)
3. [Controllers & Endpoints](#controllers--endpoints)
   - [Auth Controller](#1-auth-controller)
   - [User Controller](#2-user-controller)
   - [Chat Controller](#3-chat-controller)
   - [Group Controller](#4-group-controller)
   - [Channel Controller](#5-channel-controller)
   - [Message Controller](#6-message-controller)
   - [Reaction Controller](#7-reaction-controller)
   - [Notification Controller](#8-notification-controller)
   - [Permission Controller](#9-permission-controller)
   - [OTP Controller](#10-otp-controller)
   - [Conversation Controller](#11-conversation-controller)
4. [Common Data Types](#common-data-types)
5. [Error Handling](#error-handling)

---

## Authentication

### Bearer Token
Most endpoints require JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your_jwt_token>
```

### Obtaining a Token
**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "credential": "username_or_email@example.com",
  "password": "userPassword123"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "error": null
}
```

### Token Claims
The JWT token contains the following claims:
- `nameid`: User ID (GUID)
- `unique_name`: Username
- `email`: User email
- `role`: User role (Admin, User, etc.)

---

## Response Format

All API responses follow this standard format:

```json
{
  "success": true|false,
  "statusCode": 200|400|401|403|404|500,
  "data": null|object|array,
  "error": null|"error message"
}
```

### Response Properties

| Property | Type | Description |
|----------|------|-------------|
| `success` | boolean | Indicates if the request was successful |
| `statusCode` | int | HTTP status code |
| `data` | any | Response payload (varies by endpoint) |
| `error` | string | Error message (null if successful) |

### Example Success Response
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com"
  },
  "error": null
}
```

### Example Error Response
```json
{
  "success": false,
  "statusCode": 401,
  "data": null,
  "error": "Invalid credentials"
}
```

---

## Controllers & Endpoints

### 1. Auth Controller

**Base Route:** `POST /api/auth`

Purpose: Handle user authentication, registration, and password management.

#### 1.1 Register User

**Endpoint:** `POST /api/auth/register`

**Authorization:** None (Public)

**Purpose:** Create a new user account in the system.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| email | string | Body | Yes | User email address (must be unique) |
| username | string | Body | Yes | Username (must be unique) |
| password | string | Body | Yes | User password (min 6 characters) |
| otpCode | string | Body | Yes | OTP code from email verification |

**Request Body:**
```json
{
  "email": "user@example.com",
  "username": "john_doe",
  "password": "SecurePassword123",
  "otpCode": "123456"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "error": null
}
```

**Status Codes:**
- `200` - Registration successful, JWT token returned
- `400` - Invalid input or user already exists
- `500` - Server error

---

#### 1.2 Login User

**Endpoint:** `POST /api/auth/login`

**Authorization:** None (Public)

**Purpose:** Authenticate user with credentials and receive JWT token.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| credential | string | Body | Yes | Username or email address |
| password | string | Body | Yes | User password |

**Request Body:**
```json
{
  "credential": "john_doe",
  "password": "SecurePassword123"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "error": null
}
```

**Status Codes:**
- `200` - Login successful
- `400` - Invalid credentials
- `404` - User not found
- `500` - Server error

---

#### 1.3 Update Password

**Endpoint:** `PUT /api/auth/update-password`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Change password for authenticated user.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| oldPassword | string | Body | Yes | Current password |
| newPassword | string | Body | Yes | New password (min 6 characters) |

**Request Body:**
```json
{
  "oldPassword": "OldPassword123",
  "newPassword": "NewPassword456"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Password updated successfully
- `400` - Invalid old password
- `401` - Unauthorized
- `500` - Server error

---

#### 1.4 Forgot Password

**Endpoint:** `PUT /api/auth/forgot-password`

**Authorization:** Not Required (Public)

**Purpose:** Reset password using OTP verification code.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| email | string | Body | Yes | User email address |
| otpCode | string | Body | Yes | OTP code from email |
| newPassword | string | Body | Yes | New password |

**Request Body:**
```json
{
  "email": "user@example.com",
  "otpCode": "123456",
  "newPassword": "NewPassword789"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Password reset successfully
- `400` - Invalid OTP or email
- `404` - User not found
- `500` - Server error

---

### 2. User Controller

**Base Route:** `GET /api/users`

Purpose: Manage user profiles, search for users, and retrieve user information.

#### 2.1 Get Current User

**Endpoint:** `GET /api/users/me`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Retrieve the profile of the currently authenticated user.

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "onlineStatus": "online",
    "lastSeen": "2025-12-02T15:30:00Z",
    "description": "Software developer",
    "avatarUrl": "https://example.com/avatar.jpg",
    "addMePolicy": "everyone"
  },
  "error": null
}
```

**Status Codes:**
- `200` - User profile retrieved
- `401` - Unauthorized
- `404` - User not found

---

#### 2.2 Get User by ID

**Endpoint:** `GET /api/users/{id}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Retrieve profile of a specific user by their ID.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| id | GUID | Path | Yes | User ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "onlineStatus": "online",
    "lastSeen": "2025-12-02T15:30:00Z",
    "description": "Software developer",
    "avatarUrl": "https://example.com/avatar.jpg",
    "addMePolicy": "everyone"
  },
  "error": null
}
```

**Status Codes:**
- `200` - User profile retrieved
- `401` - Unauthorized
- `404` - User not found

---

#### 2.3 Search Users

**Endpoint:** `GET /api/users/search/{username}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Search for users by username.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| username | string | Path | Yes | Username to search for |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "onlineStatus": "offline",
      "lastSeen": "2025-12-02T10:00:00Z",
      "description": "Software developer",
      "avatarUrl": "https://example.com/avatar.jpg",
      "addMePolicy": "everyone"
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Search results retrieved
- `400` - Invalid search query
- `401` - Unauthorized

---

#### 2.4 Update Current User Profile

**Endpoint:** `PUT /api/users/me`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Update the profile of the currently authenticated user.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| username | string | Form | No | New username |
| email | string | Form | No | New email address |
| description | string | Form | No | User bio/description |
| addMePolicy | string | Form | No | Privacy policy (everyone, contacts, nobody) |
| file | IFormFile | Form | No | Avatar image file |

**Request (Multipart Form):**
```
Content-Type: multipart/form-data

username=john_doe_updated
description=Full stack developer
addMePolicy=contacts
file=<avatar.jpg>
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe_updated",
    "email": "john@example.com",
    "description": "Full stack developer",
    "addMePolicy": "contacts",
    "avatarUrl": "https://example.com/avatar-new.jpg"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Profile updated
- `400` - Invalid input
- `401` - Unauthorized
- `500` - Server error

---

#### 2.5 Update User Last Seen

**Endpoint:** `PUT /api/users/last-seen`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Update the last seen timestamp for the current user.

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Last seen updated
- `401` - Unauthorized
- `500` - Server error

---

#### 2.6 Get All Users (Admin)

**Endpoint:** `GET /api/users/`

**Authorization:** Required (Admin Role)

**Purpose:** Retrieve all users in the system (admin only).

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "onlineStatus": "online"
    },
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "username": "jane_smith",
      "email": "jane@example.com",
      "onlineStatus": "offline"
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Users list retrieved
- `401` - Unauthorized
- `403` - Forbidden (not admin)

---

#### 2.7 Delete User (Admin)

**Endpoint:** `DELETE /api/users/{userId}`

**Authorization:** Required (Admin Role)

**Purpose:** Delete a user account from the system.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| userId | GUID | Path | Yes | User ID to delete |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - User deleted
- `401` - Unauthorized
- `403` - Forbidden (not admin)
- `404` - User not found

---

### 3. Chat Controller

**Base Route:** `GET /api/chats`

Purpose: Manage direct messages and chat conversations between users.

#### 3.1 Get User's Chats

**Endpoint:** `GET /api/chats/me`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Retrieve all direct message chats for the authenticated user.

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "john_doe",
      "description": "",
      "type": "dm",
      "privacy": "private",
      "isOnline": true,
      "lastMessage": {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "content": "Hey, how are you?",
        "createdAt": "2025-12-02T15:30:00Z"
      },
      "unreadCount": 0,
      "members": [
        {
          "id": "550e8400-e29b-41d4-a716-446655440000",
          "username": "john_doe"
        }
      ]
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Chats retrieved
- `401` - Unauthorized
- `500` - Server error

---

#### 3.2 Get Specific Chat

**Endpoint:** `GET /api/chats/{chatId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Retrieve a specific chat with all its messages.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Path | Yes | Chat ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "john_doe",
    "description": "",
    "type": "dm",
    "privacy": "private",
    "members": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "email": "john@example.com"
      }
    ],
    "messages": [
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "senderId": "550e8400-e29b-41d4-a716-446655440000",
        "content": "Hello!",
        "hasFiles": false,
        "createdAt": "2025-12-02T15:30:00Z",
        "reactions": {
          "👍": ["550e8400-e29b-41d4-a716-446655440000"]
        }
      }
    ]
  },
  "error": null
}
```

**Status Codes:**
- `200` - Chat retrieved
- `401` - Unauthorized
- `404` - Chat not found

---

#### 3.3 Get Chat Profile

**Endpoint:** `GET /api/chats/{chatId}/profile`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Retrieve detailed profile information about a chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Path | Yes | Chat ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Development Team",
    "description": "Team chat for developers",
    "type": "group",
    "privacy": "private",
    "created": "2025-01-01T00:00:00Z",
    "memberCount": 5,
    "members": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "role": "admin"
      }
    ]
  },
  "error": null
}
```

**Status Codes:**
- `200` - Chat profile retrieved
- `401` - Unauthorized
- `404` - Chat not found

---

#### 3.4 Ban User from Chat

**Endpoint:** `POST /api/chats/ban/{userId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Ban a user from accessing the chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| userId | GUID | Path | Yes | User ID to ban |
| chatId | GUID | Query | Yes | Chat ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - User banned
- `401` - Unauthorized
- `403` - Permission denied
- `404` - User or chat not found

---

#### 3.5 Unban User from Chat

**Endpoint:** `POST /api/chats/unban/{userId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Remove a ban on a user to allow them to access the chat again.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| userId | GUID | Path | Yes | User ID to unban |
| chatId | GUID | Query | Yes | Chat ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - User unbanned
- `401` - Unauthorized
- `403` - Permission denied
- `404` - User or chat not found

---

#### 3.6 Update Chat Privacy

**Endpoint:** `PUT /api/chats/privacy-type`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Change the privacy settings of a chat (public, private, etc).

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Chat ID |
| privacyType | string | Query | Yes | Privacy type (public, private) |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Privacy updated
- `400` - Invalid privacy type
- `401` - Unauthorized
- `403` - Permission denied

---

#### 3.7 Search Chats

**Endpoint:** `POST /api/chats/search`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Search for chats by name or other criteria.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| searchTerm | string | Body | Yes | Search query |

**Request Body:**
```json
{
  "searchTerm": "Development"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Development Team",
      "type": "group",
      "memberCount": 5
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Search completed
- `401` - Unauthorized
- `400` - Invalid search term

---

### 4. Group Controller

**Base Route:** `POST /api/groups`

Purpose: Create and manage group chats.

#### 4.1 Create Group

**Endpoint:** `POST /api/groups/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Create a new group chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| name | string | Form | Yes | Group name |
| description | string | Form | No | Group description |
| privacy | string | Form | Yes | Privacy type (public/private) |
| file | IFormFile | Form | No | Group avatar/icon image |

**Request (Multipart Form):**
```
Content-Type: multipart/form-data

name=Development Team
description=Team for backend developers
privacy=private
file=<group-icon.jpg>
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Development Team",
    "description": "Team for backend developers",
    "type": "group",
    "privacy": "private",
    "created": "2025-12-02T15:30:00Z",
    "members": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "username": "john_doe"
      }
    ]
  },
  "error": null
}
```

**Status Codes:**
- `200` - Group created
- `400` - Invalid input
- `401` - Unauthorized
- `500` - Server error

---

#### 4.2 Add Member to Group

**Endpoint:** `POST /api/groups/add-member`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Add a user to an existing group.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Group ID |
| addingUserId | GUID | Query | Yes | User ID to add |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Member added
- `401` - Unauthorized
- `403` - Permission denied
- `404` - Group or user not found
- `409` - User already in group

---

#### 4.3 Join Group

**Endpoint:** `POST /api/groups/join`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Join a public group as the authenticated user.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| groupId | GUID | Query | Yes | Group ID to join |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Joined group
- `401` - Unauthorized
- `403` - Group is private
- `404` - Group not found

---

#### 4.4 Leave Group

**Endpoint:** `DELETE /api/groups/leave`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Remove the authenticated user from a group.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Group ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Left group
- `401` - Unauthorized
- `404` - Group not found

---

#### 4.5 Delete Group

**Endpoint:** `DELETE /api/groups/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Delete a group (admin/owner only).

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Group ID to delete |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Group deleted
- `401` - Unauthorized
- `403` - Permission denied
- `404` - Group not found

---

#### 4.6 Update Group

**Endpoint:** `PUT /api/groups/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Update group details (name, description, etc).

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Group ID |
| name | string | Form | No | New group name |
| description | string | Form | No | New description |
| file | IFormFile | Form | No | New avatar image |

**Request (Multipart Form):**
```
Content-Type: multipart/form-data

name=Dev Team Updated
description=Updated team description
file=<new-icon.jpg>
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Dev Team Updated",
    "description": "Updated team description"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Group updated
- `400` - Invalid input
- `401` - Unauthorized
- `403` - Permission denied

---

#### 4.7 Search Groups

**Endpoint:** `GET /api/groups/search`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Search for groups by name.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| searchTerm | string | Query | Yes | Search query |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Development Team",
      "description": "Team for developers",
      "memberCount": 5
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Search completed
- `400` - Invalid search term
- `401` - Unauthorized

---

### 5. Channel Controller

**Base Route:** `POST /api/channels`

Purpose: Create and manage channel chats (similar to groups but with additional features).

#### 5.1 Create Channel

**Endpoint:** `POST /api/channels/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Create a new channel.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| name | string | Form | Yes | Channel name |
| description | string | Form | No | Channel description |
| privacy | string | Form | Yes | Privacy type (public/private) |
| file | IFormFile | Form | No | Channel icon image |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "announcements",
    "type": "channel",
    "privacy": "public"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Channel created
- `400` - Invalid input
- `401` - Unauthorized

---

#### 5.2 Add Member to Channel

**Endpoint:** `POST /api/channels/add-member`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Add a user to a channel.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Channel ID |
| addingUserId | GUID | Query | Yes | User ID to add |

**Status Codes:**
- `200` - Member added
- `401` - Unauthorized
- `403` - Permission denied
- `404` - Channel or user not found

---

#### 5.3 Join Channel

**Endpoint:** `POST /api/channels/join`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Join a public channel.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| channelId | GUID | Query | Yes | Channel ID |

**Status Codes:**
- `200` - Joined channel
- `401` - Unauthorized
- `403` - Channel is private

---

#### 5.4 Leave Channel

**Endpoint:** `DELETE /api/channels/leave`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Leave a channel.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Channel ID |

---

#### 5.5 Delete Channel

**Endpoint:** `DELETE /api/channels/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Delete a channel (admin only).

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Channel ID |

---

#### 5.6 Update Channel

**Endpoint:** `PUT /api/channels/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Update channel details.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Query | Yes | Channel ID |
| name | string | Form | No | New channel name |
| description | string | Form | No | New description |
| file | IFormFile | Form | No | New icon image |

---

#### 5.7 Search Channels

**Endpoint:** `GET /api/channels/search`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Search for channels by name.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| searchTerm | string | Query | Yes | Search query |

---

### 6. Message Controller

**Base Route:** `POST /api/messages`

Purpose: Handle message sending, editing, and reactions.

#### 6.1 Send Message

**Endpoint:** `POST /api/messages/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Send a message to a chat or direct message.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Form | Yes | Destination chat/DM ID |
| content | string | Form | Yes | Message content |
| file | IFormFile | Form | No | Attachment file |

**Request (Multipart Form):**
```
Content-Type: multipart/form-data

chatId=550e8400-e29b-41d4-a716-446655440000
content=Hello everyone!
file=<document.pdf>
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "chatId": "550e8400-e29b-41d4-a716-446655440000",
    "senderId": "550e8400-e29b-41d4-a716-446655440002",
    "content": "Hello everyone!",
    "hasFiles": true,
    "createdAt": "2025-12-02T15:30:00Z"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Message sent
- `400` - Invalid input
- `401` - Unauthorized
- `403` - No permission to send to chat
- `404` - Chat not found

---

#### 6.2 Update Message

**Endpoint:** `PUT /api/messages/{messageId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Edit an existing message.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| messageId | GUID | Path | Yes | Message ID to edit |
| content | string | Form | Yes | New message content |
| file | IFormFile | Form | No | New attachment |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "content": "Hello everyone! (edited)",
    "updatedAt": "2025-12-02T15:35:00Z"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Message updated
- `401` - Unauthorized
- `403` - Can only edit own messages
- `404` - Message not found

---

#### 6.3 Delete Message

**Endpoint:** `DELETE /api/messages/{messageId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Delete a message.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| messageId | GUID | Path | Yes | Message ID to delete |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Message deleted
- `401` - Unauthorized
- `403` - Can only delete own messages
- `404` - Message not found

---

#### 6.4 Add Reaction

**Endpoint:** `POST /api/messages/reaction`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Add a reaction (emoji) to a message.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| messageId | GUID | Query | Yes | Message ID |
| reactionId | GUID | Query | Yes | Reaction ID (emoji ID) |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Reaction added
- `401` - Unauthorized
- `404` - Message or reaction not found

---

#### 6.5 Remove Reaction

**Endpoint:** `DELETE /api/messages/reaction`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Remove a reaction from a message.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| messageId | GUID | Query | Yes | Message ID |
| reactionId | GUID | Query | Yes | Reaction ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Reaction removed
- `401` - Unauthorized
- `404` - Message or reaction not found

---

### 7. Reaction Controller

**Base Route:** `GET /api/reactions`

Purpose: Manage emoji reactions available in the system.

#### 7.1 Get All Reactions

**Endpoint:** `GET /api/reactions/`

**Authorization:** None (Public)

**Purpose:** Retrieve all available reactions/emojis.

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "👍 Thumbs Up",
      "emoji": "👍",
      "category": "gestures"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "❤️ Heart",
      "emoji": "❤️",
      "category": "emotions"
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Reactions retrieved
- `500` - Server error

---

#### 7.2 Get Specific Reaction

**Endpoint:** `GET /api/reactions/{reactionId}`

**Authorization:** None (Public)

**Purpose:** Get details of a specific reaction.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| reactionId | GUID | Path | Yes | Reaction ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "👍 Thumbs Up",
    "emoji": "👍"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Reaction retrieved
- `404` - Reaction not found

---

#### 7.3 Create Reaction

**Endpoint:** `POST /api/reactions/`

**Authorization:** None (Public)

**Purpose:** Create a new reaction/emoji.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| name | string | Form | Yes | Reaction name |
| emoji | string | Form | Yes | Emoji character |
| category | string | Form | Yes | Reaction category |
| file | IFormFile | Form | No | Custom emoji image |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "👍 Thumbs Up",
    "emoji": "👍"
  },
  "error": null
}
```

**Status Codes:**
- `200` - Reaction created
- `400` - Invalid input

---

#### 7.4 Update Reaction

**Endpoint:** `PUT /api/reactions/{reactionId}`

**Authorization:** None (Public)

**Purpose:** Update an existing reaction.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| reactionId | GUID | Path | Yes | Reaction ID |
| name | string | Form | No | New name |
| emoji | string | Form | No | New emoji |
| file | IFormFile | Form | No | New image |

**Status Codes:**
- `200` - Reaction updated
- `400` - Invalid input
- `404` - Reaction not found

---

#### 7.5 Delete Reaction

**Endpoint:** `DELETE /api/reactions/{reactionId}`

**Authorization:** None (Public)

**Purpose:** Delete a reaction.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| reactionId | GUID | Path | Yes | Reaction ID |

**Status Codes:**
- `200` - Reaction deleted
- `404` - Reaction not found

---

### 8. Notification Controller

**Base Route:** `PUT /api/notifications`

Purpose: Manage user notifications.

#### 8.1 Mark Notification as Seen

**Endpoint:** `PUT /api/notifications/{notificationId}/seen`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Mark a single notification as seen/read.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| notificationId | GUID | Path | Yes | Notification ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Notification marked as seen
- `401` - Unauthorized
- `404` - Notification not found

---

#### 8.2 Mark Multiple Notifications as Seen

**Endpoint:** `PUT /api/notifications/seen/batch`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Mark multiple notifications as seen in one request.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| notificationIds | Guid[] | Body | Yes | Array of notification IDs |

**Request Body:**
```json
{
  "notificationIds": [
    "550e8400-e29b-41d4-a716-446655440000",
    "550e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Notifications marked as seen
- `400` - Invalid input
- `401` - Unauthorized

---

#### 8.3 Mark All Notifications as Seen

**Endpoint:** `PUT /api/notifications/seen/all`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Mark all notifications for the user as seen.

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - All notifications marked as seen
- `401` - Unauthorized

---

### 9. Permission Controller

**Base Route:** `POST /api/permissions`

**Controller-level Authorization:** All endpoints require JWT Bearer Token

Purpose: Manage user permissions within chats/groups/channels.

#### 9.1 Grant Permission

**Endpoint:** `POST /api/permissions/grant`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Grant a specific permission to a user in a chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Body | Yes | Chat ID |
| userId | GUID | Body | Yes | User to grant permission |
| permission | string | Body | Yes | Permission name |

**Request Body:**
```json
{
  "chatId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "permission": "CanDeleteMessages"
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Permission granted
- `400` - Invalid input
- `403` - Permission denied
- `404` - User or chat not found

---

#### 9.2 Revoke Permission

**Endpoint:** `POST /api/permissions/revoke`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Remove a specific permission from a user.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Body | Yes | Chat ID |
| userId | GUID | Body | Yes | User to revoke permission |
| permission | string | Body | Yes | Permission name |

**Request Body:**
```json
{
  "chatId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "permission": "CanDeleteMessages"
}
```

**Status Codes:**
- `200` - Permission revoked
- `400` - Invalid input
- `403` - Permission denied

---

#### 9.3 Get User Permissions

**Endpoint:** `GET /api/permissions/{chatId}/user/{userId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Get all permissions for a specific user in a chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Path | Yes | Chat ID |
| userId | GUID | Path | Yes | User ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "permissionName": "CanDeleteMessages",
      "grantedAt": "2025-12-02T15:30:00Z"
    },
    {
      "permissionName": "CanEditMessages",
      "grantedAt": "2025-12-02T15:30:00Z"
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Permissions retrieved
- `404` - User or chat not found

---

#### 9.4 Get All Chat Permissions

**Endpoint:** `GET /api/permissions/{chatId}/all`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Get all permissions defined in a chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Path | Yes | Chat ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "username": "john_doe",
      "permissions": ["CanDeleteMessages", "CanEditMessages"]
    }
  ],
  "error": null
}
```

**Status Codes:**
- `200` - Permissions retrieved
- `404` - Chat not found

---

#### 9.5 Revoke All User Permissions

**Endpoint:** `DELETE /api/permissions/{chatId}/user/{userId}`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Remove all permissions from a user in a chat.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| chatId | GUID | Path | Yes | Chat ID |
| userId | GUID | Path | Yes | User ID |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - All permissions revoked
- `403` - Permission denied
- `404` - User or chat not found

---

### 10. OTP Controller

**Base Route:** `POST /api/otp`

Purpose: Handle one-time password (OTP) generation and sending for email verification.

#### 10.1 Send OTP to User Email

**Endpoint:** `POST /api/otp/send-to-user`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Send OTP to the authenticated user's email address.

**Request Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "message": "OTP sent to your email",
    "expiryTime": "10 minutes"
  },
  "error": null
}
```

**Status Codes:**
- `200` - OTP sent
- `401` - Unauthorized
- `500` - Email service error

---

#### 10.2 Send OTP to Email Address

**Endpoint:** `POST /api/otp/send-to-email`

**Authorization:** None (Public)

**Purpose:** Send OTP to a specified email address (for registration/password reset).

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| email | string | Query | Yes | Email address to send OTP |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "message": "OTP sent successfully",
    "expiryTime": "10 minutes"
  },
  "error": null
}
```

**Status Codes:**
- `200` - OTP sent
- `400` - Invalid email
- `429` - Too many requests (rate limit)
- `500` - Email service error

---

### 11. Conversation Controller

**Base Route:** `DELETE /api/conversations`

Purpose: Manage direct message conversations.

#### 11.1 Delete Conversation

**Endpoint:** `DELETE /api/conversations/`

**Authorization:** Required (JWT Bearer Token)

**Purpose:** Delete a direct message conversation thread.

**Request Parameters:**

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| conversationId | GUID | Query | Yes | Conversation ID to delete |

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": null,
  "error": null
}
```

**Status Codes:**
- `200` - Conversation deleted
- `401` - Unauthorized
- `404` - Conversation not found

---

## Common Data Types

### User Object

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "username": "john_doe",
  "email": "john@example.com",
  "onlineStatus": "online|offline|away",
  "lastSeen": "2025-12-02T15:30:00Z",
  "description": "Software developer",
  "avatarUrl": "https://example.com/avatar.jpg",
  "addMePolicy": "everyone|contacts|nobody"
}
```

### Chat/Message Object

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "john_doe",
  "description": "",
  "type": "dm|group|channel",
  "privacy": "public|private",
  "created": "2025-12-02T00:00:00Z",
  "members": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "username": "john_doe"
    }
  ],
  "lastMessage": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "content": "Hello!",
    "createdAt": "2025-12-02T15:30:00Z"
  },
  "unreadCount": 0
}
```

### Message Object

```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "chatId": "550e8400-e29b-41d4-a716-446655440000",
  "senderId": "550e8400-e29b-41d4-a716-446655440002",
  "senderName": "john_doe",
  "content": "Hello everyone!",
  "hasFiles": false,
  "createdAt": "2025-12-02T15:30:00Z",
  "updatedAt": null,
  "reactions": {
    "👍": ["550e8400-e29b-41d4-a716-446655440000"],
    "❤️": ["550e8400-e29b-41d4-a716-446655440001"]
  }
}
```

### Notification Object

```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "New Message",
  "message": "john_doe sent you a message",
  "type": "message|invitation|mention",
  "isRead": false,
  "relatedEntityId": "660e8400-e29b-41d4-a716-446655440001",
  "createdAt": "2025-12-02T15:30:00Z"
}
```

### Permission Object

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "chatId": "550e8400-e29b-41d4-a716-446655440000",
  "permissionName": "CanDeleteMessages|CanEditMessages|CanBanUsers",
  "grantedAt": "2025-12-02T15:30:00Z",
  "grantedBy": "550e8400-e29b-41d4-a716-446655440002"
}
```

---

## Error Handling

### Standard Error Response

All errors follow this format:

```json
{
  "success": false,
  "statusCode": 400,
  "data": null,
  "error": "Detailed error message"
}
```

### Common HTTP Status Codes

| Code | Meaning | Description |
|------|---------|-------------|
| `200` | OK | Request successful |
| `400` | Bad Request | Invalid input or malformed request |
| `401` | Unauthorized | Missing or invalid authentication token |
| `403` | Forbidden | User lacks required permissions |
| `404` | Not Found | Resource not found |
| `409` | Conflict | Resource already exists or conflict with existing data |
| `429` | Too Many Requests | Rate limit exceeded |
| `500` | Internal Server Error | Server-side error |

### Authentication Errors

```json
{
  "success": false,
  "statusCode": 401,
  "data": null,
  "error": "Invalid or expired token"
}
```

### Permission Errors

```json
{
  "success": false,
  "statusCode": 403,
  "data": null,
  "error": "You don't have permission to perform this action"
}
```

### Validation Errors

```json
{
  "success": false,
  "statusCode": 400,
  "data": null,
  "error": "The value 'me' is not valid. (Parameter 'id')"
}
```

---

## API Usage Examples

### Example 1: Complete Login Flow

```bash
# 1. Send OTP to email
curl -X POST http://localhost:5000/api/otp/send-to-email?email=user@example.com

# 2. Register with OTP
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "john_doe",
    "password": "SecurePass123",
    "otpCode": "123456"
  }'

# 3. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "credential": "john_doe",
    "password": "SecurePass123"
  }'
```

### Example 2: Create Group and Send Message

```bash
# 1. Create group
curl -X POST http://localhost:5000/api/groups/ \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "name=Dev Team" \
  -F "description=Team chat" \
  -F "privacy=private"

# 2. Add member (use chatId from response)
curl -X POST "http://localhost:5000/api/groups/add-member?chatId=GROUP_ID&addingUserId=USER_ID" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# 3. Send message
curl -X POST http://localhost:5000/api/messages/ \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "chatId=GROUP_ID" \
  -F "content=Hello team!"
```

---

## Best Practices

1. **Always include Authorization header** for protected endpoints
2. **Handle rate limiting** (HTTP 429) by implementing exponential backoff
3. **Cache responses** where appropriate (reactions, permissions)
4. **Use pagination** for large data sets
5. **Validate input** before sending to API
6. **Handle 401/403 errors** by refreshing token or redirecting to login
7. **Log errors** for debugging and monitoring
8. **Use connection pooling** for database efficiency

---

**Last Updated:** 2025-12-02
**API Version:** 1.0
**Status:** Production Ready
