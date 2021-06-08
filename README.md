# wow-market-watcher

The WoW Market Watcher is a service that, on a scheduled bases, collects auction house data from Blizzard's API and save it as a time series.
The services exposes a REST API that:

- Allows users to create and manage accounts.
- Query against the database of items, realms, etc
- Create and update watch lists
- Query the time series database

- Site: https://rob893.github.io/wow-market-watcher-ui
- UI Repo: https://github.com/rob893/wow-market-watcher-ui

## Local Development

### Prerequisites

- .NET 5 SDK installed

### Basic Commands

- `dotnet test` - Run tests
- `dotnet build` - Build project
- `dotnet publish -c Release` - Publish release build
- `dotnet watch run` - Run the project in watch mode (will restart when files change)

## CICD

All CICD is done through Github Actions.
All merges into `master` branch will kick off CICD to build the project and deploy it to Azure and a DigitalOcean droplet.
The project is hosted on a Windows Azure App Service in Azure and on a Linux VM running Docker as a Docker container in DigitalOcean.

## Required Secrets

This project requires the following secrets either in an appsettings file or as appropriate environment variables.

The project will not run without these secrets.

Please contact repo owner for details.
