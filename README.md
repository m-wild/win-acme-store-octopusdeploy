# Plugin.Store.OctopusDeploy

[win-acme](https://github.com/win-acme/win-acme) plugin to store certificates in [Octopus Deploy](https://octopus.com/) server.

## Octopus URL

This should be the base URL of your Octopus Deploy server, something like `"https://octopus.example.com"`

## Octopus API Key

This can be obtained from **My Profile > My API Keys > New API Key**. You probably want to set this not to expire as it will be saved in the renewal config JSON.

## Unattended usage

```
--store octopusdeploy --octopusurl "https://octopus.example.com" --octopusapikey "API-KEY-HERE" [--octopusspaceid "Spaces-1"] [--octopusenvironmentid "Environments-1"]
```


## TODO

- [x] Basic plugin to upload certs to Octopus Deploy
- [ ] Port to simple-acme
- [ ] Github actions to build and create releases
- [ ] Support Octopus tenant options