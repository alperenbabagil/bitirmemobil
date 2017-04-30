using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class RecommendationRequest
    {
        public Dictionary<string, double> pairs { get; set; }
        public int sessionId { get; set; }
    }
}
