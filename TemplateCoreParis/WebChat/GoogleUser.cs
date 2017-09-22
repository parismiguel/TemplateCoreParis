using TemplateCoreParis.Controllers;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TemplateCoreParis.WebChat
{
    public class GoogleUser
    {
        static readonly string[] Scopes = { DirectoryService.Scope.AdminDirectoryUser, DirectoryService.Scope.AdminDirectoryUserSecurity };

        private const string ApplicationName = "VCA ChatBot";
        private const string ClientSecretJsonFile = "TemplateCoreParis_Client_Secret.json";
        private const string GoogleFolder = "Google";


        public static string RunPasswordReset(string userEmailString, string userPassword)
        {
            string msg;

            try
            {
                DirectoryService directoryService = GetGoogleDirectoryService();

                //Email is considered the Primary on Google Accoutns
                var userkey = userEmailString;

                //Set User attributes, in this example the password.
                var userBody = new User
                {
                    Password = userPassword
                };

                //Prepares the update request
                UsersResource.UpdateRequest updateRequest = directoryService.Users.Update(userBody, userkey);

                //Executes the update request
                updateRequest.Execute();

                msg = "Clave modificada!";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return msg;
        }



        public static bool IsEmailRegistered(string userEmailString)
        {

            try
            {
                DirectoryService directoryService = GetGoogleDirectoryService();

                //Email is considered the Primary on Google Accoutns
                var userkey = userEmailString;

                UsersResource.GetRequest getUser = directoryService.Users.Get(userkey);
                User _user = getUser.Execute();

                if (_user == null || _user.Suspended == true)
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
                //Add you exception here
            }

            return true;
        }


        public static MyGoogleUserInfo GetGoogleUserInfo(string userEmailString)
        {

            try
            {
                DirectoryService directoryService = GetGoogleDirectoryService();

                //Email is considered the Primary on Google Accoutns
                var userkey = userEmailString;

                UsersResource.GetRequest getUser = directoryService.Users.Get(userkey);
                User _user = getUser.Execute();

                

                IList<VerificationCode> _verificationCodes = GetVerificationCodes(userkey);

                if (_user != null)
                {
                    MyGoogleUserInfo myuser = new MyGoogleUserInfo()
                    {
                        CustomerId = _user.CustomerId,
                        GivenName = _user.Name.GivenName,
                        FamilyName = _user.Name.FamilyName,
                        ThumbnailPhotoUrl = _user.ThumbnailPhotoUrl,
                        IsEnrolledIn2Sv = _user.IsEnrolledIn2Sv,
                        IsDelegatedAdmin = _user.IsDelegatedAdmin,
                        HashFunction = _user.HashFunction,
                        IsAdmin = _user.IsAdmin,
                        IsEnforcedIn2Sv = _user.IsEnforcedIn2Sv,
                        Password = _user.Password,
                        Phones = _user.Phones,
                        VerificationCodes = _verificationCodes,
                        Suspended = _user.Suspended

                    };

                    return myuser;
                }            

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }


        public static string GenerateVerificationCodes(string userKey)
        {
            try
            {
                DirectoryService _service = GetGoogleDirectoryService();

                var generateVerificationCodesRequest = _service.VerificationCodes.Generate(userKey);
                var tokensList = generateVerificationCodesRequest.Execute();

                return tokensList;

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static IList<VerificationCode> GetVerificationCodes(string userKey)
        {
            try
            {
                DirectoryService _service = GetGoogleDirectoryService();

                var verificationCodesRequest = _service.VerificationCodes.List(userKey);
                var verificationCodes = verificationCodesRequest.Execute();

                //var verificationCode = verificationCodes.Items[0].VerificationCodeValue;
                var verificationCodesList = verificationCodes.Items;

                return verificationCodesList;

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return null;


        }

        private static DirectoryService GetGoogleDirectoryService()
        {
            try
            {
                //vcaperuadmin@ajegroup.com Psw: maximo01
                ////Set location for Google Token to be locally stored
                var googleTokenLocation = Path.Combine(HomeController._wwwRoot.WebRootPath, GoogleFolder);


                //Load the Client Configuration in JSON Format as a stream which is used for API Calls
                var fileStream = new FileStream(ClientSecretJsonFile, FileMode.Open, FileAccess.Read);


                //This will create a Token Response User File on the GoogleFolder indicated on your Application
                var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(fileStream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                  new FileDataStore(googleTokenLocation)).Result;



                //Create Directory API service.
                var _service = new DirectoryService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = ApplicationName,
                });


                return _service;

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return null;
        }


        public class MyGoogleUserInfo
        {
            public string CustomerId { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string ThumbnailPhotoUrl { get; set; }
            public bool? IsEnrolledIn2Sv { get; set; }
            public bool? IsDelegatedAdmin { get; set; }

            public string HashFunction { get; set; }
            public bool? IsAdmin { get; set; }
            public bool? IsEnforcedIn2Sv { get; set; }

            public bool? Suspended { get; set; }

            public string Password { get; set; }

            public IList<UserPhone> Phones { get; set; }
            public IList<VerificationCode> VerificationCodes { get; set; }
        }
    }
}
