using System;
using System.IO;
using Newtonsoft.Json;
using Ryushin.Encryption;
using Ryushin.Utils;
using UnityEngine;

namespace Ryushin.Core {
    /// <summary>
    /// FileManager - Files will be saved in app persistentDataPath
    /// </summary>
    public class FileManager {
        public static bool Load<T>(string folder, string fileName, string encryptPass, out T result) {
            try {
                if (Load(folder, fileName, encryptPass, out var content)) {
                    result = JsonConvert.DeserializeObject<T>(content);
                    return true;
                }

                result = default;
                return false;
            }
            catch (Exception e) {
                ALog.LogError(e.Message);
                throw;
            }
        }

        public static void Save<T>(string folder, string fileName, string encryptPass, T obj,
            bool createFolderIfNeed = true) {
            try {
                var content = JsonConvert.SerializeObject(obj);
                ALog.Log("SaveObject: " + content);
                Save(folder, fileName, encryptPass, content, createFolderIfNeed);
            }
            catch (Exception e) {
                ALog.Log(e.Message);
                throw;
            }
        }

        public static void Save(string folder, string fileName, string encryptPass, string content, bool createFolderIfNeed = true) {
            if (createFolderIfNeed) CreateFolderIfNeeded(folder);
            var fullPath = Application.persistentDataPath + "/" + folder + "/" + fileName;

            try {
                // Decrypt data
                string encrypted = RijndaelEncryption.Encrypt(content, encryptPass);
                using var writer = new StreamWriter(fullPath);
                writer.Write(encrypted);
                ALog.Log(fileName + " saved at " + fullPath);
            }
            catch (Exception e) {
                ALog.Log(e.Message);
            }
        }

        public static bool Load(string folder, string fileName, string encryptPass, out string content) {
            CreateFolderIfNeeded(folder);
            var fullPath = Application.persistentDataPath + "/" + folder + "/" + fileName;
            if (!CheckExists(fullPath)) {
                content = null;
                ALog.Log(fullPath + " not existed!");
                return false;
            }

            using var reader = new StreamReader(fullPath);
            var dataToLoad = reader.ReadToEnd();

            // Decrypt
            try {
                content = RijndaelEncryption.Decrypt(dataToLoad, encryptPass);
                ALog.Log($"Load file {fileName} content: " + content);
                return true;
            }
            catch (Exception e) {
                ALog.Log(e.Message);
                content = null;
                return false;
            }
        }

        public static bool CheckExists(string filename) {
            return File.Exists(filename);
        }

        public static void CreateFolderIfNeeded(string folder) {
            var fullPath = Application.persistentDataPath + "/" + folder;
            if (Directory.Exists(fullPath)) {
                return;
            }

            try {
                // Determine whether the directory exists.
                if (Directory.Exists(fullPath)) {
                    Console.WriteLine("That path exists already.");
                    return;
                }

                // Try to create the directory.
                Directory.CreateDirectory(fullPath);
                ALog.Log("The directory was created successfully at {0}." + Directory.GetCreationTime(fullPath));
            }
            catch (Exception e) {
                ALog.Log("The process failed: {0} " + e);
            }
        }
    }
}