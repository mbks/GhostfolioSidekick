﻿using CsvHelper.Configuration.Attributes;

namespace GhostfolioSidekick.FileImporter.Trading212
{
    public class Trading212Record
    {
        public string Action { get; set; }
 
        public DateTime Time { get; set; }

        public string ISIN { get; set; }

        public string Ticker { get; set; }

        public string Name { get; set; }

        [Name("No. of shares")]
        public decimal? NumberOfShares { get; set; }

        [Name("Price / share")]
        public decimal? Price { get; set; }

        [Name("Currency (Price / share)")]
        public string Currency { get; set; }

        [Name("Exchange rate")]
        public decimal? ExchangeRate { get; set; }

        [Name("Currency (Result)")]
        public string CurrencySource { get; set; }

        [Optional]
        [Name("Stamp duty reserve tax")]
        public decimal? FeeUK { get; set; }

        [Optional]
        [Name("Currency (Stamp duty reserve tax)")]
        public string FeeUKCurrency { get; set; }

        public string Notes { get; set; }

        [Name("ID")]
        public string Id { get; set; }

        [Name("Currency conversion fee")]
        public decimal? ConversionFee { get; set; }

        [Name("Currency (Currency conversion fee)")]
        public string ConversionFeeCurrency { get; set; }
    }
}