FROM ubuntu:22.04

# Avoid prompts during install
ENV DEBIAN_FRONTEND=noninteractive

# Update apt and install dependencies
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    gnupg \
    ca-certificates \
    apt-transport-https \
    software-properties-common \
    lsb-release

# Install latest Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_current.x | bash - && \
    apt-get install -y nodejs

# Install .NET 9 SDK using dotnet-install script
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    rm dotnet-install.sh

# Configure .NET environment
ENV DOTNET_ROOT="/root/.dotnet"
ENV PATH="$PATH:/root/.dotnet:/root/.dotnet/tools"

# Install PostgreSQL
RUN apt-get install -y postgresql

# Clean up apt caches
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

CMD ["/bin/bash"]
