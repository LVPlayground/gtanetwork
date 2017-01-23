using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;

public class AdminScript : Script
{
	public AdminScript()
	{
		API.onPlayerRespawn += onDeath;
		API.onPlayerConnected += OnPlayerConnected;
		API.onUpdate += onUpdate;
		API.onResourceStart += onResStart;
		API.onPlayerDisconnected += onPlayerDisconnected;
	}

    [Command(SensitiveInfo = true, ACLRequired = true)]
    public void Login(Client sender, string password)
    {
        var logResult = API.loginPlayer(sender, password);
        switch (logResult)
        {
            case 0:
                API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ No account found with your name.");
                break;
            case 3:
            case 1: {
                    var playerAclGroup = API.getPlayerAclGroup(sender);
                    API.sendChatMessageToPlayer(sender, "~g~Login successful!~w~ Logged in as ~b~" + playerAclGroup + "~w~. Please check '/help'.");
                    foreach (var player in API.getAllPlayers())
                    {
                        if (API.isPlayerLoggedIn(player) && player != sender) {
                            API.sendChatMessageToPlayer(player, string.Format("Player '{0}' from group '{1}' is now loggedin.", sender.name, playerAclGroup));
                        }
                    }
                    break;
                }
            case 2:
                API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ Wrong password!");
                break;
            case 4:
                API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ You're already logged in!");
                break;
            case 5:
                API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ ACL has been disabled on this server.");
                break;
        }
    }

    [Command(ACLRequired = true)]
    public void SetTime(Client sender, int hours, int minutes)
    {
        API.setTime(hours, minutes);
    }

    [Command(ACLRequired = true)]
    public void SetWeather(Client sender, int newWeather)
    {
        API.setWeather(newWeather);
    }

    [Command(ACLRequired = true)]
    public void Logout(Client sender)
    {
        API.logoutPlayer(sender);
    }

   [Command(ACLRequired = true)]
    public void Start(Client sender, string resource)
    {
        if (!API.doesResourceExist(resource))
        {
            API.sendChatMessageToPlayer(sender, "~r~No such resource found: '" + resource + "'");
        }
        else if (API.isResourceRunning(resource))
        {
            API.sendChatMessageToPlayer(sender, "~r~Resource '" + resource + "' is already running!");
        }
        else
        {
            API.startResource(resource);
            API.sendChatMessageToPlayer(sender, "~g~Started resource '" + resource + "'");
        }
    }

    [Command(ACLRequired = true)]
    public void Stop(Client sender, string resource)
    {
        if (!API.doesResourceExist(resource))
        {
            API.sendChatMessageToPlayer(sender, "~r~No such resource found: '" + resource + "'");
        }
        else if (!API.isResourceRunning(resource))
        {
            API.sendChatMessageToPlayer(sender, "~r~Resource '" + resource + "' is not running!");
        }
        else
        {
            API.stopResource(resource);
            API.sendChatMessageToPlayer(sender, "~g~Stopped resource '" + resource + "'");
        }
    }


    [Command(ACLRequired = true)]
    public void Restart(Client sender, string resource)
    {
        if (API.doesResourceExist(resource))
        {
            API.stopResource(resource);
            API.startResource(resource);

            API.sendChatMessageToPlayer(sender, "~g~Restarted resource '" + resource + "'");
        }
        else
        {
            API.sendChatMessageToPlayer(sender, "~r~No such resource found: '" + resource + "'");
        }
    }

    [Command(GreedyArg = true, ACLRequired = true)]
    public void Kick(Client sender, Client target, string reason)
    {
        API.kickPlayer(target, reason);
    }

    [Command(ACLRequired = true)]
    public void Kill(Client sender, Client target)
    {
        API.setPlayerHealth(target, -1);
    }

	public void onPlayerDisconnected(Client player, string reason)
	{
		API.logoutPlayer(player);
	}

	public void OnPlayerConnected(Client player)
    {    	
        var log = API.loginPlayer(player, "");
        if (log == 1)
        {
        	API.sendChatMessageToPlayer(player, "Logged in as ~b~" + API.getPlayerAclGroup(player) + "~w~.");
        }
        else if (log == 2)
        {
			API.sendChatMessageToPlayer(player, "Please log in with ~b~/login [password]");
        }
    }

    [Command(ACLRequired = true)]
    public void Help(Client player)
    {
        API.sendChatMessageToPlayer(player, "/settime, /setweather, /kick, /kill, /logout");
    }

    [Command("v", ACLRequired = true)]
    public void onVehicleCommand(Client player, string vehicleIdOrName)
    {
        var playerPosition = API.getEntityPosition(player);
        var playerRotation = API.getEntityRotation(player);
        var vehicleDictionary = new Dictionary<string, VehicleHash>();

        foreach (var vehicleHash in Enum.GetValues(typeof(VehicleHash)).Cast<VehicleHash>())
        {
            vehicleDictionary.Add(vehicleHash.ToString(), vehicleHash);
        }

        var vehicle = vehicleDictionary.FirstOrDefault(v => v.Key.StartsWith(vehicleIdOrName)).Value;

        API.createVehicle(vehicle, playerPosition, playerRotation, 0, 0);
    }
}
