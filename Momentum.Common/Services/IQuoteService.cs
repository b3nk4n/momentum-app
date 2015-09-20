﻿using Momentum.Common.Models;
using System.Threading.Tasks;

namespace Momentum.Common.Services
{
    /// <summary>
    /// Quote service interface to load daily qoutes from the web.
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>
        /// Loads a quote from the web.
        /// </summary>
        /// <returns>Returns the laoded quote or NULL in case of an error.</returns>
        Task<QuoteDataModel> LoadQuoteAsync();

        /// <summary>
        /// Gets or sets the region language for the quotes to load.
        /// </summary>
        string RegionLanguageIso { get; set; }
    }
}
