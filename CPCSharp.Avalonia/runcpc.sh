#!/bin/bash
cd $(dirname $0)
Configuration=Release Platform=MacOS dotnet run -- "$@"
