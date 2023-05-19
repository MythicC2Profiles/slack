FROM itsafeaturemythic/mythic_python_dotnet:latest

# RUN apt update && apt install wget nuget -y
# RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
# RUN dpkg -i packages-microsoft-prod.deb
# RUN rm packages-microsoft-prod.deb

# RUN  apt-get update; \
#   apt-get install -y apt-transport-https && \
#   apt-get update && \
#   apt-get install -y dotnet-sdk-6.0

WORKDIR /Mythic/
COPY [".", "."]
# WORKDIR /Mythic/slack/c2_code/src/
# RUN dir
# RUN dotnet publish -c Release -o /Mythic/slack/c2_code/
# WORKDIR /Mythic/
CMD ["python3", "main.py"]