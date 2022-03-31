# Windows Credentials Manager Updater Utility App
Small utility that updates the username and/or password for your desired servers under Windows Credentials Manager

# About the code
- It is written in .NET Framework 4.7.2

# How to use?
It's really simple, you just need to perform the following steps:

## Create your JSON File
The app reads the server list from a JSON file with the following format:

```json
{
  "ServerList": [
    {
      "Description": "Some Server #1",
      "Server": "SMSVR1"
    },
    {
      "Description": "Some Server #2",
      "Server": "192.168.1.10"
    },
  ]
}
```

## Run the App
Once the JSON file has been created you just need to run the app and pick the JSON file:

![Main Window Screen](Screenshots/Main.png?raw=true "Main Window Screen")

Hit update on the screen and it will update the servers under Windows Credentials Manager for you:

![Windows Credentials Manager](Screenshots/WCM.png?raw=true "Windows Credentials Manager")

That's it. I hope it is useful to you. Thanks.