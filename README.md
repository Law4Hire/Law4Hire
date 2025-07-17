# Law4Hire

This repository includes a `.devcontainer` definition for working in a development environment based on **Ubuntu 22.04** with:

- **Node.js** (latest)
- **.NET 9 SDK**
- **PostgreSQL**

## Using the Dev Container

1. Install [VS Code](https://code.visualstudio.com/) and the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).
2. Open the project in VS Code and reopen it in the container when prompted. The container uses Microsoft's `dotnet-install.sh` script to install the .NET 9 SDK.

The environment exposes `dotnet`, `node`, and `psql` commands from the integrated terminal.
