#!/bin/bash

# Install dotnet 7
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel STS

# Install the visual studio remote debugger
curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg
