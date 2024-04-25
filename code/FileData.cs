using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;


public static class FileData
{
    /// <summary>
    /// Removes the last part of a folder path, or the filename off a path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string TruncatePath(string path)
    {
        if (path.EndsWith("\\"))
        {
            path = path.Substring(0, path.Length - 1);
            int index = path.LastIndexOf('\\');
            if (index < 0)
                return string.Empty;
            return path.Substring(0, index + 1);
        }
        else if (path.EndsWith("/"))
        {
            path = path.Substring(0, path.Length - 1);
            int index = path.LastIndexOf('/');
            if (index < 0)
                return string.Empty;
            return path.Substring(0, index + 1);
        }
        else
        {
            int index = path.LastIndexOfAny("/\\".ToCharArray());
            if (index < 0)
                return string.Empty;
            return path.Substring(0, index + 1);
        }
    }
    /// <summary>
    /// Writes the source to a file at the path.
    /// Existing files are overwritten.
    /// </summary>
    /// <param name="path">target file path</param>
    /// <param name="source">the text to write</param>
    public static bool ToFile(string source, string path)
    {
        try
        {
            path = NormalizePath(path);
            File.WriteAllText(path, source);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads the file at the path and returns the contents as a string.
    /// </summary>
    /// <param name="path">The file to read</param>
    public static string FromFile(string path)
    {
        path = NormalizePath(path);
        return System.IO.File.ReadAllText(path);
    }
    /// <summary>
    /// Append to an existing file, or creats if needed.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="path"></param>
    public static bool AddToFile(string source, string path)
    {
        try
        {
            path = NormalizePath(path);
            File.AppendAllText(path, source);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Return true if a file exists
    /// </summary>
    /// <param name="path">The file to check</param>
    public static bool IsFile(string path)
    {
        path = NormalizePath(path);
        return System.IO.File.Exists(path);
    }
    /// <summary>
    /// Delete a file if possible (fails silently)
    /// </summary>
    /// <param name="path">The file to delete</param>
    public static bool DeleteFile(string path)
    {
        try
        {
            path = NormalizePath(path);
            if (IsFile(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }

    }

    /// <summary>
    /// Runs a file using the system default handler.
    /// Fails silently, but returns the process if successful, null otherwise.
    /// The process can be inspected for errors too.
    /// </summary>
    /// <param name="path"></param>
    public static Process? RunFile(string path)
    {
        try
        {
            path = NormalizePath(path);
            if (IsFile(path))
            {
                System.Diagnostics.ProcessStartInfo processStartInfo =
                    new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true, };
                return System.Diagnostics.Process.Start(processStartInfo);
            }
            return null;
        }
        catch
        {
            return null; // intentionally suppressed.
        }
    }
    /// <summary>
    /// Ensure existence of a folder (recursively) or file, setting the file last access date to now. Returns true if the file/folder was newly created.
    /// </summary>
    /// <param name="path">If this ends with a slash (forward or backward are accepted) it is regarded as a folder name, otherwise a file is assumed</param>
    /// <returns></returns>
    public static bool Touch(string path)
    {
        path = NormalizePath(path);
        if (path.EndsWith("\\"))
        {   // interpret as a folder
            return EnsureFolder(path);
        }
        else
        {   // interpret as a file
            var created = EnsureFile(path);
            if (created == false)
            {
                File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
            }
            return created; ;
        }
    }
    /// <summary>
    /// Ensures that a directory path exists. If the submission does not end with a slash the portion beyond the last slash is ignored. 
    /// This allows you to verify the path portino of a file path ignoring the file name.
    /// </summary>
    /// <param name="path">The intended path,using forward or back slashes</param>
    /// <returns></returns>
    public static bool EnsureFolder(string path)
    {
        bool created = false;
        path = NormalizePath(path);
        if (path.Contains("\\"))
        {
            if (!path.EndsWith("\\"))
            {
                path = path.Substring(0, path.LastIndexOf("\\") + 1);
            }
        }
        string current = "";
        string fullpath = path;
        do
        {
            current += fullpath.Substring(current.Length, path.IndexOf("\\") + 1);
            if (current.Length < 1)
            {
                continue;
            }
            path = fullpath.Substring(current.Length);
            if (current.EndsWith(":\\"))
            {
                var y = IsDrive(current);
                if (y == false) return false;
                continue;
            }
            if (!Directory.Exists(current))
            {
                Directory.CreateDirectory(current);
                created = true;
            }
        } while (path.Length > 0);
        return created;
    }
    /// <summary>
    /// Find or create a file and any supporting folder path.
    /// Returns true if the file was newly created, false if one already existed.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool EnsureFile(string path)
    {
        bool created = false;
        path = NormalizePath(path);
        EnsureFolder(path);
        if (!File.Exists(path))
        {
            using (var f = File.Create(path))
            {
                created = true;
            } // the file needs to get closed!
        }
        return created;
    }
    /// <summary>
    /// Return true if a drive exists.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsDrive(string driveName)
    {
        foreach (var drive in System.IO.DriveInfo.GetDrives())
        {
            if (drive.Name.ToLower().StartsWith(driveName.ToLower()))
                return true;
        }
        return false;
    }
    /// <summary>
    /// Return a dos-style path for a linux-style path (forward to back slashes if needed) 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";
        return path.Replace("/", "\\");
    }
    /// <summary>
    /// Check if a folder exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFolder(string path)
    {
        return Directory.Exists(NormalizePath(path));
    }
    /// <summary>
    /// Return the simple file names with extension of files within a path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetFileNames(string path)
    {
        path = NormalizePath(path);
        try
        {
            return Directory.GetFiles(path).Select(fn => Path.GetFileName(fn));
        }
        catch
        {
            return new string[0];
        }
    }
    /// <summary>
    /// Return the full file paths with extension of files within a path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetFullFileNames(string path)
    {
        path = NormalizePath(path);
        return Directory.GetFiles(path);
    }
    /// <summary>
    /// Returns the full paths of all folders within the supplied path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetFullFolderNames(string path)
    {
        path = NormalizePath(path);
        return Directory.GetDirectories(path);
    }
    /// <summary>
    /// Returns the local paths of all folders within the supplied path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetFolderNames(string path)
    {
        path = NormalizePath(path);
        try
        {
            return Directory.GetDirectories(path).Select(dn => (dn).Substring(dn.LastIndexOf('\\') + 1));
        }
        catch
        {
            return new string[0];
        }
    }
    /// <summary>
    /// Reads a file line by line, passing each, along with the index, to an action. 
    /// Returns the number of lines read. 
    /// The action could be used to filter or modify lines and pass to another file stream. 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="action">A method taking a string for the line and a long for the line number.</param>
    /// <returns></returns>
    public static long ReadFileLines(string path, Action<string, long> action)
    {
        long index = 0;
        path = NormalizePath(path);
        using (var reader = new StreamReader(path))
        {
            string line;
            do
            {
                line = reader.ReadLine();
                if (line != null)
                {
                    action.Invoke(line, index);
                    index++;
                }
            } while (line != null);
        }
        return index;
    }
    /// <summary>
    /// Exchnges the names of two files, optionally swapping the file dates too.
    /// </saummary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <param name="withFileDates"></param>
    /// <returns></returns>
    /// 

    public static bool SwapFiles(string pathA, string pathB, bool withFileDates = false)
    {
        var fileInfoA = new FileInfo(NormalizePath(pathA));
        var fileInfoB = new FileInfo(NormalizePath(pathB));
        string tag = $"_ren_{DateTime.UtcNow}".Replace("/", "-").Replace(" ", "_").Replace(":", "-");
        string fullNameA = Path.Join(fileInfoA.DirectoryName, fileInfoA.Name);
        string fullNameB = Path.Join(fileInfoB.DirectoryName, fileInfoB.Name);
        try
        {
            // keep the dates because they will be overwrtten by the move process.
            DateTime modA = fileInfoA.LastWriteTimeUtc;
            DateTime modB = fileInfoB.LastWriteTimeUtc;
            DateTime createA = fileInfoA.CreationTimeUtc;
            DateTime createB = fileInfoB.CreationTimeUtc;
            fileInfoA.MoveTo(NormalizePath(fullNameB) + tag);
            fileInfoB.MoveTo(NormalizePath(fullNameA));
            fileInfoA.MoveTo(fullNameB);
            if (withFileDates)
            {
                fileInfoA.CreationTimeUtc = createB;
                fileInfoB.CreationTimeUtc = createA;
                fileInfoA.LastWriteTimeUtc = modB;
                fileInfoB.LastWriteTimeUtc = modA;
            }
            else
            {
                fileInfoA.CreationTimeUtc = createA;
                fileInfoB.CreationTimeUtc = createB;
                fileInfoA.LastWriteTimeUtc = modA;
                fileInfoB.LastWriteTimeUtc = modB;
            }
            return true;
        }
        catch  
        {
            if (fileInfoA != null && fileInfoA.Exists)
                fileInfoA.MoveTo(fullNameA);
            if (fileInfoB != null && fileInfoB.Exists)
                fileInfoB.MoveTo(fullNameB);
            return false;
        }
    }
    /// <summary>
    /// Get Utc Created, Modified and Accessed date-times.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static (DateTime Created, DateTime Modified, DateTime Accessed) GetFileDatesUtc(string path)
    {
        var fileInfo = new FileInfo(NormalizePath(path));
        return (Created: fileInfo.CreationTimeUtc, Modified: fileInfo.LastWriteTimeUtc, Accessed: fileInfo.LastAccessTimeUtc);
    }
    /// <summary>
    /// Set the dates created and/or modified on a file.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="createdDate"></param>
    /// <param name="modifedDate"></param>
    /// <returns></returns>
    public static bool SetFileDatesUtc(string path, DateTime? createdDate = null, DateTime? modifedDate = null)
    {
        path = NormalizePath(path);
        var fileInfo = new FileInfo(path);
        try
        {
            if (createdDate.HasValue)
            {
                fileInfo.CreationTimeUtc = createdDate.Value;
            }
            if (modifedDate.HasValue)
            {
                fileInfo.LastWriteTimeUtc = modifedDate.Value;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Get the length in bytes of a file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static long GetFileSize(string path)
    {
        path = NormalizePath(path);
        var fileInfo = new FileInfo(path);
        return fileInfo.Length;
    }
    /// <summary>
    /// If the path ends with a slash or backslash, this is take as a folder name.
    /// Returns whether a folder is empty, or a file has 0 bytes, or either does not exist.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(string path)
    {
        path = NormalizePath(path);
        if (path.EndsWith("\\"))
        {
            var x = Directory.GetFiles(path);
            return x.Length == 0;
        }
        // otherwise treat as a file
        var info = new FileInfo(path);
        return !info.Exists || info.Length == 0;
    }
    /// <summary>
    /// Examines the current folder and deletes if empty. This will delete subfolders if enmpty too.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool DropEmptyFolders(string path)
    {
        bool deletedAny = false;
        try
        {
            path = NormalizePath(path);
            var test = CountContent(path);
            if (test.Files != 0)
                return false;
            var folder = new DirectoryInfo(path);
            // this call will delete files and folders if they exist, hence the earlier check.
            folder.Delete(recursive: true);
            deletedAny = true;
        }
        catch
        {
            return false;
        }
        return deletedAny;
    }

    public static (int Files, int Folders, long Size) CountContent(string path) => CountContents(path, 0, 0, 0);
    private static (int Files, int Folders, long Size) CountContents(string path, int fileCount = 0, int folderCount = 0, long size = 0)
    {

        DirectoryInfo root = new DirectoryInfo(path);
        if (!root.Exists)
            return (0, 0, 0);
        folderCount += root.GetDirectories().Length;
        fileCount += root.GetFiles().Length;
        size += root.GetFiles().Sum(f => f.Length);
        foreach (var dir in root.GetDirectories())
        {
            var sub = CountContents(dir.FullName, fileCount, folderCount, size);
            fileCount += sub.Files;
            size += sub.Size;
        }
        return (fileCount, folderCount, size);
    }
    /// <summary>
    /// Touch a zero-byte temporary file in a folder under the user's temnporary folder. 
    /// Returns the full path to the file. If no subfolder is provided, will be in the temp foplder directly.
    /// The subfolder path may include start and end slashes or not.
    /// </summary>
    /// <returns></returns>
    /// 
    public static string TouchTemporaryFile(string subFolder = "")
    {
        subFolder = NormalizePath(subFolder);
        string tempFile = Path.GetTempFileName();

        if (!string.IsNullOrEmpty(subFolder))
        {
            string path = $"{Path.GetTempPath()}\\{subFolder}\\{Path.GetFileName(tempFile)}".Replace("\\\\", "\\");
            EnsureFolder(path);
            FileInfo fileInfo = new FileInfo(tempFile);
            fileInfo.MoveTo(path);
            tempFile = fileInfo.FullName;
        }

        return tempFile;
    }
}
