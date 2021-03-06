﻿//
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

using Google.Apis.Requests;
using Google.Apis.Util;
using Google.Solutions.Common.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Google.Solutions.Common.ApiExtensions.Request
{
    public static class RetryExtensions
    {
        public async static Task<Stream> ExecuteAsStreamWithRetryAsync<TResponse>(
            this ClientServiceRequest<TResponse> request,
            ExponentialBackOff backOff)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    return await request.ExecuteAsStreamAsync();
                }
                catch (GoogleApiException e) when (e.Error != null && e.Error.Code == 429)
                {
                    // Too many requests.
                    if (retries < backOff.MaxNumOfRetries)
                    {
                        TraceSources.Common.TraceWarning(
                            "Too many requests - backing of and retrying...", retries);

                        retries++;
                        await Task.Delay(backOff.GetNextBackOff(retries));
                    }
                    else
                    {
                        // Retried too often already.
                        TraceSources.Common.TraceWarning("Giving up after {0} retries", retries);
                        throw;
                    }
                }
            }
        }
    }
}
