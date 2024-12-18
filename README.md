# slack

```FYSA: Currently looking for new maintainers, no agents currently support this profile and it may not even work in general.```


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

Browse to C2 Profiles, then click on the dropdown arrow next to `Start Profile`, then click `View/Edit Config` to change the above values.

# Slack App Setup

1. Browse to https://api.slack.com/apps
2. Click `Create New App`
3. Choose your own workspace to add this app to on Step 1
4. On Step 2, choose YAML, then copy the below manifest template, swapping out `NameGoesHere` for any arbitrary app name
5. Confirm the Bot Scopes (which should read `channels:history, channels:join, chat:write, chat:write.customize, chat:write.public, files:read, files:write, reactions:read, reactions:write`), then click Create
6. Your app has been created. Click `Install to Workspace` under `Install your app`, then select your Slack workspace.
7. You will now be at the application page for Slack, something like https://api.slack.com/apps/APP_GUID_HERE/general?success=1

For ease of setup the following YAML manifest can be used when creating a new app and installing it to a workspace:

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

You now have most of what you need for the slack C2 configuration.

 - For `subscription_token`, make an App-Level Token (Slack app web site -> Basic Information -> App-Level Tokens -> Generate Token and Scopes) with the `connections:write` scope, click Generate, then copy the value beginning with `xapp-`
 - For `message_token`, browse from the main Slack app page to the `OAuth & Permissions` page, then copy the `Bot User OAuth Token` beginning with `xoxb-`
 - For `channel_id`, log in to the web version of your Slack workspace, navigate to the channel you'd like the Slack C2 to use, then copy the last part of the URL (example: `https://app.slack.com/client/T01LKD0B2AW/C03F752RT5E` is the full URL, `T01LKD0B2AW` is the workspace ID, while `C03F752RT5E` is the channel ID)
 - `debug` will display debug messages while the server is operating, however this will cause a decent hit to server performance and a lot of output. It's recommended to only use this for small bursts during testing.
 - `clear_messages` instructs the server to clear the channel of messages before it starts, this may take some time depending on how many messages need to be deleted.
