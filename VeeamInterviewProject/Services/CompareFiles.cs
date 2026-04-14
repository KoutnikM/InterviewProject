using NLog.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VeeamInterviewProject.Abstractions;

namespace VeeamInterviewProject.Services
{
    internal class CompareFiles : ICompare
    {
        private readonly IAppLogger logger;

        public CompareFiles(IAppLogger logger)
        {
            this.logger = logger;
        }
        /// <summary>
        /// Handles comparison of source file and target file, 1st compares size, then time and at end uses MD5
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool AreFilesDifferent(string source, string target)
        {
            var sourceFile = new FileInfo(source);
            var targetFile = new FileInfo(target);

            if(!IsSameSize(sourceFile, targetFile))
            {
                return true; 
            }
            if (!IsSameTime(sourceFile, targetFile))
            {
                return true;
            }

            return CompareMD5(source, target);
        }
        /// <summary>
        /// Computes MD5 hashes of two files and compares them
        /// </summary>
        /// <param name="fileSource"></param>
        /// <param name="fileTarget"></param>
        /// <returns></returns>
        private bool CompareMD5(string source, string target)
        {
            logger.InfoMessage($"Comparing hash source: {source} and target: {target}");

            using (var md5 = MD5.Create())
            {
                using (var stream1 = File.OpenRead(source))
                using (var stream2 = File.OpenRead(target))
                {
                    var hash1 = md5.ComputeHash(stream1);
                    var hash2 = md5.ComputeHash(stream2);

                    return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2); 
                }
            }
        }
        /// <summary>
        /// Compare DateTime of files
        /// </summary>
        /// <param name="timeSource"></param>
        /// <param name="timeTarget"></param>
        /// <returns></returns>
        private bool IsSameTime(FileInfo source, FileInfo target)
        {
            logger.InfoMessage($"Comparing timestap of source: {source.Name} and target: {target.Name}");
            return source.LastWriteTime == target.LastWriteTime;
        }
        /// <summary>
        /// Compares size of files
        /// </summary>
        /// <param name="sizeSource"></param>
        /// <param name="sizeTarget"></param>
        /// <returns></returns>
        private bool IsSameSize(FileInfo source, FileInfo target)
        {
            logger.InfoMessage($"Comparing size of source: {source.Name} and target: {target.Name}");
            return source.Length == target.Length;
        }
    }
}
