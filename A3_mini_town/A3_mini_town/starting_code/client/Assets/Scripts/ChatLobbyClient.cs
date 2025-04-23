using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/**
 * The main ChatLobbyClient where you will have to do most of your work.
 * 
 * @author J.C. Wichman
 */
public class ChatLobbyClient : MonoBehaviour
{
    private static Thread heartbeatThread = new Thread(HeartBeat);
    //reference to the helper class that hides all the avatar management behind a blackbox
    private AvatarAreaManager _avatarAreaManager;
    //reference to the helper class that wraps the chat interface
    private PanelWrapper _panelWrapper;

    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    private static TcpClient client;
    private shared.Avatar avatar;
    private readonly Dictionary<int,shared.Avatar> avatarDictionary = new Dictionary<int, shared.Avatar>();

    private void Start()
    {
        connectToServer();

        //register for the important events
        _avatarAreaManager = FindFirstObjectByType<AvatarAreaManager>();
        _avatarAreaManager.OnAvatarAreaClicked += onAvatarAreaClicked;

        _panelWrapper = FindFirstObjectByType<PanelWrapper>();
        _panelWrapper.OnChatTextEntered += onChatTextEntered;

        heartbeatThread.Start();
    }

    private void connectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(_server, _port);
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to server:");
            Debug.Log(e.Message);
        }
    }

    private void onAvatarAreaClicked(Vector3 pClickPosition)
    {
        Debug.Log("ChatLobbyClient: you clicked on " + pClickPosition);
        //TODO pass data to the server so that the server can send a position update to all clients (if the position is valid!!)
        StreamUtil.WriteObject(client.GetStream(), new ClientMoveRequest(pClickPosition.x, pClickPosition.y, pClickPosition.z));
    }

    private void onChatTextEntered(string pText)
    {
        _panelWrapper.ClearInput();
        sendString(pText);
    }

    private void sendString(string pOutString)
    {
        try
        {
            //we are still communicating with strings at this point, this has to be replaced with either packet or object communication
            Debug.Log("Sending:" + pOutString);
            StreamUtil.WriteObject(client.GetStream(), new ClientChatMessage(pOutString));
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            client.Close();
            connectToServer();
        }
    }

    // RECEIVING CODE

    private void Update()
    {
        try
        {
            if (client.Available > 0)
            {
                ISerializable readObject = StreamUtil.ReadObject(client.GetStream());
                if (avatar == null) 
                { 
                    if (readObject is AcceptClientMessage acceptClientMessage)
                    {
                        avatar = acceptClientMessage.GetAvatar();
                    }
                }
                else
                {
                    if (readObject is ServerChatMessage serverChatMessage)
                    {
                        string message = serverChatMessage.readText();
                        Debug.Log("Received:" + message);
                        showMessage(message);
                    }
                    else if (readObject is UpdateAvatarMessage updateAvatarMessage)
                    {
                        shared.Avatar readAvatar = updateAvatarMessage.GetAvatar();
                        if (readAvatar.GetID() == avatar.GetID())
                            avatar = readAvatar;
                        else
                            avatarDictionary[readAvatar.GetID()] = readAvatar;
                    }
                    else if (readObject is UpdateAllAvatarsMessage updateAllAvatarsMessage)
                    {
                        avatarDictionary.Clear();
                        foreach (shared.Avatar readAvatar in updateAllAvatarsMessage.GetAvatars())
                        {
                            avatarDictionary[readAvatar.GetID()] = readAvatar;
                        }
                    }
                    else if (readObject is RemoveAvatarMessage removeAvatarMessage)
                    {
                        avatarDictionary.Remove(removeAvatarMessage.GetID());
                    }
                }
            }
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            client.Close();
            connectToServer();
        }
    }

    private void showMessage(string pText)
    {
        //This is a stub for what should actually happen
        //What should actually happen is use an ID that you got from the server, to get the correct avatar
        //and show the text message through that
        List<int> allAvatarIds = _avatarAreaManager.GetAllAvatarIds();
        
        if (allAvatarIds.Count == 0)
        {
            Debug.Log("No avatars available to show text through:" + pText);
            return;
        }

        int randomAvatarId = allAvatarIds[UnityEngine.Random.Range(0, allAvatarIds.Count)];
        AvatarView avatarView = _avatarAreaManager.GetAvatarView(randomAvatarId);
        avatarView.Say(pText);
    }

    private static void HeartBeat()
    {
        StreamUtil.WriteObject(client.GetStream(), new HeartBeatMessage());
        Thread.Sleep(500);
    }

}
