﻿using System.DirectoryServices;

namespace FSS.Omnius.Modules.Nexus.Service
{
    public interface INexusLdapService
    {
        SearchResult SearchByLogin(string login, string baseDN = "", string[] properties = null);
        SearchResult SearchByEmail(string email, string baseDN = "", string[] properties = null);
        SearchResultCollection GetGroups(string baseDN = "", string[] properties = null);
        SearchResultCollection GetUsers(string baseDN = "", string[] properties = null);
        SearchResultCollection Search(string filter, string baseDN = "", string[] properties = null);
        SearchResult FindOne(string filter, string baseDN = "", string[] properties = null);
        void UseServer(string serverName);
    }
}