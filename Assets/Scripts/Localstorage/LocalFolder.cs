using System.IO;
using UnityEngine;

/// <summary>
/// Provides methods for managing local folders and files related to routes.
/// </summary>
public static class LocalFolder
{
    /// <summary>
    /// Provides methods for managing folders and files for ERW routes.
    /// </summary>
    public class ERWFolder
    {
        private Route CurrentRoute;
        public ERWFolder(Route route)
        {
            CurrentRoute = route;
        }

        /// <summary>
        /// Creates or resets the folder structure for the specified route.
        /// </summary>
        /// <param name="route">The route for which to create or reset the folder structure.</param>
        public static ERWFolder CreateOrResetFolder(Route route)
        {

            var folder = new ERWFolder(route);

            if (Directory.Exists(folder.GetFolderPath()))
            {
                Directory.Delete(folder.GetFolderPath(), true);
            }
            Directory.CreateDirectory(folder.GetFolderPath());
            Directory.CreateDirectory(folder.GetPictureFolderPath());
            Directory.CreateDirectory(folder.GetVideoFolderPath());            

            return folder;

        }

        /// <summary>
        /// Deletes the folder for the current route.
        /// </summary>
        public void DeleteFolder()
        {
            if (Directory.Exists(GetFolderPath()))
            {
                Directory.Delete(GetFolderPath(), true);
            }
        }

        /// <summary>
        /// Gets the folder name for the current route.
        /// </summary>
        /// <returns>The folder name for the current route.</returns>
        public string GetFolderName()
        {
            return CurrentRoute.Name;
        }

        /// <summary>
        /// Gets the folder path for the current route.
        /// </summary>
        /// <returns>The folder path for the current route.</returns>
        public string GetFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, GetFolderName());
        }

        /// <summary>
        /// Gets the picture folder path for the current route.
        /// </summary>
        /// <returns>The picture folder path for the current route.</returns>
        public string GetPictureFolderPath()
        {
            return Path.Combine(GetFolderPath(), "Fotos");
        }

        /// <summary>
        /// Gets the picture path for the current route and filename.
        /// </summary>
        /// <param name="filename">The filename of the picture.</param>
        /// <returns>The picture path for the current route and filename.</returns>
        public string GetPicturePath(string filename)
        {
            string ImgDir = GetPictureFolderPath();
            return Path.Combine(ImgDir, filename);
        }

        /// <summary>
        /// Deletes the picture for the current route and filename.
        /// </summary>
        /// <param name="filename">The filename of the picture to delete.</param>
        public void DeletePicture(string filename)
        {
            string fullPath = GetPicturePath(filename);
            File.Delete(fullPath);
        }

        /// <summary>
        /// Gets the video folder path for the current route.
        /// </summary>
        /// <returns>The video folder path for the current route.</returns>
        public string GetVideoFolderPath()
        {
            return Path.Combine(GetFolderPath(), "Videos");
        }

        /// <summary>
        /// Gets the video path for the current route and filename.
        /// </summary>
        /// <param name="filename">The filename of the video.</param>
        /// <returns>The video path for the current route and filename.</returns>
        public string GetVideoPath(string filename)
        {
            string VidDir = GetVideoFolderPath();
            return Path.Combine(VidDir, filename);
        }

        /// <summary>
        /// Deletes the video for the current route and filename.
        /// </summary>
        /// <param name="filename">The filename of the video to delete.</param>
        public void DeleteVideo(string filename)
        {
            string fullPath = GetVideoPath(filename);
            File.Delete(fullPath);
        }

    }
}


