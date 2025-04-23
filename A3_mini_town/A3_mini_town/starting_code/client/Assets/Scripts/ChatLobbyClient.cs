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
    private int ID = -1;
    private bool accepted;

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
        sendChat(pText);
    }

    private void sendChat(string pOutString)
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
                if (accepted == false) 
                { 
                    if (readObject is AcceptClientMessage acceptClientMessage)
                    {
                        ID = acceptClientMessage.GetId();
                        accepted = true;
                    }
                }
                else
                {
                    if (readObject is ServerChatMessage serverChatMessage)
                    {
                        string message = serverChatMessage.readText();
                        Debug.Log("Received:" + message);
                        showMessage(message, serverChatMessage.getSenderID());
                    }
                    else if (readObject is UpdateAvatarMessage updateAvatarMessage)
                    {
                        shared.Avatar readAvatar = updateAvatarMessage.GetAvatar();
                        if (!_avatarAreaManager.HasAvatarView(readAvatar.GetID()))
                        {
                            _avatarAreaManager.AddAvatarView(readAvatar.GetID());
                        }
                        AvatarView view = _avatarAreaManager.GetAvatarView(readAvatar.GetID());
                        view.Move(new Vector3(readAvatar.GetPos().X, readAvatar.GetPos().Y, readAvatar.GetPos().Z));
                        view.SetSkin(readAvatar.getSkinID());
                    }
                    else if (readObject is UpdateAllAvatarsMessage updateAllAvatarsMessage)
                    {
                        _avatarAreaManager.Clear();
                        foreach (shared.Avatar readAvatar in updateAllAvatarsMessage.GetAvatars())
                        {
                            AvatarView view = _avatarAreaManager.AddAvatarView(readAvatar.GetID());
                            if (readAvatar.GetID()==ID)
                            {
                                view.ShowRing(true);
                            }
                            view.Teleport(new Vector3(readAvatar.GetPos().X, readAvatar.GetPos().Y, readAvatar.GetPos().Z));
                            view.SetSkin(readAvatar.getSkinID());
                        }
                    }
                    else if (readObject is RemoveAvatarMessage removeAvatarMessage)
                    {
                        _avatarAreaManager.RemoveAvatarView(removeAvatarMessage.GetID());
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

    private void showMessage(string pText, int senderID)
    {
        AvatarView avatarView = _avatarAreaManager.GetAvatarView(senderID);
        avatarView.Say(pText);
    }

    private static void HeartBeat()
    {
        while (true)
        {
            StreamUtil.WriteObject(client.GetStream(), new HeartBeatMessage());
            Thread.Sleep(500);
        }
    }

}
