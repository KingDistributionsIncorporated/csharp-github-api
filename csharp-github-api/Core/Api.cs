﻿//-----------------------------------------------------------------------
// <copyright file="Api.cs" company="TemporalCohesion.co.uk">
//     Copyright [2010] [Stuart Grassie]
//
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
// </copyright>
//----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using csharp_github_api.Extensions;
using csharp_github_api.Models;

namespace csharp_github_api.Core
{
    using System.Net;
    using RestSharp;
    using System.Diagnostics;

    /// <summary>
    /// Base class for specific API classes.
    /// </summary>
    public abstract class Api
    {
        public string BaseUrl;
        protected RestClient Client;
        protected IAuthenticator Authenticator;
        private int _rateLimitRemaining;

        protected Api(){}

        /// <summary>
        /// Instantiattes a new instance of the <see cref="Api"/> class.
        /// </summary>
        /// <param name="baseUrl">The base url for GitHub's API.</param>
        protected Api(string baseUrl)
        {
            BaseUrl = baseUrl;
            Client = new RestClient(BaseUrl);
        }

        /// <summary>
        /// Instantiattes a new instance of the <see cref="Api"/> class.
        /// </summary>
        /// <param name="baseUrl">The base url for GitHub's API.</param>
        /// <param name="authenticator">The <see cref="IAuthenticator"/> class to use to authenticate requests to the user api.</param>
        protected Api(string baseUrl, IAuthenticator authenticator)
        {
            BaseUrl = baseUrl;
            Authenticator = authenticator;
            Client = new RestClient(BaseUrl);
        }

        /// <summary>
        /// Gets an integer which holds the API rate limit.
        /// </summary>
        public int RateLimit
        {
            get; private set;
        }

        /// <summary>
        /// Gets an integer which holds the remaining count of API requests calls
        /// </summary>
        public int RateLimitRemaining
        {
            get { return _rateLimitRemaining; }
            private set 
            { 
                _rateLimitRemaining = value; 

                Debug.WriteLine(string.Format("Current remaining rate limit: {0}", _rateLimitRemaining));

                if (_rateLimitRemaining <= 0)
                    throw new GitHubResponseException(string.Format("Github API rate limit ({0}) has been reached.", RateLimit));
            }
        }

        protected virtual RestClient GetRestClient()
        {
            return new RestClient(BaseUrl);
        }

        protected virtual void CheckRateLimit(IEnumerable<Parameter> headers)
        {
            var rateLimits = headers.AsQueryable().Where(x => x.Name.StartsWith("X-RateLimit"));
            var actualRateLimit = rateLimits.Single(x => x.Name.EndsWith("-Limit"));
            var remainingRateLimit = rateLimits.Single(x => x.Name.EndsWith("-Remaining"));

            RateLimit = Convert.ToInt32(actualRateLimit.Value);
            RateLimitRemaining = Convert.ToInt32(remainingRateLimit.Value);
        }
    }
}
