// Borrowed from BlogEngine.NET

#region Using

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Globalization;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml;

#endregion

namespace XmlProviders {
    /// <summary>
    /// 
    /// </summary>
    public class XmlMembershipProvider : MembershipProvider {
        private Dictionary<string, MembershipUser> _Users;
        private string _XmlFileName;
        private MembershipPasswordFormat passwordFormat;

        #region Properties

        // MembershipProvider Properties
        /// <summary>
        /// 
        /// </summary>
        public override string ApplicationName {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool EnablePasswordRetrieval {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool EnablePasswordReset {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int MaxInvalidPasswordAttempts {
            get { return 5; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters {
            get { return 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int MinRequiredPasswordLength {
            get { return 5; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int PasswordAttemptWindow {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat {
            get { return passwordFormat; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string PasswordStrengthRegularExpression {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresQuestionAndAnswer {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresUniqueEmail {
            get { return false; }
        }

        #endregion

        #region Supported methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config) {
            if (config == null)
                throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "XmlMembershipProvider";

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML membership provider");
            }
            
            base.Initialize(name, config);

            // Initialize _XmlFileName and make sure the path
            // is app-relative
            string path = config["xmlFileName"];

            if (String.IsNullOrEmpty(path))
				path = "~/App_Data/users.xml";

            if (!VirtualPathUtility.IsAppRelative(path))
                throw new ArgumentException
                    ("xmlFileName must be app-relative");

            string fullyQualifiedPath = VirtualPathUtility.Combine
                (VirtualPathUtility.AppendTrailingSlash
                (HttpRuntime.AppDomainAppVirtualPath), path);

            _XmlFileName = HostingEnvironment.MapPath(fullyQualifiedPath);
            config.Remove("xmlFileName");

            // Make sure we have permission to read the XML data source and
            // throw an exception if we don't
            FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.Write, _XmlFileName);
            permission.Demand();

            //Password Format
            if (config["passwordFormat"] == null)
            {
                config["passwordFormat"] = "Hashed";
                passwordFormat = MembershipPasswordFormat.Hashed;
            }
            else if (String.Compare(config["passwordFormat"], "clear", true) == 0)
            {
                passwordFormat = MembershipPasswordFormat.Clear;
            }
            else
            {
                passwordFormat = MembershipPasswordFormat.Hashed;
            }
            config.Remove("passwordFormat");

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0) {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                    throw new ProviderException("Unrecognized attribute: " + attr);
            }
        }

        /// <summary>
        /// Returns true if the username and password match an exsisting user.
        /// </summary>
        public override bool ValidateUser(string username, string password) 
        {
            bool validated = false;
            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                return validated;

            try 
            {
                ReadMembershipDataStore();

                // Validate the user name and password
                MembershipUser user;
                if (_Users.TryGetValue(username, out user)) 
                {
                    validated = CheckPassword(user.Comment, password);
                }

                return validated;
            } 
            catch (Exception) 
            {
                return validated;
            }
        }

        private bool CheckPassword(string storedPassword, string inputPassword)
        {
            bool validated = false;
            if (storedPassword == "")
            {
                // This is a special case used for resetting.
                if (String.Compare(inputPassword, "admin", true) == 0)
                    validated = true;
            }
            else
            {
                if (passwordFormat == MembershipPasswordFormat.Hashed)
                {
                    if (storedPassword == Utils.HashPassword(inputPassword))
                        validated = true;
                }
                else if (inputPassword == storedPassword)
                    validated = true;
            }
            return validated;
        }

        /// <summary>
        /// Retrieves a user based on his/hers username.
        /// the userIsOnline parameter is ignored.
        /// </summary>
        public override MembershipUser GetUser(string username, bool userIsOnline) {
            if (String.IsNullOrEmpty(username))
                return null;

            ReadMembershipDataStore();

            // Retrieve the user from the data source
            MembershipUser user;
            if (_Users.TryGetValue(username, out user))
                return user;

            return null;
        }

        /// <summary>
        /// Retrieves a collection of all the users.
        /// This implementation ignores pageIndex and pageSize,
        /// and it doesn't sort the MembershipUser objects returned.
        /// </summary>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
            ReadMembershipDataStore();
            MembershipUserCollection users = new MembershipUserCollection();

            foreach (KeyValuePair<string, MembershipUser> pair in _Users) {
                users.Add(pair.Value);
            }

            totalRecords = users.Count;
            return users;
        }

        /// <summary>
        /// Changes a users password.
        /// </summary>
        public override bool ChangePassword(string username, string oldPassword, string newPassword) 
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_XmlFileName);
            XmlNodeList nodes = doc.GetElementsByTagName("User");
            foreach (XmlNode node in nodes) 
            {
                if (node["UserName"].InnerText.Equals(username, StringComparison.OrdinalIgnoreCase)) 
                {
                    if (CheckPassword(node["Password"].InnerText, oldPassword))
                    {
                        string passwordPrep;
                        if (passwordFormat == MembershipPasswordFormat.Hashed)
                            passwordPrep = Utils.HashPassword(newPassword);
                        else
                            passwordPrep = newPassword;
                        node["Password"].InnerText = passwordPrep;
                        doc.Save(_XmlFileName);

                        _Users = null;
                        ReadMembershipDataStore();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new user store he/she in the XML file
        /// </summary>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
						ReadMembershipDataStore();

					if (_Users.ContainsKey(username))
						throw new NotSupportedException("The username is already in use. Please choose another username.");

            XmlDocument doc = new XmlDocument();
            doc.Load(_XmlFileName);

            XmlNode xmlUserRoot = doc.CreateElement("User");
            XmlNode xmlUserName = doc.CreateElement("UserName");
            XmlNode xmlPassword = doc.CreateElement("Password");
            XmlNode xmlEmail = doc.CreateElement("Email");
            XmlNode xmlLastLoginTime = doc.CreateElement("LastLoginTime");

            xmlUserName.InnerText = username;

            string passwordPrep;
            if (passwordFormat == MembershipPasswordFormat.Hashed)
                passwordPrep = Utils.HashPassword(password);
            else
                passwordPrep = password;
            xmlPassword.InnerText = passwordPrep;

            xmlEmail.InnerText = email;
            xmlLastLoginTime.InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            xmlUserRoot.AppendChild(xmlUserName);
            xmlUserRoot.AppendChild(xmlPassword);
            xmlUserRoot.AppendChild(xmlEmail);
            xmlUserRoot.AppendChild(xmlLastLoginTime);

            doc.SelectSingleNode("Users").AppendChild(xmlUserRoot);
            doc.Save(_XmlFileName);

            status = MembershipCreateStatus.Success;
						MembershipUser user = new MembershipUser(Name, username, username, email, passwordQuestion, passwordPrep, isApproved, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.MaxValue);
            _Users.Add(username, user);
            return user;
        }

        /// <summary>
        /// Deletes the user from the XML file and 
        /// removes him/her from the internal cache.
        /// </summary>
        public override bool DeleteUser(string username, bool deleteAllRelatedData) {
            XmlDocument doc = new XmlDocument();
            doc.Load(_XmlFileName);

            foreach (XmlNode node in doc.GetElementsByTagName("User")) {
                if (node.ChildNodes[0].InnerText.Equals(username, StringComparison.OrdinalIgnoreCase)) {
                    doc.SelectSingleNode("Users").RemoveChild(node);
                    doc.Save(_XmlFileName);
                    _Users.Remove(username);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a user based on the username parameter.
        /// the userIsOnline parameter is ignored.
        /// </summary>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
            if (providerUserKey == null)
                throw new ArgumentNullException("providerUserKey");

            XmlDocument doc = new XmlDocument();
            doc.Load(_XmlFileName);

            foreach (XmlNode node in doc.SelectNodes("//User")) {
                if (node.ChildNodes[0].InnerText.Equals(providerUserKey.ToString(), StringComparison.OrdinalIgnoreCase)) {
                    string userName = node.ChildNodes[0].InnerText;
                    string password = node.ChildNodes[1].InnerText;
                    string email = node.ChildNodes[2].InnerText;
                    DateTime lastLoginTime = DateTime.Parse(node.ChildNodes[3].InnerText, CultureInfo.InvariantCulture);
                    return new MembershipUser(Name, providerUserKey.ToString(), providerUserKey, email, string.Empty, password, true, false, DateTime.Now, lastLoginTime, DateTime.Now, DateTime.Now, DateTime.MaxValue);
                }
            }

            return default(MembershipUser);
        }

        /// <summary>
        /// Retrieves a username based on a matching email.
        /// </summary>
        public override string GetUserNameByEmail(string email) {
            if (email == null)
                throw new ArgumentNullException("email");

            XmlDocument doc = new XmlDocument();
            doc.Load(_XmlFileName);

            foreach (XmlNode node in doc.GetElementsByTagName("User")) {
                if (node.ChildNodes[2].InnerText.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)) {
                    return node.ChildNodes[0].InnerText;
                }
            }

            return null;
        }

        /// <summary>
        /// Updates a user. The username will not be changed.
        /// </summary>
        public override void UpdateUser(MembershipUser user) {
            XmlDocument doc = new XmlDocument();
            doc.Load(_XmlFileName);

            foreach (XmlNode node in doc.GetElementsByTagName("User")) {
                if (node.ChildNodes[0].InnerText.Equals(user.UserName, StringComparison.OrdinalIgnoreCase)) 
                {
                    if (user.Comment.Length > 30) 
                    {
                        node.ChildNodes[1].InnerText = user.Comment;
                    }
                    node.ChildNodes[2].InnerText = user.Email;
                    node.ChildNodes[3].InnerText = user.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    doc.Save(_XmlFileName);
                    _Users[user.UserName] = user;
                }
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Builds the internal cache of users.
        /// </summary>
        private void ReadMembershipDataStore() {
            lock (this) {
                if (_Users == null) {
                    _Users = new Dictionary<string, MembershipUser>(16, StringComparer.OrdinalIgnoreCase);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_XmlFileName);
                    XmlNodeList nodes = doc.GetElementsByTagName("User");

                    foreach (XmlNode node in nodes) {
                        MembershipUser user = new MembershipUser(
                            Name,                       // Provider name
                            node["UserName"].InnerText, // Username
                            node["UserName"].InnerText, // providerUserKey
                            node["Email"].InnerText,    // Email
                            String.Empty,               // passwordQuestion
                            node["Password"].InnerText, // Comment
                            true,                       // isApproved
                            false,                      // isLockedOut
                            DateTime.Now,               // creationDate
                            DateTime.Parse(node["LastLoginTime"].InnerText, CultureInfo.InvariantCulture), // lastLoginDate
                            DateTime.Now,               // lastActivityDate
                            DateTime.Now, // lastPasswordChangedDate
                            new DateTime(1980, 1, 1)    // lastLockoutDate
                        );

                        _Users.Add(user.UserName, user);
                    }
                }
            }
        }

        #endregion

        #region Unsupported methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public override string ResetPassword(string username, string answer) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public override bool UnlockUser(string userName) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usernameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetNumberOfUsersOnline() {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        /// <returns></returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
            throw new NotSupportedException();
        }

		/// <summary>
		/// Gets the password for the user with the given username.
		/// </summary>
		/// <param name="username">the given username</param>
		/// <returns>the password if user is found, null otherwise.</returns>
		public string GetPassword(string username)
		{
            throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the password for the specified user name from the data source.
		/// </summary>
		/// <param name="username">The user to retrieve the password for.</param>
		/// <param name="answer">The password answer for the user.</param>
		/// <returns>
		/// The password for the specified user name.
		/// </returns>
		public override string GetPassword(string username, string answer)
		{
            throw new NotSupportedException();
		}

        #endregion

    }
}