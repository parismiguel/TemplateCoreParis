﻿/**
* Copyright 2017 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.VCA.Watson.Watson.Http;

namespace IBM.VCA.Watson.Watson.Service
{
    public interface IWatsonService
    {
        IClient Client { get; set; }

        string ServiceName { get; set; }
        string ApiKey { get; set; }
        string Endpoint { get; set; }
        string UserName { get; set; }
        string Password { get; set; }

        void SetCredential(string userName, string password);
    }
}