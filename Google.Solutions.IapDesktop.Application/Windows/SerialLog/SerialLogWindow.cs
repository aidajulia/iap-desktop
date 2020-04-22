//
// Copyright 2019 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Apis.Auth.OAuth2.Responses;
using Google.Solutions.Compute;
using Google.Solutions.Compute.Extensions;
using Google.Solutions.Compute.Text;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Google.Solutions.IapDesktop.Application.Windows.SerialLog
{
    [ComVisible(false)]
    public partial class SerialLogWindow : ToolWindow
    {
        private readonly ManualResetEvent keepTailing = new ManualResetEvent(true);
        private volatile bool formClosing = false;

        public VmInstanceReference Instance { get; }

        public SerialLogWindow(VmInstanceReference vmInstance)
        {
            InitializeComponent();

            this.TabText = $"Log ({vmInstance.InstanceName})";
            this.Instance = vmInstance;
        }

        internal void TailSerialPortStream(IAsyncReader<string> stream)
        {
            // The data could have ANSI control sequences embedded, so parse that.
            var scanner = new AnsiScanner(stream);

            Task.Run(async () =>
            {
                bool exceptionCaught = false;
                while (!exceptionCaught)
                {
                    // Check if we can continue to tail.
                    this.keepTailing.WaitOne();

                    var newOutput = new StringBuilder();
                    try
                    {
                        var tokens = await scanner.ReadAsync();
                        if (tokens == null)
                        {
                            return;
                        }

                        foreach (var token in tokens)
                        {
                            if (token.Type == AnsiTextToken.TokenType.Text)
                            {
                                newOutput.Append(token.Value.Replace("\n", "\r\n"));
                            }
                            else if (token.Type == AnsiTextToken.TokenType.Command && 
                                     token.Value == AnsiTextToken.ClearEntireScreen)
                            {
                                // This is the only command worth interpreting, all other commands
                                // are basically junk.
                                newOutput.Append("\r\n");
                            }
                        }
                    }
                    catch (TokenResponseException e)
                    {
                        newOutput.Append("Reading from serial port failed - session timed out " +
                            $"({e.Error.ErrorDescription})");
                        exceptionCaught = true;
                    }
                    catch (Exception e)
                    {
                        newOutput.Append($"Reading from serial port failed: {e.Unwrap().Message}");
                        exceptionCaught = true;
                    }

                    // By the time we read the data, the form might have begun closing. In this
                    // case, updating the UI would cause an exception.
                    if (!this.formClosing && newOutput.Length > 0)
                    {
                        BeginInvoke((Action)(() => this.log.AppendText(newOutput.ToString())));
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            });
        }

        private void SerialPortOutputWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.formClosing = true;
        }

        private void SerialLogWindow_Enter(object sender, EventArgs e)
        {
            // Start tailing (again).
            this.keepTailing.Set();
        }

        private void SerialLogWindow_Leave(object sender, EventArgs e)
        {
            // Pause.
            this.keepTailing.Reset();
        }
    }
}
