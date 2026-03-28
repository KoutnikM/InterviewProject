using System;
using System.Collections.Generic;
using System.Text;
using VeeamInterviewProject.Abstractions;

namespace VeeamInterviewProject.Services
{
    internal class SyncFolder : IFolderSynchronisation
    {
        private readonly string source;
        private readonly string target;
        private readonly IAppLogger logger;
        private readonly int interval;
        private readonly ICompare compare;

        /// <summary>
        /// Contructor asigns arguments
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="logger"></param>
        /// <param name="interval"></param>
        /// <param name="compare"></param>
        public SyncFolder(string source, string target, IAppLogger logger, int interval, ICompare compare)
        {
            this.source = source;
            this.target = target;
            this.logger = logger;
            this.interval = interval;
            this.compare = compare;

        }
        /// <summary>
        /// Creates missing directories, call SyncDirectories to copy or update files and removes obsolete files
        /// </summary>
        /// <param name="token">Cancellation token used to stop while cycle</param>
        /// <returns></returns>
        public async Task Sync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    logger.InfoMessage($"Starting synchronization of {source} and {target}");
                    if (!Directory.Exists(source))
                    {
                        logger.ErrorMessage("Source directory does not exist.");
                        throw new Exception("Source directory does not exist.");
                    }
                    if (!Directory.Exists(target))
                    {
                        Directory.CreateDirectory(target);
                    }

                    await SyncDirectories(source, target);
                    Remove(source, target);

                    logger.InfoMessage($"Next synchronization in {interval} seconds");
                    await Task.Delay(TimeSpan.FromSeconds(interval), token);
                }
                catch (TaskCanceledException)
                {
                    logger.InfoMessage("Synchronization stopped");
                    break;
                }
                catch (Exception ex)
                {
                    logger.ErrorMessage($"{ex.Message}");
                    logger.InfoMessage($"Next synchronization in {interval} seconds");
                    await Task.Delay(TimeSpan.FromSeconds(interval), token);
                }
            }

        }
        /// <summary>
        /// Copies and updates files, recursively repeats process inside subdirectories handles access and IO exceoptions
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        /// <returns></returns>
        private async Task SyncDirectories(string sourceDirectory, string targetDirectory)
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
                logger.InfoMessage($"Created directory: {targetDirectory}");
            }

            IEnumerable<string> files;

            try
            {
                files = Directory.EnumerateFiles(sourceDirectory);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.ErrorMessage($"Cannot access directory {sourceDirectory}: {ex.Message}");
                return;
            }

            await Parallel.ForEachAsync(files, async (file, token) =>
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string replicaFile = Path.Combine(targetDirectory, fileName);
                    if (!File.Exists(replicaFile))
                    {
                        if(Copy(file, replicaFile, false))
                        {
                            logger.InfoMessage($"Copied file: {file} -> {replicaFile}");
                        }
                    }
                    else if (compare.Compare(file, replicaFile))
                    {
                        if (Copy(file, replicaFile, true))
                        {
                            logger.InfoMessage($"Updated file: {file} -> {replicaFile}");
                        }                        
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.ErrorMessage($"Cannot access directory {file}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.ErrorMessage($"Error while processing file {file}: {ex.Message}");
                }
            });

            var subDirs = Directory.GetDirectories(sourceDirectory);

            await Parallel.ForEachAsync(subDirs, async (dir, token) =>
            {
                try
                {
                    var dirName = Path.GetFileName(dir);
                    var replicaSubDir = Path.Combine(targetDirectory, dirName);

                    await SyncDirectories(dir, replicaSubDir);
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.ErrorMessage($"Cannot access directory {dir}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.ErrorMessage($"Error processing directory {dir}: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// Removes obsolete files and directories
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        private void Remove(string sourceDirectory, string targetDirectory)
        {
            foreach (var file in Directory.GetFiles(targetDirectory))
            {
                var fileName = Path.GetFileName(file);
                var sourceFile = Path.Combine(sourceDirectory, fileName);

                if (!File.Exists(sourceFile))
                {
                    try
                    {
                        File.Delete(file);
                        logger.InfoMessage($"Deleted file: {file}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.ErrorMessage($"Permission error: {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        logger.ErrorMessage($"IO error while deleting file: {ex.Message}");
                    }
                }
            }

            foreach (var dir in Directory.GetDirectories(targetDirectory))
            {
                string dirName = Path.GetFileName(dir);
                string sourceSubDir = Path.Combine(sourceDirectory, dirName);

                if (!Directory.Exists(sourceSubDir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        logger.InfoMessage($"Deleted directory: {dir}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.ErrorMessage($"Permission error: {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        logger.ErrorMessage($"IO error while deleting directory: {ex.Message}");
                    }
                }
                else
                {
                    Remove(dir, sourceSubDir);
                }
            }
        }
        /// <summary>
        /// Copy file inside try catch block to handle exceptions
        /// </summary>
        /// <param name="file"></param>
        /// <param name="replicaFile"></param>
        /// <param name="overWrites"></param>
        /// <returns></returns>
        private bool Copy(string file, string replicaFile, bool overWrites)
        {
            try
            {
                File.Copy(file, replicaFile, overWrites);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.ErrorMessage($"Permission error: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                logger.ErrorMessage($"IO error while copying {file}: {ex.Message}");
                return false;
            }
        }
    }
}
