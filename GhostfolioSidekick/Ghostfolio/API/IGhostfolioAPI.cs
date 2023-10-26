﻿using GhostfolioSidekick.Model;

namespace GhostfolioSidekick.Ghostfolio.API
{
	public interface IGhostfolioAPI
	{
		Task<Asset?> FindSymbolByIdentifier(string? identifier, Func<IEnumerable<Asset>, Asset?> selector = null);

		Task<Money?> GetConvertedPrice(Money money, Currency targetCurrency, DateTime date);

		Task<Money?> GetMarketPrice(Asset asset, DateTime date);

		Task<Account?> GetAccountByName(string name);

		Task UpdateAccount(Account account);

		Task<IEnumerable<MarketDataList>> GetMarketData();

		Task<MarketDataList> GetMarketData(string symbol, string dataSource);

		Task UpdateMarketData(SymbolProfile marketData);

		Task DeleteSymbol(SymbolProfile marketData);

		Task CreateManualSymbol(Asset asset);

		Task<IEnumerable<Activity>> GetAllActivities();

		Task SetMarketPrice(SymbolProfile assetProfile, Money money);
	}
}
