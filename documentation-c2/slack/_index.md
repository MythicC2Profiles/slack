+++
title = "slack"
chapter = false
weight = 5
+++

## Overview
This C2 profile consists of a server that listens for new event messages on a specific slack channel, on receive of a new message, it deserializes the message and passes the contents on to the Mythic server via the standard REST API. It then takes the result, serializes it, and writes the result to that same channel.

### Slack C2 Workflow
{{<mermaid>}}
sequenceDiagram
    participant M as Mythic
    participant H as Slack Channel
    participant A as Agent
    A ->>+ H: Write to channel for tasking
    H ->>+ M: forward request to Mythic
    M -->>- H: reply with tasking
    H -->>- A: Write to channel with tasking
{{< /mermaid >}}
Legend:

1.) The agent writes a message to the slack channel indicating it's serverbound

2.) The server gets a notification of the message, deletes it, and forwards the message to Mythic

2a.) If the message is too large for a standard message, the message contents are instead written to a file, and the deserialized message will be an empty string

3.) The server receives the response from the Mythic server, serializes it, and writes it to the slack channel

3a.) If the message is too large for a standard message, the message contents are instead written to a file, and the deserialized message will be an empty string

4.) The agent polls for new messages in the channel, waiting for messages designated for its GUID

5.) The agent deserializes the message and performs the requested tasks

## Configuration Options
The profile reads a `config.json` file for the required tokens, channel to write to, and whether to output debug messages.

```JSON
{
  "subscription_token": "xapp-1-dsfsdfsdfsd~",
  "message_token": "xoxb-15645453~",
  "channel_id": "D04F773RT5F",
  "debug": false,
  "clear_messages": true
}
```

 If debug is enabled, you should get a notification when the server starts with information about the configuration:

```
[23:26:10.4666021] Channel: D04F773RT5F
[23:26:10.4748411] Subscription Token: xapp-1-dsfsdfsdfsd~
[23:26:10.4751146] Message Token: xoxb-15645453~
Server Started!
```

A note about debugging:
- With `debug` set to `true`, you'll be able to `view stdout/stderr` from within the UI for the container, but it's not recommended to always have this on (especially if you start using something like SOCKS). There can be a lot of traffic and a lot of debugging information captured here which can be both a performance and memory bottleneck depending on your environment and operational timelines.
- It's recommended to have it on initially to help troubleshoot payload connectivity and configuration issues, but then to set it to `false` for actual operations

## Generating a token

An app can be created with the following manifest. Replacing `display_name` and `name` with your preferred values:

```
display_information:
  name: NameGoesHere
features:
  bot_user:
    display_name: NameGoesHere
    always_online: false
oauth_config:
  scopes:
    bot:
      - channels:history
      - channels:join
      - chat:write
      - chat:write.customize
      - chat:write.public
      - files:read
      - files:write
      - reactions:read
      - reactions:write
settings:
  event_subscriptions:
    bot_events:
      - file_shared
      - message.channels
  interactivity:
    is_enabled: true
  org_deploy_enabled: false
  socket_mode_enabled: true
  token_rotation_enabled: false
```

### Profile Options
#### Slack Channel ID
The channel ID where slack messages will be written to.

#### Slack Message Token
A token that will be used by the agent to read and write messages to the associate channel.

#### Message Checks
How many times to check for a response from the server before assuming something went wrong with the message send.

#### Time Between Checks
How long to wait between each message check.

#### Callback Interval
A number to indicate how many seconds the agent should wait in between tasking requests.

#### Callback Jitter
Percentage of jitter effect for callback interval.

#### Crypto Type
Indicate if you want to use no crypto (i.e. plaintext) or if you want to use Mythic's aes256_hmac. Using no crypto is really helpful for agent development so that it's easier to see messages and get started faster, but for actual operations you should leave the default to aes256_hmac.

#### Perform Key Exchange
T or F for if you want to perform a key exchange with the Mythic Server. When this is true, the agent uses the key specified by the base64 32Byte key to send an initial message to the Mythic server with a newly generated RSA public key. If this is set to `F`, then the agent tries to just use the base64 of the key as a static AES key for encryption. If that key is also blanked out, then the requests will all be in plaintext.

#### User Agent
The User Agent to be passed in the HTTP requests for calls to the REST API

#### Proxy Host
If you need to manually specify a proxy endpoint, do that here. This follows the same format as the callback host.

#### Proxy Password
If you need to authenticate to the proxy endpoint, specify the password here.

#### Proxy Username
If you need to authenticate to the proxy endpoint, specify the username here.

#### Proxy Port
If you need to manually specify a proxy endpoint, this is where you specify the associated port number.

#### Kill Date
Date for the agent to automatically exit, typically the after an assessment is finished.

## OPSEC
All message contents are written in plaintext and will be available to anyone who has permissions to read messages within your slack server. To avoid accidental leakage of operational data, ensure that encryption is being used for the actual message contents.

## Development

All of the code for the server is .NET 6 and located in `C2_Profiles/slack/c2_code/slack-server`.

The server will be notified of any new messages written in the specified channel, it will deserialize the message and attempt to determine whether `to_server` is set to `true` or `false` if `true` then it forwards the message to the Mythic server and writes the response to the channel. If not, it does nothing.

The message format serializes into the following JSON:

```
{
  "message":"base64encoded mythic message",
  "sender_id":"GUID",
  "to_server":false,
  "id":1,
  "final":true
  }
```

The `message` parameter contains the raw mythic message to be forwarded to the server. If the message parameter is empty, the agent/server should check for a file for the contents of the mythic message. A good rule of thumb is if the message is larger than 3850 characters, then it should be uploaded as a file.

The `sender_id` is a guid generated by the agent to be included with every message. Any messages with an agents generated GUID and the to_server parameter set to `false` are intended to be processed by the agent.

THe `to_server` parameter indicates the intended recipient. If set to `true` the server will process the message, if `false` the agent is meant to.

The `id` parameter is not currently in use, so safe to just set it to `1` or `0` for now.

The `final` parameter is not currently in use, so safe to just set it to `true` for now.