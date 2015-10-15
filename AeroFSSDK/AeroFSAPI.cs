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
    public interface AeroFSAPI
    {
        /// <summary>
        /// Retrieve the attributes of a folder of interest.
        /// </summary>
        /// <param name="folderID">The ID of the folder of interest.</param>
        /// <param name="fields">Additional on-demand fields to query.</param>
        /// <returns>A Folder object containing attributes of the folder of interest.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        Folder GetFolder(FolderID folderID, GetFolderFields fields = GetFolderFields.None);

        /// <summary>
        /// Retrieve the attributes of a file of interest.
        /// </summary>
        /// <param name="fileID">The ID of the file of interest</param>
        /// <param name="fields">Additional on-demand fields to query.</param>
        /// <returns>A File object containing attributes of the file of interest.</returns>
        File GetFile(FileID fileID, GetFileFields fields = GetFileFields.None);

        /// <summary>
        /// List the files and folders under the top-level root folder.
        /// </summary>
        /// <returns>A Children object containing a list of files and folders.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        Children ListRoot();

        /// <summary>
        /// List the files and folders under a given folder.
        /// </summary>
        /// <param name="folderID">The ID of the given folder.</param>
        /// <returns>A Children object containing a list of files and folders.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        Children ListChildren(FolderID folderID);

        /// <summary>
        /// Create a new folder under an existing folder.
        /// </summary>
        /// <param name="parent">The ID of the parent folder.</param>
        /// <param name="name">The name of the newly created folder.</param>
        /// <returns>The newly created folder.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        Folder CreateFolder(FolderID parent, string name);

        /// <summary>
        /// Delete an existing folder.
        /// </summary>
        /// <param name="folderID">The ID of the folder to be deleted.</param>
        /// <exception cref="WebException">If the request failed.</exception>
        void DeleteFolder(FolderID folderID);

        /// <summary>
        /// Create a new file under an existing folder.
        /// </summary>
        /// <param name="parent">The ID of the parent folder.</param>
        /// <param name="name">The name of the newly created file.</param>
        /// <returns>The newly created file.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        File CreateFile(FolderID parent, string name);

        /// <summary>
        /// Delete an existing file.
        /// </summary>
        /// <param name="fileID">The ID of the file to be deleted.</param>
        /// <exception cref="WebException">If the request failed.</exception>
        void DeleteFile(FileID fileID);

        /// <summary>
        /// Start a new upload sequence to update the content of a file and upload the first chunk.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="content">The stream containing the content to be uploaded.</param>
        /// <returns>The upload progress of the new upload sequence.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        UploadProgress StartUpload(FileID fileID, Stream content);

        /// <summary>
        /// Resume progress on an existing upload sequence.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="uploadID">The ID of the upload sequence.</param>
        /// <returns>The upload progress of the existing upload sequence.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        UploadProgress ResumeUpload(FileID fileID, UploadID uploadID);

        /// <summary>
        /// Continue an existing upload sequence and upload a chunk of content.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="progress">The current progress of this upload sequence.</param>
        /// <param name="content">The stream containing the content to be uploaded.</param>
        /// <returns>A new UploadProgress indicating the latest progress of this upload sequence.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        UploadProgress UploadContent(FileID fileID, UploadProgress progress, Stream content);

        /// <summary>
        /// Finish a upload sequence indicating the completion of the sequence.
        /// </summary>
        /// <remarks>
        /// If this request succeeds, the content update of this file should now be observable.
        /// </remarks>
        /// <param name="fileID">The ID of the file to be updated.</param>
        /// <param name="progress">The upload progress of this upload sequence.</param>
        /// <returns>The new E-Tag for the file.</returns>
        /// <exception cref="WebException">If the request failed.</exception>
        string FinishUpload(FileID fileID, UploadProgress progress);
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
