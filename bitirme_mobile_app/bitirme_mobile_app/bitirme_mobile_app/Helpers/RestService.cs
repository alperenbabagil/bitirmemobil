using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
using bitirme_mobile_app.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Helpers
{
    public class RestService
    {
        private string baseUrl = Constants.ip;

        public async Task<bool> login(User user)
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            var resp = await postBaseFunc(jsonData, "login");
            if (resp == "true") return true;
            else return false;
        }

        public async Task<bool> signup(User user)
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            var resp = await postBaseFunc(jsonData, "sign_up");
            if (resp == "true") return true;
            else return false;
        }

        public async Task<string> sendRecommendationRequest(IList<MovieRateListViewItem> lvItems,int id)
        {
            Dictionary<string, double> pairs1 = new Dictionary<string, double>();
            foreach (var lvi in lvItems)
            {
                pairs1[lvi.Movie.ImdbId] = lvi.Rating;
            }
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(new RecommendationRequest() {pairs=pairs1,sessionId=id });
            var resp = await postBaseFunc(jsonData, "recommendation_request");
            return resp;
        }



        public async Task<List<Movie>> getTopMovies()
        {
            try
            {
                var resp = await postBaseFunc(null, "getTopMovies");

                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Movie>>(resp);
                //List<Stok> Mycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stok>>(result);
            }
            catch (Exception s)
            {
                Logger.errLog("RestService.cs-getTopMovies", s);
                //handleExceptions(s, "Stoklar alınamadı");
            }
            return null;
        }

        //public async Task<List<Movie>> getRecommendations(List<RecommendationRequest> recReqs)
        //{
        //    try
        //    {
        //        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(recReqs);
        //        var resp = await postBaseFunc(jsonData, "getRecommendations");
        //        List<Movie> movies = null;
        //        try
        //        {
        //            movies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Movie>>(resp);
        //        }
        //        catch (Exception e)
        //        {

        //        }
        //        return movies;
        //        //List<Stok> Mycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stok>>(result);
        //    }
        //    catch (Exception s)
        //    {
        //        Logger.errLog("RestService.cs-getRecommendations", s);
        //        //handleExceptions(s, "Stoklar alınamadı");
        //    }
        //    return null;

        //}

        public async Task<List<string>> getTop250Ids()
        {
            try
            {
                string jsonData = null;
                var resp = await postBaseFunc(jsonData, "getTop250Ids");
                List<string> moviesIds = null;
                try
                {
                    moviesIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(resp);
                }
                catch (Exception e)
                {

                }
                return moviesIds;
                //List<Stok> Mycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stok>>(result);
            }
            catch (Exception s)
            {
                Logger.errLog("RestService.cs-getTop250Ids", s);
                //handleExceptions(s, "Stoklar alınamadı");
            }
            return null;

        }
        /// <summary>
        /// start and end indexes not checked. So they must be valid
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public async Task<List<Movie>> getMovieInfoFromWeb(List<string> ids,int startIndex,int endIndex)
        {
            List<Movie> movies = new List<Movie>();
            try
            {
                using (var client = new HttpClient())
                {
                    for(int i= startIndex; i< endIndex; i++)
                    {
                        client.BaseAddress = new Uri("http://www.omdbapi.com/?i=tt"+ids[i]);
                        HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                        var result = await response.Content.ReadAsStringAsync();
                        //dynamic data = JObject.Parse(source);
                        dynamic tmp = null;
                        try
                        {
                            tmp = JsonConvert.DeserializeObject(result);
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                        Newtonsoft.Json.Linq.JArray ratings = tmp.Ratings;
                        var rate0 = ratings[0];
                        var ratingString = (string)(ratings[0])["Value"];
                        var ratingDouble = Double.Parse(ratingString.Split('/')[0]);
                        Movie movie = new Movie()
                        {
                            ImdbId = ids[i],
                            Name = (string)tmp.Title,
                            Genres = new List<string>(((string)tmp.Genre).Split(',')),
                            ImdbImageUrl = (string)tmp.Poster,
                            //Rating= (JsonConvert.DeserializeObject<Ratings>(tmp.Ratings))
                            Rating = ratingDouble
                        };
                        movies.Add(movie);
                    }
                }

                return movies;
                //List<Stok> Mycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stok>>(result);
            }
            catch (Exception s)
            {
                Logger.errLog("RestService.cs-getTop250Ids", s);
                //handleExceptions(s, "Stoklar alınamadı");
            }
            return movies;

        }

        public class Ratings
        {
            public string Source { get; set; }
            public string Value { get; set; }
        }


        public async Task<bool> checkUserIsCorrect(User user)
        {
            try
            {
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(user);

                var resultstr = await postBaseFunc(jsonData, "usrcheck");

                var result = resultstr == "True" ? true : false;

                //return Newtonsoft.Json.JsonConvert.DeserializeObject<List<RaporUrunBaz>>(resp);
                //List<Stok> Mycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stok>>(result);
                //if (result as string == "onay") return true;
                //if (result as string == "red") return false;
                return result;
            }
            catch (Exception s)
            {
                Logger.errLog("checkUserIsCorrect", s);
                throw s;
            }
        }

        private async Task<string> postBaseFunc(string jsonData, string path)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(baseUrl + "/" + path);
                    if (App.CurrentUser != null) client.DefaultRequestHeaders.Add("userData", Newtonsoft.Json.JsonConvert.SerializeObject(App.CurrentUser));
                    StringContent content = null;
                    if (jsonData != null) content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(client.BaseAddress, content);
                    var result = await response.Content.ReadAsStringAsync();
                    result = result.Replace("\\", "");
                    // result = result.Substring(1);
                    // result = result.Substring(0, result.Length - 1);
                    return result;
                }
                catch (Exception e)
                {
                    handleExceptions(e, "network error");
                    return null;
                }
                
            }
        }

        private async void handleExceptions(Exception e, string msg)
        {
            //await PopupManager.Instance.showInfoPopup(App.AppInstance.MainPage.Navigation.NavigationStack.LastOrDefault(), msg);
        }


    }
}
