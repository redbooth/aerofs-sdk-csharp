using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AeroFSSDK
{
    /// <summary>
    /// A specification of the AeroFS API accessible through an API client.
    /// </summary>
    ///
    /// <remarks>
    /// All calls throw WebException on failures.
    /// </remarks>
    public interface AeroFSAPI
    {
        /// <summary>
        /// Retrieve the attributes of a folder of interest.
        /// </summary>
        /// <param name="folderID">The ID of the folder of interest.</param>
        /// <param name="fields">Additional on-demand fields to query.</param>
        /// <returns>A Folder object containing attributes of the folder of interest.</returns>
        Folder GetFolder(FolderID folderID, GetFolderFields fields = GetFolderFields.None);

        /// <summary>
        /// Retrieve the attributes of a file of interest.
        /// </summary>
        /// <param name="fileID">The ID of the file of interest</param>
        /// <param name="fields">Additional on-demand fields to query.</param>
        /// <returns>A File object containing attributes of the file of interest.</returns>
        File GetFile(FileID fileID, GetFileFields fields = GetFileFields.None);

        /// <summary>
        /// Retrieve the path to a file of interest
        /// </summary>
        /// <param name="fileID">The ID of the file of interest</param>
        /// <returns>A List of Folder objects representing the path to the file of interest.</returns>
        ParentPath GetFilePath(FileID fileID);

        /// <summary>
        /// Rename a file and/or move it to a new parent.
        /// </summary>
        /// <param name="fileID">The ID of the folder to be renamed and/or moved.</param>
        /// <param name="parent">The ID of a new parent folder for the file to be moved.</param>
        /// <param name="name">The new name for the file to be moved.</param>
        /// <param name="etags">Optional paramater representing Entity Tags for conditional requests.</param>
        /// <returns></returns>
        File MoveFile(FileID fileID, FolderID parent, string name, IList<string> etags = null);

        /// <summary>
        /// List the files and folders under the top-level root folder.
        /// </summary>
        /// <returns>A Children object containing a list of files and folders.</returns>
        Children ListRoot();

        /// <summary>
        /// List the files and folders under a given folder.
        /// </summary>
        /// <param name="folderID">The ID of the given folder.</param>
        /// <returns>A Children object containing a list of files and folders.</returns>
        Children ListChildren(FolderID folderID);

        /// <summary>
        /// Create a new folder under an existing folder.
        /// </summary>
        /// <param name="parent">The ID of the parent folder.</param>
        /// <param name="name">The name of the newly created folder.</param>
        /// <returns>The newly created folder.</returns>
        Folder CreateFolder(FolderID parent, string name);

        /// <summary>
        /// Delete an existing folder.
        /// </summary>
        /// <param name="folderID">The ID of the folder to be deleted.</param>
        void DeleteFolder(FolderID folderID);

        /// <summary>
        /// Share an already existing folder.
        /// </summary>
        /// <param name="folderID">The ID of the folder to be shared.</param>
        void ShareFolder(FolderID folderID);

        /// <summary>
        /// Rename a folder and/or move it to a new parent.
        /// </summary>
        /// <param name="folderID">The ID of the folder to be renamed and/or moved.</param>
        /// <param name="parent">The ID of a new parent folder for the folder to be moved.</param>
        /// <param name="name">The new name for the folder to be moved.</param>
        /// <param name="etags">Optional paramater representing Entity Tags for conditional requests.</param>
        /// <returns></returns>
        Folder MoveFolder(FolderID folderID, FolderID parent, string name, IList<string> etags = null);

        /// <summary>
        /// Create a new file under an existing folder.
        /// </summary>
        /// <param name="parent">The ID of the parent folder.</param>
        /// <param name="name">The name of the newly created file.</param>
        /// <returns>The newly created file.</returns>
        File CreateFile(FolderID parent, string name);

        /// <summary>
        /// Delete an existing file.
        /// </summary>
        /// <param name="fileID">The ID of the file to be deleted.</param>
        void DeleteFile(FileID fileID);

        /// <summary>
        /// Start a new upload sequence to update the content of a file and upload the first chunk.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="content">The stream containing the content to be uploaded.</param>
        /// <returns>The upload progress of the new upload sequence.</returns>
        UploadProgress StartUpload(FileID fileID, Stream content);

        /// <summary>
        /// Resume progress on an existing upload sequence.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="uploadID">The ID of the upload sequence.</param>
        /// <returns>The upload progress of the existing upload sequence.</returns>
        UploadProgress ResumeUpload(FileID fileID, UploadID uploadID);

        /// <summary>
        /// Continue an existing upload sequence and upload a chunk of content.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="progress">The current progress of this upload sequence.</param>
        /// <param name="content">The stream containing the content to be uploaded.</param>
        /// <returns>A new UploadProgress indicating the latest progress of this upload sequence.</returns>
        UploadProgress UploadContent(FileID fileID, UploadProgress progress, Stream content);

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="fileID">The ID of the file to be downloaded.</param>
        /// <returns>A Stream object containing the file's data.</returns>
        Stream DownloadFile(FileID fileID);

        /// <summary>
        /// Finish a upload sequence indicating the completion of the sequence.
        /// </summary>
        /// <remarks>
        /// If this request succeeds, the content update of this file should now be observable.
        /// </remarks>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="progress">The upload progress of this upload sequence.</param>
        /// <returns>The new E-Tag for the file.</returns>
        string FinishUpload(FileID fileID, UploadProgress progress);

        /// <summary>
        /// List all links in a shared folder.
        /// </summary>
        /// <param name="shareID">The ShareID of the shared folder.</param>
        /// <returns>A list containing all links in the shared folder.</returns>
        IList<Link> ListLinks(ShareID shareID);

        /// <summary>
        /// Get all information on a given link.
        /// </summary>
        /// <param name="shareID">The ShareID of the shared folder containing the link.</param>
        /// <param name="key">The key of the inquired link.</param>
        /// <returns>All informations on the inquired link.</returns>
        Link GetLinkInfo(ShareID shareID, LinkID key);

        /// <summary>
        /// Create a link to a target file or folder.
        /// </summary>
        /// <param name="objectID">The ObjectID of the target file or folder.</param>
        /// <param name="password">Optionally requiring a password to access the link.
        ///     Empty string and null value are interpreted as no passwords.</param>
        /// <param name="requireLogin">Optionally restricting link access to authenticated users.
        ///     Null value is interpreted as leaving this to appliance default.</param>
        /// <param name="expiry">Optionally sets an expiry in UTC.
        ///     Null value is interpreted as the new link should not expire ever.</param>
        /// <returns>The newly created link.</returns>
        Link CreateLink(ObjectID objectID,
            string password = null, bool? requireLogin = null, long? expiry = null);

        /// <summary>
        /// Delete an existing link.
        /// </summary>
        /// <param name="shareID">The ShareID of the given link's target.</param>
        /// <param name="key">The key of the link to be deleted.</param>
        void DeleteLink(ShareID shareID, LinkID key);

        /// <summary>
        /// Update information on a given link.
        /// </summary>
        /// <param name="shareID">The SharedID of the given link's target.</param>
        /// <param name="key">The key of the given link.</param>
        /// <param name="password">Optionally updating the setting to require a password to access the link.
        ///     Empty string and null value are interpreted as do not update existing value.</param>
        /// <param name="requireLogin">Optionally updating the setting to restrict link access to authenticated users.
        ///     Null value is interpreted as do not update existing value.</param>
        /// <param name="expiry">Optionally update the expiry of the link.
        ///     Null value is interpreted as do not update existing value.</param>
        /// <returns>The updated information on the given link.</returns>
        /// <remarks>
        /// This call is unable to achieve any of the following:
        ///     - Remove the password associated with the link.
        ///     - Remove the expiry of the link.
        /// </remarks>
        Link UpdateLinkInfo(ShareID shareID, LinkID key,
            string password = null, bool? requireLogin = null, long? expiry = null);

        /// <summary>
        /// Update the password required to access a given link.
        /// </summary>
        /// <param name="shareID">The SharedID of the given link's target.</param>
        /// <param name="key">The key of the given link.</param>
        /// <param name="password">The new password for the link.</param>
        /// <returns>The updated information on the given link.</returns>
        /// <remarks>
        /// This call is unable to remove the password for a link.
        /// </remarks>
        Link UpdateLinkPassword(ShareID shareID, LinkID key, string password);

        /// <summary>
        /// Remove requiring a password to access the given link.
        /// </summary>
        /// <param name="shareID">The ShareID of the given link's target.</param>
        /// <param name="key">The key of the given link.</param>
        /// <returns>The updated information on the given link.</returns>
        Link RemoveLinkPassword(ShareID shareID, LinkID key);

        /// <summary>
        /// Update the setting to restrict link access to authenticated users for a given link.
        /// </summary>
        /// <param name="shareID">The SharedID of the given link's target.</param>
        /// <param name="key">The key of the given link.</param>
        /// <param name="requireLogin">Setting this value to true will restrict link access to authenticated users only.</param>
        /// <returns>The updated information on the given link.</returns>
        Link UpdateLinkRequireLogin(ShareID shareID, LinkID key, bool requireLogin);

        /// <summary>
        /// Update the expiry of a given link.
        /// </summary>
        /// <param name="shareID">The ShareID of the given link's target.</param>
        /// <param name="key">The key of the given link.</param>
        /// <param name="expiry">The new expiry, in UTC, for the given link.</param>
        /// <remarks>
        /// - This call is unable to remove the expiry for the given link.
        /// - The Expires field in the response is unlikely to match the input expiry.
        /// </remarks>
        /// <returns>The updated information on the given link.</returns>
        Link UpdateLinkExpiry(ShareID shareID, LinkID key, long expiry);

        /// <summary>
        /// Remove the expiry of a given link.
        /// </summary>
        /// <param name="shareID">The ShareID of the given link's target.</param>
        /// <param name="key">The key of the given link.</param>
        /// <returns>The updated information on the given link.</returns>
        Link RemoveLinkExpiry(ShareID shareID, LinkID key);

        /// <summary>
        /// Retrieve information on an existing user.
        /// </summary>
        /// <param name="email">The email address of the inquired user.</param>
        /// <returns>The inquired user information.</returns>
        User GetUserInfo(string email);

        /// <summary>
        /// Create a new AeroFS user.
        /// </summary>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="firstName">The first name of the new user.</param>
        /// <param name="lastName">The last name of the new user.</param>
        /// <returns>The newly created User object.</returns>
        User CreateUser(string email, string firstName, string lastName);

        /// <summary>
        /// Update an existing AeroFS user.
        /// </summary>
        /// <param name="email">The email address of the user to update.</param>
        /// <param name="firstName">The updated first name of the user.</param>
        /// <param name="lastName">The updated last name of the  user.</param>
        /// <returns>The newly updated User object.</returns>
        User UpdateUser(string email, string firstName = null, string lastName = null);

        /// <summary>
        /// Retrieve a list of users registered with the AeroFS appliance.
        /// </summary>
        /// <param name="limit">The maximum number of users to return.</param>
        /// <param name="after">Cursor used for pagination: page will begin after this user index.</param>
        /// <param name="before">Cursor used for pagination: page will end before this user index.</param>
        /// <returns>A List containing all users registered with the AeroFS appliance.</returns>
        UserPage ListUsers(int limit = 20, int after = -1, int before = -1);

        /// <summary>
        /// Delete a user from the AeroFS appliance.
        /// </summary>
        void DeleteUser(string email);

        /// <summary>
        /// Change a user's AeroFS password, if it is locally managed.
        /// </summary>
        /// <param name="email">The email address of the user whose password should be changed.</param>
        /// <param name="password">The new password.</param>
        void ChangeUserPassword(string email, string password);

        /// <summary>
        /// Disable a user's AeroFS password until manual reset, if it is locally managed.
        /// </summary>
        /// <param name="email">The email address of the user whose password should be disabled.</param>
        void DisableUserPassword(string email);

        /// <summary>
        /// Check if a user has two-factor authentication enabled.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>A bool value indicating whether or not the user has two-factor auth. enabled.</returns>
        bool CheckUserTwoFactorAuthEnabled(string email);

        /// <summary>
        /// Disable two-factor authentication for a user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        void DisableUserTwoFactorAuth(string email);

        /// <summary>
        /// Retrieve information about an outstanding invitation to AeroFS.
        /// </summary>
        /// <param name="email">The user that has been invited to join AeroFS.</param>
        /// <returns>An Invitee object representing the invitation to the user.</returns>
        Invitee GetInviteeInfo(string email);

        /// <summary>
        /// Send an invitation to a potential AeroFS user.
        /// </summary>
        /// <param name="emailTo">The email address of the invitee.</param>
        /// <param name="emailFrom">The email address of the user who is sending the invitation.</param>
        /// <returns></returns>
        Invitee CreateInvitation(string emailTo, string emailFrom);

        /// <summary>
        /// Remove the invitation associated with an email address.
        /// </summary>
        /// <param name="email">The email address of the user whose invitation should be deleted.</param>
        void DeleteInvitation(string email);

        /// <summary>
        /// List all shared folders for a given user.
        /// </summary>
        /// <param name="email">The email of the user whose shared folders are to be listed.</param>
        /// <param name="etags">Optional parameter for sequential conditional requests.</param>
        /// <returns>A list of SharedFolder objects representing all the user's shared folders.</returns>
        SharedFolderList ListSharedFolders(string email, IList<string> etags = null);

        /// <summary>
        /// Create a new shared folder.
        /// </summary>
        /// <param name="name">The name of the shared folder to create.</param>
        /// <returns>A SharedFolder object representing the newly created shared folder.</returns>
        SharedFolder CreateSharedFolder(string name);

        /// <summary>
        /// Retrieve metadata for a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="etags">Optional paramater representing Entity Tags for conditional requests.</param>
        /// <returns>Retrieves metadata for a Shared Folder.</returns>
        SharedFolder GetSharedFolder(ShareID sharedFolderID, IList<string> etags = null);

        /// <summary>
        /// List all the members of a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="etags">Optional paramater representing Entity Tags for conditional requests.</param>
        /// <returns>A list of SFMember objects representing the members of a Shared Folder.</returns>
        SFMemberList ListSFMembers(ShareID sharedFolderID, IList<string> etags = null);

        /// <summary>
        /// Retrieve information for one member of a shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email address of the memeber.</param>
        /// <param name="etags">Optional paramater representing Entity Tags for conditional requests.</param>
        /// <returns>An SFMember object representing the member of the shared folder.</returns>
        SFMember GetSFMember(ShareID sharedFolderID, string email, IList<string> etags = null);

        /// <summary>
        /// Add a user to a given shared folder, bypassing the invitation flow.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email of the new member.</param>
        /// <param name="permissions">The permissions for the new member of the shared folder.</param>
        /// <returns>An SFMember object representing the new member of the shared folder.</returns>
        SFMember AddSFMemberToSharedFolder(ShareID sharedFolderID, string email, IList<Permission> permissions);

        /// <summary>
        /// Update permissions for one member of a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email of the member.</param>
        /// <param name="permissions">A list of the member's new permissions.</param>
        /// <param name="etags">Optional paramater representing Entity Tags for conditional requests.</param>
        /// <returns>An SFMember object representing the updated member of the shared folder.</returns>
        SFMember SetSFMemberPermissions(ShareID sharedFolderID, string email, IList<Permission> permissions, IList<string> etags = null);
    }

    /// <summary>
    /// Content state for Files.
    /// </summary>
    public enum ContentState
    {
        Available,
        Syncing,
        Deselected,
        Insufficient_Storage
    }

    /// <summary>
    /// Permissions for Shared Folders.
    /// </summary>
    public enum Permission
    {
        Write,
        Manage
    }

    /// <summary>
    /// Flags for on-demand fields in the GetFile() call.
    /// </summary>
    [Flags]
    public enum GetFileFields
    {
        None = 0x0,
        Path = 0x1,
    }

    /// <summary>
    /// Flags for on-demand fields in the GetFolder() call.
    /// </summary>
    [Flags]
    public enum GetFolderFields
    {
        None = 0x0,
        Path = 0x1,
        Children = 0x2,
    }
}
