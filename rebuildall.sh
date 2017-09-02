#dotnet clean -c Release
dotnet build -c Release
dotnet publish -c Release
rm -r obj/Docker/publish 
mv bin/Release/netcoreapp2.0/publish obj/Docker/publish
docker build -t verkest .

