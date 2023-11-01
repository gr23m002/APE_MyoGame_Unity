using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UDPManager : MonoBehaviour
    {
        private const string ServerGreeting = "iAjZ3hJGmh6cQd";
        private const string ClientGreeting = "G2SxAQMAS4mPV8";
        private const int Port = 2390;
        
        private readonly IPEndPoint _broadcastEndpoint = new(IPAddress.Parse("255.255.255.255"), Port);
        private const int BroadcastInterval = 5;
        private UdpClient _udpServer;

        private IPEndPoint _remoteEndPoint;
        private IPEndPoint _arduinoEndPoint;
        private bool _isConnected;

        private Slider _e1Slider;
        private Slider _e2Slider;
        
        private int _e1Value = -1;
        private int _e2Value = -1;
        
        private void Start()
        {
            StartUDPServer(Port);
            _e1Slider = GameObject.Find("E1 Slider").GetComponent<Slider>();
            _e2Slider = GameObject.Find("E2 Slider").GetComponent<Slider>();
            
            _e1Slider.value = 0;
            _e2Slider.value = 0;
        }

        private IEnumerator BroadcastPairingMessage()
        {
            while (!_isConnected)
            {
                Debug.Log("Broadcasting...");
                SendData(ServerGreeting, _broadcastEndpoint);

                yield return new WaitForSeconds(BroadcastInterval);
            }
        }

        private void StartUDPServer(int port)
        {
            _udpServer = new UdpClient(port);
            StartCoroutine(BroadcastPairingMessage());
            _udpServer.BeginReceive(ReceiveData, null);
        }

        private void Update()
        {
            if (!_isConnected) return;
            
            if (_e1Value > -1) _e1Slider.value = _e1Value;
            if (_e2Value > -1) _e2Slider.value = _e2Value;
        }

        private void ReceiveData(IAsyncResult result)
        {
            byte[] receivedBytes = _udpServer.EndReceive(result, ref _remoteEndPoint);
            string message = System.Text.Encoding.UTF8.GetString(receivedBytes);
            
            Debug.Log("Received from client: " + message);

            if (message == ClientGreeting)
            {
                _arduinoEndPoint = _remoteEndPoint;
                _isConnected = true;
                SendData("ACK", _arduinoEndPoint);
            }

            if (message.StartsWith("M;"))
            {
                _e1Value = int.Parse(message.Split(";")[1]);
                _e2Value = int.Parse(message.Split(";")[2]);
            }
            
            _udpServer.BeginReceive(ReceiveData, null); // Continue listening
        }

        private void SendData(string message, IPEndPoint endPoint)
        {
            byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(message);

            _udpServer.Send(sendBytes, sendBytes.Length, endPoint);
        
            Debug.Log("Sent to client: >" + message + "<");
        }
    }
}
