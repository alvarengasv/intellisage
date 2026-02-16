copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.SDK.Constants.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.SDK.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.Monitor.SDK.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.GameHosting.SDK.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.Scripting.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.Scripting.SDK.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.Web.Shared.dll" ReferenceAssemblies
copy /Y "E:\TCAdmin3\Source\TCAdminv3\src\TCAdmin.Monitor\bin\Release\net9.0\win-x64\TCAdmin.Docker.SDK.dll" ReferenceAssemblies

E:\source\ConfuserEx-CLI\Confuser.CLI.exe ReferenceAssemblies.crproj

rd /q /s ..\publish
dotnet build --configuration Release ..\IntelliSage.sln
dotnet publish ..\IntelliSage.sln

@echo off
rem upload framework from intellisage\bin\Release\net9.0\wwwroot (without .br)
rem upload framework from intellisage\publish\wwwroot (with .br)
rem if dotnet build fails, delete the contents of publish, re-build with visual studio
pause