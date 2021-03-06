﻿//
// Copyright 2020 Google LLC
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

using Google.Solutions.Common.Diagnostics;
using Google.Solutions.IapDesktop.Application.ObjectModel;
using Google.Solutions.IapDesktop.Application.Services.Adapters;
using Google.Solutions.IapTunneling.Iap;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.IapDesktop.Application.Services.Integration
{
    public interface ITunnelService
    {
        Task<Tunnel> CreateTunnelAsync(TunnelDestination tunnelEndpoint);
    }

    public class TunnelService : ITunnelService
    {
        private readonly IAuthorizationAdapter authorizationService;

        public TunnelService(IServiceProvider serviceProvider)
        {
            this.authorizationService = serviceProvider.GetService<IAuthorizationAdapter>();
        }

        public Task<Tunnel> CreateTunnelAsync(TunnelDestination tunnelEndpoint)
        {
            using (TraceSources.IapDesktop.TraceMethod().WithParameters(tunnelEndpoint))
            {
                var iapEndpoint = new IapTunnelingEndpoint(
                    this.authorizationService.Authorization.Credential,
                    tunnelEndpoint.Instance,
                    tunnelEndpoint.RemotePort,
                    IapTunnelingEndpoint.DefaultNetworkInterface);


                // Start listener to enable clients to connect. Do not await
                // the listener as we want to continue listeining in the
                // background.
                var listener = SshRelayListener.CreateLocalListener(iapEndpoint);
                var cts = new CancellationTokenSource();

                _ = listener.ListenAsync(cts.Token);

                // Return the tunnel which allows the listener to be stopped
                // via the CancellationTokenSource.
                return Task.FromResult(new Tunnel(iapEndpoint, listener, cts));
            }
        }
    }
}
