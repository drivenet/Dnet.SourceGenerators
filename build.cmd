@echo off
del /q packages\Dnet.SourceGenerators.*.nupkg
mkdir packages
dotnet pack src/Dnet.SourceGenerators -c Integration -o packages
