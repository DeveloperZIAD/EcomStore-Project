# «·„—Õ·… 1: »Ì∆… »‰«¡ «· ÿ»Ìﬁ (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. ‰”Œ „·›«  «·‹ .csproj √Ê·« ·⁄„· Restore ··„ﬂ »«  »‘ﬂ· „‰›’· (· Õ”Ì‰ ”—⁄… «·»‰«¡ ·«Õﬁ«)
COPY ["API/API.csproj", "API/"]
COPY ["business layer/business layer.csproj", "business layer/"]
COPY ["Data layer/Data layer.csproj", "Data layer/"]

# 2.  ‰›Ì– √„— «·‹ Restore
RUN dotnet restore "API/API.csproj"

# 3. ‰”Œ »«ﬁÌ „·›«  «·ﬂÊœ «·„’œ—Ì »«·ﬂ«„·
COPY . .

# 4. »‰«¡ «·„‘—Ê⁄ Ê ÕÊÌ· «·ﬂÊœ ≈·Ï „·›«  Ã«Â“… ·· ‘€Ì·
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# «·„—Õ·… 2: „—Õ·… «·‰‘— (Publish Stage)
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# «·„—Õ·… 3: »Ì∆… «· ‘€Ì· «·‰Â«∆Ì… (Final Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# ‰”Œ «·„·›«  «·‰Â«∆Ì… ›ﬁÿ „‰ „—Õ·… «·‰‘—
COPY --from=publish /app/publish .

#  ÕœÌœ „·› «· ‘€Ì· «·—∆Ì”Ì
ENTRYPOINT ["dotnet", "API.dll"]