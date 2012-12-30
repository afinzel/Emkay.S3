﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace Emkay.S3
{
    public class PublishFiles : PublishBase
    {
        public PublishFiles(string key, string secret, int timeoutMilliseconds = 300000, bool publicRead = true, ITaskLogger logger = null)
            : base(key, secret, timeoutMilliseconds, publicRead, logger)
        {
        }

        public PublishFiles(IS3Client client, int timeoutMilliseconds = 300000, bool publicRead = true, ITaskLogger logger = null)
            : base(client, timeoutMilliseconds, publicRead, logger)
        {
        }

        [Required]
        public string[] SourceFiles { get; set; }

        [Required]
        public string DestinationFolder { get; set; }

        public override bool Execute()
        {
            Logger.LogMessage(MessageImportance.Normal,
                              string.Format("Publishing {0} files", SourceFiles.Length));

            Logger.LogMessage(MessageImportance.Normal,
                              string.Format("to S3 bucket {0}", Bucket));

            if (!string.IsNullOrEmpty(DestinationFolder))
                Logger.LogMessage(MessageImportance.Normal,
                                  string.Format("destination folder {0}", DestinationFolder));

            try
            {
                Client.EnsureBucketExists(Bucket);
                Publish(Client, SourceFiles, Bucket, DestinationFolder, PublicRead, TimeoutMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(MessageImportance.High,
                                  string.Format("Publishing folder has failed because of {0}", ex.Message));
                return false;
            }
        }

        private static void Publish(IS3Client client,
                                    IEnumerable<string> sourceFiles,
                                    string bucket,
                                    string destinationFolder,
                                    bool publicRead,
                                    int timeoutMilliseconds)
        {
            foreach (var f in sourceFiles)
            {
                var info = new FileInfo(f);
                client.PutFile(bucket, CreateRelativePath(destinationFolder, info.Name), info.FullName, publicRead, timeoutMilliseconds);
            }
        }
    }
}