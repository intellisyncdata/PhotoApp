using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Common
{
    public class ApiUrl
    {
        public const string ApiCheckCode = "https://photo-app-api.intellisyncdata.com/api/v1/apps/setup_photoapp";
        public const string ApiDeactive = "http://photo-app-api.intellisyncdata.com/api/v1/photo_apps/1/";
        public const string ApiGetLayout = "https://photo-app-api.intellisyncdata.com/api/v1/apps/{0}/layouts";
        public const string ApiSendMessage = "https://photo-app-api.intellisyncdata.com/api/v1/apps/{0}/ping";
        public const string ApiCreateTransactionPayment = "https://photo-app-api.intellisyncdata.com/api/v1/apps/{0}/payments/create";
        public const string ApiCompleteTransactionPayment = "https://photo-app-api.intellisyncdata.com/api/v1/apps/{0}/payments/confirm";
        public const string ApiGetBackground = "https://your-api-url.com/check-code";
        /// <summary>
        /// Method: POST
        /// {0}: <int>photo_app_id</int>
        /// {1}: <int>payment_id</int>
        /// Body: images, video (form-data)
        /// </summary>
        public const string ApiUploadMedia = "https://photo-app-api.intellisyncdata.com/api/v1/apps/{0}/payments/{1}/media_upload";
        public const string DefaultToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwidHlwZSI6InVzZXIiLCJ1c2VybmFtZSI6ImFkbWluIiwidXNlcl90eXBlIjoiYWRtaW4iLCJwZXJtaXNzaW9ucyI6WyJzdGF0aXN0aWNzIiwidXNlcnMiLCJ0cmFuc2FjdGlvbnMiLCJwaG90b19hcHBzIiwibGF5b3V0cyIsImJnX2xheW91dHMiLCJncm91cHMiXX0.IGELf2fzurgVABaALcU0rVKlbYc0NaBrJ7SVeqqhQk8";
    }
}
