// Borrowed from BlogEngine.NET

// Written by: Roman D. Clarkson
// http://www.romanclarkson.com  inspirit@romanclarkson.com


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml;
using System.Xml.Serialization;

namespace XmlProviders
{
	/// <summary>
	/// 
	/// </summary>
	public class Role
	{
		private string _Name;
		private List<string> _UserNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="Role"/> class.
		/// </summary>
		/// <param name="name">A name.</param>
		public Role(string name)
		{
			_Name = name;
			_UserNames = new List<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Role"/> class.
		/// </summary>
		public Role()
		{
			_UserNames = new List<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Role"/> class.
		/// </summary>
		/// <param name="name">A name.</param>
		/// <param name="userNames">A list of users in role.</param>
		public Role(string name, List<string> userNames)
		{
			_Name = name;
			_UserNames = userNames;
		}


		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		/// <summary>
		/// Gets the users.
		/// </summary>
		/// <value>The users.</value>
		public List<string> Users
		{
			get { return _UserNames; }
			//set { _UserNames = value; }
		}
	}




    ///<summary>
    ///</summary>
    public class XmlRoleProvider : RoleProvider
    {
        #region Properties

        private List<Role> _Roles = new List<Role>();
        private List<string> _UserNames;
        private string _XmlFileName;
        //readonly string[] _DefaultRolesToAdd = new string[] { BlogSettings.Instance.AdministratorRole };


        ///<summary>
        ///Gets or sets the name of the application to store and retrieve role information for.
        ///</summary>
        ///
        ///<returns>
        ///The name of the application to store and retrieve role information for.
        ///</returns>
        ///
        public override string ApplicationName
        {
            get { return "BlogEngine.NET"; }
            set { }
        }

        ///<summary>
        ///Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        ///</returns>
        ///
        ///<param name="roleName">The name of the role to search for in the data source. </param>
        public override bool RoleExists(string roleName)
        {
            List<string> currentRoles = new List<string>(GetAllRoles());
            return (currentRoles.Contains(roleName)) ? true : false;
        }

        ///<summary>
        ///Gets a list of all the roles for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the roles stored in the data source for the configured applicationName.
        ///</returns>
        ///
        public override string[] GetAllRoles()
        {
            List<string> allRoles = new List<string>();
            foreach (Role role in _Roles)
            {
                allRoles.Add(role.Name);
            }
            return allRoles.ToArray();
        }

        ///<summary>
        ///Gets a list of users in the specified role for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        ///</returns>
        ///
        ///<param name="roleName">The name of the role to get the list of users for. </param>
        public override string[] GetUsersInRole(string roleName)
        {
            //  ReadRoleDataStore();
            List<string> UsersInRole = new List<string>();

            foreach (Role role in _Roles)
            {
                if (role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string user in role.Users)
                    {
                        UsersInRole.Add(user.ToLowerInvariant());
                    }
                }
            }
            return UsersInRole.ToArray();
        }

        ///<summary>
        ///Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        ///</returns>
        ///
        ///<param name="username">The user name to search for.</param>
        ///<param name="roleName">The role to search in.</param>
        public override bool IsUserInRole(string username, string roleName)
        {
            foreach (Role role in _Roles)
            {
                if (role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string user in role.Users)
                    {
                        if (user.Equals(username, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }

        ///<summary>
        ///Gets a list of the roles that a specified user is in for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        ///</returns>
        ///
        ///<param name="username">The user to return a list of roles for.</param>
        public override string[] GetRolesForUser(string username)
        {
            //  ReadRoleDataStore();
            List<string> rolesForUser = new List<string>();

            foreach (Role role in _Roles)
            {
                foreach (string user in role.Users)
                {
                    if (user.Equals(username, StringComparison.OrdinalIgnoreCase))
                        rolesForUser.Add(role.Name);
                }
            }
            return rolesForUser.ToArray();
        }

        #endregion

        #region Supported methods

        ///<summary>
        ///Gets an array of user names in a role where the user name contains the specified user name to match.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
        ///</returns>
        ///
        ///<param name="usernameToMatch">The user name to search for.</param>
        ///<param name="roleName">The role to search in.</param>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            List<string> UsersInRole = new List<string>();
            if (IsUserInRole(usernameToMatch, roleName))
                UsersInRole.AddRange(_UserNames);
            return UsersInRole.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            ReadMembershipDataStore();
            if (config == null)
                throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "XmlMembershipProvider";

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML role provider");
            }

            base.Initialize(name, config);

            // Initialize _XmlFileName and make sure the path
            // is app-relative
            string path = config["xmlFileName"];

            if (String.IsNullOrEmpty(path))
                path = "~/App_Data/roles.xml";


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

            //if (!System.IO.File.Exists(_XmlFileName))
                //AddUsersToRoles(_UserNames.ToArray(), _DefaultRolesToAdd);

            //Now that we know a xml file exists we can call it.
            ReadRoleDataStore();

            //if (!RoleExists(BlogSettings.Instance.AdministratorRole))
            //    AddUsersToRoles(_UserNames.ToArray(), _DefaultRolesToAdd);



            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                    throw new ProviderException("Unrecognized attribute: " + attr);
            }


        }

        ///<summary>
        ///Adds the specified user names to the specified roles for the configured applicationName.
        ///</summary>
        ///
        ///<param name="roleNames">A string array of the role names to add the specified user names to. </param>
        ///<param name="usernames">A string array of user names to be added to the specified roles. </param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            List<string> currentRoles = new List<string>(GetAllRoles());
            if (usernames.Length != 0 && roleNames.Length != 0)
            {
                foreach (string _rolename in roleNames)
                {
                    if (!currentRoles.Contains(_rolename))
                    {
                        _Roles.Add(new Role(_rolename, new List<string>(usernames)));
                    }
                }

                foreach (Role role in _Roles)
                {
                    foreach (string _name in roleNames)
                    {
                        if (role.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (string s in usernames)
                            {
                                if (!role.Users.Contains(s))
                                    role.Users.Add(s);
                            }
                        }
                    }
                }
            }
            Save();
        }

        ///<summary>
        ///Removes the specified user names from the specified roles for the configured applicationName.
        ///</summary>
        ///
        ///<param name="roleNames">A string array of role names to remove the specified user names from. </param>
        ///<param name="usernames">A string array of user names to be removed from the specified roles. </param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames.Length != 0 && roleNames.Length != 0)
            {
                foreach (Role role in _Roles)
                {
                    foreach (string _name in roleNames)
                    {
                        if (role.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (string user in usernames)
                            {

								RemoveItemFromList(role.Users, user);
								/*
                                if (role.Name.Equals(BlogSettings.Instance.AdministratorRole, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (role.Users.Count != 1)
                                    {
                                        //if (role.Users.Contains(user))
                                        //    role.Users.Remove(user);
                                        RemoveItemFromList(role.Users, user);
                                    }
                                }
                                else
                                {
                                    //if (role.Users.Contains(user))
                                    //    role.Users.Remove(user);
                                    RemoveItemFromList(role.Users, user);
                                }
								 * */
                            }
                            break;
                        }
                    }
                }
            }
            Save();
        }

        private static void RemoveItemFromList(ICollection<string> list, string item)
        {
            if (list == null || string.IsNullOrEmpty(item) || list.Count == 0)
                return;

            List<string> usersToRemove = new List<string>();

            foreach (string u in list)
            {
                if (u.Equals(item, StringComparison.OrdinalIgnoreCase))
                    usersToRemove.Add(u);
            }
            foreach (string u in usersToRemove)
            {
                list.Remove(u);
            }
        }

        ///<summary>
        ///Removes a role from the data source for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///true if the role was successfully deleted; otherwise, false.
        ///</returns>
        ///
        ///<param name="throwOnPopulatedRole">If true, throw an exception if roleName has one or more members and do not delete roleName.</param>
        ///<param name="roleName">The name of the role to delete.</param>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            //if (!roleName.Equals(BlogSettings.Instance.AdministratorRole, StringComparison.OrdinalIgnoreCase))
           	//{
                _Roles.Remove(new Role(roleName));
                Save();
                return true;
            //}
            //return false;
        }

        ///<summary>
        ///Adds a new role to the data source for the configured applicationName.
        ///</summary>
        ///
        ///<param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (!_Roles.Contains(new Role(roleName)))
            {
                _Roles.Add(new Role(roleName));
                Save();
            }

        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Builds the internal cache of users.
        /// </summary>
        private void ReadRoleDataStore()
        {
            lock (this)
            {
                XmlDocument doc = new XmlDocument();

                try
                {
                    doc.Load(_XmlFileName);
                    XmlNodeList nodes = doc.GetElementsByTagName("role");
                    foreach (XmlNode roleNode in nodes)
                    {
                        Role tempRole = new Role(roleNode.SelectSingleNode("name").InnerText);
                        foreach (XmlNode userNode in roleNode.SelectNodes("users/user"))
                        {
                            tempRole.Users.Add(userNode.InnerText);
                        }
                        _Roles.Add(tempRole);

                    }
                }
                catch (XmlException)
                {
                    //AddUsersToRoles(_UserNames.ToArray(), _DefaultRolesToAdd);
                }

            }
        }

        ///<summary>
        ///</summary>
        public void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(_XmlFileName, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("roles");

                foreach (Role _role in _Roles)
                {
                    writer.WriteStartElement("role");
                    writer.WriteElementString("name", _role.Name);
                    writer.WriteStartElement("users");
                    foreach (string username in _role.Users)
                    {
                        writer.WriteElementString("user", username);
                    }
                    writer.WriteEndElement(); //closes users
                    writer.WriteEndElement(); //closes role
                }
            }

        }

        /// <summary>
        /// Only so we can add users to the adminstrators role.
        /// </summary>
        private void ReadMembershipDataStore()
        {
            string fullyQualifiedPath = VirtualPathUtility.Combine
              (VirtualPathUtility.AppendTrailingSlash
              (HttpRuntime.AppDomainAppVirtualPath), "~/App_data/users.xml");

            lock (this)
            {
                if (_UserNames == null)
                {
                    _UserNames = new List<string>();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(HostingEnvironment.MapPath(fullyQualifiedPath));
                    XmlNodeList nodes = doc.GetElementsByTagName("User");

                    foreach (XmlNode node in nodes)
                    {
                        _UserNames.Add(node["UserName"].InnerText);
                    }

                }
            }
        }
        #endregion
    }

}