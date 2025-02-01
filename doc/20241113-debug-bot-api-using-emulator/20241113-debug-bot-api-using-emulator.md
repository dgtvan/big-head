This is how to test the BotApi using an emulator channel.

1/ Start BOT API using the `Emulator` profile.
![Emulator profile](20241113-debug-emulator-profile.png "Emulator profile")

2/ Start `Bot Framework Emulator` with the following configuration. Click `Connect`.
![Emulator profile](20241113-debug-run-emulator.png "Emulator profile")

3/ Test connection.
![Emulator profile](20241113-debug-test-emulator.png "Emulator profile")

## Known issue

1/ The API throws the error "No Authentication header".

You must make sure the `BOT_ID` and `BOT_PASSWORD` be empty in the `appsettings.json`, `appsettings.Emulator.json`, and `secret.json` (if you are using User Secret).