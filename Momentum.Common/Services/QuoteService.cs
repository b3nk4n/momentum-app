﻿using Momentum.Common.Models;
using Ninject;
using System;
using System.Threading.Tasks;
using UWPCore.Framework.Data;
using UWPCore.Framework.Networking;
using UWPCore.Framework.Storage;

namespace Momentum.Common.Services
{
    /// <summary>
    /// Quote service class to load daily qoutes from the web.
    /// </summary>
    public class QuoteService : IQuoteService
    {
        public const string QUOTE_URI = "http://www.bsautermeister.de/dailyfocus/api/quotes.php?format=json&method=random&lang=";

        IHttpService _httpService;
        ISerializationService _serializationService;

        /// <summary>
        /// The time stamp the last time an quote has been loaded successfully.
        /// </summary>
        private static StoredObjectBase<DateTimeOffset> QuoteDay = new LocalObject<DateTimeOffset>("quoteDate", DateTimeOffset.MinValue);

        /// <summary>
        /// The saved quote data of the last loaded quote for reuse.
        /// </summary>
        private static StoredObjectBase<string> LastQuoteDataModel = new LocalObject<string>("quoteDataModel", null);

        /// <summary>
        /// Creates a QuoteService instance.
        /// </summary>
        /// <param name="regionLanguageIso">The region language in ISO format.</param>
        [Inject]
        public QuoteService(string regionLanguageIso, IHttpService httpService, ISerializationService serializationService)
        {
            _httpService = httpService;
            _serializationService = serializationService;

            RegionLanguageIso = regionLanguageIso;
        }

        public async Task<QuoteDataModel> LoadQuoteAsync()
        {
            // reuse the quote, when we are at the same day
            if (!AppUtils.NeedsUpdate(QuoteDay.Value))
            {
                var quoteJson = LastQuoteDataModel.Value;

                if (!string.IsNullOrEmpty(quoteJson))
                {
                    var quote = _serializationService.DeserializeJson<QuoteDataModel>(quoteJson);
                    return GetWithFixedQuotations(quote);
                }
            }

            try
            {
                var res = await _httpService.GetAsync(new Uri(QUOTE_URI + RegionLanguageIso, UriKind.Absolute));

                if (res != null &&
                    res.IsSuccessStatusCode)
                {
                    var jsonString = await res.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        var quoteResult = _serializationService.DeserializeJson<QuoteResultModel>(jsonString);

                        if (quoteResult.data != null)
                        {
                            LastQuoteDataModel.Value = _serializationService.SerializeJson(quoteResult.data);

                            QuoteDay.Value = DateTimeOffset.Now;

                            return GetWithFixedQuotations(quoteResult.data);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // no network connectivity
            }

            return GetDefaultQuote();
        }

        public string RegionLanguageIso { get; set; }

        /// <summary>
        /// Gets the default quote.
        /// </summary>
        /// <returns>Returns the default quote.</returns>
        private QuoteDataModel GetDefaultQuote()
        {
            if (RegionLanguageIso.StartsWith("de"))
            {
                return new QuoteDataModel()
                {
                    author = "Friedrich von Schiller",
                    quote = "\"Jeder Tag ist eine neue Chance, das zu tun, was du möchtest.\""
                };
            }
            else if (RegionLanguageIso.StartsWith("tr"))
            {
                return new QuoteDataModel()
                {
                    author = "Çobanoğlu, Özkul",
                    quote = "\"Sabır acıdır, meyvesi tatlıdır.\""
                };
            }
            else
            {
                return new QuoteDataModel()
                {
                    author = "Confucius",
                    quote = "\"Id does not matter how slowly you go as long as you do not stop.\""
                };
            }
        }

        /// <summary>
        /// Gets the quote data and ensures that the quote has quotations.
        /// </summary>
        /// <param name="data">The quote data to fix.</param>
        /// <returns>Returns the fixed quote data.</returns>
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
