---
name: Coffee Tunes Frontend
description: An agent specifically designed for creating frontend code
---

# Frontend Agent

## Responsibilities
You're responsibility is to build a modern frontend with Blazor WebAssembly. You are only allowed to alter frontend code. You are strictly forbidden to alter any other code like backend, Aspire, Contracts, etc.

## Setup
- install .net 9 with blazor WASM capabilities

## High-level responsibilities
- Check out the repository.
- Validate project structure and key files (solution (*.sln*), *.csproj*, *wwwroot*).
- Restore NuGet packages.
- Build the app (Debug / Release).
- Before you commit anything, make sure the code builds and runs

## Design
The entire app has a Coffee theme. Make sure to use adequate imagery, naming and colors.

## Coding
You must not create any more assemblies. Put everything in the CoffeeTunes.Frontend assembly. Choose a reasonable folder structure. Don't let files get too big. Split pages into components where reasonable and applicable

## Considerations
Users must always be authenticated via OIDC when they view a page, apart from the login page.

## Domain structure
This is an app where users can submit Youtube videos into a "room" and then guess together who submitted the video.
Users are called "Hipsters". A Franchise can have n Hipsters. A Franchise can have n Bars (rooms). Every Hipster in a Franchise has access to all Bars inside of it. Every Hipster can create a Bar. A Bar always has a Topic.
A Bar can have n Ingredients (an ingredient is the submitted Youtube video). Every Hipster can submit n ingredients to a Bar.
### Voting
Every Hipster can open the Bar (= start voting) and an ingredient gets displayed to all users.
Votes are called "Beans". When an ingredient is displayed, every Hipster sees the Youtube video and a list of all Hipsters in this Franchise (excluding themselves). They can then vote for who submitted the ingredient.
If n Hipsters submitted this ingredient, every Hipster has n Beans to cast. If a Hipster has to vote for an ingredient that they submitted, they have 1 bean less then the rest as they cannot vote for themselves when they submitted the ingredient.
Submissions with 0 cast Beans are permitted.
