/*
    MIT License

    Copyright (c) 2021 Marx Jason

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/

#nullable enable

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utilities
{
    public static class FileHelper
    {
        private static Regex _assetPath = new Regex(@".*/(Assets/.*/)[^/]+", RegexOptions.Compiled);


        #region Runtime functions

        /// <summary>
        /// Get random File name
        /// </summary>
        /// <param name="extension">The extension to apply to the file</param>
        /// <returns>A random File name</returns>
        public static string GetFileName(string extension)
        {
            return Path.ChangeExtension(Path.GetRandomFileName(), extension);
        }

        /// <summary>
        /// Copy File from source to target path
        /// </summary>
        /// <param name="source">The file to copy</param>
        /// <param name="target">The target path</param>
        public static void CopyFileTo(string source, string target)
        {
            if (File.Exists(target))
                File.Delete(target);

            if (!File.Exists(source))
                throw new FileNotFoundException($"[{source}] was not found");

            File.Copy(source, target);
        }

        /// <summary>
        /// Create a new File if the file doesn't yet exist.
        /// </summary>
        /// <param name="path">The path to the File</param>
        public static void CreateFile(string path)
        {
            if(!File.Exists(path))
                File.Create(path);
        }

        public static void AppendToFile(string path, params string[] lines)
        {
            CreateFile(path);
            File.WriteAllLines(path, lines);    
        }

        /// <summary>
        /// Calculate the hash of one or more file(s).
        /// </summary>
        /// <param name="filename">The files</param>
        /// <returns>The hash of the files</returns>
        public static string CalculateHASH(params FileInfo[] filename)
        {
            using (var md5 = MD5.Create())
            {
                List<Byte> hash = new List<Byte>();
                foreach (FileInfo file in filename)
                {
                    using (var stream = File.OpenRead(file.FullName))
                    {
                        hash.AddRange(md5.ComputeHash(stream));
                    }
                }

                var result = BitConverter.ToString(md5.ComputeHash(hash.ToArray())).Replace("-", "").ToLowerInvariant();
                return result;
            }
        }

        /// <summary>
        /// Get directory from two parameters
        /// </summary>
        /// <param name="root">The root directory</param>
        /// <param name="dir">The directory to get</param>
        /// <returns>The directory</returns>
        public static DirectoryInfo GetDirectory(DirectoryInfo root, string dir)
        {
            return GetDirectory(root.FullName, dir);
        }

        /// <summary>
        /// Get directory from two parameters
        /// </summary>
        /// <param name="root">The root directory</param>
        /// <param name="dir">The directory to get</param>
        /// <returns>The directory</returns>
        public static DirectoryInfo GetDirectory(string root, string dir)
        {
            return new DirectoryInfo(GetFile(root, dir).FullName);
        }

        /// <summary>
        /// Get file from two parameters
        /// </summary>
        /// <param name="root">The root directory</param>
        /// <param name="file">The file to return</param>
        /// <returns>The file</returns>
        public static FileInfo GetFile(DirectoryInfo root, string file)
        {
            return GetFile(root.FullName, file);
        }

        /// <summary>
        /// Get file from two parameters
        /// </summary>
        /// <param name="root">The root directory</param>
        /// <param name="file">The file to return</param>
        /// <returns>The file</returns>
        public static FileInfo GetFile(string root, string file)
        {
            if (file == null) return new FileInfo(root);
            return new FileInfo(Path.Combine(root, file));
        }


        /// <summary>
        /// Gets all files in given directory
        /// </summary>
        /// <param name="dir">The directory to get the files from.</param>
        /// <param name="extensions">The extension of files to return</param>
        /// <returns>The files in the given folder, empty array if directory doesn't exist</returns>
        public static FileInfo[] GetFiles(DirectoryInfo dir, params string[] extensions)
        {
            if (dir == null || !dir.Exists)
                return new FileInfo[] { };

            FileInfo[] files = dir.GetFiles();

            if (extensions.Count() != 0)
                files = files.Where(file => extensions.Contains(file.Extension.ToLower())).ToArray();

            return files;
        }


        private static string GetNameWithoutGenericArity(this Type t)
        {
            string name = t.Name;
            int index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            return $"{Application.dataPath.Replace("/Assets", "")}/{relativePath}";
        }

        public static DirectoryInfo? GetParentDirectory(string absolutePath)
        {
            if (!File.Exists(absolutePath))
                return null;

            return new FileInfo(absolutePath).Directory;
        }

        #endregion

        #region Editor Only Functions
        #if UNITY_EDITOR

        /// <summary>
        /// Get relative path from absolute given path.
        /// </summary>
        /// <param name="path">The absolute path.</param>
        /// <returns>The relative path</returns>
        public static string? GetAssetPath(string path)
        {
            path = path.Replace('\\', '/');

            var assetMatch = _assetPath.Match(path);
            if (assetMatch.Success)
                return assetMatch.Groups[1].Value;

            return null;
        }

        public static void PrintAllScripts()
        {
            var g = AssetDatabase.FindAssets("t:Script");
            string r = "Script: ";
            foreach (var item in g)
            {
                r += $"\n{AssetDatabase.GUIDToAssetPath(item)}";
            }
            Debug.Log(r);

        }

        public static string GetAssetFilePathByType(Type type)
        {
            var g = AssetDatabase.FindAssets("t:Script " + GetNameWithoutGenericArity(type));
            foreach (var asset in g)
            {
                var result = AssetDatabase.GUIDToAssetPath(asset);
                if (result.EndsWith(GetNameWithoutGenericArity(type) + ".cs"))
                    return result;
            }

            return "";
        }

        public static string GetAssetDirectoryPathByType(Type type)
        {
            var g = GetAssetFilePathByType(type);
            return g.Substring(0, g.LastIndexOf("/"));
        }


        #endif
        #endregion
    }
}
