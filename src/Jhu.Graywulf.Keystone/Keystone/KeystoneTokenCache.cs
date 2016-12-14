using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Jhu.Graywulf.Keystone
{
    /// <summary>
    /// Implements a cache to store Keystone tokens
    /// </summary>
    public class KeystoneTokenCache : IDisposable
    {
        #region Static members

        private static KeystoneTokenCache instance;

        public static KeystoneTokenCache Instance
        {
            get { return instance; }
        }

        static KeystoneTokenCache()
        {
            instance = new KeystoneTokenCache();
        }

        #endregion
        #region Private member variables

        private ConcurrentDictionary<string, Token> tokens;
        private ConcurrentDictionary<string, string> users;
        protected Timer collectionTimer;

        private TimeSpan collectionInterval;

        #endregion
        #region Properties

        /// <summary>
        /// Gets or sets the garbage collection interval.
        /// </summary>
        public TimeSpan CollectionInterval
        {
            get { return collectionInterval; }
            set
            {
                collectionInterval = value;
                StartTimer();
            }
        }

        #endregion
        #region Constructors and initializers

        protected KeystoneTokenCache()
            : base()
        {
            InitializeMembers();
            StartTimer();
        }

        private void InitializeMembers()
        {
            this.collectionInterval = new TimeSpan(0, 5, 0);

            this.tokens = new ConcurrentDictionary<string, Token>(StringComparer.InvariantCultureIgnoreCase);

            // TODO: Are Keystone user names case-sensivitve?
            this.users = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // Create the timer but don't start it
            this.collectionTimer = new System.Threading.Timer(CollectionTimerCallback);
        }

        public void Dispose()
        {
            if (collectionTimer != null)
            {
                collectionTimer.Dispose();
                collectionTimer = null;
            }
        }

        #endregion
        #region Cache collection logic

        /// <summary>
        /// Called by the timer to perform the garbage collection.
        /// </summary>
        /// <param name="state"></param>
        private void CollectionTimerCallback(object state)
        {
            var delete = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var now = DateTime.Now;

            // Get a dictionary of the current keys. This will be a snapshot
            // of the current contents of the cache.
            var keys = tokens.Keys;

            foreach (var tokenID in keys)
            {
                var token = tokens[tokenID];

                if (token.ExpiresAt <= now)
                {
                    // If no event was fired mark it for delete anyway
                    delete.Add(tokenID);
                }
            }

            // Collect expired items from the cache
            foreach (var tokenID in delete)
            {
                Token token;

                // Remove the token itself
                if (tokens.TryRemove(tokenID, out token))
                {
                    // Remove the correspoing user-token pair
                    string oldTokenID;
                    var key = GetUserKey(token);

                    users.TryRemove(key, out oldTokenID);
                }
            }

            // Restart timer
            StartTimer();
        }

        /// <summary>
        /// Starts or restarts the garbage collection timer.
        /// </summary>
        private void StartTimer()
        {
            // Reschedule the timer, but turn of periodic firing.
            collectionTimer.Change((int)collectionInterval.TotalMilliseconds, System.Threading.Timeout.Infinite);
        }

        #endregion

        /// <summary>
        /// Adds a token to the cache
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool TryAdd(Token token)
        {
            string tokenID;
            Token oldToken = null;

            // Because a single user can have multiple tokens, we attempt to
            // remove the old entry first

            // TODO: a better approach would be to keep the token with longer
            // expiration time

            if (users.TryRemove(GetUserKey(token), out tokenID))
            {
                tokens.TryRemove(tokenID, out oldToken);
            }

            // Add the new token by username and id
            users.TryAdd(GetUserKey(token), token.ID);
            return tokens.TryAdd(token.ID, token);
        }

        public bool TryRemoveByTokenID(string tokenID, out Token token)
        {
            // Remove token first, then the corresponding user ID
            if (tokens.TryRemove(tokenID, out token))
            {
                return users.TryRemove(GetUserKey(token), out tokenID);
            }

            return false;
        }

        /// <summary>
        /// Removes an token from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemoveByUserName(string projectName, string userName, out Token token)
        {
            string tokenID;

            // Find tokenID by user name
            if (users.TryGetValue(GetUserKey(projectName, userName), out tokenID))
            {
                return TryRemoveByTokenID(tokenID, out token);
            }
            else
            {
                token = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get a token from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool TryGetValueByTokenID(string tokenID, out Token token)
        {
            // Only non-expired items are returned

            if (tokens.TryGetValue(tokenID, out token))
            {
                // Make sure time zone is taken into account
                var now = DateTime.Now.ToUniversalTime();

                if (token.ExpiresAt > now)
                {
                    return true;
                }
                else
                {
                    // Remove expired item
                    if (tokens.TryRemove(tokenID, out token))
                    {
                        users.TryRemove(GetUserKey(token), out tokenID);
                    }
                }
            }

            // In this case no valid token is found
            token = null;
            return false;
        }

        public bool TryGetValueByUserName(string projectName, string userName, out Token token)
        {
            string tokenID;
            var key = GetUserKey(projectName, userName);

            if (users.TryGetValue(key, out tokenID))
            {
                return TryGetValueByTokenID(tokenID, out token);
            }
            else
            {
                token = null;
                return false;
            }
        }

        /// <summary>
        /// Clears all items from the cache
        /// </summary>
        public void Clear()
        {
            tokens.Clear();
            users.Clear();
        }

        private string GetUserKey(Token token)
        {
            if (token.Project == null)
            {
                return token.User.Name;
            }
            else
            {
                return GetUserKey(token.Project.Name, token.User.Name);
            }
        }

        private string GetUserKey(string projectName, string userName)
        {
            if (projectName == null)
            {
                return userName;
            }
            else
            {
                return String.Format("{0}:{1}", projectName, userName);
            }
        }
    }
}
