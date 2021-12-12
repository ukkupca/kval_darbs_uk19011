﻿
using System.Collections.Generic;
using GameManagerData.data;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Controllers
{
    public class EmptyActiveSocketController : MonoBehaviour
    {
        public static List<EmptyActiveSocketData> EmptyActiveSocketData = new List<EmptyActiveSocketData>();
        private static SocketController _controller = new SocketController();

        public static void AddData(EmptyActiveSocketData data)
        {
            EmptyActiveSocketData.Add(data);
            Debug.Log("New controller data created " + data.controllerID);
        }
        
        public static void RemoveData(EmptyActiveSocketData data)
        {
            EmptyActiveSocketData.Remove(data);
        }

        public static void AddSocket(string controllerID, XRSocketInteractor socket)
        {
            foreach (var data in EmptyActiveSocketData)
            {
                Debug.Log("Adding socket");
                if (controllerID == data.controllerID)
                {
                    data.emptyActiveSockets.Add(socket);
                    return;
                }
            }
        }
        
        public static void RemoveSocket(string controllerID, XRSocketInteractor socket)
        {
            foreach (var data in EmptyActiveSocketData)
            {
                if (controllerID == data.controllerID)
                {
                    data.RemoveSocket(socket);
                }
            }
        }

        public static void TurnOnAllSockets()
        {
            foreach (var data in EmptyActiveSocketData)
            {
                if (data.isControllerEmpty)
                {
                    _controller.TurnOnControllerSocket(data.controllerSocket);
                    break;
                }
                foreach (var socket in data.emptyActiveSockets)
                {
                    _controller.TurnOnSocket(socket);
                }
            }
        }
        
        public static void TurnOffAllSockets()
        {
            foreach (var data in EmptyActiveSocketData)
            {
                if (data.isControllerEmpty)
                {
                    _controller.TurnOffControllerSocket(data.controllerSocket);
                    break;
                }
                foreach (var socket in data.emptyActiveSockets)
                {
                    _controller.TurnOffSocket(socket);
                }
            }
        }

        public static void TurnOnAllForSpecificHome(string controllerID)
        {
            foreach (var data in EmptyActiveSocketData)
            {
                if (data.isControllerEmpty)
                {
                    _controller.TurnOnControllerSocket(data.controllerSocket);
                    break;
                }
                if (controllerID == data.controllerID)
                {
                    foreach (var socket in data.emptyActiveSockets)
                    {
                        _controller.TurnOnSocket(socket);
                    }
                }
            }
        }
        
        public static void TurnOffAllForSpecificHome(string controllerID)
        {
            foreach (var data in EmptyActiveSocketData)
            {
                // if (data.isControllerEmpty)
                // {
                //     _controller.TurnOffControllerSocket(data.controllerSocket);
                //     break;
                // }
                
                Debug.Log("Turning off this: " + data.controllerID);
                if (controllerID == data.controllerID)
                {
                    foreach (var socket in data.emptyActiveSockets)
                    {
                        Debug.Log("Turning off this: " + socket);
                        _controller.TurnOffSocket(socket);
                    }
                }
            }
        }
    }
}