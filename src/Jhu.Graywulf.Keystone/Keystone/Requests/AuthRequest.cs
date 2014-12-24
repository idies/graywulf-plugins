using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.Keystone
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class AuthRequest
    {
        [JsonProperty("auth")]
        public Auth Auth { get; private set; }

        public static AuthRequest Create(string domainID, string username, string password, Domain scopeDomain, Project scopeProject)
        {
            Domain domain = null;
            if (domainID != null)
            {
                domain = new Domain()
                {
                    Name = domainID
                };
            }
            else
            {
                domain = scopeDomain;
            }

            Scope scope = null;
            if (scopeProject != null)
            {
                // There must be a domain ID in project if project is set
                scopeProject.Domain = domain;

                scope = new Scope()
                {
                    Project = scopeProject
                };
            }
            else if (scopeDomain != null)
            {
                scope = new Scope()
                {
                    Domain = scopeDomain,
                };
            }

            return new AuthRequest()
            {
                Auth = new Auth()
                {
                    Identity = new Identity()
                    {
                        Methods = new[] { "password" },
                        Password = new Password()
                        {
                            User = new User()
                            {
                                Domain = domain,
                                Name = username,
                                Password = password
                            }
                        },
                    },
                    Scope = scope
                }
            };
        }

        /// <summary>
        /// Creates a request to renew a token, scoped equally to the old one.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AuthRequest Create(Token token)
        {
            Scope scope = null;

            if (token.Project != null)
            {
                scope = new Scope()
                    {
                        Project = token.Project,
                    };
            }

            return new AuthRequest()
            {
                Auth = new Auth()
                {
                    Identity = new Identity()
                    {
                        Methods = new[] { "token" },
                        Token = token
                    },
                    Scope = scope
                }
            };
        }

        public static AuthRequest Create(Token token, Trust trust)
        {
            Scope scope = null;

            if (trust != null)
            {
                scope = new Scope()
                {
                    Trust = trust,
                };
            }

            return new AuthRequest()
            {
                Auth = new Auth()
                {
                    Identity = new Identity()
                    {
                        Methods = new[] { "token" },
                        Token = token
                    },
                    Scope = scope
                }
            };
        }

        public static RestMessage<AuthRequest> CreateMessage(string domain, string username, string password, Domain scopeDomain, Project scopeProject)
        {
            return new RestMessage<AuthRequest>(Create(domain, username, password, scopeDomain, scopeProject));
        }

        public static RestMessage<AuthRequest> CreateMessage(Token token)
        {
            return new RestMessage<AuthRequest>(Create(token));
        }

        public static RestMessage<AuthRequest> CreateMessage(Token token, Trust trust)
        {
            return new RestMessage<AuthRequest>(Create(token, trust));
        }
    }
}
