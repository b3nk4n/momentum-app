using Momentum.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPCore.Framework.Data;
using UWPCore.Framework.Networking;
using UWPCore.Framework.Storage;

namespace Momentum.App.Services
{
    public class QuoteService : IQuoteService
    {
        public const string QUOTE_URI = "http://www.bsautermeister.de/impetus/api/quotes.php?format=json&method=random";

        IHttpService _httpService;
        ISerializationService _serializationService;

        /// <summary>
        /// The time stamp the last time an quote has been loaded successfully.
        /// </summary>
        private static StoredObjectBase<int> QuoteDay = new LocalObject<int>("_quoteDay_", -1);

        /// <summary>
        /// The saved quote data of the last loaded quote for reuse.
        /// </summary>
        private static StoredObjectBase<string> LastQuoteDataModel = new LocalObject<string>("_quoteDataModel_", null);

        /// <summary>
        /// Creates a QuoteService instance.
        /// </summary>
        /// <param name="regionLanguageIso">The region language in ISO format.</param>
        public QuoteService(string regionLanguageIso = "en-US")
        {
            _httpService = new HttpService();
            _serializationService = new DataContractSerializationService();

            RegionLanguageIso = regionLanguageIso;
        }

        public async Task<QuoteDataModel> LoadQuoteAsync()
        {
            // reuse the quote, when we are at the same day
            if (DateTime.Now.Day == QuoteDay.Value)
            {
                var quoteJson = LastQuoteDataModel.Value;

                if (!string.IsNullOrEmpty(quoteJson))
                {
                    var quote = _serializationService.DeserializeJson<QuoteDataModel>(quoteJson);
                    return GetWithFixedQuotations(quote);
                }
            }

            var jsonString = await _httpService.GetAsync(new Uri(QUOTE_URI, UriKind.Absolute));

            if (!string.IsNullOrEmpty(jsonString))
            {
                var quoteResult = _serializationService.DeserializeJson<QuoteResultModel>(jsonString);

                if (quoteResult.data != null)
                {
                    QuoteDay.Value = DateTime.Now.Day;

                    LastQuoteDataModel.Value = _serializationService.SerializeJson(quoteResult.data);

                    return GetWithFixedQuotations(quoteResult.data);
                }
            }

            return GetDefaultQuote();
        }

        public string RegionLanguageIso { get; set; } // TODO: use language info for localization

        /// <summary>
        /// Gets the default quote.
        /// </summary>
        /// <returns>Returns the default quote.</returns>
        private QuoteDataModel GetDefaultQuote()
        {
            return new QuoteDataModel()
            {
                author = "Confucius",
                quote = "\"Id does not matter how slowly you go as long as you do not stop.\""
            };


        }

        private QuoteDataModel GetWithFixedQuotations(QuoteDataModel data)
        {
            if (!data.quote.StartsWith("\""))
                data.quote = "\"" + data.quote;
            if (!data.quote.EndsWith("\""))
                data.quote = data.quote + "\"";

            return data;
        }
    }
}
