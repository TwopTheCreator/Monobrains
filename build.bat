@echo off
echo Building Monobrains IDE...

echo.
echo Restoring NuGet packages...
dotnet restore Monobrains.sln

echo.
echo Building C# project...
dotnet build Monobrains.sln --configuration Release

echo.
echo Creating Python backend directory...
if not exist "Backend" mkdir Backend

echo.
echo Copying Python backend files...
copy "Backend\monobrains_backend.py" "Monobrains\bin\Release\net6.0-windows\Backend\"

echo.
echo Creating plugin directory...
if not exist "Monobrains\bin\Release\net6.0-windows\Plugins" mkdir "Monobrains\bin\Release\net6.0-windows\Plugins"

echo.
echo Creating themes directory...
if not exist "Monobrains\bin\Release\net6.0-windows\Themes" mkdir "Monobrains\bin\Release\net6.0-windows\Themes"

echo.
echo Creating languages directory...
if not exist "Monobrains\bin\Release\net6.0-windows\Languages" mkdir "Monobrains\bin\Release\net6.0-windows\Languages"

echo.
echo Creating templates directory...
if not exist "Monobrains\bin\Release\net6.0-windows\Templates" mkdir "Monobrains\bin\Release\net6.0-windows\Templates"

echo.
echo Build completed successfully!
echo.
echo To run Monobrains IDE:
echo cd Monobrains\bin\Release\net6.0-windows
echo Monobrains.exe
echo.
pause
