@echo off
rmdir /s /q packages
mkdir packages
dotnet pack src/Dnet.SourceGenerators -c Integration -o packages
