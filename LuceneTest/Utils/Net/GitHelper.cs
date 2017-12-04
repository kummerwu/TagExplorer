using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer.Utils.Net
{
    class GitHelper
    {
        string baseDir;
        string gitLink;
        string GitUser, GitEMail, GitLoginUser, GitLoginPass;
        Repository repo;
        public GitHelper()
        {
            GitUser = StaticCfg.Ins.GitUser;
            GitEMail = StaticCfg.Ins.GitEMail;
            GitLoginUser = StaticCfg.Ins.GitLoginUser;
            GitLoginPass = StaticCfg.Ins.GitLoginPassword;
            gitLink = StaticCfg.Ins.GitLink;
            baseDir = CfgPath.DocBasePath;
        }
        public void Connect(string git,string dir)
        {
            gitLink = git;
            baseDir = dir;
            //Repository repo = new Repository(baseDir);
            
        }
        static Credentials CreateUsernamePasswordCredentials(string user, string pass, bool secure)
        {
            //if (secure)
            //{
            //    return new SecureUsernamePasswordCredentials
            //    {
            //        Username = user,
            //        Password = Constants.StringToSecureString(pass),
            //    };
            //}

            return new UsernamePasswordCredentials
            {
                Username = user,
                Password = pass,
            };
        }
        public void Clone()
        {
            if (!Repository.IsValid(baseDir))
            {
                Repository.Clone(gitLink, baseDir, new CloneOptions()
                {
                    CredentialsProvider = (_url, _user, _cred) => CreateUsernamePasswordCredentials(GitLoginUser, GitLoginPass, false)
                });
            }
        }

        public void Push()
        {
            try
            {
                Identity id = new Identity(GitUser, GitEMail);
                Signature sig = new Signature(id, DateTimeOffset.Now);
                repo = new Repository(baseDir);
                Commands.Stage(repo, "*");
                repo.Commit("AUTO_PUSH:" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString(), sig, sig);
                repo.Network.Push(repo.Head, new PushOptions()
                {
                    CredentialsProvider = (_url, _user, _cred) => CreateUsernamePasswordCredentials(GitLoginUser, GitLoginPass, false)
                });
            }catch(Exception e)
            {
                Logger.E(e);
            }
        }
        public void Pull()
        {
            if(Repository.IsValid(baseDir))
            {
                PullIn();
            }
            else
            {
                Clone();
            }
        }
        public void PullIn()
        {
            try
            {
                Identity id = new Identity(GitUser, GitEMail);
                Signature sig = new Signature(id, DateTimeOffset.Now);
                repo = new Repository(baseDir);
                PullOptions options = new PullOptions
                {
                    FetchOptions = new FetchOptions()
                };
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = GitLoginUser,
                            Password = GitLoginPass
                        });
                Commands.Pull(repo, sig, options);
                //repo.Network.Pull(new LibGit2Sharp.Signature("kummerwu", "kummerwu@foxmail.com", new DateTimeOffset(DateTime.Now)), options);
            }catch(Exception e)
            {
                Logger.E(e);
            }
        }
        public void StageChanges()
        {
            try
            {
                RepositoryStatus status = repo.RetrieveStatus();
                List<string> filePaths = status.Modified.Select(mods => mods.FilePath).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:RepoActions:StageChanges " + ex.Message);
            }
        }
    }
}
