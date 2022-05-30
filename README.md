# slack

This is a Mythic C2 Profile called slack. It uses the slack REST API to receive messages and forward them to the Mythic server. This profile supports:

* Kill Dates
* Sleep Intervals
* Custom Headers
* Proxy Information
* Encryption of channel messages

The c2 profile has `mythic_c2_container==0.0.23` PyPi package installed and reports to Mythic as version "4".

## How to install an agent in this format within Mythic

When it's time for you to test out your install or for another user to install your c2 profile, it's pretty simple. Within Mythic you can run the `mythic-cli` binary to install this in one of three ways:

* `sudo ./mythic-cli install github https://github.com/MythicC2Profiles/slack` to install the main branch
* `sudo ./mythic-cli install github https://github.com/MythicC2Profiles/slack branchname` to install a specific branch of this repo

Now, you might be wondering _when_ should you or a user do this to properly add your profile to their Mythic instance. There's no wrong answer here, just depends on your preference. The three options are:

# Configuring proper tokens

The c2 profile handles configuration using a `config.json` file which can be modified within the Mythic instance. It contains 5 parameters

* subscription_token
* message_token
* channel_id
* debug
* clear_messages

# Slack App Setup

For ease of setup the following manifest can be used when creating a new app and installing it to a workspace:

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

You'll then grab a bot token and an app-level token. The app-level token can be found under `Basic Information` on the left side of your app page. It should have the `connections:write` permission and will be the value for the `subscription_token` config parameter. 

The bot token can be found under `OAuth & Permissions` and is provided to you when you install the bot to your workspace. Ensure the bot token starts with `xoxb-` it will be the value for the `message_token` parameter. 

If you're creating the app manually, the bot should have the scopes outlined in the above manifest, events must be enabled, and `Socket Mode` must be enabled. 

The `channel_id` can be discovered by logging into the Web version of the slack application. Navigate to the channel you want to use for your c2 and copy the last part of the URL. ex.)
```
https://app.slack.com/client/T01LKD0B2AW/C03F752RT5E
```

`T01LKD0B2AW` is the workspace ID, while `C03F752RT5E` is the channel ID. You'll place the channel ID in the config.json file.

`debug` will display debug messages while the server is operating, however this will cause a decent hit to server performance and a lot of output. It's recommended to only use this for small bursts during testing.

`clear_messages` instructs the server to clear the channel of messages before it starts, this may take some time depending on how many messages need to be deleted.
