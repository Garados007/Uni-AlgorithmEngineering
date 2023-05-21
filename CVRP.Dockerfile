FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder
COPY ./src /src
WORKDIR /src
RUN dotnet publish \
        -c Release \
        -o output \
        --nologo \
        CVRP/CVRP.csproj

FROM mcr.microsoft.com/dotnet/runtime:7.0
COPY --from=builder /src/output /src/output
WORKDIR /src/output
ENTRYPOINT [ "/src/output/dijkstra" ]
CMD [ "/src/output/dijkstra" ]

# This dockerfile can be build with:
#   docker build -f CVRP.Dockerfile -t "cvrp:max_brauer" .
#
# To run a command:
#   docker run -v "$(pwd)/data:/data" cvrp:max_brauer solve /data/problem.vrp
