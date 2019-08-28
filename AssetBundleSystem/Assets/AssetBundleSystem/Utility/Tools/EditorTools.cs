using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CommonUtils
{

	public static class EditorTools
	{
        /// <summary>
        /// t: findtype *.xxxx:fileextension f:filename
        /// </summary>
        /// <param name="selectparams"></param>
        /// <param name="targetpaths"></param>
        /// <returns></returns>
	    public static HashSet<string> FindAllAsset(string selectparams, params string[] targetpaths)
	    {
	        HashSet<string> result = new HashSet<string>();
            if (targetpaths.Length >0)
            {
#if UNITY_EDITOR
                foreach (var eachpath in targetpaths)
                {
                    var matches = Regex.Matches(selectparams,"t:[a-zA-Z0-9_]+");
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var guids = AssetDatabase.FindAssets(match.Value, new string[] {eachpath});
                            foreach (var eachguid in guids)
                            {
                                result.Add(AssetDatabase.GUIDToAssetPath(eachguid));
                            }
                        }
                    }

                    var fematches = Regex.Matches(selectparams,"\\*\\.+[^. ]+");
                    if (fematches.Count > 0)
                    {
                        foreach (Match match in fematches)
                        {
                            var files = GetAllFilesInSelectDir(eachpath, match.Value);
                            foreach (var file in files)
                            {
                                string fullname = file.FullName;
                                fullname = GetRightFormatPath(fullname);
                                fullname = GetUnityAssetPath(fullname);
                                result.Add(fullname);
                            }
                        }
                    }

                    var fnmatches = Regex.Matches(selectparams,"\\*\\.+[^. ]+");
                    if (fnmatches.Count > 0)
                    {
                        foreach (Match match in fnmatches)
                        {
                            var files = GetAllFilesInSelectDir(eachpath, match.Value);
                            foreach (var file in files)
                            {
                                string fullname = file.FullName;
                                fullname = GetRightFormatPath(fullname);
                                fullname = GetUnityAssetPath(fullname);
                                result.Add(fullname);
                            }
                        }
                    }
                }
#endif
            }
            return result;
	    }

        public static void RunProcess(string filename, string arguments, int timeout)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    Debug.Log("Start Run " + filename + " arguments =" + arguments);

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout) && process.ExitCode == 0)
                    {
                        Debug.Log("Finished ExitCode " + process.ExitCode + " Run " + filename + " arguments =" + arguments);
                    }
                    else if (process.ExitCode != 0)
                    {
                        throw new Exception("Run " + filename + " arguments= " + arguments + " Exit Error :" + process.ExitCode);
                    }
                    else
                    {
                        throw new Exception("Run " + filename + " arguments= " + arguments + " Time Out");
                    }
                }
            }
        }


        public static string GetRelativeAssetPath(string fullPath)
		{
		    fullPath = GetRightFormatPath(fullPath);
			int idx = fullPath.IndexOf("Assets");
            if (idx < 0)
            {
                return fullPath;
            }
			string assetRelativePath = fullPath.Substring(idx);
			return assetRelativePath;
		}

		public static string GetRightFormatPath(string path)
		{
			return path.Replace("\\", "/");
		}

        public static DirectoryInfo[] GetAllDirectories(string path)
        {
            string fullpath = Path.GetFullPath(path);
            DirectoryInfo dir = null;
            if (File.Exists(fullpath))
            {
                dir = new DirectoryInfo(Path.GetDirectoryName(fullpath));
            }
            else
            {
                dir = new DirectoryInfo(fullpath);
            }


            return dir.GetDirectories("*", SearchOption.AllDirectories);
        }

        public static FileInfo[] GetAllFilesInSelectDir(string path, string pattern)
		{
			string fullpath = Path.GetFullPath(path);
			DirectoryInfo dir = null;
			if (File.Exists(fullpath))
			{
				dir = new DirectoryInfo(Path.GetDirectoryName(fullpath));
			}
			else
			{
				dir = new DirectoryInfo(fullpath);
			}


			return dir.GetFiles(pattern, SearchOption.AllDirectories);
		}


		public static string GetUnityAssetPath(string path)
		{
			int index = path.IndexOf("Assets/");
			if (index != -1)
				path = path.Substring(index);

			index = path.IndexOf("Assets\\");
			if (index == -1)
				return path;
			return path.Substring(index);
		}

		public static void DirectoryCopy(string sourceDirName, string destDirName, string fileApend, string ignorename, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
				    "Source directory does not exist or could not be found: "
				    + sourceDirName);
			}

			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				if (temppath.EndsWith(ignorename))
				{
					continue;
				}
				file.CopyTo(temppath + fileApend, true);
			}
			if (copySubDirs)
			{

				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);

					DirectoryCopy(subdir.FullName, temppath, fileApend, ignorename, copySubDirs);
				}
			}
		}

		public static void DirectoryDelete(string sourceDirName)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
				    "Source directory does not exist or could not be found: "
				    + sourceDirName);
			}
			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files)
			{
				file.Delete();
			}

			foreach (DirectoryInfo subdir in dirs)
			{
				DirectoryDelete(subdir.FullName);
			}
		}

		public static void DirectoryDeleteFull(string sourceDirName)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
				    "Source directory does not exist or could not be found: "
				    + sourceDirName);
			}

			FileInfo[] files = dir.GetFiles();

			for (int i = 0; i < files.Length; ++i)
			{
				files[i].Delete();
			}

			for (int i = 0; i < dirs.Length; ++i)
			{
				dirs[i].Delete(true);
			}

		}

		public static void DirectoryCopyEndWith(string sourceDirName, string destDirName, string endwith, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
				    "Source directory does not exist or could not be found: "
				    + sourceDirName);
			}

			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}
			FileInfo[] files = dir.GetFiles();
			for (int i = 0; i < files.Length; ++i)
			{
				FileInfo file = files[i];
				string temppath = Path.Combine(destDirName, file.Name);
				if (!string.IsNullOrEmpty(endwith) && temppath.EndsWith(endwith, StringComparison.Ordinal))
				{
					file.CopyTo(temppath, true);
				}
				else if (string.IsNullOrEmpty(endwith))
				{
					file.CopyTo(temppath, true);
				}
			}

			if (copySubDirs)
			{
				for (int i = 0; i < dirs.Length; ++i)
				{
					DirectoryInfo subdir = dirs[i];
					string temppath = Path.Combine(destDirName, subdir.Name);

					DirectoryCopyEndWith(subdir.FullName, temppath, endwith, copySubDirs);
				}
			}
		}

		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
				    "Source directory does not exist or could not be found: "
				    + sourceDirName);
			}

			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}
			FileInfo[] files = dir.GetFiles();

			for (int i = 0; i < files.Length; ++i)
			{
				FileInfo file = files[i];
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, true);
			}

			if (copySubDirs)
			{
				for (int i = 0; i < dirs.Length; ++i)
				{
					DirectoryInfo subdir = dirs[i];
					string temppath = Path.Combine(destDirName, subdir.Name);

					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		public static bool FindContainsFile(string dir, string fileName)
		{
			if (dir == null || dir.Trim() == "" || fileName == null || fileName.Trim() == "" || !Directory.Exists(dir))
			{
				return false;
			}

			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			return FindFile(dirInfo, fileName);

		}

		public static bool FindFile(DirectoryInfo dir, string fileName)
		{
			var dirs = dir.GetDirectories();
			for (int i = 0; i < dirs.Length; ++i)
			{
				DirectoryInfo subdir = dirs[i];
				var files = subdir.GetFiles();
				for (int j = 0; j < files.Length; ++j)
				{
					FileInfo file = files[j];
					if (file.FullName.EndsWith(fileName))
					{
						return true;
					}
				}

				FindFile(subdir, fileName);
			}

			return false;
		}

	}

}


