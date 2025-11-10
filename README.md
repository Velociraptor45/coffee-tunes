# ☕ Coffee Tunes

A fun, coffee-themed collaborative guessing game where hipsters submit YouTube videos and guess who contributed what!

## 🎯 Overview

Coffee Tunes is a real-time multiplayer game built with Blazor WebAssembly where users (called "Hipsters") can:
- Create and join "Franchises" (organizations)
- Set up "Bars" (game rooms) with specific topics
- Submit "Ingredients" (YouTube videos) to Bars
- Vote on who submitted each video by casting "Beans" (votes)
- Track statistics and enjoy a complete playlist of all submissions

## 🏗️ Architecture

The application consists of:
- **CoffeeTunes.Frontend**: Blazor WebAssembly frontend
- **CoffeeTunes.WebApi**: ASP.NET Core Web API backend
- **CoffeeTunes.Contracts**: Shared contracts between frontend and backend
- **CoffeeTunes.Aspire.AppHost**: .NET Aspire orchestration

## ☕ Domain Concepts

### The Coffee-Themed Vocabulary

- **Hipster**: A user in the system
- **Franchise**: An organization that can have multiple Hipsters
- **Bar**: A game room within a Franchise with a specific topic
- **Ingredient**: A YouTube video submission
- **Beans**: Votes cast during the guessing phase
- **Brewing**: The active voting session
- **Supply**: Available submission slots (when supply runs out, the Bar is full)

### How It Works

1. **Setup Phase**
   - A Hipster creates a Franchise or joins an existing one
   - Hipsters in a Franchise can create Bars with specific topics
   - All Hipsters in a Franchise have access to all Bars within it

2. **Submission Phase**
   - Each Hipster can submit up to a configured maximum number of Ingredients (YouTube videos) to a Bar
   - Submissions continue until either all Hipsters have submitted their maximum or the Bar is manually opened

3. **Brewing Phase**
   - Any Hipster can "open" the Bar to start the voting process
   - One Ingredient is displayed at a time
   - Hipsters vote on who they think submitted the video by casting Beans

4. **Voting Rules**
   - If *n* Hipsters submitted an Ingredient, each voter has *n* Beans to distribute
   - Hipsters cannot vote for themselves, so they have one less Bean for Ingredients they submitted
   - Submissions with 0 Beans are allowed
   - Results can be revealed after voting on each Ingredient

5. **After Voting**
   - View statistics showing voting patterns and results
   - Browse the complete playlist of all submissions

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Modern web browser with WebAssembly support
- Docker for containerized execution

### Running Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/Velociraptor45/coffee-tunes.git
   cd coffee-tunes/src
   ```
   
2. **Create appsettings.Local.json in `CoffeeTunes.WebApi` and fill out your authentication and YouTube API key details:**
   ```json
   {
     "CT_AUTH_AUTHORITY": "",
     "CT_AUTH_AUDIENCE": "account",
     "CT_AUTH_VALID_TYPES__0": "JWT",
     "CT_AUTH_CLAIM_NAME": "given_name",
     "CT_AUTH_CLAIM_ROLE": "",
     "CT_AUTH_ROLE_NAME_USER": "",
     "CT_YT_API_KEY": ""
   }
   ```

3. **Create an appsettings.json in `CoffeeTunes.Frontend/wwwroot` and fill out your authentication details:**
   ```json
   {
     "CT_AUTH_AUTHORITY": "",
     "CT_AUTH_CLIENT_ID": "",
     "CT_AUTH_ROLE_NAME_USER": "",
     "CT_AUTH_CLAIM_NAME": "given_name",
     "CT_AUTH_CLAIM_ROLE": "",
     "CT_API_URL": "https://localhost:5050"
   }
   ```

4. **Restore dependencies**
   ```bash
   dotnet restore
   ```

5. **Run with .NET Aspire (recommended)**
   ```bash
   dotnet run --project CoffeeTunes.Aspire.AppHost
   ```
   This will start both the API and frontend, and open the Aspire dashboard.

6. **Or run components separately**
   
   Terminal 1 (Backend):
   ```bash
   dotnet run --project CoffeeTunes.WebApi
   ```
   
   Terminal 2 (Frontend):
   ```bash
   dotnet run --project CoffeeTunes.Frontend
   ```

### Building for Production

```bash
dotnet publish CoffeeTunes.Frontend/CoffeeTunes.Frontend.csproj -c Release
```

The output will be in `CoffeeTunes.Frontend/bin/Release/net9.0/publish/wwwroot/`

## 🎨 Features

### Real-Time Collaboration
- SignalR-powered real-time updates
- See who's joined the Bar in real-time
- Live voting updates
- Synchronized video playback controls

### Three Main Views

#### 🔥 Brewing View
- Active voting interface during game sessions
- Displays current Ingredient with embedded YouTube player
- Shows who has voted
- Synchronized play/pause controls
- Results reveal functionality

#### 🧪 Ingredients View
- Manage your submitted Ingredients
- View all Hipsters in the Franchise and their contribution status
- Add or remove Ingredients (before Bar opens)
- Visual indicators for used/unused Ingredients

#### 📊 Stats View
- Comprehensive voting statistics
- Track performance over time
- See voting patterns and accuracy

#### 🎵 Playlist View (Post-Game)
- Available only when Bar has no supply left
- Browse complete playlist of all submitted videos
- Quick access to all Ingredients with thumbnails
- Direct YouTube links for each entry

### Coffee-Themed Design
- Warm brown and beige color palette
- Coffee cup loading animations
- Consistent coffee-related iconography and naming
- Smooth, modern UI with gradient effects

## 🔐 Authentication

The application uses OpenID Connect (OIDC) for authentication. All pages require authentication except the login page.

## 🛠️ Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 9)
- **Backend**: ASP.NET Core Web API (.NET 9)
- **Real-time**: SignalR
- **API Client**: RestEase
- **Authentication**: OpenID Connect (OIDC)
- **Orchestration**: .NET Aspire
- **Styling**: Scoped CSS with CSS Grid and Flexbox

## 🎮 Game Flow Example

1. Alice creates a Franchise called "Music Lovers"
2. Bob and Charlie join the "Music Lovers" Franchise
3. Alice creates a Bar with topic "80s Rock Anthems"
4. All three Hipsters submit 3 Ingredients each (9 total)
5. Bob opens the Bar to start Brewing
6. The first Ingredient is displayed - a YouTube video
7. Each Hipster votes on who they think submitted it:
   - If Alice submitted it, she has 2 Beans (can't vote for herself)
   - Bob and Charlie each have 3 Beans
8. Results are revealed showing who actually submitted it and how everyone voted
9. Process repeats for all 9 Ingredients
10. After all voting is complete, everyone can view the Stats and Playlist

## 🤝 Contributing

This is a personal project, but suggestions and feedback are welcome!

## 📄 License

See LICENSE file for details.

## 🎵 Why Coffee Tunes?

The name combines two universal pleasures: coffee and music. Just as coffee brings people together for conversation, this app brings people together to share and discover music in a fun, competitive way. Plus, all that caffeine helps with those tricky guessing decisions! ☕🎶
