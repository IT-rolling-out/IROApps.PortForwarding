# IROApps.PortForwarding

Your own webhooks server, analog of ngrok for heroku.

Files not supported.

# Instruction

1. Deploy this to Heroku via deploy button. Set your own `ADMIN_KEY`.

[![Deploy](https://www.herokucdn.com/deploy/button.svg)](https://heroku.com/deploy?template=https://github.com/IT-rolling-out/IROApps.PortForwarding)

2. Build `IROApps.PortForwarding.ClientApp` and launch it with current arguments:

`/AddressTo https://localhost:5001 /Server https://your-app.heroku.com /AdminKey pass`

Where `AddressTo` - your local address and `Server` - your heroku application.