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
        /// <returns>A ParentPath object containing a list of ancestor folders.</returns>
        ParentPath GetFilePath(FileID fileID);

        /// <summary>
        /// Rename a file and/or move it to a new parent.
        /// </summary>
        /// <param name="fileID">The ID of the folder to be renamed and/or moved.</param>
        /// <param name="parent">The ID of a new parent folder for the file to be moved.</param>
        /// <param name="name">The new name for the file to be moved.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-Match request header.</param>
        /// <returns>A File object containing the attributes of the moved file.</returns>
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
        /// Rename a folder and/or move it to a new parent.
        /// </summary>
        /// <param name="folderID">The ID of the folder to be renamed and/or moved.</param>
        /// <param name="parent">The ID of a new parent folder for the folder to be moved.</param>
        /// <param name="name">The new name for the folder to be moved.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-Match request header.</param>
        /// <returns>A Folder object containing the attributes of the moved folder.</returns>
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
        Stream DownloadFileContent(FileID fileID);

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
        /// <param name="firstName">Optional: A new, updated first name for the user.</param>
        /// <param name="lastName">Optional: A new, updated last name for the user.</param>
        /// <returns>The newly updated User object.</returns>
        User UpdateUser(string email, string firstName = null, string lastName = null);

        /// <summary>
        /// Retrieve a list of users registered with the AeroFS appliance.
        /// </summary>
        /// <param name="limit">Optional: The maximum number of users to return.</param>
        /// <param name="after">Optional: Cursor (user email) used for pagination: page will begin after this user.</param>
        /// <param name="before">Optional: Cursor (user email) used for pagination: page will end before this user.</param>
        /// <returns>A List containing all users registered with the AeroFS appliance.</returns>
        UserPage ListUsers(int? limit = null, string after = null, string before = null);

        /// <summary>
        /// Delete a user from the AeroFS appliance.
        /// </summary>
        void DeleteUser(string email);

        /// <summary>
        /// Change a user's AeroFS password, if it is locally managed.
        /// </summary>
        /// <param name="email">The email address of the user whose password should be changed.</param>
        /// <param name="password">The new password.</param>
        void UpdateUserPassword(string email, string password);

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
        bool IsUserTwoFactorAuthEnabled(string email);

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
        /// <returns>An Invitee object.</returns>
        Invitee CreateInvitee(string emailTo, string emailFrom);

        /// <summary>
        /// Remove the invitation associated with an email address.
        /// </summary>
        /// <param name="email">The email address of the user whose invitation should be deleted.</param>
        void DeleteInvitee(string email);

        /// <summary>
        /// List all shared folders for a given user.
        /// </summary>
        /// <param name="email">The email of the user whose shared folders are to be listed.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-None-Match request header. If this field is provided, a null value will be returned on 304 Not Modified response.</param>
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
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-None-Match request header. If this field is provided, a null value will be returned on 304 Not Modified response.</param>
        /// <returns>Retrieves metadata for a Shared Folder.</returns>
        SharedFolder GetSharedFolder(ShareID sharedFolderID, IList<string> etags = null);

        /// <summary>
        /// List all the members of a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-None-Match request header. If this field is provided, a null value will be returned on 304 Not Modified response.</param>
        /// <returns>A list of SFMember objects representing the members of a Shared Folder.</returns>
        SFMemberList ListSFMembers(ShareID sharedFolderID, IList<string> etags = null);

        /// <summary>
        /// Retrieve information for one member of a shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email address of the memeber.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-None-Match request header. If this field is provided, a null value will be returned on 304 Not Modified response.</param>
        /// <returns>An SFMember object representing the member of the shared folder.</returns>
        SFMember GetSFMember(ShareID sharedFolderID, string email, IList<string> etags = null);

        /// <summary>
        /// Add a user to a given shared folder, bypassing the invitation flow.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email of the new member.</param>
        /// <param name="permissions">The permissions for the new member of the shared folder.</param>
        /// <returns>An SFMember object representing the new member of the shared folder.</returns>
        SFMember AddSFMemberToSharedFolder(ShareID sharedFolderID, string email, IEnumerable<Permission> permissions);

        /// <summary>
        /// Update permissions for one member of a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email of the member.</param>
        /// <param name="permissions">A list of the member's new permissions.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-Match request header.</param>
        /// <returns>An SFMember object representing the updated member of the shared folder.</returns>
        SFMember SetSFMemberPermissions(ShareID sharedFolderID, string email, IEnumerable<Permission> permissions, IList<string> etags = null);

        /// <summary>
        /// Remove a member from a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email of the member to remove.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-Match request header.</param>
        void RemoveMemberFromSharedFolder(ShareID sharedFolderID, string email, IList<string> etags = null);

        /// <summary>
        /// List all the groups in a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <returns>A list of SFGroupMember objects.</returns>
        SFGroupMemberList ListSFGroupMembers(ShareID sharedFolderID);

        /// <summary>
        /// Retrieve information for one group within a shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="groupID">The ID of the group to retrieve.</param>
        /// <returns>An SFGroupMember object.</returns>
        SFGroupMember GetSFGroupMember(ShareID sharedFolderID, GroupID groupID);

        /// <summary>
        /// Add a group to a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="groupID">The ID of the group to be added to the shared folder.</param>
        /// <param name="persmissions">The permissions to be granted to the newly added group.</param>
        /// <returns>An SFGroupMember object.</returns>
        SFGroupMember AddGroupToSharedFolder(ShareID sharedFolderID, GroupID groupID, IEnumerable<Permission> persmissions);

        /// <summary>
        /// Update permissions for one group in a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="groupID">The ID of the group whose permissions are to be updated.</param>
        /// <param name="permissions">The updated set of permissions for the group.</param>
        /// <returns>An SFGroupMember object.</returns>
        SFGroupMember UpdateGroupPermissions(ShareID sharedFolderID, GroupID groupID, IEnumerable<Permission> permissions);

        /// <summary>
        /// Remove a group from a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="groupID">The ID of the group to remove.</param>
        void RemoveGroupFromSharedFolder(ShareID sharedFolderID, GroupID groupID);

        /// <summary>
        /// List all pending members of a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="etags">Optional: list of strings representing ETags to be used in the If-None-Match request header. If this field is provided, a null value will be returned on 304 Not Modified response.</param>
        /// <returns>A list of SFPendingMember objects.</returns>
        SFPendingMemberList ListSFPendingMembers(ShareID sharedFolderID, IList<string> etags = null);

        /// <summary>
        /// Retrieve information pertaining to a pending member of a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email address of the pending member.</param>
        /// <returns>An SFPendingMember object.</returns>
        SFPendingMember GetSFPendingMember(ShareID sharedFolderID, string email);

        /// <summary>
        /// Invite a user to join a given shared folder.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email address of the user to invite.</param>
        /// <param name="permissions">The permissions of the user to invite.</param>
        /// <param name="note">A note for the newly invited user.</param>
        /// <returns>An SFPendingMember object.</returns>
        SFPendingMember InviteUserToSharedFolder(ShareID sharedFolderID, string email, IEnumerable<Permission> permissions, string note);

        /// <summary>
        /// Remove a pending member from a given shared folder, revoking their invitation.
        /// </summary>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="email">The email address of the member to be removed.</param>
        void RemoveSFPendingMember(ShareID sharedFolderID, string email);

        /// <summary>
        /// List all pending shared folder invitations for a given user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A list of Invitation objects.</returns>
        InvitationList ListInvitations(string email);

        /// <summary>
        /// Retrieve a pending shared folder invitation for a given user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <returns>An Invitation object.</returns>
        Invitation GetInvitation(string email, ShareID sharedFolderID);

        /// <summary>
        /// Accept a shared folder invitation on behalf of a given user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        /// <param name="external">A bool value indicating whether or not to join the folder as external.</param>
        /// <returns>A SharedFolder object.</returns>
        SharedFolder AcceptInvitation(string email, ShareID sharedFolderID, bool? external = null);

        /// <summary>
        /// Ignore a pending shared folder invitation for a given user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="sharedFolderID">The ID of the shared folder.</param>
        void DeleteInvitation(string email, ShareID sharedFolderID);

        /// <summary>
        /// List AeroFS groups.
        /// </summary>
        /// <param name="offset">Offset for the start of the returned list.</param>
        /// <param name="numResults">Number of results in the returned list.</param>
        /// <returns>A list of Group objects.</returns>
        GroupList ListGroups(int? offset = null, int? numResults = null);

        /// <summary>
        /// Create a new AeroFS group.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <returns>A Group object.</returns>
        Group CreateGroup(string name);

        /// <summary>
        /// Get information about a group.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <returns>A Group object.</returns>
        Group GetGroup(GroupID groupID);

        /// <summary>
        /// Delete a group.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        void DeleteGroup(GroupID groupID);

        /// <summary>
        /// List all the members of a group.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <returns>A list of GroupMember objects.</returns>
        GroupMemberList ListGroupMembers(GroupID groupID);

        /// <summary>
        /// Add a user to a group.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <param name="email">The email of the user to add to the group.</param>
        /// <returns>A GroupMember object.</returns>
        GroupMember AddMemberToGroup(GroupID groupID, string email);

        /// <summary>
        /// Retrieve information on one member of a group.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <param name="email">The email address of the group member to retrieve.</param>
        /// <returns>A GroupMember object.</returns>
        GroupMember GetGroupMember(GroupID groupID, string email);

        /// <summary>
        /// Remove an AeroFS user from one of their groups.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <param name="email">The email address of the user to remove.</param>
        void RemoveGroupMember(GroupID groupID, string email);

        /// <summary>
        /// List all devices belonging to a given user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A list of Device objects.</returns>
        DeviceList ListDevices(string email);

        /// <summary>
        /// Retrieve metadata for a given device.
        /// </summary>
        /// <param name="deviceID">The ID of the device.</param>
        /// <returns>A Device object.</returns>
        Device GetDevice(DeviceID deviceID);

        /// <summary>
        /// Update metadata for a given device.
        /// </summary>
        /// <param name="deviceID">The ID of the device.</param>
        /// <param name="name">The new name for the device.</param>
        /// <returns>A Device object.</returns>
        Device UpdateDevice(DeviceID deviceID, string name);

        /// <summary>
        /// Retrieve status information for a given device.
        /// </summary>
        /// <param name="deviceID">The ID of the device.</param>
        /// <returns>A DeviceStatus object.</returns>
        DeviceStatus GetDeviceStatus(DeviceID deviceID);
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
