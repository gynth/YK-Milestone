using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCommandWrapper;
using ServerCommandWrapper.Basic;
using ServerCommandWrapper.Ntlm;

namespace Milestone
{
    class SystemAccess
    {
        private static readonly Guid IntegrationId = new Guid("BE07504F-B330-4475-9AE4-1A7FF10BD486");
        private const string IntegrationName = "TCP Video Viewer";
        private const string Version = "1.0";

        public string Token;
        //public LoginInfo LoginInfo;
        //public AuthenticationType AuthenticationType = AuthenticationType.WindowsDefault;
        public AuthenticationType AuthenticationType = AuthenticationType.Basic;
        private NtlmConnection _ntlmConnection;
        private BasicConnection _basicConnection;

        public String Server = "localhost";
        public String User = "";
        public String Password = "";
        public String Domain = "";

        public SystemAccess(string server, string token)
        {
            this.Server = server;
            this.Token = token;
        }

        public void Connect(String server)
        {
            if (_basicConnection != null)
            {
                _basicConnection.Logout();
                _basicConnection = null;
            }
            if (_ntlmConnection != null)
            {
                _ntlmConnection.Logout();
                _ntlmConnection = null;
            }
            Server = server;
            switch (AuthenticationType)
            {
                case AuthenticationType.Basic:
                    {
                        int port = 443;
                        _basicConnection = new BasicConnection("admin", "admin", Server, port);
                        _basicConnection.Login(IntegrationId, Version, IntegrationName);
                        break;
                    }
                case AuthenticationType.Windows:
                case AuthenticationType.WindowsDefault:
                    {
                        _ntlmConnection = new NtlmConnection("", AuthenticationType, "", "", Server);
                        _ntlmConnection.Login(IntegrationId, Version, IntegrationName);
                        break;
                    }
                default:
                    //empty
                    break;
            }
        }

        public List<Camera> GetSystemCameras()
        {
            if (this.Token == null)
                return new List<Camera>();

            Connect(Server);

            switch (AuthenticationType)
            {
                case AuthenticationType.Basic:
                    _basicConnection.GetConfiguration(this.Token);
                    return ExtractCameraDataFrom(_basicConnection.ConfigurationInfo);

                case AuthenticationType.Windows:
                case AuthenticationType.WindowsDefault:
                    _ntlmConnection.GetConfiguration(this.Token);
                    return ExtractCameraDataFrom(_ntlmConnection.ConfigurationInfo);

                default:
                    return new List<Camera>();
            }
        }

        public List<Camera> ExtractCameraDataFrom(ServerCommandService.ConfigurationInfo confInfo)
        {
            List<Camera> cameras = new List<Camera>();
            foreach (ServerCommandService.RecorderInfo recorder in confInfo.Recorders)
            {
                foreach (ServerCommandService.CameraInfo cameraInfo in recorder.Cameras)
                {
                    Camera cam = new Camera();

                    int colonIndex = recorder.WebServerUri.LastIndexOf(':');
                    int slashIndex = recorder.WebServerUri.LastIndexOf('/');
                    String portStr = recorder.WebServerUri.Substring(colonIndex + 1, slashIndex - colonIndex - 1);

                    cam.Guid = cameraInfo.DeviceId;
                    cam.Name = cameraInfo.Name;
                    cam.RecorderUri = new Uri(recorder.WebServerUri);

                    cameras.Add(cam);
                }
            }

            return cameras;
        }

        public struct Camera
        {
            /// <summary>
            /// The GUID of the camera
            /// </summary>
            public Guid Guid;

            /// <summary>
            /// Name of the camera
            /// </summary>
            public String Name;

            /// <summary>
            /// Uri of where the camera is located
            /// </summary>
            public Uri RecorderUri;
        }
    }
}
