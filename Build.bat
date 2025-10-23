dotnet build --configuration Release IntelliSage.sln
dotnet publish IntelliSage.sln

rem upload framework from intellisage\bin\Release\net8.0\wwwroot (not from publish)
rem if dotnet build fails, re-build with visual studio
pause
