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

using Google.Solutions.Common;
using System.Diagnostics;

namespace Google.Solutions.IapDesktop.Application.Services.Windows
{
    public class CloudConsoleService
    {
        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                Verb = "open",
                FileName = url
            });
        }

        public void OpenVmInstance(VmInstanceReference instance)
        {
            OpenUrl("https://console.cloud.google.com/compute/instancesDetail/zones/" +
                    $"{instance.Zone}/instances/{instance.InstanceName}?project={instance.ProjectId}");
        }

        public void OpenVmInstanceLogs(VmInstanceReference instance, ulong instanceId)
        {
            OpenUrl("https://console.cloud.google.com/logs/viewer?" +
                   $"resource=gce_instance%2Finstance_id%2F{instanceId}&project={instance.ProjectId}");
        }

        public void OpenIapOverviewDocs()
        {
            OpenUrl("https://cloud.google.com/iap/docs/tcp-forwarding-overview");
        }

        public void OpenIapAccessDocs()
        {
            OpenUrl("https://cloud.google.com/iap/docs/using-tcp-forwarding");
        }
        public void ConfigureIapAccess(string projectId)
        {
            OpenUrl($"https://console.cloud.google.com/security/iap?project={projectId}");
        }
    }
}
