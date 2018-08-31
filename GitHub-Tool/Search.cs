﻿using System;
using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Octokit.Internal;

namespace GitHub_Tool
{
    class Search
    {
        //.ConfigureAwait(false)



        public async Task<List<FileInformation>> SearchCode(String term, int minNumberOfCommits, String extension, String name = null) 
        {

            var client = MainWindow.createGithubClient();


            List<FileInformation> files = new List<FileInformation>();


            var codeRequest = new SearchCodeRequest(term);   //var client = MainWindow.createGithubClient()

            if ( name != null)       //User = "NikosSyris"
            {
                codeRequest.User = name;
            }

            if (extension != null)   //Extension = "cs",
            {
                codeRequest.Extension = extension;
            }

            

            //if (size != null)
            //{
            //    codeRequest.Size = Range.GreaterThanOrEquals(size.Value);
            //}

            //codeRequest.Size = Range.GreaterThan(500);
            //NikosSyris/GitHub-Tool

            var result = await client.Search.SearchCode(codeRequest).ConfigureAwait(false);


            var numberOfResults = result.TotalCount;
            codeRequest.Page = 0;

            while (true)
            {
                codeRequest.Page += 1;

                for (var j = 0; j < 100; j++)       // na valw .perPage instead of 100
                {
                    var temp = result.Items.ElementAt(j);
                    var filePath = temp.Path;
                    //var sizeTemp = temp.Repository.Size;  
                    var repo = temp.Repository.Name;
                    var owner = temp.Repository.Owner.Login;

                    var commitRequest = new CommitRequest { Path = filePath };

                    var commitsForFile = await client.Repository.Commit.GetAll(owner, repo, commitRequest).ConfigureAwait(false); 
                    
                    if (commitsForFile.Count > minNumberOfCommits)    // TODO also put equals
                    {
                        files.Add(new FileInformation(owner, repo, filePath, commitsForFile));
                    }
                    numberOfResults--;

                    if (numberOfResults == 0)
                    {
                        break;
                    }

                }

                if (numberOfResults == 0)
                {
                    break;
                }
            }

            List<FileInformation> Sortedfiles = files.OrderByDescending(x => x.AllCommits.Count).ToList();


            return Sortedfiles;

        }



        public async Task<List<Repository>> SearchRepos(String owner, String name)
        {

            var client = MainWindow.createGithubClient();

            List<Repository> repos = new List<Repository>();



            var repoRequest = new SearchRepositoriesRequest();
            repoRequest.Size = Range.GreaterThan(1);
            repoRequest.User = owner;

            var result1 = await client.Search.SearchRepo(repoRequest);

            foreach (var rep in result1.Items)
            {
                repos.Add(new Repository(rep.Name, rep.Owner.Login, rep.Size));
            }



            return repos;

        }


        public async Task<Repository> getRepoStructure(Repository repository)
        {

            var client = MainWindow.createGithubClient();
            
            var result = await client.Repository.Content.GetAllContents(repository.Owner, repository.Name).ConfigureAwait(false);

            Folder rootFolder = new Folder("root");


            //rootFolder = await getFolderContent(owner, repo, rootFolder).ConfigureAwait(false);

            foreach (var item in result)
            {
                if (item.Type == "dir")
                {
                    Folder newFolder = new Folder(item.Name);
                    rootFolder.FolderList.Add(newFolder);
                    newFolder = await getFolderContent(repository, newFolder).ConfigureAwait(false);
                }
                else if (item.Type == "file")
                {
                    rootFolder.FileList.Add(new File(item.Name, item.Path, repository.Owner, repository.Name));
                    repository.RepositoryContentList.Add(new File(item.Name, item.Path, repository.Owner, repository.Name));
                }
            }

            repository.RootFolder = rootFolder;


            return repository;
        }



        public async Task<Folder> getFolderContent(Repository repository, Folder folder)
        {

            var client = MainWindow.createGithubClient();
            var result = await client.Repository.Content.GetAllContents(repository.Owner, repository.Name, folder.Name).ConfigureAwait(false);
            
            foreach (var item in result)
            {
                if (item.Type == "file")
                {
                    folder.FileList.Add(new File(item.Name, item.Path, repository.Owner, repository.Name));
                    repository.RepositoryContentList.Add(new File(item.Name, item.Path, repository.Owner, repository.Name));
                }
                else
                {
                    var newFolderName = folder.Name + "/" + item.Name;
                    Folder newFolder = new Folder(newFolderName);
                    folder.FolderList.Add(newFolder);
                    newFolder = await getFolderContent(repository, newFolder).ConfigureAwait(false);
                }
            }

            return folder;
        }




        public async Task<IReadOnlyList<GitHubCommit>> getCommitsForFIle(string owner, string repo, string path)
        {

            var client = MainWindow.createGithubClient();
            var commitRequest = new CommitRequest { Path = path };

            var commitsForFile = await client.Repository.Commit.GetAll(owner, repo, commitRequest).ConfigureAwait(false);

            return commitsForFile;



        }



    }
}
