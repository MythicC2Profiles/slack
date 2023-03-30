FROM itsafeaturemythic/mythic_base_image:0.0.1

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
RUN python3 -m pip install mythic-container==0.2.0rc4
WORKDIR /Mythic/slack/c2_code/src/slack-server
RUN dotnet publish -c Release -o /Mythic/slack/c2_code/
CMD ["python3", "main.py"]