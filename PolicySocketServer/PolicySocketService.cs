using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace PolicySocketServer
{
    public partial class PolicySocketService : ServiceBase
    {

        //TCP Client
        private Socket m_listener;

        //TCP Port
        private int m_listenerPort;

        //the policy file name 
        private string m_policyFileName;

        // the policy to return to the client
        private byte[] m_policy;

        public PolicySocketService()
        {
            InitializeComponent();
            this.ServiceName = "Policy Socket Service";
            if (!System.Diagnostics.EventLog.SourceExists("PolicySocketService"))
            {
                System.Diagnostics.EventLog.CreateEventSource("PolicySocketService", "Listener Log");
            }
            eventLog1.Source = "PolicySocketService";
            eventLog1.Log = "Listener Log";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Starting Policy Socket Service");
            initializeSocketService();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Stopping Policy Socket Service");
            m_listener.Close();
        }

        private void initializeSocketService()
        {
            eventLog1.WriteEntry("Starting Socket Listener");
            //get the policy file name
            m_policyFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\" + Properties.Settings.Default.policyfilename;

            //get the listener port
            m_listenerPort = Properties.Settings.Default.listenerport;

            //read the policy file
            FileStream policyStream = new FileStream(m_policyFileName, FileMode.Open);
            m_policy = new byte[policyStream.Length];
            policyStream.Read(m_policy, 0, m_policy.Length);
            policyStream.Close();

            //Create the Listening Socket
            m_listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            //Put the socket into dual mode to allow a single socket
            // to accept both IPv4 and IPv6 connections
            // Otherwise, server needs to listen on two sockets,
            // one for IPv4 and one for IPv6
            // NOTE: dual-mode sockets are supported on Vista and later
            m_listener.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, 0);

            m_listener.Bind(new IPEndPoint(IPAddress.IPv6Any, m_listenerPort));
            m_listener.Listen(10);

            m_listener.BeginAccept(new AsyncCallback(OnConnection), null); 
        }

        // Called when we receive a connection from a client
        public void OnConnection(IAsyncResult res)
        {
            Socket client = null;

            try
            {
                client = m_listener.EndAccept(res);
            }
            catch (SocketException)
            {
                return;
            }

            // handle this policy request with a PolicyConnection
            PolicyConnection pc = new PolicyConnection(client, m_policy);

            // look for more connections
            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }
    }

    // Encapsulate and manage state for a single connection from a client
    class PolicyConnection
    {
        private Socket m_connection;

        // buffer to receive the request from the client
        private byte[] m_buffer;
        private int m_received;

        // the policy to return to the client
        private byte[] m_policy;

        // the request that we're expecting from the client
        private static string s_policyRequestString = "<policy-file-request/>";



        public PolicyConnection(Socket client, byte[] policy)
        {
            m_connection = client;
            m_policy = policy;

            m_buffer = new byte[s_policyRequestString.Length];
            m_received = 0;

            try
            {
                // receive the request from the client
                m_connection.BeginReceive(m_buffer, 0, s_policyRequestString.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException)
            {
                m_connection.Close();
            }
        }

        // Called when we receive data from the client
        private void OnReceive(IAsyncResult res)
        {
            try
            {
                m_received += m_connection.EndReceive(res);

                // if we haven't gotten enough for a full request yet, receive again
                if (m_received < s_policyRequestString.Length)
                {
                    m_connection.BeginReceive(m_buffer, m_received, s_policyRequestString.Length - m_received, SocketFlags.None, new AsyncCallback(OnReceive), null);
                    return;
                }

                // make sure the request is valid
                string request = System.Text.Encoding.UTF8.GetString(m_buffer, 0, m_received);
                if (StringComparer.InvariantCultureIgnoreCase.Compare(request, s_policyRequestString) != 0)
                {
                    m_connection.Close();
                    return;
                }

                // send the policy
                m_connection.BeginSend(m_policy, 0, m_policy.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (SocketException)
            {
                m_connection.Close();
            }
        }

        // called after sending the policy to the client; close the connection.
        public void OnSend(IAsyncResult res)
        {
            try
            {
                m_connection.EndSend(res);
            }
            finally
            {
                m_connection.Close();
            }
        }
    }
}
