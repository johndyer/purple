using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Purple.Core
{
	public class UtilityMethods
	{
		public static void FindFilesRecursive(List<string> filePaths, string searchPattern, string rootPath)
		{
			FindFilesRecursive(filePaths, searchPattern, rootPath, rootPath, false);
		}

		public static void FindFilesRecursive(List<string> filePaths, string searchPattern, string rootPath, bool removeExtension)
		{
			FindFilesRecursive(filePaths, searchPattern, rootPath, rootPath, removeExtension);
		}

		public static void FindFilesRecursive(List<string> filePaths, string searchPattern, string rootPath, string directoryPath, bool removeExtension)
		{
			string[] files = Directory.GetFiles(directoryPath, searchPattern);

			foreach (string filename in files)
			{
				// remove the root path
				string filePath = filename.Replace(rootPath, "");

				// remove the extension?
				if (removeExtension)
					filePath = filePath.Substring(0, filePath.LastIndexOf("."));

				filePaths.Add(filePath);
			}

			string[] subDirectories = Directory.GetDirectories(directoryPath);

			foreach (string directory in subDirectories)
			{
				FindFilesRecursive(filePaths, searchPattern, rootPath, directory, removeExtension);
			}

		}

		public static void FindFoldersRecursive(List<string> folderPaths, string rootPath)
		{
			FindFoldersRecursive(folderPaths, rootPath, rootPath);
		}

		public static void FindFoldersRecursive(List<string> folderPaths, string rootPath, string directoryPath)
		{
			string[] folders = Directory.GetDirectories(directoryPath);

			foreach (string folder in folders)
			{
				// remove the root path
				string folderPath = folder.Replace(rootPath, "");

				// add to list
				folderPaths.Add(folderPath);

				FindFoldersRecursive(folderPaths, rootPath, folder);
			}
		}
	}
}
