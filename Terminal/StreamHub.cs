﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Terminal { 
    public class StreamHub : Hub
    {
        ChannelWriter<string> writer = null;
        public ChannelReader<string> RunCommand(string cmd)
        {
            var channel = Channel.CreateUnbounded<string>();
            writer = channel.Writer;

         
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    //for windows 
                    FileName = "cmd.exe",
                    Arguments = $"/c {cmd}",

                    //for linux
                    //FileName = "/bin/bash",
                    //Arguments = $"-c \"{cmd.Replace("\"", "\\\"")}\"",

                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += ReadOutputHandler;
            process.Exited += Process_Exited;
           

            process.Start();
            process.BeginOutputReadLine();
        
            return channel.Reader;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            writer.TryComplete();
        }

        private async void ReadOutputHandler(object sendingProcess, DataReceivedEventArgs data)
        {
            if (!String.IsNullOrEmpty(data.Data))
            {
                Debug.WriteLine(data.Data);
                await writer.WriteAsync(data.Data);
                await Task.Delay(500);
                if (((Process)sendingProcess).HasExited)
                {
                    writer.TryComplete();
                }
            }
        }
      
    }
}
