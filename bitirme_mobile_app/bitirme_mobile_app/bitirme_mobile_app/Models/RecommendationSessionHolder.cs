using bitirme_mobile_app.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class RecommendationSessionHolder
    {
        /// <summary>
        /// it is in the main memory during program is running. It saved to permanent memory also to save 
        /// previous recommendation sessions
        /// </summary>
        /// 

        [JsonProperty]
        private List<RecommendationSession> _recommendationSessions { get; set; }

        [JsonProperty]
        private int _lastId = 0;

        public RecommendationSessionHolder()
        {
            _recommendationSessions = new List<RecommendationSession>();
        }       

        public int addNewSession(RecommendationSession session)
        {
            _lastId++;
            session.id = _lastId;
            _recommendationSessions.Add(session);
            return _lastId;
        }

        public async Task deleteSession(RecommendationSession session)
        {
            _recommendationSessions.Remove(session);
            await DBHelper.updateDB();
        }

        /// <summary>
        /// actual sessions is made private because adding list must be happen via only addNewSession func to update last id
        /// Logic might be changed in future if a real db used
        /// </summary>
        /// <returns></returns>
        public List<RecommendationSession> getSessions()
        {
            return new List<RecommendationSession>(_recommendationSessions);
        }
    }
}
