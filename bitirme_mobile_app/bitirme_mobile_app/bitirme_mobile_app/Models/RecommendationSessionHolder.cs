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
        public List<RecommendationSession> recommendationSessions { get; set; }
    }
}
