using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using VideoOS.Platform;
using VideoOS.Platform.SDK.Platform;

namespace Milestone
{
    //object[] info
    //0: token
    //1: device
    //2: call
    //3: PTZ flag(up, down.....)
    //4: Scale No. 배차번호
    //5: Rec Owner - '0':자동녹화, '1':수동녹화
    //6: Camera Name
    //7: User Id
    //8: rec_fr_dttm
    //9: rec_to_dttm

    //1: Scale No. 배차번호
    //2: Rec Yn    - '0':녹화중, '1':녹화대기, '2':녹화재시작 확인, '3': 녹화시작실패, '4': 녹화종료실패
    //3: Rec Dt

    public class Startup
    {
        string token = "";
        string device = "";

        string[] imgRtn = new string[3];
        string[] recRtn = new string[3];
        
        public byte[] LastJPEG { get; private set; } = null;
        string MILESTONE_IP = "10.10.10.136";
        string compRate = "50";

        RecorderCommandService.RecorderCommandService rcs = new RecorderCommandService.RecorderCommandService();

        public Startup()
        {

        }
        

        public async Task<object> getToken(object[] info)
        {
            try
            {
                //ServerCommandWrapper.Ntlm.NtlmConnection Connect = new ServerCommandWrapper.Ntlm.NtlmConnection("", AuthenticationType.WindowsDefault, "", "", info[0].ToString(), 80);

                ServerCommandWrapper.Basic.BasicConnection Connect = new ServerCommandWrapper.Basic.BasicConnection("admin", "admin", info[0].ToString(), 443);

                Connect.Login(new Guid("BE07504F-B330-4475-9AE4-1A7FF10BD486"), "1.0", "");

                string[] rtn = new string[4];
                rtn[0] = info[0].ToString();
                rtn[1] = Connect.LoginInfo.Token;
                rtn[2] = Connect.LoginInfo.TimeToLive.ToString();
                rtn[3] = Connect.LoginInfo.RegistrationTimeField.ToString();

                return rtn;
            }
            catch (Exception e)
            {
                string[] rtn = new string[4];
                rtn[0] = info[0].ToString();
                rtn[1] = e.Message;
                rtn[2] = "";
                rtn[3] = "";

                return rtn;
            }
        }

        public string getTokenTemp(object[] info)
        {
            try
            {
                //ServerCommandWrapper.Ntlm.NtlmConnection Connect = new ServerCommandWrapper.Ntlm.NtlmConnection("", AuthenticationType.WindowsDefault, "", "", info[0].ToString(), 80);

                ServerCommandWrapper.Basic.BasicConnection Connect = new ServerCommandWrapper.Basic.BasicConnection("admin", "admin", info[0].ToString(), 443);

                Connect.Login(new Guid("BE07504F-B330-4475-9AE4-1A7FF10BD486"), "1.0", "");

                return Connect.LoginInfo.Token;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public async Task<object> getCamera(object[] info)
        {
            //object token = await getToken(info);

            SystemAccess sa = new SystemAccess(info[0].ToString(), info[1].ToString());
            return sa.GetSystemCameras();
        }

        public object getCameraTemp(object[] info)
        {
            //object token = await getToken(info);

            SystemAccess sa = new SystemAccess(info[0].ToString(), info[1].ToString());
            return sa.GetSystemCameras();
        }

        public async Task<object> Connect(object[] info)
        {
            //카메라 최초 접속시 쓰레드 시작
            if (info[2].ToString() == "Start")
            {
                this.token = info[0].ToString();
                this.device = info[1].ToString();

                if (this.token == "")
                {
                    return "Token Empty";
                }

                if (this.device == "")
                {
                    return "Device Empty";
                }

                //Thread _recvThread = new Thread(Live);
                //_recvThread.Start();
                
                //VideoOS.Platform.SDK.Environment.Initialize();          // General initialize.  Always required
                //VideoOS.Platform.SDK.UI.Environment.Initialize();       // Initialize ActiveX references, e.g. usage of ImageViewerActiveX etc
                //VideoOS.Platform.SDK.Export.Environment.Initialize();   // Initialize export references

                //Uri uri = new Uri("http://" + MILESTONE_IP);
                ////NetworkCredential nc = System.Net.CredentialCache.DefaultNetworkCredentials;
                //CredentialCache cc = VideoOS.Platform.Login.Util.BuildCredentialCache(uri, "admin", "admin", "Basic");
                ////CredentialCache cc = VideoOS.Platform.Login.Util.BuildCredentialCache(uri, "http://desktop-g7ejpeu:7563/\admin", "admin", "Negotiate");

                //VideoOS.Platform.SDK.Environment.AddServer(uri, cc);

                //try
                //{
                //    Guid IntegrationId = new Guid("1478D9D6-6168-4520-ACE3-4B795E6F3501");
                //    const string IntegrationName = "Export Sample";
                //    const string Version = "1.0";
                //    const string ManufacturerName = "Sample Manufacturer";

                //    VideoOS.Platform.SDK.Environment.Login(uri, IntegrationId, IntegrationName, Version, ManufacturerName, true);
                //    VideoOS.Platform.SDK.Environment.LoadConfiguration(uri);
                //}
                //catch (ServerNotFoundMIPException snfe)
                //{
                //    imgRtn[0] = device;
                //    imgRtn[1] = "N";
                //    imgRtn[2] = snfe.Message;
                //    return imgRtn;
                //}
                //catch (InvalidCredentialsMIPException ice)
                //{
                //    imgRtn[0] = device;
                //    imgRtn[1] = "N";
                //    imgRtn[2] = ice.Message;
                //    return imgRtn;
                //}
                //catch (Exception e)
                //{
                //    imgRtn[0] = device;
                //    imgRtn[1] = "N";
                //    imgRtn[2] = e.Message;
                //    return imgRtn;
                //}
                
                imgRtn[0] = device;
                imgRtn[1] = "Y";
                imgRtn[2] = "OK";
                return imgRtn;
            }
            else if (info[2].ToString() == "PTZ")
            {
                return PTZ(info[0].ToString(), info[1].ToString(), info[3].ToString());
            }
            else if (info[2].ToString() == "Download")
            {
                string call = "";
                try
                {
                    string strFile = @"F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    FileInfo fi = new FileInfo(strFile);
                    if (fi.Exists)
                    {
                        return "0";
                    }

                    call  = "-i ";
                    call += @"""F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mkv";
                    call += @"""";
                    call += " -c copy ";
                    call += @"""F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    call += @"""";

                    //call = "ffmpeg -i ";
                    //call += @"F:\IMS\Replay\" + info[4].ToString() + @"\";
                    //call += @"""" + info[6].ToString();
                    //call += @"""\";
                    //call += info[4].ToString() + ".mkv";
                    //call += " -c copy ";
                    //call += @"F:\IMS\Replay\" + info[4].ToString() + @"\";
                    //call += @"""" + info[6].ToString();
                    //call += @"""\";
                    //call += info[4].ToString() + ".mp4";


                    //call = @"-i F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mkv";
                    //call += " -c copy ";
                    //call += @"F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";

                    ProcessStartInfo psiProcInfo = new ProcessStartInfo();
                    Process pro = new Process();
                    psiProcInfo.FileName = "ffmpeg.exe";
                    psiProcInfo.Arguments = call;
                    psiProcInfo.UseShellExecute = false;
                    psiProcInfo.CreateNoWindow = true;
                    psiProcInfo.RedirectStandardOutput = false;
                    psiProcInfo.RedirectStandardError = true;
                    psiProcInfo.RedirectStandardInput = false;

                    pro.StartInfo = psiProcInfo;
                    pro.Start();

                    //pro.StandardInput.Close();

                    pro.WaitForExit();
                    pro.Close();

                    return "0";
                }
                catch (Exception e)
                {
                    //return call;
                    return e.Message + call;
                }
            }
            else if (info[2].ToString() == "Video")
            {
                string strFile = @"F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                string call = "";
                try
                {
                    FileInfo fi = new FileInfo(strFile);
                    if (fi.Exists)
                    {
                        return "0";
                    }

                    string path = System.IO.Directory.GetCurrentDirectory() + @"\Milestone\VideoOs.exe";
                    //string path = System.IO.Directory.GetCurrentDirectory() + @"\VideoOs.exe";
                    ProcessStartInfo psiProcInfo = new ProcessStartInfo();
                    Process pro = new Process();
                    psiProcInfo.FileName = path;
                    psiProcInfo.Arguments = info[4].ToString() + "\a" + info[6].ToString().Replace(" ", "!SPACE!") + "\a" + info[8].ToString().Replace(" ", "!SPACE!") + "\a" + info[9].ToString().Replace(" ", "!SPACE!");
                    psiProcInfo.UseShellExecute = false;
                    psiProcInfo.CreateNoWindow = true;
                    psiProcInfo.RedirectStandardOutput = true;
                    psiProcInfo.RedirectStandardError = true;
                    psiProcInfo.RedirectStandardInput = false;

                    pro.StartInfo = psiProcInfo;
                    pro.Start();

                    pro.WaitForExit();
                    string resultValue = pro.StandardOutput.ReadToEnd();
                    pro.Close();


                    //mp4변환
                    call  = "-i ";
                    call += @"""F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mkv";
                    call += @"""";
                    call += " -c copy ";
                    call += @"""F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    call += @"""";

                    ProcessStartInfo psiProcInfo2 = new ProcessStartInfo();
                    Process pro2 = new Process();
                    psiProcInfo2.FileName = "ffmpeg.exe";
                    psiProcInfo2.Arguments = call;
                    psiProcInfo2.UseShellExecute = false;
                    psiProcInfo2.CreateNoWindow = true;
                    psiProcInfo2.RedirectStandardOutput = false;
                    psiProcInfo2.RedirectStandardError = true;
                    psiProcInfo2.RedirectStandardInput = false;

                    pro2.StartInfo = psiProcInfo2;
                    pro2.Start();

                    //pro.StandardInput.Close();

                    pro2.WaitForExit();
                    pro2.Close();

                    //해당폴더의 mkv파일 삭제
                    File.Delete(@"F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mkv");


                    //HLS변환
                    ProcessStartInfo psiProcInfo3 = new ProcessStartInfo();
                    Process pro3 = new Process();
                    string call2 = " -i ";
                    call2 += @"""F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    call2 += @"""";
                    call2 += " -codec: copy -start_number 0 -hls_time 10 -hls_list_size 0 -f hls ";
                    call2 += @"""F:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".m3u8";
                    call2 += @"""";

                    psiProcInfo3.FileName = "ffmpeg.exe";
                    psiProcInfo3.Arguments = call2;
                    psiProcInfo3.UseShellExecute = false;
                    psiProcInfo3.CreateNoWindow = true;
                    psiProcInfo3.RedirectStandardOutput = false;
                    psiProcInfo3.RedirectStandardError = false;
                    psiProcInfo3.RedirectStandardInput = false;

                    pro3.StartInfo = psiProcInfo3;
                    pro3.Start();
                    pro3.WaitForExit();
                    pro3.Close();

                    return "0";
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            else if (info[2].ToString() == "Video_test")
            {
                try
                {
                    //string strFile = @"D:\IMS\Replay\" + info[4].ToString() + @"\" + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    //FileInfo fi = new FileInfo(strFile);
                    //if (fi.Exists)
                    //{
                    //    return "0";
                    //}

                    ////string path = System.IO.Directory.GetCurrentDirectory() + @"\server\Milestone\VideoOs.exe";
                    //string path = System.IO.Directory.GetCurrentDirectory() + @"\VideoOs.exe";
                    //ProcessStartInfo psiProcInfo = new ProcessStartInfo();
                    //Process pro = new Process();
                    //psiProcInfo.FileName = path;
                    //psiProcInfo.Arguments = info[4].ToString() + "\a" + info[6].ToString().Replace(" ", "!SPACE!") + "\a" + info[8].ToString().Replace(" ", "!SPACE!") + "\a" + info[9].ToString().Replace(" ", "!SPACE!");
                    //psiProcInfo.UseShellExecute = false;
                    //psiProcInfo.CreateNoWindow = true;
                    //psiProcInfo.RedirectStandardOutput = true;
                    //psiProcInfo.RedirectStandardError = true;
                    //psiProcInfo.RedirectStandardInput = false;

                    //pro.StartInfo = psiProcInfo;
                    //pro.Start();

                    //pro.WaitForExit();
                    //string resultValue = pro.StandardOutput.ReadToEnd();
                    //pro.Close();

                    ////mp4변환
                    //string call = "-i ";
                    //call += @"""D:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mkv";
                    //call += @"""";
                    //call += " -c copy ";
                    //call += @"""D:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    //call += @"""";

                    //ProcessStartInfo psiProcInfo2 = new ProcessStartInfo();
                    //Process pro2 = new Process();
                    //psiProcInfo2.FileName = "ffmpeg.exe";
                    //psiProcInfo2.Arguments = call;
                    //psiProcInfo2.UseShellExecute = false;
                    //psiProcInfo2.CreateNoWindow = true;
                    //psiProcInfo2.RedirectStandardOutput = false;
                    //psiProcInfo2.RedirectStandardError = true;
                    //psiProcInfo2.RedirectStandardInput = false;

                    //pro2.StartInfo = psiProcInfo2;
                    //pro2.Start();
                    
                    //pro2.WaitForExit();
                    //pro2.Close();

                    //HLS변환
                    ProcessStartInfo psiProcInfo3 = new ProcessStartInfo();
                    Process pro3 = new Process();
                    //string call2 = " -y -an -i ";
                    //call2 += @"""D:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    //call2 += " -profile:v baseline -level 3.0 -s 1280x1024 -start_number 0 -hls_time 2 -hls_list_size 0 -f hls ";
                    //call2 += @"""D:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".m3u8";

                    string call2 = " -i ";
                    call2 += @"""D:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mp4";
                    call2 += @"""";
                    call2 += " -codec: copy -start_number 0 -hls_time 10 -hls_list_size 0 -f hls ";
                    call2 += @"""D:\IMS\Replay\" + info[4].ToString() + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".m3u8";
                    call2 += @"""";



                    //ffmpeg -i "D:\\IMS\\Replay\\202107140004\\TRUEN Co., Ltd. TN-B336W12C (10.10.136.128) - 카메라 1\\202107140004.mp4" -codec: copy -start_number 0 -hls_time 10 -hls_list_size 0 -f hls "D:\\IMS\\Replay\\202107140004\\TRUEN Co., Ltd. TN-B336W12C (10.10.136.128) - 카메라 1\\202107140004.m3u8"

                    psiProcInfo3.FileName = "ffmpeg.exe";
                    psiProcInfo3.Arguments = call2;
                    psiProcInfo3.UseShellExecute = false;
                    psiProcInfo3.CreateNoWindow = true;
                    psiProcInfo3.RedirectStandardOutput = false;
                    psiProcInfo3.RedirectStandardError = false;
                    psiProcInfo3.RedirectStandardInput = false;

                    pro3.StartInfo = psiProcInfo3;
                    pro3.Start();
                    pro3.WaitForExit();
                    pro3.Close();

                    return "0";
                }
                catch(Exception e)
                {
                    return e.Message;
                }
            }
            else if(info[2].ToString() == "Replay")
            {
                try
                {
                    string strFile = @"F:\IMS\Replay\" + info[4].ToString() + @"\" + @"\" + info[6].ToString() + @"\" + info[4].ToString() + ".mkv";
                    FileInfo fi = new FileInfo(strFile);
                    if (fi.Exists)
                    {
                        return "0";
                    }

                    string path = System.IO.Directory.GetCurrentDirectory() + @"\server\Milestone\VideoOs.exe";
                    //string path = System.IO.Directory.GetCurrentDirectory() + @"\VideoOs.exe";
                    ProcessStartInfo psiProcInfo = new ProcessStartInfo();
                    Process pro = new Process();
                    psiProcInfo.FileName = path;
                    psiProcInfo.Arguments = info[4].ToString() + "!CV!" + info[6].ToString().Replace(" ", "!SPACE!");
                    psiProcInfo.UseShellExecute = false;
                    psiProcInfo.CreateNoWindow = true;
                    psiProcInfo.RedirectStandardOutput = true;
                    psiProcInfo.RedirectStandardError = true;
                    psiProcInfo.RedirectStandardInput = false;
                    
                    pro.StartInfo = psiProcInfo;
                    pro.Start();

                    //pro.StandardInput.Close();

                    pro.WaitForExit();
                    string resultValue = pro.StandardOutput.ReadToEnd();
                    pro.Close();

                    return resultValue;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            else if (info[2].ToString() == "Status")
            {
                return recRtn;
            }
            else //Live
            {
                if (this.token != info[0].ToString())
                {
                    this.token = info[0].ToString();
                }

                return imgRtn;
            }
        }

        public Image ByteArrayToImage(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            Image recImg = Image.FromStream(ms);
            return recImg;
        }

        #region Method

        public void Status()
        {
            int min = 0;
            int sec = 0;

            Guid[] arrGuid = new Guid[] { new Guid(this.device) };
            RecorderCommandService.ManualRecordingInfo[] recYn = new RecorderCommandService.ManualRecordingInfo[1];

            while (true)
            {
                try
                {
                    recYn = rcs.IsManualRecording(this.token, arrGuid);
                    //1. 녹화중.
                    if (recYn[0].IsManualRecording)
                    {
                        sec++;
                        if (sec > 59)
                        {
                            min++;
                            sec = 0;
                        }

                        recRtn[2] = min.ToString("00") + ":" + sec.ToString("00");
                    }
                    else
                    {
                        //2. 현재시간을 오라클 디비에저장한다.
                        recRtn[0] = "";
                        recRtn[1] = "1";
                        recRtn[2] = "00:00";
                        return;
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception msg)
                {

                }
            }
        }

        public string PTZ(string token, string device, string ptz)
        {
            Stream networkStream = null;
            try
            {
                networkStream = ConnectToImageServer(MILESTONE_IP);
                if (networkStream != null)
                {
                    object _liveSocketSendLock = new object();
                    int bytes;
                    int maxBuf = 1024 * 8;
                    Byte[] bytesReceived = new Byte[maxBuf];
                    string page;

                    string sendBuffer = FormatConnect(device, token);

                    // Deliberately not encoded as UTF-8
                    // With XPCO and XPE/WinAuth only the camera GUID and the token are used. These are always 7 bit ASCII
                    // With XPE/BasicAuth, the username and password are in clear text. The server expect bytes in it's own current code page.
                    // Encoding this as UTF-8 will prevent corrent authentication with other than 7-bit ASCII characters in username and password
                    // Encoding this with "Default" will at least make other than 7-bit ASCII work when client's and server's code pages are alike
                    // XPE's Image Server Manager has an option of manually selecting a code page.
                    // But there is no way in which a client can obtain the XPE server's code page selection.
                    Byte[] bytesSent = Encoding.Default.GetBytes(sendBuffer);

                    lock (_liveSocketSendLock)
                    {
                        networkStream.Write(bytesSent, 0, bytesSent.Length);
                    }

                    bytes = RecvUntil(networkStream, bytesReceived, 0, maxBuf);
                    page = Encoding.UTF8.GetString(bytesReceived, 0, bytes);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(page);
                    XmlNodeList nodes = doc.GetElementsByTagName("connected");
                    foreach (XmlNode node in nodes)
                    {
                        if (node.InnerText.ToLower() == "yes")
                        {
                            string ptzBuffer = FormatPTZ(ptz);
                            Byte[] bytesSent2 = Encoding.Default.GetBytes(ptzBuffer);
                            lock (_liveSocketSendLock)
                            {
                                networkStream.Write(bytesSent2, 0, bytesSent2.Length);
                            }

                            page = Encoding.UTF8.GetString(bytesSent2, 0, bytesSent2.Length);
                        }
                    }
                }

                return "OK";
            }
            catch (Exception msg)
            {
                return msg.Message;
            }
            finally
            {
                networkStream.Close();
                networkStream.Dispose();
            }
        }

        public void Live()
        {
            Stream networkStream = null;
            bool _live = false;
            object _liveSocketSendLock = new object();
            int Length = 0;
            string Type = "";
            string Current = "";
            string Next = "";
            string Prev = "";
            object Data = null;

            int maxBuf = 1024 * 8;
            Byte[] bytesReceived = new Byte[maxBuf];
            int bytes;
            string page;

            string curToken = "";
            string curDevice = "";
            int seq = 0;

            try
            {
                //Question
                // Others may now send on this socket, preferably using DoLiveCmd()
                //_reqCounter = 2;
                //_streamLive = networkStream;
                while (true)
                {
                    if (curToken != this.token || networkStream == null)
                    {
                        if (networkStream != null)
                        {
                            try
                            {
                                networkStream.Close();
                                networkStream = null;
                            }
                            catch
                            {

                            }
                        }

                        curToken = this.token;
                        curDevice = this.device;

                        networkStream = ConnectToImageServer(MILESTONE_IP);
                        // Errors are handled by ConnectToImageServer
                        if (networkStream == null) continue;

                        _live = true;

                        string sendBuffer = FormatConnect(curDevice, curToken);

                        // Deliberately not encoded as UTF-8
                        // With XPCO and XPE/WinAuth only the camera GUID and the token are used. These are always 7 bit ASCII
                        // With XPE/BasicAuth, the username and password are in clear text. The server expect bytes in it's own current code page.
                        // Encoding this as UTF-8 will prevent corrent authentication with other than 7-bit ASCII characters in username and password
                        // Encoding this with "Default" will at least make other than 7-bit ASCII work when client's and server's code pages are alike
                        // XPE's Image Server Manager has an option of manually selecting a code page.
                        // But there is no way in which a client can obtain the XPE server's code page selection.
                        Byte[] bytesSent = Encoding.Default.GetBytes(sendBuffer);

                        lock (_liveSocketSendLock)
                        {
                            networkStream.Write(bytesSent, 0, bytesSent.Length);
                        }


                        bytes = RecvUntil(networkStream, bytesReceived, 0, maxBuf);
                        page = Encoding.UTF8.GetString(bytesReceived, 0, bytes);

                        bool authenticated = false;
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(page);
                        XmlNodeList nodes = doc.GetElementsByTagName("connected");
                        foreach (XmlNode node in nodes)
                        {
                            if (node.InnerText.ToLower() == "yes")
                            {
                                authenticated = true;
                            }
                        }

                        if (!authenticated)
                        {
                            //// Tell the application I'm done
                            //if (OnConnectionStoppedMethod != null)
                            //{
                            //    Control pj = (Control)_renderObject;
                            //    pj.Dispatcher.Invoke(OnConnectionStoppedMethod,
                            //        new ConnInfo(IscErrorCode.InvalidCredentials, ""));
                            //}

                            imgRtn[1] = "N";
                            imgRtn[2] = "authenticated Error";
                            networkStream = null;

                            continue; // This is a thread. It won't help returning an error code
                        }

                        sendBuffer = FormatLive(100);
                        bytesSent = Encoding.UTF8.GetBytes(sendBuffer);
                        lock (_liveSocketSendLock)
                        {
                            networkStream.Write(bytesSent, 0, bytesSent.Length);
                        }

                        //page = Encoding.UTF8.GetString(bytesSent, 0, bytesSent.Length);

                        Thread.Sleep(1000);
                    }

                    // Buffer size housekeeping
                    int curBufSize = maxBuf;

                    bytes = RecvUntil(networkStream, bytesReceived, 0, maxBuf);
                    if (bytes < 0)
                    {

                        imgRtn[1] = "N";
                        imgRtn[2] = "Receive error A";

                        continue;
                    }

                    if (bytesReceived[0] == '<')
                    {
                        // This is XML status message
                        //김경현 주석
                        //page = Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                        //XmlDocument statDoc = new XmlDocument();
                        //statDoc.LoadXml(page);


                        //if (OnStatusItemReceivedMethod != null)
                        //{
                        //    Control pj = (Control)_renderObject;
                        //    pj.Dispatcher.Invoke(OnStatusItemReceivedMethod, statDoc);
                        //}

                        imgRtn[1] = "N";
                        imgRtn[2] = "This is XML status message <";

                        continue;
                    }


                    if (bytesReceived[0] == 'I')
                    {
                        // Image
                        //ImageInfo h = ParseHeader(bytesReceived, 0, bytes);
                        ParseHeader(bytesReceived, 0, bytes, ref Length, ref Type, ref Current, ref Next, ref Prev, ref Data);

                        // Takes two first bytes
                        bytes = RecvFixed(networkStream, bytesReceived, 0, 2);
                        if (2 != Math.Abs(bytes))
                        {
                            //throw new Exception("Receive error 2");

                            imgRtn[1] = "N";
                            imgRtn[2] = "Receive error 2";

                            continue;
                        }

                        if (bytesReceived[0] == 0xFF && bytesReceived[1] == 0xD8)
                        {
                            int neededBufSize = Length;
                            if (neededBufSize > curBufSize)
                            {
                                int newBufSize = RoundUpBufSize(neededBufSize);
                                curBufSize = newBufSize;
                                byte b0 = bytesReceived[0];
                                byte b1 = bytesReceived[1];
                                bytesReceived = new byte[curBufSize];
                                bytesReceived[0] = b0;
                                bytesReceived[1] = b1;
                            }

                            bytes = RecvFixed(networkStream, bytesReceived, 2, neededBufSize - 2);
                            bytes = Math.Abs(bytes);
                            if (bytes != neededBufSize - 2)
                            {
                                //throw new Exception("Receive error B");

                                imgRtn[1] = "N";
                                imgRtn[2] = "Receive error B";

                                continue;
                            }
                        }
                        else
                        {
                            bytes = RecvFixed(networkStream, bytesReceived, 2, 30);
                            if (Math.Abs(bytes) != 30)
                            {
                                //throw new Exception("Receive error C");

                                imgRtn[1] = "N";
                                imgRtn[2] = "Receive error C";

                                continue;
                            }

                            short dataType = BitConverter.ToInt16(GetReversedSubarray(bytesReceived, 0, 2), 0);
                            int length = BitConverter.ToInt32(GetReversedSubarray(bytesReceived, 2, 4), 0);
                            short codec = BitConverter.ToInt16(GetReversedSubarray(bytesReceived, 6, 2), 0);
                            short seqNo = BitConverter.ToInt16(GetReversedSubarray(bytesReceived, 8, 2), 0);
                            short flags = BitConverter.ToInt16(GetReversedSubarray(bytesReceived, 10, 2), 0);
                            long timeStampSync = BitConverter.ToInt64(GetReversedSubarray(bytesReceived, 12, 8), 0);
                            long timeStampPicture = BitConverter.ToInt64(GetReversedSubarray(bytesReceived, 20, 8), 0);
                            int reserved = BitConverter.ToInt32(GetReversedSubarray(bytesReceived, 28, 4), 0);

                            bool isKeyFrame = (flags & 0x01) == 0x01;
                            int payloadLength = length - 32;

                            if (payloadLength > curBufSize)
                            {
                                int newBufSize = RoundUpBufSize(payloadLength);
                                curBufSize = newBufSize;
                                bytesReceived = new byte[curBufSize];
                            }

                            //this appears to be the correct amount of data
                            bytes = RecvFixed(networkStream, bytesReceived, 0, payloadLength);
                            bytes = Math.Abs(bytes);
                            if (bytes != payloadLength)
                            {
                                //throw new Exception("Receive error D");

                                imgRtn[1] = "N";
                                imgRtn[2] = "Receive error D";

                                continue;
                            }
                        }

                        LastJPEG = bytesReceived;

                        try
                        {
                            //김경현 주석
                            //byte[] ms = new byte[bytes];
                            //Buffer.BlockCopy(bytesReceived, 0, ms, 0, bytes);
                            //Data = ms;

                            lock (imgRtn)
                            {
                                imgRtn[1] = "Y";
                                imgRtn[2] = "data:image/PNG;base64," + Convert.ToBase64String(bytesReceived);
                                Thread.Sleep(1);
                            }

                            //rtn[2] = ByteArrayToImage(bytesReceived);

                            //Control pi = (Control)_renderObject;
                            //pi.Dispatcher.Invoke(OnImageReceivedMethod, h);
                        }
                        catch (OutOfMemoryException)
                        {
                            //Control pp = (Control)_renderObject;
                            //pp.Dispatcher.Invoke(OnConnectionStoppedMethod,
                            //    new ConnInfo(IscErrorCode.OutOfMemory, ""));
                            //StopLive();

                            imgRtn[1] = "N";
                            imgRtn[2] = "OutOfMemoryException";
                        }
                        catch (Exception e)
                        {
                            //Control pp = (Control)_renderObject;
                            //pp.Dispatcher.Invoke(OnConnectionStoppedMethod,
                            //    new ConnInfo(IscErrorCode.NotJpegError, e.Message));

                            imgRtn[1] = "N";
                            imgRtn[2] = e.Message;
                        }
                    }
                }

                // Tell the application I'm done
                //Control ipj = (Control)_renderObject;
                //ipj.Dispatcher.Invoke(OnConnectionStoppedMethod, new ConnInfo(IscErrorCode.Success, ""));
            }
            catch (OutOfMemoryException)
            {
                imgRtn[1] = "N";
                imgRtn[2] = "OutOfMemoryException 2";
            }
            catch (IOException e)
            {
                imgRtn[1] = "N";
                imgRtn[2] = e.Message;
            }
            catch (Exception e)
            {
                imgRtn[1] = "N";
                imgRtn[2] = e.Message;
            }
            finally
            {
                networkStream?.Dispose();
            }
        }
        #endregion

        #region SendFormat
        private string FormatPTZ(string command)
        {
            string sendBuffer = string.Format(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<methodcall>" +
                "<requestid>2</requestid>" +
                "<methodname>ptz</methodname>" +
                "<ptzcommand>{0}</ptzcommand>" +
                "</methodcall>",
                command);

            return sendBuffer;
        }

        private string FormatConnect(string _cameraGuid, string _token)
        {
            string sendBuffer = string.Format(
                //"<?xml version=\"1.0\" encoding=\"utf-8\"?><methodcall><requestid>" + requestid + "</requestid>" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><methodcall><requestid>0</requestid>" +
                "<methodname>connect</methodname><username>a</username><password>a</password>" +
                "<cameraid>{0}</cameraid><alwaysstdjpeg>yes</alwaysstdjpeg>" +
                "<transcode><allframes>yes</allframes></transcode>" + // Add this line to get all frames in a GOP transcoded
                "<connectparam>id={1}&amp;connectiontoken={2}" +
                "</connectparam></methodcall>\r\n\r\n",
                _cameraGuid, _cameraGuid, _token);

            return sendBuffer;
        }

        private string FormatLive(int _quality)
        {
            string sendBuffer;

            if (_quality == 100 || _quality < 1 || _quality > 104)
            {
                sendBuffer = string.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?><methodcall><requestid>1</requestid>" +
                    "<methodname>live</methodname>" +
                    "<compressionrate>" + compRate + "</compressionrate>" +
                    "</methodcall>\r\n\r\n");
            }
            else
            {
                sendBuffer = string.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?><methodcall><requestid>1</requestid>" +
                    "<methodname>live</methodname>" +
                    "<compressionrate>{0}</compressionrate>" +
                    "</methodcall>\r\n\r\n",
                    _quality);
            }

            return sendBuffer;
        }
        #endregion

        #region CommonMethod

        private Stream ConnectToImageServer(string ipAddress)
        {
            Stream networkStream = null;
            //String oper = "";
            try
            {
                //oper = "new Socket";
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ipAddress), 7563);
                sock.Connect(ipe);

                //oper = "NetworkStream";
                networkStream = new NetworkStream(sock, true);

                networkStream.ReadTimeout = 10000;
                networkStream.WriteTimeout = 2000;
            }
            catch (Exception e)
            {

            }

            return networkStream;
        }

        private void ParseHeader(byte[] buf, int offset, int bytes, ref int Length, ref string Type, ref string Current, ref string Next, ref string Prev, ref object Data)
        {
            string response = Encoding.UTF8.GetString(buf, offset, bytes);
            string[] headers = response.Split('\n');
            foreach (string header in headers)
            {
                string[] keyVal = header.Split(':');
                if (keyVal[0].ToLower() == "content-length" && keyVal.Length > 1)
                {
                    Length = int.Parse(keyVal[1]);
                }

                if (keyVal[0].ToLower() == "content-type" && keyVal.Length > 1)
                {
                    Type = keyVal[1].Trim('\r').ToLower();
                }

                if (keyVal[0].ToLower() == "current" && keyVal.Length > 1)
                {
                    Current = keyVal[1].Trim('\r');
                }

                if (keyVal[0].ToLower() == "next" && keyVal.Length > 1)
                {
                    Next = keyVal[1].Trim('\r');
                }

                if (keyVal[0].ToLower() == "prev" && keyVal.Length > 1)
                {
                    Prev = keyVal[1].Trim('\r');
                }
            }
        }
        private byte[] GetReversedSubarray(byte[] array, int start, int length)
        {
            return array.Skip(start).Take(length).Reverse().ToArray();
        }
        private int RoundUpBufSize(int needed)
        {
            int roundup = (needed / 1024) * 1024 / 100 * 130;
            return roundup;
        }
        private static int RecvFixed(Stream stream, byte[] buf, int offset, int size)
        {
            int miss = size;
            int got = 0;
            int bytes = 0;
            int get = 1;
            int maxb = 1024 * 16;

            do
            {
                get = miss > maxb ? maxb : miss;
                bytes = stream.Read(buf, offset + got, get);
                got += bytes;
                miss -= bytes;
            } while (got < size);

            if (got > size)
            {
                throw new Exception("Buffer overflow");
            }

            if (size < 4)
                return -got;

            int i = offset + got - 4;
            if (buf[i] == '\r' && buf[i + 1] == '\n' && buf[i + 2] == '\r' && buf[i + 3] == '\n')
            {
                return got;
            }

            return -got;

        }

        private static int RecvUntil(Stream stream, byte[] buf, int offset, int size)
        {
            int miss = size;
            int got = 0;
            int bytes = 0;
            int ended = 4;
            int i = 0;

            while (got < size && ended > 0)
            {
                i = offset + got;
                bytes = stream.Read(buf, i, 1);
                if (buf[i] == '\r' || buf[i] == '\n')
                {
                    ended--;
                }
                else
                {
                    ended = 4;
                }

                got += bytes;
                miss -= bytes;
            }

            if (got > size)
            {
                throw new Exception("Buffer overflow");
            }

            if (ended == 0)
            {
                return got;
            }

            return -got;
        }

        public List<Item> getchikldren(List<Item> oItem)
        {
            List<Item> oCams = new List<Item>();

            foreach (Item childSite in oItem)
            {
                if (childSite.FQID.Kind == Kind.Camera && childSite.FQID.FolderType == FolderType.No) oCams.Add(childSite);
                oCams.AddRange(getchikldren(childSite.GetChildren()));
            }
            return oCams;
        }


        public List<Item> GetCameras(Item oItem)
        {
            List<Item> oCams = new List<Item>();

            if (oItem.HasChildren != VideoOS.Platform.HasChildren.No)
            {
                if (oItem.FQID.Kind == Kind.Camera && oItem.FQID.FolderType == FolderType.No) oCams.Add(oItem);
                oCams = getchikldren(oItem.GetChildren());

            }

            return oCams;
        }
        #endregion
    }
}
